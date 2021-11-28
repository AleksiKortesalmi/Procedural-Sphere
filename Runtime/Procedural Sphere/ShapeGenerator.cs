using UnityEngine;

public class ShapeGenerator
{
    readonly ShapeSettings settings;
    /// <summary>
    /// Noise settings struct to used to send noise settings data to the GPU.
    /// </summary>
    struct NoiseSettingsStruct
    {
        public Vector3 offset;
        public float frequency;
        public float firstLayerRoughness;
        public float roughness;
        public float persistence;
        public float strength;
        public float minValue;
        public float weightMultiplier;
        public int numLayers;
        public int firstLayerAsMask;
    }
    readonly NoiseSettingsStruct[] structuredNoiseSettings;
    float[] noiseValues;


    /// <summary>
    /// Constructor + Initialize gradient look up table (gradientLUT) and noiseSettings.
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="gradient"></param>
    public ShapeGenerator(ShapeSettings settings)
    {
        this.settings = settings;

        structuredNoiseSettings = new NoiseSettingsStruct[settings.noiseLayers.Length];
    }

    /// <summary>
    ///  Dispatch a compute shader that calculates a noise value for each vertex with a compute shader.
    /// </summary>
    /// <param name="vertices">Mesh's current vertices. Returns new positions based on noise values.</param>
    /// <param name="sphereVertices">Mesh's vertices in a perfect sphere shape.</param>
    /// <param name="calculateNoiseOnSphere"></param>
    /// <returns>Noise values per vertex. (Useful for mapping vertex colors.)</returns>
    public float[] GenerateShape(ref Vector3[] vertices, Vector3[] sphereVertices, ComputeShader calculateNoiseOnSphere)
    {
        // Convert NoiseSettings to NoiseSettingsStruct
        int enabledNoiseLayersLength = 0;
        for (int i = 0; i < settings.noiseLayers.Length; i++)
        {
            if (settings.noiseLayers[i].enabled)
                enabledNoiseLayersLength++;
        }
        int index = 0;
        for (int i = 0; i < settings.noiseLayers.Length; i++)
        {
            if (settings.noiseLayers[i].enabled)
            {
                structuredNoiseSettings[index].offset = settings.noiseLayers[i].noiseSettings.simpleNoiseSettings.offset;
                structuredNoiseSettings[index].frequency = settings.noiseLayers[i].noiseSettings.simpleNoiseSettings.roughness;
                structuredNoiseSettings[index].firstLayerRoughness = settings.noiseLayers[i].noiseSettings.simpleNoiseSettings.firstLayerRoughness;
                structuredNoiseSettings[index].roughness = settings.noiseLayers[i].noiseSettings.simpleNoiseSettings.roughness;
                structuredNoiseSettings[index].persistence = settings.noiseLayers[i].noiseSettings.simpleNoiseSettings.persistence;
                structuredNoiseSettings[index].strength = settings.noiseLayers[i].noiseSettings.simpleNoiseSettings.strength;
                structuredNoiseSettings[index].minValue = settings.noiseLayers[i].noiseSettings.simpleNoiseSettings.minValue;
                structuredNoiseSettings[index].weightMultiplier = settings.noiseLayers[i].noiseSettings.filterType == NoiseSettings.FilterType.Rigid ? settings.noiseLayers[i].noiseSettings.rigidNoiseSettings.weightMultiplier : 0;
                structuredNoiseSettings[index].numLayers = settings.noiseLayers[i].noiseSettings.simpleNoiseSettings.numLayers;
                structuredNoiseSettings[index].firstLayerAsMask = settings.noiseLayers[i].firstLayerAsMask;

                index++;
            }
        }

        // Buffer initializations

        // Noise settings
        int structSize = sizeof(float) * 11 + sizeof(int);
        ComputeBuffer noiseSettingsBuffer = new ComputeBuffer(structuredNoiseSettings.Length, structSize);
        noiseSettingsBuffer.SetData(structuredNoiseSettings);

        // Vertices
        ComputeBuffer vertexBuffer = new ComputeBuffer(vertices.Length, sizeof(float) * 3);
        vertexBuffer.SetData(sphereVertices);

        // Noise values
        if (noiseValues == null)
            noiseValues = new float[vertices.Length];
        ComputeBuffer noiseValuesBuffer = new ComputeBuffer(vertices.Length, sizeof(float));
        noiseValuesBuffer.SetData(noiseValues);

        calculateNoiseOnSphere.SetBuffer(0, "vertices", vertexBuffer);
        calculateNoiseOnSphere.SetBuffer(0, "noiseSettings", noiseSettingsBuffer);
        calculateNoiseOnSphere.SetBuffer(0, "noiseValues", noiseValuesBuffer);

        calculateNoiseOnSphere.SetFloat("radius", settings.radius);
        calculateNoiseOnSphere.SetInt("numNoiseLayers", settings.noiseLayers.Length);

        int numThreadGroups = Mathf.CeilToInt((float)vertices.Length / 512);
        calculateNoiseOnSphere.Dispatch(0, numThreadGroups, 1, 1);

        vertexBuffer.GetData(vertices);
        noiseValuesBuffer.GetData(noiseValues);

        vertexBuffer.Dispose();
        noiseSettingsBuffer.Dispose();
        noiseValuesBuffer.Dispose();

        return noiseValues;
    }

    /// <summary>
    /// Dispatch a compute shader that calculates vertex position on a sphere for each vertex.
    /// </summary>
    public void UniformDistributionOnSphere(ref Vector3[] vertices, ref Vector3[] normals, int resolution, ComputeShader calculateVerticesOnSphere)
    {
        ComputeBuffer vertexBuffer = new ComputeBuffer(vertices.Length, sizeof(float) * 3);
        vertexBuffer.SetData(vertices);

        ComputeBuffer normalBuffer = new ComputeBuffer(normals.Length, sizeof(float) * 3);
        normalBuffer.SetData(normals);

        calculateVerticesOnSphere.SetBuffer(0, "vertices", vertexBuffer);
        calculateVerticesOnSphere.SetBuffer(0, "normals", normalBuffer);

        calculateVerticesOnSphere.SetInt("resolution", resolution);
        calculateVerticesOnSphere.SetFloat("radius", settings.radius);

        int numThreadGroups = Mathf.CeilToInt((float)vertices.Length / 512);
        calculateVerticesOnSphere.Dispatch(0, numThreadGroups, 1, 1);

        vertexBuffer.GetData(vertices);
        normalBuffer.GetData(normals);

        vertexBuffer.Dispose();
        normalBuffer.Dispose();
    }
}
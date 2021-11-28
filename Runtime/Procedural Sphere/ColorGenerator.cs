using UnityEngine;

public class ColorGenerator
{
    readonly ColorSettings settings;
    /// <summary>
    /// Gradient look up table. Eliminates need for Gradient.Evaluate which is too slow to be run thousands of times per frame.
    /// </summary>
    readonly Color32[] gradientLUT;
    /// <summary>
    /// Total range of noise values.
    /// </summary>
    Vector2 noiseValueMinMax;


    public ColorGenerator(ColorSettings settings)
    {
        this.settings = settings;

        gradientLUT = new Color32[256];
        for (int i = 0; i < 256; i++)
        {
            gradientLUT[i] = settings.sphereColorGradient.Evaluate(i / 255f);
        }

        noiseValueMinMax = new Vector2(float.MaxValue, float.MinValue);
    }

    /// <summary>
    /// Set vertex colors by evaluating a color from a gradient based comparing a noise value to the total range of noisevalues.
    /// </summary>
    /// <param name="vertexColors">Mesh's current vertex colors. Sets new colors based on noise values.</param>
    /// <param name="noiseValues">Noise values calculated for vertex height.</param>
    public void GenerateColors(ref Color32[] vertexColors, float[] noiseValues)
    {
        // Calculate the total range of the noise values
        for (int i = 0; i < noiseValues.Length; i++)
        {
            if (noiseValues[i] < noiseValueMinMax.x)
                noiseValueMinMax.x = noiseValues[i];
            else if (noiseValues[i] > noiseValueMinMax.y)
                noiseValueMinMax.y = noiseValues[i];
        }

        // Set colors from gradientLUT
        for (int i = 0; i < noiseValues.Length; i++)
        {
            vertexColors[i] = gradientLUT[(int)((noiseValues[i] - noiseValueMinMax.x) / (noiseValueMinMax.y - noiseValueMinMax.x) * 255)];
        }
    }

    public void UpdateGradientLUT()
    {
        for (int i = 0; i < 256; i++)
        {
            gradientLUT[i] = settings.sphereColorGradient.Evaluate(i / 255f);
        }
    }
}

using UnityEditor;
using UnityEngine;


[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralSphere : MonoBehaviour
{
	#region Fields

	public ShapeSettings shapeSettings;
	public ColorSettings colorSettings;
	public ComputeShader calculateVerticesOnSphere;
	public ComputeShader calculateNoiseOnSphere;

	[Range(2, 99)]
	public int resolution;
	[Tooltip("Animate noise with the position of a transform. Optional.")]

	public bool customOffset;
	[DrawIf("customOffset")]
	public Transform customOffsetTransform;
	[DrawIf("customOffset")]
	public float customOffsetSensitivity = 1;

	[Tooltip("Enable GPU Instancing by linking procedural spheres.")]
	[HideInInspector]
	public bool enableGPUInstancing = false;
	[DrawIf("enableGPUInstancing")]
	[HideInInspector]
	public ProceduralSphere copySphere;

	// Array initialization (Check ProceduralSphereEditor)
	[HideInInspector]
	public int noiseLayersLength = 0;

	#endregion

	void Start()
	{
		meshFilter = GetComponent<MeshFilter>();

		if (!enableGPUInstancing)
			GenerateMesh();
	}

	void Update()
	{
		if(!enableGPUInstancing && shapeSettings.noiseLayers.Length != 0)
		{
			// Animate noise with the position of the transform given
			if (customOffset)
			{
				Vector3 offset = customOffsetTransform.position * customOffsetSensitivity;

				for (int i = 0; i < shapeSettings.noiseLayers.Length; i++)
				{
					shapeSettings.noiseLayers[i].noiseSettings.simpleNoiseSettings.offset = offset;
				}

				ApplyShapeAndColor();
			}
			// Animate noise with offsetVelocity
			else if (shapeSettings.offsetVelocity != 0)
			{
				Vector3 offset = Vector3.forward * shapeSettings.offsetVelocity * Time.deltaTime;

				for (int i = 0; i < shapeSettings.noiseLayers.Length; i++)
				{
					shapeSettings.noiseLayers[i].noiseSettings.simpleNoiseSettings.offset += offset;
				}

				ApplyShapeAndColor();
			}
		}
	}

	void LateUpdate()
	{
		if (enableGPUInstancing)
		{
			meshFilter.mesh = copySphere.mesh;
		}
	}

	
	#region Mesh Generation

	[HideInInspector]
	public MeshFilter meshFilter;
	Mesh mesh;
	Color32[] vertexColors;
	Vector3[] originalVertices;
	Vector3[] vertices;
	Vector3[] normals;
	float[] noiseValues;

	ShapeGenerator shapeGenerator;
	ColorGenerator ColorGenerator;

	void GenerateMesh()
	{
		meshFilter.mesh = mesh = new Mesh();
		// Change indexFormat to UInt32 to support more vertices (4 Billion)
		mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt16;
		mesh.name = "Procedural Sphere";

		shapeGenerator = new ShapeGenerator(shapeSettings);
		ColorGenerator = new ColorGenerator(colorSettings);

		int cornerVertices = 8;
		int edgeVertices = (resolution * 3 - 3) * 4;
		int faceVertices = (resolution - 1) * (resolution - 1) * 6;
		vertices = new Vector3[cornerVertices + edgeVertices + faceVertices];
		vertexColors = new Color32[vertices.Length];
		noiseValues = new float[vertices.Length];
		originalVertices = new Vector3[vertices.Length];
		normals = new Vector3[vertices.Length];

		CreateVertices();

		CreateTriangles();

		ApplyShapeAndColor();

#if UNITY_EDITOR
		mesh.RecalculateBounds();
#endif
	}

	void ApplyShapeAndColor()
	{
		noiseValues = shapeGenerator.GenerateShape(ref vertices, originalVertices, calculateNoiseOnSphere);

		ColorGenerator.GenerateColors(ref vertexColors, noiseValues);

		mesh.vertices = vertices;
		mesh.colors32 = vertexColors;

		if (Application.isPlaying)
			mesh.RecalculateNormals();
	}

	void CreateVertices()
	{
		// Wall Vertices
		int v = 0;
		for (int y = 0; y <= resolution; y++)
		{
			for (int x = 0; x <= resolution; x++)
			{
				vertices[v++] = new Vector3(x, y, 0);
			}
			for (int z = 1; z <= resolution; z++)
			{
				vertices[v++] = new Vector3(resolution, y, z);
			}
			for (int x = resolution - 1; x >= 0; x--)
			{
				vertices[v++] = new Vector3(x, y, resolution);
			}
			for (int z = resolution - 1; z > 0; z--)
			{
				vertices[v++] = new Vector3(0, y, z);
			}
		}
		// Top cap vertices
		for (int z = 1; z < resolution; z++)
		{
			for (int x = 1; x < resolution; x++)
			{
				vertices[v++] = new Vector3(x, resolution, z);
			}
		}
		// Bottom cap vertices
		for (int z = 1; z < resolution; z++)
		{
			for (int x = 1; x < resolution; x++)
			{
				vertices[v++] = new Vector3(x, 0, z);
			}
		}

		// Transform the cube to a sphere with good geometry distribution
		shapeGenerator.UniformDistributionOnSphere(ref vertices, ref normals, resolution, calculateVerticesOnSphere);

		vertices.CopyTo(originalVertices, 0);
		mesh.vertices = vertices;
		mesh.normals = normals;
	}

	void CreateTriangles()
	{
		int quads = resolution * resolution * 6;
		int[] triangles = new int[quads * 6];

		int ring = resolution * 4;
		int t = 0, v = 0;

		// Wall triangles
		for (int y = 0; y < resolution; y++, v++)
		{
			for (int q = 0; q < ring - 1; q++, v++)
			{
				t = SetQuad(triangles, t, v, v + 1, v + ring, v + ring + 1);
			}
			t = SetQuad(triangles, t, v, v - ring + 1, v + ring, v + 1);
		}

		t = CreateTopFace(triangles, t, ring);
		CreateBottomFace(triangles, t, ring);

		mesh.triangles = triangles;
	}

	int CreateTopFace(int[] triangles, int t, int ring)
	{
		// First row triangles
		int v = ring * resolution;
		for (int x = 0; x < resolution - 1; x++, v++)
		{
			t = SetQuad(triangles, t, v, v + 1, v + ring - 1, v + ring);
		}
		t = SetQuad(triangles, t, v, v + 1, v + ring - 1, v + 2);

		// Middle triangles
		int vMin = ring * (resolution + 1) - 1;
		int vMid = vMin + 1;
		int vMax = v + 2;

		for (int z = 1; z < resolution - 1; z++, vMin--, vMid++, vMax++)
		{
			t = SetQuad(triangles, t, vMin, vMid, vMin - 1, vMid + resolution - 1);
			for (int x = 1; x < resolution - 1; x++, vMid++)
			{
				t = SetQuad(
					triangles, t,
					vMid, vMid + 1, vMid + resolution - 1, vMid + resolution);
			}
			t = SetQuad(triangles, t, vMid, vMax, vMid + resolution - 1, vMax + 1);
		}

		// Last row triangles
		int vTop = vMin - 2;
		t = SetQuad(triangles, t, vMin, vMid, vTop + 1, vTop);
		for (int x = 1; x < resolution - 1; x++, vTop--, vMid++)
		{
			t = SetQuad(triangles, t, vMid, vMid + 1, vTop, vTop - 1);
		}
		t = SetQuad(triangles, t, vMid, vTop - 2, vTop, vTop - 1);

		return t;
	}

	int CreateBottomFace(int[] triangles, int t, int ring)
	{
		// First row triangles
		int v = 1;
		int vMid = vertices.Length - (resolution - 1) * (resolution - 1);
		t = SetQuad(triangles, t, ring - 1, vMid, 0, 1);
		for (int x = 1; x < resolution - 1; x++, v++, vMid++)
		{
			t = SetQuad(triangles, t, vMid, vMid + 1, v, v + 1);
		}
		t = SetQuad(triangles, t, vMid, v + 2, v, v + 1);

		// Middle triangles
		int vMin = ring - 2;
		vMid -= resolution - 2;
		int vMax = v + 2;

		for (int z = 1; z < resolution - 1; z++, vMin--, vMid++, vMax++)
		{
			t = SetQuad(triangles, t, vMin, vMid + resolution - 1, vMin + 1, vMid);
			for (int x = 1; x < resolution - 1; x++, vMid++)
			{
				t = SetQuad(
					triangles, t,
					vMid + resolution - 1, vMid + resolution, vMid, vMid + 1);
			}
			t = SetQuad(triangles, t, vMid + resolution - 1, vMax + 1, vMid, vMax);
		}

		// Last row triangles
		int vTop = vMin - 1;
		t = SetQuad(triangles, t, vTop + 1, vTop, vTop + 2, vMid);
		for (int x = 1; x < resolution - 1; x++, vTop--, vMid++)
		{
			t = SetQuad(triangles, t, vTop, vTop - 1, vMid, vMid + 1);
		}
		t = SetQuad(triangles, t, vTop, vTop - 1, vMid, vTop - 2);

		return t;
	}

	static int SetQuad(int[] triangles, int i, int v00, int v10, int v01, int v11)
	{
		triangles[i] = v00;
		triangles[i + 1] = triangles[i + 4] = v01;
		triangles[i + 2] = triangles[i + 3] = v10;
		triangles[i + 5] = v11;
		return i + 6;
	}

	#endregion

	#region Editor related

#if UNITY_EDITOR

	public void OnSettingsUpdated()
	{
		if(!meshFilter)
			meshFilter = GetComponent<MeshFilter>();

		GenerateMesh();

		if(shapeSettings.noiseLayers.Length != 0)
			ApplyShapeAndColor();
	}

#endif

	#endregion
}
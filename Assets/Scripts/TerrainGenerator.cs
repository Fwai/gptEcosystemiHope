using UnityEngine;

/// <summary>
/// Generates a procedural terrain mesh based on Perlin noise. The terrain size
/// and resolution (grid) can be configured. The height field includes a
/// falloff towards the edges so that the terrain forms an island or plateau.
/// The generated mesh is applied to a MeshFilter and MeshCollider.
/// </summary>
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class TerrainGenerator : MonoBehaviour
{
    [Header("Size & Resolution")]
    public int size = 140;      // length/width of the terrain in world units
    public int grid = 80;       // number of subdivisions per side

    [Header("Noise Parameters")]
    public float largeScale = 0.75f;
    public float detailScale = 0.25f;
    public float heightScale = 10f;
    public int seed = 12345;

    private MeshFilter _meshFilter;
    private MeshCollider _meshCollider;

    private System.Random _rng;
    private float _ox1, _oz1, _ox2, _oz2;

    void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _meshCollider = GetComponent<MeshCollider>();
        // Provide a default material if none assigned
        MeshRenderer mr = GetComponent<MeshRenderer>();
        if (mr != null && mr.sharedMaterial == null)
        {
            var mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0.45f, 0.6f, 0.35f);
            mr.sharedMaterial = mat;
        }
    }

    /// <summary>
    /// Generates a new terrain mesh using the current seed and noise parameters.
    /// </summary>
    public void Generate()
    {
        _rng = new System.Random(seed);
        // Random offsets to avoid repeating patterns when regenerating
        _ox1 = (float)_rng.NextDouble() * 10000f;
        _oz1 = (float)_rng.NextDouble() * 10000f;
        _ox2 = (float)_rng.NextDouble() * 10000f;
        _oz2 = (float)_rng.NextDouble() * 10000f;

        // Build the mesh and assign to the filter and collider
        Mesh m = BuildMesh();
        _meshFilter.sharedMesh = m;
        _meshCollider.sharedMesh = m;
    }

    private Mesh BuildMesh()
    {
        int vertsPerSide = grid + 1;
        Vector3[] verts = new Vector3[vertsPerSide * vertsPerSide];
        Vector2[] uvs = new Vector2[verts.Length];
        int[] tris = new int[grid * grid * 6];

        // Generate vertices and UVs
        for (int z = 0; z <= grid; z++)
        {
            for (int x = 0; x <= grid; x++)
            {
                int i = z * vertsPerSide + x;
                float wx = -size * 0.5f + (x / (float)grid) * size;
                float wz = -size * 0.5f + (z / (float)grid) * size;

                float y = WorldHeight(wx, wz);
                verts[i] = new Vector3(wx, y, wz);
                uvs[i] = new Vector2(x / (float)grid, z / (float)grid);
            }
        }

        // Generate triangle indices
        int t = 0;
        for (int z = 0; z < grid; z++)
        {
            for (int x = 0; x < grid; x++)
            {
                int i = z * (grid + 1) + x;
                tris[t++] = i;
                tris[t++] = i + grid + 1;
                tris[t++] = i + 1;

                tris[t++] = i + 1;
                tris[t++] = i + grid + 1;
                tris[t++] = i + grid + 2;
            }
        }

        Mesh m = new Mesh();
        m.name = "ProceduralTerrain";
        m.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        m.vertices = verts;
        m.uv = uvs;
        m.triangles = tris;
        m.RecalculateNormals();
        return m;
    }

    /// <summary>
    /// Computes a normalized height value in [0,1] for world coordinates. Uses a blend
    /// of two Perlin noise layers and applies a radial falloff to make edges lower.
    /// </summary>
    /// <param name="x">World X coordinate (centered at 0)</param>
    /// <param name="z">World Z coordinate (centered at 0)</param>
    /// <returns>Normalized height [0,1]</returns>
    public float Height01FromWorld(float x, float z)
    {
        float nx = Mathf.Clamp(x / (size * 0.5f), -1f, 1f);
        float nz = Mathf.Clamp(z / (size * 0.5f), -1f, 1f);

        float n1 = Mathf.PerlinNoise(_ox1 + nx / largeScale, _oz1 + nz / largeScale);
        float n2 = Mathf.PerlinNoise(_ox2 + nx / detailScale, _oz2 + nz / detailScale);
        float h = 0.7f * n1 + 0.3f * n2;

        float radius = Mathf.Sqrt(nx * nx + nz * nz);
        float edge = Mathf.Clamp01(1f - radius * 0.7f);
        float baseH = Mathf.Clamp01(0.5f + 0.35f * (h * 2f - 1f));
        return baseH * (0.85f + 0.15f * edge);
    }

    /// <summary>
    /// Returns the actual world height by scaling the normalized height.
    /// </summary>
    public float WorldHeight(float x, float z)
    {
        return Height01FromWorld(x, z) * heightScale;
    }
}
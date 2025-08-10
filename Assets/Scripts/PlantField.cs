using UnityEngine;

/// <summary>
/// Maintains plant biomass on a 2D grid and handles growth. Each cell stores
/// a biomass value which increases over time. Sampling and eating functions
/// allow animals to query or consume biomass. Growth rate and maximum
/// biomass can be adjusted via inspector.
/// </summary>
public class PlantField : MonoBehaviour
{
    public int grid = 80;
    public int size = 140;
    public float growth = 0.35f;
    public float maxBiomass = 1.0f;

    public float[] biomass;
    private TerrainGenerator _terrain;

    /// <summary>
    /// Initializes the plant field based on the terrain. Creates the grid
    /// array and sets initial biomass values according to terrain height
    /// (lower areas are moister and start with more plants).
    /// </summary>
    public void Init(TerrainGenerator terrain)
    {
        _terrain = terrain;
        grid = terrain.grid;
        size = terrain.size;
        biomass = new float[grid * grid];

        float cellSize = size / (float)grid;
        for (int z = 0; z < grid; z++)
        {
            for (int x = 0; x < grid; x++)
            {
                float wx = -size * 0.5f + (x + 0.5f) * cellSize;
                float wz = -size * 0.5f + (z + 0.5f) * cellSize;
                float h01 = _terrain.Height01FromWorld(wx, wz);
                float humidity = 1f - h01;
                float baseVal = Mathf.Clamp01(0.6f * humidity + 0.2f * Random.value);
                biomass[z * grid + x] = baseVal * 0.8f;
            }
        }
    }

    /// <summary>
    /// Advances plant growth for a time step. Each cell grows by a small
    /// random amount up to maxBiomass.
    /// </summary>
    public void Step(float dt)
    {
        float inc = growth * dt;
        for (int i = 0; i < biomass.Length; i++)
        {
            float rand = 0.5f + 0.5f * Random.value;
            float v = biomass[i] + inc * rand;
            biomass[i] = v > maxBiomass ? maxBiomass : v;
        }
    }

    /// <summary>
    /// Converts world coordinates to a 1D index into the biomass array. Clamps
    /// coordinates to the grid boundaries.
    /// </summary>
    public int IndexFromWorldXZ(float x, float z)
    {
        float gxF = Mathf.InverseLerp(-size * 0.5f, size * 0.5f, x) * grid;
        float gzF = Mathf.InverseLerp(-size * 0.5f, size * 0.5f, z) * grid;
        int gx = Mathf.Clamp(Mathf.FloorToInt(gxF), 0, grid - 1);
        int gz = Mathf.Clamp(Mathf.FloorToInt(gzF), 0, grid - 1);
        return gz * grid + gx;
    }
}
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Central manager that runs the ecosystem simulation. It maintains lists of
/// animals, updates plant growth, ticks each animal, handles reproduction,
/// births and deaths, and provides helper functions for sampling the terrain
/// and plants. It also allows regenerating the world with a new seed.
/// </summary>
public class SimulationManager : MonoBehaviour
{
    [Header("References")]
    public TerrainGenerator terrain;
    public PlantField plants;

    [Header("Simulation Parameters")]
    public bool paused = false;
    public float mutationRate = 0.08f;
    public float plantGrowth = 0.35f;
    public float plantMax = 1.0f;
    public float herbivoreEatRate = 0.15f;
    public float predatorEatGain = 0.6f;
    public float reproThreshold = 1.2f;
    public float reproCooldown = 8f;

    [Header("Bootstrap Settings")]
    public int startHerbivores = 60;
    public int startPredators = 10;

    private List<Animal> animals = new List<Animal>();
    private List<Animal> toAdd = new List<Animal>();
    private List<Animal> toRemove = new List<Animal>();

    private float acc;

    void Reset()
    {
        if (!terrain) terrain = FindObjectOfType<TerrainGenerator>();
        if (!plants) plants = GetComponent<PlantField>();
    }

    void Update()
    {
        // Toggle pause
        if (Input.GetKeyDown(KeyCode.Space)) paused = !paused;
        // Regenerate world with random seed
        if (Input.GetKeyDown(KeyCode.R)) RegenerateWorld(Random.Range(1, int.MaxValue));

        float dt = Mathf.Min(Time.deltaTime, 0.05f);
        if (paused) return;

        // Fixed timestep simulation at 30 Hz
        acc += dt;
        const float FIXED = 1f / 30f;
        while (acc >= FIXED)
        {
            Step(FIXED);
            acc -= FIXED;
        }
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 520, 20),
            "t=" + Time.timeSinceLevelLoad.ToString("0.0") + "s • Herb: " + CountSpecie("herb") +
            " • Pred: " + CountSpecie("pred") + " • Paused=" + paused);
        GUI.Label(new Rect(10, 28, 520, 20),
            "Tips: Click terrain=spawn • Space=pause • R=regen");
    }

    /// <summary>
    /// Removes any existing animals and spawns a starting population of herbivores
    /// and predators. Called from the bootstrap after the terrain and plants
    /// have been initialized.
    /// </summary>
    public void BootstrapPopulation()
    {
        // Destroy existing animals
        foreach (var a in animals)
        {
            if (a) Destroy(a.gameObject);
        }
        animals.Clear();
        toAdd.Clear();
        toRemove.Clear();

        // Spawn initial herbivores and predators
        for (int i = 0; i < startHerbivores; i++)
        {
            Spawn("herb", RandomPointWithin(terrain.size * 0.5f), GeneUtils.DefaultFor("herb"), Random.value < 0.5f ? "F" : "M");
        }
        for (int i = 0; i < startPredators; i++)
        {
            Spawn("pred", RandomPointWithin(terrain.size * 0.5f), GeneUtils.DefaultFor("pred"), Random.value < 0.5f ? "F" : "M");
        }
        // Commit the toAdd list to animals
        if (toAdd.Count > 0)
        {
            animals.AddRange(toAdd);
            toAdd.Clear();
        }
    }

    /// <summary>
    /// Regenerates the terrain and plant field with a new seed, repositioning
    /// existing animals on the new height field. Call this to reload the world.
    /// </summary>
    public void RegenerateWorld(int newSeed)
    {
        if (!terrain) terrain = FindObjectOfType<TerrainGenerator>();
        if (!plants) plants = GetComponent<PlantField>();
        if (!terrain || !plants) return;

        terrain.seed = newSeed;
        terrain.Generate();

        plants.growth = plantGrowth;
        plants.maxBiomass = plantMax;
        plants.Init(terrain);

        // Reposition animals on the new height field
        foreach (var a in animals)
        {
            if (!a) continue;
            Vector3 p = a.transform.position;
            float y = SampleHeight(p.x, p.z);
            a.transform.position = new Vector3(p.x, y + 0.2f + a.genes.size * 0.4f, p.z);
        }
    }

    /// <summary>
    /// Advances the simulation by dt. Updates plants and animals, handles
    /// births and deaths.
    /// </summary>
    private void Step(float dt)
    {
        // Grow plants
        plants.Step(dt);
        // Tick animals
        for (int i = 0; i < animals.Count; i++)
        {
            Animal a = animals[i];
            if (a) a.Tick(dt);
        }
        // Remove dead animals
        if (toRemove.Count > 0)
        {
            for (int i = 0; i < toRemove.Count; i++)
            {
                Animal d = toRemove[i];
                animals.Remove(d);
                if (d) Destroy(d.gameObject);
            }
            toRemove.Clear();
        }
        // Add newborn animals
        if (toAdd.Count > 0)
        {
            animals.AddRange(toAdd);
            toAdd.Clear();
        }
    }

    /// <summary>
    /// Returns the height of the terrain at a given world coordinate.
    /// </summary>
    public float SampleHeight(float x, float z)
    {
        return terrain.WorldHeight(x, z);
    }

    /// <summary>
    /// Returns the plant biomass at the grid cell corresponding to a world
    /// position.
    /// </summary>
    public float SamplePlantsAt(float x, float z)
    {
        int idx = plants.IndexFromWorldXZ(x, z);
        return plants.biomass[idx];
    }

    /// <summary>
    /// Removes up to 'amount' of biomass at the given position and returns how
    /// much was actually eaten.
    /// </summary>
    public float EatPlantsAt(Vector3 pos, float amount)
    {
        int idx = plants.IndexFromWorldXZ(pos.x, pos.z);
        float available = plants.biomass[idx];
        float eat = available < amount ? available : amount;
        plants.biomass[idx] -= eat;
        return eat;
    }

    /// <summary>
    /// Marks an animal as dead to be removed at the end of the tick.
    /// </summary>
    public void MarkDead(Animal a)
    {
        if (!toRemove.Contains(a)) toRemove.Add(a);
    }

    /// <summary>
    /// Finds the closest herbivore within a radius from a position.
    /// </summary>
    public Animal FindClosestHerbivore(Vector3 pos, float radius)
    {
        float r2 = radius * radius;
        Animal best = null;
        float bestD2 = float.MaxValue;
        for (int i = 0; i < animals.Count; i++)
        {
            Animal a = animals[i];
            if (!a || a.specie != "herb") continue;
            float d2 = (a.transform.position - pos).sqrMagnitude;
            if (d2 < r2 && d2 < bestD2)
            {
                bestD2 = d2;
                best = a;
            }
        }
        return best;
    }

    /// <summary>
    /// Finds a mating partner for an animal within a radius. The partner must
    /// be of the same species, opposite sex, mature, with enough energy and
    /// not on cooldown.
    /// </summary>
    public Animal FindMate(Animal self, float radius)
    {
        float r2 = radius * radius;
        for (int i = 0; i < animals.Count; i++)
        {
            Animal a = animals[i];
            if (!a || a == self) continue;
            if (a.specie != self.specie) continue;
            if (a.sex == self.sex) continue;
            if (a.cooldown > 0f || a.age <= a.maturity || a.energy < reproThreshold) continue;
            if ((a.transform.position - self.transform.position).sqrMagnitude <= r2)
                return a;
        }
        return null;
    }

    /// <summary>
    /// Creates a child from two parents of the same species. Combines genes
    /// and applies mutation. Spawns the child near the midpoint between
    /// parents. Returns the new animal.
    /// </summary>
    public Animal SpawnChild(Animal a, Animal b)
    {
        if (a.specie != b.specie) return null;
        GeneUtils.Genes g = GeneUtils.Child(a.genes, b.genes, mutationRate);
        Vector3 p = (a.transform.position + b.transform.position) * 0.5f + new Vector3(Random.Range(-0.5f, 0.5f), 0, Random.Range(-0.5f, 0.5f));
        string sx = Random.value < 0.5f ? "F" : "M";
        return Spawn(a.specie, p, g, sx);
    }

    /// <summary>
    /// Spawns a new animal of a given species at a position with specific
    /// genes and sex. The new animal is added to a buffer and created in
    /// the next simulation step.
    /// </summary>
    public Animal Spawn(string specie, Vector3 pos, GeneUtils.Genes genes, string sex)
    {
        GameObject go = new GameObject(specie == "herb" ? "Herbivore" : "Predator");
        float y = SampleHeight(pos.x, pos.z);
        go.transform.position = new Vector3(pos.x, y + 0.2f + genes.size * 0.4f, pos.z);
        Animal comp;
        if (specie == "herb") comp = go.AddComponent<Herbivore>();
        else                  comp = go.AddComponent<Predator>();
        go.AddComponent<SphereCollider>().radius = 0.6f;
        comp.Init(this, specie, genes, sex);
        toAdd.Add(comp);
        return comp;
    }

    /// <summary>
    /// Returns a random point within half the terrain size. Used for
    /// populating the initial animals.
    /// </summary>
    public Vector3 RandomPointWithin(float half)
    {
        float x = Random.Range(-half, half);
        float z = Random.Range(-half, half);
        return new Vector3(x, 0, z);
    }

    private int CountSpecie(string s)
    {
        int c = 0;
        for (int i = 0; i < animals.Count; i++)
        {
            Animal a = animals[i];
            if (a && a.specie == s) c++;
        }
        return c;
    }
}
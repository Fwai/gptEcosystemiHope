using UnityEngine;

/// <summary>
/// Bootstrap component that sets up the ecosystem scene on play.
/// It creates a terrain if none exists, sets up the simulation manager, plant field,
/// spawner, camera and lighting. It then regenerates the world and populates
/// the initial animal populations.
/// </summary>
public class EcosystemBootstrap : MonoBehaviour
{
    void Start()
    {
        // Ensure a terrain exists; create one if not found.
        TerrainGenerator terrain = FindObjectOfType<TerrainGenerator>();
        if (!terrain)
        {
            GameObject gTerrain = new GameObject("Terrain");
            gTerrain.transform.position = Vector3.zero;
            gTerrain.AddComponent<MeshFilter>();
            gTerrain.AddComponent<MeshRenderer>();
            gTerrain.AddComponent<MeshCollider>();
            terrain = gTerrain.AddComponent<TerrainGenerator>();
        }
        terrain.Generate();

        // Find or create the ecosystem manager, plant field and spawner
        SimulationManager sim = FindObjectOfType<SimulationManager>();
        PlantField plants = FindObjectOfType<PlantField>();
        Spawner spawner = FindObjectOfType<Spawner>();

        if (!plants)
        {
            GameObject eco = new GameObject("Ecosystem");
            plants = eco.AddComponent<PlantField>();
            sim = eco.AddComponent<SimulationManager>();
            spawner = eco.AddComponent<Spawner>();
        }

        // Wire references
        sim.terrain = terrain;
        sim.plants = plants;
        spawner.sim = sim;

        // Initialize plants and initial population
        sim.RegenerateWorld(Random.Range(1, int.MaxValue));
        sim.BootstrapPopulation();

        // Ensure a main camera exists and has an orbit controller
        if (Camera.main != null)
        {
            if (!Camera.main.GetComponent<SimpleOrbitCamera>())
                Camera.main.gameObject.AddComponent<SimpleOrbitCamera>();
        }
        else
        {
            GameObject cam = new GameObject("Main Camera");
            cam.tag = "MainCamera";
            Camera c = cam.AddComponent<Camera>();
            c.clearFlags = CameraClearFlags.Skybox;
            cam.transform.position = new Vector3(0, 30, -45);
            cam.transform.LookAt(Vector3.zero);
            cam.AddComponent<SimpleOrbitCamera>();
        }

        // Add a directional light if none exists
        if (!FindObjectOfType<Light>())
        {
            GameObject sun = new GameObject("Directional Light");
            var l = sun.AddComponent<Light>();
            l.type = LightType.Directional;
            sun.transform.rotation = Quaternion.Euler(50, 45, 0);
        }
    }
}
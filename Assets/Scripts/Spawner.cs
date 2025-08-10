using UnityEngine;

/// <summary>
/// Provides a simple UI for spawning animals into the simulation. Users can
/// choose a preset (Lapin, Cerf, Loup, Lynx) or define custom gene values
/// and spawn animals either at the mouse cursor or at random positions. An
/// option to spawn animals by clicking on the terrain is also provided.
/// </summary>
public class Spawner : MonoBehaviour
{
    public SimulationManager sim;

    public enum AnimalPreset { Lapin, Cerf, Loup, Lynx, Custom }
    public AnimalPreset preset = AnimalPreset.Lapin;

    [Header("Spawn Settings")]
    public int count = 8;
    public bool clickToSpawn = false;

    [Header("Genes (Custom or for refining presets)")]
    [Range(0.05f, 1.2f)] public float gSpeed = 0.6f;
    [Range(0.05f, 1.0f)] public float gSight = 0.6f;
    [Range(0.3f, 1.5f)]  public float gSize  = 0.8f;
    [Range(0f, 1f)]      public float gHue   = 0.33f;

    private Rect _win = new Rect(10, 60, 340, 280);

    void Start()
    {
        if (!sim) sim = FindObjectOfType<SimulationManager>();
        ApplyPreset();
    }

    void Update()
    {
        if (!sim) return;
        if (clickToSpawn && Input.GetMouseButtonDown(0))
        {
            if (TryGetMouseOnTerrain(out Vector3 p))
                SpawnBatchAt(p);
        }
    }

    void OnGUI()
    {
        _win = GUI.Window(6701, _win, DrawWindow, "Spawner — Écosystème");
    }

    private void DrawWindow(int id)
    {
        GUI.Label(new Rect(10, 25, 320, 18), "Préréglages");
        string[] names = System.Enum.GetNames(typeof(AnimalPreset));
        int cur = (int)preset;
        int newSel = GUI.SelectionGrid(new Rect(10, 45, 320, 50), cur, names, 4);
        if (newSel != cur)
        {
            preset = (AnimalPreset)newSel;
            ApplyPreset();
        }

        GUI.Label(new Rect(10, 100, 160, 18), "Nombre");
        string cnt = GUI.TextField(new Rect(170, 100, 50, 20), count.ToString());
        if (int.TryParse(cnt, out int parsed)) count = Mathf.Clamp(parsed, 1, 500);

        GUI.Label(new Rect(10, 125, 320, 18), "Gènes (Custom ou affiner)");
        GUI.Label(new Rect(10, 145, 80, 18), "Speed");
        gSpeed = GUI.HorizontalSlider(new Rect(90, 154, 240, 18), gSpeed, 0.05f, 1.2f);
        GUI.Label(new Rect(10, 165, 80, 18), "Sight");
        gSight = GUI.HorizontalSlider(new Rect(90, 174, 240, 18), gSight, 0.05f, 1.0f);
        GUI.Label(new Rect(10, 185, 80, 18), "Size");
        gSize  = GUI.HorizontalSlider(new Rect(90, 194, 240, 18), gSize, 0.3f, 1.5f);
        GUI.Label(new Rect(10, 205, 80, 18), "Hue");
        gHue   = GUI.HorizontalSlider(new Rect(90, 214, 240, 18), gHue, 0f, 1f);

        if (GUI.Button(new Rect(10, 235, 150, 28), "Spawn @ Curseur"))
        {
            if (TryGetMouseOnTerrain(out Vector3 p)) SpawnBatchAt(p);
        }
        if (GUI.Button(new Rect(170, 235, 160, 28), "Spawn @ Aléatoire"))
        {
            Vector3 p = sim.RandomPointWithin(sim.terrain.size * 0.45f);
            SpawnBatchAt(p);
        }

        clickToSpawn = GUI.Toggle(new Rect(10, 268, 150, 20), clickToSpawn, "Clique pour spawner");

        // Allow window dragging
        GUI.DragWindow(new Rect(0, 0, 10000, 20));
    }

    private void ApplyPreset()
    {
        switch (preset)
        {
            case AnimalPreset.Lapin: // fast, small herbivore
                gSpeed = 0.9f; gSight = 0.6f; gSize = 0.5f; gHue = 0.33f;
                break;
            case AnimalPreset.Cerf: // large herbivore with good eyesight
                gSpeed = 0.65f; gSight = 0.8f; gSize = 1.2f; gHue = 0.28f;
                break;
            case AnimalPreset.Loup: // agile predator
                gSpeed = 0.95f; gSight = 0.85f; gSize = 0.9f; gHue = 0.05f;
                break;
            case AnimalPreset.Lynx: // stealthy medium predator
                gSpeed = 0.8f; gSight = 0.95f; gSize = 0.8f; gHue = 0.08f;
                break;
            case AnimalPreset.Custom:
            default:
                // Leave sliders unchanged
                break;
        }
    }

    private bool TryGetMouseOnTerrain(out Vector3 point)
    {
        point = Vector3.zero;
        Camera cam = Camera.main;
        if (!cam) return false;
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 5000f))
        {
            point = hit.point;
            return true;
        }
        return false;
    }

    private void SpawnBatchAt(Vector3 p)
    {
        if (!sim) return;
        // Determine species based on preset
        string specie = (preset == AnimalPreset.Lapin || preset == AnimalPreset.Cerf) ? "herb" : "pred";
        for (int i = 0; i < count; i++)
        {
            GeneUtils.Genes g;
            g.speed = gSpeed;
            g.eyesight = gSight;
            g.size = gSize;
            g.hue = gHue;
            Vector3 jitter = new Vector3(Random.Range(-4f, 4f), 0, Random.Range(-4f, 4f));
            sim.Spawn(specie, p + jitter, g, Random.value < 0.5f ? "F" : "M");
        }
    }
}
using UnityEngine;

/// <summary>
/// Base class for all animals in the ecosystem. Provides common fields for
/// species, sex, genes, energy and age, and shared methods for initialization,
/// movement, reproduction and death handling. Derived classes override
/// Tick() to implement behaviour.
/// </summary>
[RequireComponent(typeof(Collider))]
public class Animal : MonoBehaviour
{
    public string specie = "herb"; // either "herb" or "pred"
    public string sex = "F";       // "F" or "M"
    public GeneUtils.Genes genes;

    [Header("State")]
    public float energy = 0.9f;
    public float age = 0f;
    public float maturity = 8f;
    public float maxAge = 120f;
    public float cooldown = 0f;

    [Header("References")]
    public SimulationManager sim;

    protected Vector3 vel;

    /// <summary>
    /// Initialize this animal with simulation references, species name, gene
    /// values and sex. Sets up scale, collider and colour. Randomizes
    /// maturity and lifespan slightly.
    /// </summary>
    public virtual void Init(SimulationManager s, string specieName, GeneUtils.Genes g, string sx)
    {
        sim = s;
        specie = specieName;
        genes = g;
        sex = sx;
        maturity = 8f + Random.Range(-2f, 2f);
        maxAge = 100f + Random.Range(-20f, 20f);
        transform.localScale = Vector3.one * (0.25f + 0.8f * genes.size);
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;

        // Add a simple renderer if not already present
        MeshRenderer rend = GetComponentInChildren<MeshRenderer>();
        if (!rend)
        {
            GameObject body = (specie == "herb") ?
                GameObject.CreatePrimitive(PrimitiveType.Sphere) :
                GameObject.CreatePrimitive(PrimitiveType.Capsule);
            Destroy(body.GetComponent<Collider>());
            body.transform.SetParent(transform, false);
            rend = body.GetComponent<MeshRenderer>();
            if (specie != "herb")
                body.transform.localScale = new Vector3(0.5f, 0.6f, 0.5f);
        }
        Shader sh = Shader.Find("Stylized/ToonLit");
        if (!sh) sh = Shader.Find("Standard");
        rend.material = new Material(sh);
        rend.material.color = Color.HSVToRGB(Mathf.Repeat(genes.hue, 1f), 0.7f, 0.9f);
    }

    /// <summary>
    /// Moves the animal smoothly towards a target point on the XZ plane,
    /// applying acceleration and damping. Adjusts y coordinate to follow
    /// terrain height and rotates to face movement direction.
    /// </summary>
    protected void MoveTowards(Vector3 target, float dt)
    {
        float spd = 1.5f + 3.5f * genes.speed;
        Vector3 dir = target - transform.position;
        dir.y = 0;
        float len = dir.magnitude + 1e-5f;
        Vector3 desired = dir / len * spd;
        vel = Vector3.Lerp(vel, desired, 0.2f);
        vel *= 0.98f;

        Vector3 p = transform.position + vel * dt;
        float y = sim.SampleHeight(p.x, p.z);
        transform.position = new Vector3(p.x, y + 0.2f + genes.size * 0.4f, p.z);
        if (vel.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(new Vector3(vel.x, 0, vel.z)), 0.15f);
    }

    /// <summary>
    /// Called every fixed simulation step to update the animal's state.
    /// Derived classes must override to implement behaviour.
    /// </summary>
    public virtual void Tick(float dt) { }

    /// <summary>
    /// Attempts to find a mate within a radius. If successful, spawns a child
    /// and reduces energy. Returns true if a child was created.
    /// </summary>
    protected bool TryMate(out Animal mate)
    {
        mate = sim.FindMate(this, 6f + 10f * genes.eyesight);
        if (mate == null) return false;
        Animal child = sim.SpawnChild(this, mate);
        if (child != null)
        {
            energy *= 0.5f;
            mate.energy *= 0.5f;
            cooldown = mate.cooldown = sim.reproCooldown * (0.5f + Random.value);
            return true;
        }
        return false;
    }
}
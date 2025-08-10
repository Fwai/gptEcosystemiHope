using UnityEngine;

/// <summary>
/// Predators hunt herbivores, gaining energy when they catch them. They
/// wander randomly when no prey is nearby, pay higher metabolic costs, and
/// reproduce when energetic and mature. They die if energy falls too low
/// or they exceed their lifespan.
/// </summary>
public class Predator : Animal
{
    public override void Tick(float dt)
    {
        age += dt;
        cooldown = Mathf.Max(0f, cooldown - dt);

        // Find nearest herbivore within sight range
        float rad = 10f + 30f * genes.eyesight;
        Animal prey = sim.FindClosestHerbivore(transform.position, rad);
        Vector3 target;
        if (prey != null) target = prey.transform.position;
        else
        {
            float jitter = 12f;
            target = transform.position + new Vector3(Random.Range(-jitter, jitter), 0, Random.Range(-jitter, jitter));
        }
        MoveTowards(target, dt);

        // Capture prey if close enough
        if (prey != null)
        {
            float d = Vector3.Distance(transform.position, prey.transform.position);
            if (d < 1.2f + 0.4f * genes.size)
            {
                sim.MarkDead(prey);
                energy += sim.predatorEatGain;
            }
        }

        // Metabolic cost
        float moveCost = 0.03f + 0.05f * genes.size + 0.04f * vel.magnitude;
        energy -= moveCost * dt;

        // Reproduce if possible
        if (cooldown <= 0f && energy >= sim.reproThreshold && age > maturity)
            TryMate(out _);

        // Death conditions
        if (energy <= -0.3f || age > maxAge) sim.MarkDead(this);
    }
}
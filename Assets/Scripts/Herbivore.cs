using UnityEngine;

/// <summary>
/// Herbivores search for areas rich in plant biomass, eat plants, pay
/// metabolic costs, and reproduce when they have enough energy and are
/// mature. They die if energy drops too low or age exceeds maxAge.
/// </summary>
public class Herbivore : Animal
{
    public override void Tick(float dt)
    {
        age += dt;
        cooldown = Mathf.Max(0f, cooldown - dt);

        // Choose a direction to move by sampling 10 directions around
        float rad = 8f + 20f * genes.eyesight;
        Vector3 best = transform.position;
        float bestScore = float.NegativeInfinity;
        for (int i = 0; i < 10; i++)
        {
            float ang = (i / 10f) * Mathf.PI * 2f;
            Vector3 cand = transform.position + new Vector3(Mathf.Cos(ang), 0, Mathf.Sin(ang)) * rad;
            float score = sim.SamplePlantsAt(cand.x, cand.z) - 0.2f * Random.value;
            if (score > bestScore) { bestScore = score; best = cand; }
        }
        MoveTowards(best, dt);

        // Eat plants
        float eat = sim.EatPlantsAt(transform.position, sim.herbivoreEatRate * dt * (1f + genes.size));
        energy += eat * 0.9f;

        // Metabolic cost, includes movement cost
        float moveCost = 0.02f + 0.04f * genes.size + 0.03f * vel.magnitude;
        energy -= moveCost * dt;

        // Reproduce if possible
        if (cooldown <= 0f && energy >= sim.reproThreshold && age > maturity)
            TryMate(out _);

        // Death conditions
        if (energy <= -0.3f || age > maxAge) sim.MarkDead(this);
    }
}
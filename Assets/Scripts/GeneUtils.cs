using UnityEngine;

/// <summary>
/// Helper struct and functions for handling genetic traits for animals. Traits
/// include speed, eyesight, size and hue. Includes methods to generate
/// default genes for herbivores and predators and to create a child gene
/// combination with mutation.
/// </summary>
public static class GeneUtils
{
    public struct Genes
    {
        public float speed;     // 0..1.5
        public float eyesight;  // 0..1
        public float size;      // 0..1.5
        public float hue;       // 0..1
    }

    /// <summary>
    /// Returns a random set of genes appropriate for the given species.
    /// Herbivores are generally smaller and less aggressive than predators.
    /// </summary>
    public static Genes DefaultFor(string specie)
    {
        if (specie == "herb")
        {
            Genes g;
            g.speed    = Random.Range(0.25f, 0.6f);
            g.eyesight = Random.Range(0.3f, 0.7f);
            g.size     = Random.Range(0.6f, 0.9f);
            g.hue      = Random.Range(0.28f, 0.45f);
            return g;
        }
        else
        {
            Genes g;
            g.speed    = Random.Range(0.5f, 0.9f);
            g.eyesight = Random.Range(0.5f, 0.9f);
            g.size     = Random.Range(0.8f, 1.2f);
            g.hue      = Random.Range(0.0f, 0.08f);
            return g;
        }
    }

    /// <summary>
    /// Creates a new set of genes for a child by averaging the parents' traits and
    /// applying random mutation. Mutation can slightly increase or decrease the
    /// trait or produce rare leaps.
    /// </summary>
    public static Genes Child(Genes a, Genes b, float mutationRate)
    {
        Genes c;
        c.speed    = Combine(a.speed,    b.speed,    mutationRate, 0.01f, 1.5f);
        c.eyesight = Combine(a.eyesight, b.eyesight, mutationRate, 0.01f, 1.0f);
        c.size     = Combine(a.size,     b.size,     mutationRate, 0.01f, 1.5f);
        c.hue      = Combine(a.hue,      b.hue,      mutationRate, 0.0f,  1.0f);
        return c;
    }

    private static float Combine(float A, float B, float mr, float lo, float hi)
    {
        float min = Mathf.Min(A, B);
        float max = Mathf.Max(A, B);
        float avg = 0.5f * (A + B);
        float r = Random.value;
        if (r < mr * 0.5f) avg = max + 0.1f * max;
        else if (r < mr)   avg = Mathf.Max(lo, min - 0.1f * min);
        else               avg += Random.Range(-0.03f, 0.03f);
        return Mathf.Clamp(avg, lo, hi);
    }
}
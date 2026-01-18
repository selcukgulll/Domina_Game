using UnityEngine;

public static class CombatResolver
{
    public static void Resolve(GladiatorData a, GladiatorData b)
    {
        float dmg = a.Strength * Random.Range(0.8f, 1.2f);
        b.HP -= dmg;
        Debug.Log($"{a.Name} hit {b.Name} for {dmg} damage.");

        if (b.HP <= 0)
            GoreSystem.Kill(b);
    }
}
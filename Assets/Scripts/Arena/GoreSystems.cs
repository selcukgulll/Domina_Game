using UnityEngine;

public static class GoreSystem
{
    public static void Kill(GladiatorData g)
    {
        // limb detach logic later
        Debug.Log(g.Name + " is DESTROYED by GoreSystem!");
        g.HP = -999;
    }
}
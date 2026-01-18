using UnityEngine;

public static class CombatResolver
{
    public static void Resolve(GladiatorData attackerData, GladiatorAI defenderAI)
    {
        GladiatorData defenderData = defenderAI.Data;

        // Hasar Hesabý
        float dmg = attackerData.Strength * Random.Range(0.8f, 1.2f);
        defenderData.HP -= dmg;

        // Görsel Efekt (View)
        var view = defenderAI.GetComponent<GladiatorView>();
        if (view != null)
        {
            view.PlayDamageEffect(defenderData.HP, defenderData.MaxHP);
        }

        // --- YENÝ MANTIK: ANINDA ÖLÜM BÝLDÝRÝMÝ ---
        if (defenderData.HP <= 0)
        {
            // 1. Gore sistemini çalýþtýr (Ses, kan vs.)
            GoreSystem.Kill(defenderData);

            // 2. ArenaManager'a haber ver (Eðer sahnedeysek)
            if (ArenaManager.I != null)
            {
                ArenaManager.I.OnFighterDied(defenderData);
            }
        }
    }
}
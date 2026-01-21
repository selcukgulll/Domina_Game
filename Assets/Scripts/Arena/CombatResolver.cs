using UnityEngine;

public static class CombatResolver
{
    public static void Resolve(GladiatorData attackerData, GladiatorAI defenderAI)
    {
        GladiatorData defenderData = defenderAI.Data;

        // Hasar Hesabý
        float dmg = attackerData.Strength * Random.Range(0.8f, 1.2f);
        defenderData.HP -= dmg;

        // GÖRSEL EFEKTLER (View)
        var view = defenderAI.GetComponent<GladiatorView>();
        if (view != null)
        {
            view.RefreshVisuals(); // Can barýný güncelle

            // --- YENÝ: ANÝMASYONLAR ---
            if (defenderData.HP > 0)
            {
                // Hâlâ yaþýyorsa hasar animasyonu
                view.PlayHit();
                defenderAI.GetHit();
            }
            else
            {
                // Öldüyse ölüm animasyonu
                // NOT: GoreSystem.Kill çaðýrmadan önce animasyonu oynatýyoruz
                view.PlayDeath();

                // ArenaManager'a bildir
                if (ArenaManager.I != null) ArenaManager.I.OnFighterDied(defenderData);
            }
        }
    }
}
using UnityEngine;

public static class CombatResolver
{
    // DÝKKAT: Ýkinci parametreyi 'GladiatorData' yerine 'GladiatorAI' yaptýk.
    // Neden? Çünkü AI scripti GameObject'in üzerinde duruyor, görsel scripte (View) oradan ulaþabiliriz.
    public static void Resolve(GladiatorData attackerData, GladiatorAI defenderAI)
    {
        GladiatorData defenderData = defenderAI.Data; // Datayý AI'dan çek

        // Hasar Hesabý
        float dmg = attackerData.Strength * Random.Range(0.8f, 1.2f);
        defenderData.HP -= dmg;

        // Debug.Log($"{attackerData.Name} hit {defenderData.Name} for {dmg} damage.");

        // --- ÝÞTE EKSÝK OLAN PARÇA BURASI ---
        // Vurulan adamýn (defenderAI) üzerindeki GladiatorView scriptini bul
        var view = defenderAI.GetComponent<GladiatorView>();

        if (view != null)
        {
            // "Efekti oynat, can barýný güncelle"
            view.PlayDamageEffect(defenderData.HP, defenderData.MaxHP);
        }
        // ------------------------------------

        if (defenderData.HP <= 0)
            GoreSystem.Kill(defenderData);
    }
}
using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager I;

    public GameState State;
    public int Day = 1;

    public ResourcesData Resources = new ResourcesData();
    public List<GladiatorData> Gladiators = new();

    // Savaþ verileri
    public GladiatorData EnemyFighter;

    [Header("Exhibition System")]
    public int LastExhibitionRefreshDay = 0; // En son ne zaman yenilendi?
    public List<GladiatorData> ExhibitionOpponents = new List<GladiatorData>(); // 3 rakip (Easy, Med, Hard)
    public bool[] ExhibitionDefeated = new bool[3]; // 3 rakibin durumu (Savaþýldý mý?)
    // Tek bir savaþçý yerine bir liste tutalým
    public List<GladiatorData> SelectedPlayerFighters = new List<GladiatorData>();

    // Eski deðiþkeni geriye uyumluluk için tutabiliriz veya property yapabiliriz
    public GladiatorData PlayerFighter
    {
        get { return SelectedPlayerFighters.Count > 0 ? SelectedPlayerFighters[0] : null; }
        set
        {
            SelectedPlayerFighters.Clear();
            if (value != null) SelectedPlayerFighters.Add(value);
        }
    }

    void Awake()
    {
        if (I == null)
        {
            I = this; // <--- BU SATIR KESÝNLÝKLE AÇIK OLMALI!

            // DontDestroyOnLoad(gameObject); // Bunu kapatmakta sorun yok.

            if (Gladiators.Count == 0)
            {
                GladiatorData starter = new GladiatorData("Slave " + Random.Range(100, 999));
            // Biraz torpil geçelim ölmesin hemen :)
            starter.Strength += 5;
            starter.HP = starter.MaxHP;
            Gladiators.Add(starter);
            }
        }
        else Destroy(gameObject);
    }

    // --- YENÝ EKLENEN KISIM: GÜN DÖNGÜSÜ ---
    public void EndDay()
    {
        Resources.ConsumeDaily(Gladiators.Count);
        bool starving = !Resources.HasBasicNeeds();

        foreach (var g in Gladiators)
        {
            if (!g.IsAlive) continue;

            // Açlýk Cezasý
            if (starving) { g.HP -= 10; g.Mentality -= 0.1f; }

            switch (g.CurrentState)
            {
                case GladiatorState.Resting:
                    // --- YENÝ ÝYÝLEÞME MANTIÐI ---
                    // Her gün Max Canýnýn %20'si kadar iyileþsin
                    float healAmount = g.MaxHP * 0.20f;

                    // Medicus varsa %20 yerine %40 iyileþsin (Bonus)
                    if (Resources.HasMedicus) healAmount *= 2f;

                    g.HP += healAmount;

                    // Caný asla MaxHP'yi geçmesin (Kapaklama)
                    if (g.HP > g.MaxHP) g.HP = g.MaxHP;

                    // Stamina da dolsun
                    g.Stamina = Mathf.Min(100, g.Stamina + 50);
                    break;

                case GladiatorState.Training:
                    if (g.Stamina > 10)
                    {
                        // DOCTORE VARSA DAHA ÇOK XP
                        int xpGain = Resources.HasDoctore ? 20 : 10;
                        g.GainXP(xpGain);

                        g.Stamina -= 10;

                        // FABER VARSA GÜÇLENME ÞANSI ARTAR
                        float strGain = Resources.HasFaber ? 0.2f : 0.1f;
                        g.Strength += strGain;
                    }
                    else
                    {
                        g.CurrentState = GladiatorState.Resting;
                    }
                    break;
            }
        }

        Day++;
    }


    // Bu fonksiyonu ArenaSelectorUI çaðýracak
    public void CheckExhibitionRefresh()
    {
        // Oyunun baþýnda veya üstünden 3 gün geçtiyse yenile
        if (ExhibitionOpponents.Count == 0 || Day >= LastExhibitionRefreshDay + 3)
        {
            GenerateNewExhibitionOpponents();
            LastExhibitionRefreshDay = Day;

            // Dövüþ durumlarýný sýfýrla (Hepsi tekrar savaþýlabilir)
            ExhibitionDefeated = new bool[3]; // [false, false, false]
            Debug.Log("Exhibition Opponents Refreshed!");
        }
    }

    void GenerateNewExhibitionOpponents()
    {
        ExhibitionOpponents.Clear();

        // 1. EASY RAKÝP (Zayýf Statlar)
        var easy = new GladiatorData(GetRandomName("Novice"));
        easy.Strength = Random.Range(5, 9);
        easy.Agility = Random.Range(5, 9);
        easy.MaxHP = easy.Strength * 8;
        easy.HP = easy.MaxHP;
        ExhibitionOpponents.Add(easy);

        // 2. MEDIUM RAKÝP (Dengeli Statlar)
        var med = new GladiatorData(GetRandomName("Warrior"));
        med.Strength = Random.Range(12, 16);
        med.Agility = Random.Range(10, 14);
        med.MaxHP = med.Strength * 10;
        med.HP = med.MaxHP;
        ExhibitionOpponents.Add(med);

        // 3. HARD RAKÝP (Boss Statlar)
        var hard = new GladiatorData(GetRandomName("Champion"));
        hard.Strength = Random.Range(20, 28);
        hard.Agility = Random.Range(18, 25);
        hard.MaxHP = hard.Strength * 12;
        hard.HP = hard.MaxHP;
        hard.Mentality = 1.2f; // Daha agresif
        ExhibitionOpponents.Add(hard);
    }

    string GetRandomName(string title)
    {
        string[] names = { "Crixus", "Gannicus", "Oenomaus", "Verus", "Priscus", "Tetraites", "Spiculus" };
        return names[Random.Range(0, names.Length)] + " the " + title;
    }
}
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
    public int LastExhibitionRefreshDay = 0;
    public List<GladiatorData> ExhibitionOpponents = new List<GladiatorData>();
    public bool[] ExhibitionDefeated = new bool[3];
    public List<GladiatorData> SelectedPlayerFighters = new List<GladiatorData>();

    [Header("Campaign System")]
    public List<CampaignMission> CampaignMissions = new List<CampaignMission>();
    public int CurrentMissionIndex = -1;

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
            I = this;
            DontDestroyOnLoad(gameObject);
            if (Gladiators.Count == 0)
            {
                GladiatorData starter = new GladiatorData("Slave " + Random.Range(100, 999));
                starter.Strength += 5;
                starter.HP = starter.MaxHP;
                Gladiators.Add(starter);
            }
        }
        else Destroy(gameObject);
    }

    // --- GÜN DÖNGÜSÜ ---
    public void EndDay()
    {
        Resources.ConsumeDaily(Gladiators.Count);
        bool starving = !Resources.HasBasicNeeds();

        foreach (var g in Gladiators)
        {
            if (!g.IsAlive) continue;

            if (starving) { g.HP -= 10; g.Mentality -= 0.1f; }

            switch (g.CurrentState)
            {
                case GladiatorState.Resting:
                    float healAmount = g.MaxHP * 0.20f;
                    if (Resources.HasMedicus) healAmount *= 2f;
                    g.HP += healAmount;
                    if (g.HP > g.MaxHP) g.HP = g.MaxHP;
                    g.Stamina = Mathf.Min(100, g.Stamina + 50);
                    break;

                case GladiatorState.Training:
                    if (g.Stamina > 10)
                    {
                        int xpGain = Resources.HasDoctore ? 20 : 10;
                        g.GainXP(xpGain);
                        g.Stamina -= 10;
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

    // --- EXHIBITION SÝSTEMÝ ---
    public void CheckExhibitionRefresh()
    {
        if (ExhibitionOpponents.Count == 0 || Day >= LastExhibitionRefreshDay + 3)
        {
            GenerateNewExhibitionOpponents();
            LastExhibitionRefreshDay = Day;
            ExhibitionDefeated = new bool[3];
            Debug.Log("Exhibition Opponents Refreshed!");
        }
    }

    void GenerateNewExhibitionOpponents()
    {
        ExhibitionOpponents.Clear();

        // 1. EASY
        var easy = new GladiatorData(GetRandomName("Novice"));
        easy.Strength = Random.Range(5, 9);
        easy.Agility = Random.Range(5, 9);
        easy.MaxHP = easy.Strength * 8;
        easy.HP = easy.MaxHP;
        ExhibitionOpponents.Add(easy);

        // 2. MEDIUM
        var med = new GladiatorData(GetRandomName("Warrior"));
        med.Strength = Random.Range(12, 16);
        med.Agility = Random.Range(10, 14);
        med.MaxHP = med.Strength * 10;
        med.HP = med.MaxHP;
        ExhibitionOpponents.Add(med);

        // 3. HARD
        var hard = new GladiatorData(GetRandomName("Champion"));
        hard.Strength = Random.Range(20, 28);
        hard.Agility = Random.Range(18, 25);
        hard.MaxHP = hard.Strength * 12;
        hard.HP = hard.MaxHP;
        hard.Mentality = 1.2f;
        ExhibitionOpponents.Add(hard);
    }

    string GetRandomName(string title)
    {
        string[] names = { "Crixus", "Gannicus", "Oenomaus", "Verus", "Priscus", "Tetraites", "Spiculus" };
        return names[Random.Range(0, names.Length)] + " the " + title;
    }

    // --- XP VE LEVEL ---
    public void GainLudusXP(int amount)
    {
        Resources.CurrentXP += amount;

        if (Resources.CurrentXP >= Resources.MaxXP)
        {
            Resources.CurrentXP -= Resources.MaxXP;
            Resources.LudusLevel++;
            Resources.MaxXP *= 1.2f;
            Debug.Log("LEVEL UP! New Level: " + Resources.LudusLevel);
        }
    }

    // --- CAMPAIGN SÝSTEMÝ ---
    public void CompleteCurrentMission()
    {
        if (CurrentMissionIndex != -1 && CurrentMissionIndex < CampaignMissions.Count)
        {
            var mission = CampaignMissions[CurrentMissionIndex];
            if (!mission.IsCompleted)
            {
                mission.IsCompleted = true;
                if (CurrentMissionIndex + 1 < CampaignMissions.Count)
                {
                    CampaignMissions[CurrentMissionIndex + 1].IsUnlocked = true;
                }
            }
        }
        CurrentMissionIndex = -1;
    }

    void Start()
    {
        if (CampaignMissions.Count > 0) CampaignMissions[0].IsUnlocked = true;

        // Senin istediðin el ile yazýlmýþ listeyi yükler
        SetupCampaign();
    }

    // --- SENÝN ÝSTEDÝÐÝN MANUEL LÝSTE ---
    void SetupCampaign()
    {
        CampaignMissions.Clear();

        // KULLANIM: 
        // AddMission( "Görev Adý",  MinLevel,  ÖdülGold, ÖdülXP,   "Rakip Adý",  Güç(STR),  Can(HP) );

        // --- BÖLÜM 1: ÇIRAKLIK (Kolay) ---
        AddMission("Training Day", 1, 100, 20, "Tahta Kukla", 4, 40);
        AddMission("First Blood", 1, 120, 30, "Acemi Hýrsýz", 5, 50);
        AddMission("Slave Revolt", 1, 150, 40, "Kaçak Köle", 6, 60);
        AddMission("Street Fight", 2, 180, 50, "Sokak Serserisi", 8, 80);
        AddMission("The Bully", 2, 200, 60, "Mahalle Abisi", 10, 100);

        // --- BÖLÜM 2: ARENAYA GÝRÝÞ (Orta) ---
        AddMission("Sand & Blood", 3, 250, 80, "Çöl Akrebi", 12, 120);
        AddMission("Iron Fist", 3, 300, 90, "Demir Yumruk", 14, 140);
        AddMission("Dual Blades", 4, 350, 100, "Çifte Býçak", 15, 150);
        AddMission("The Bear", 4, 400, 110, "Koca Ayý", 18, 200);
        AddMission("Gatekeeper", 5, 500, 150, "Zindan Bekçisi", 20, 250);

        // --- BÖLÜM 3: ÞAMPÝYONLAR LÝGÝ (Zor) ---
        AddMission("Veteran", 6, 600, 180, "Eski Asker", 22, 220);
        AddMission("Executioner", 6, 700, 200, "Cellat", 25, 250);
        AddMission("Shadow", 7, 800, 220, "Gölge Suikastçi", 28, 200);
        AddMission("The Wall", 7, 900, 250, "Yürüyen Duvar", 20, 500);
        AddMission("Beast Master", 8, 1000, 300, "Canavar Terb.", 30, 300);

        // --- BÖLÜM 4: EFSANELER (Çok Zor) ---
        AddMission("Spartan", 9, 1200, 400, "Spartalý", 35, 350);
        AddMission("Immortal", 9, 1500, 500, "Ölümsüz", 40, 400);
        AddMission("Titan", 10, 2000, 600, "Titan", 50, 600);
        AddMission("God Hand", 10, 3000, 800, "Tanrý Eli", 60, 800);

        // --- FINAL BOSS ---
        AddMission("THE CHAMPION", 12, 5000, 2000, "CRIXUS THE KING", 80, 1500);

        // Ýlk görevi aç
        if (CampaignMissions.Count > 0) CampaignMissions[0].IsUnlocked = true;

        Debug.Log("Campaign Listesi Yüklendi!");
    }

    // --- BU FONKSÝYON EKSÝKTÝ, GERÝ EKLEDÝM ---
    void AddMission(string mName, int reqLvl, int gold, int xp, string eName, float str, float hp)
    {
        CampaignMission m = new CampaignMission();
        m.MissionName = mName;
        m.RequiredLudusLevel = reqLvl;
        m.GoldReward = gold;
        m.XPReward = xp;

        GladiatorData enemy = new GladiatorData(eName);
        enemy.Strength = str;
        enemy.Agility = str * 0.8f;
        enemy.MaxHP = hp;
        enemy.HP = hp;
        enemy.Class = (GladiatorClass)Random.Range(0, 3);

        m.Enemies.Add(enemy);
        CampaignMissions.Add(m);
    }
}

// BU CLASS GameManager'ýn DIÞINDA OLMALI (Veya içinde ama parantezlere dikkat edilmeli)
// Dosyanýn en altýna koydum, sorunsuz çalýþýr.
[System.Serializable]
public class CampaignMission
{
    public string MissionName = "Battle 1";
    public int RequiredLudusLevel = 1;
    public int GoldReward = 100;
    public int XPReward = 50;
    public bool IsCompleted = false;
    public bool IsUnlocked = false;
    public List<GladiatorData> Enemies = new List<GladiatorData>();
}
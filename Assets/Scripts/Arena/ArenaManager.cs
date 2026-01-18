using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ArenaManager : MonoBehaviour
{
    // --- 1. ERÝÞÝM ÝÇÝN SINGLETON ---
    public static ArenaManager I;

    [Header("Setup")]
    public GameObject GladiatorPrefab;
    public Transform SpawnPoint1; // Player için
    public Transform SpawnPoint2; // Enemy için

    [Header("UI")]
    public GameObject ResultPanel;
    public Text ResultText;

    // Bunlarý public yaptýk ki dýþarýdan eriþip kimin kim olduðunu anlayabilelim
    public GladiatorAI playerAI;
    public GladiatorAI enemyAI;

    private bool battleEnded = false;

    void Awake()
    {
        I = this;
    }

    void Start()
    {
        // 1. ZAMANI BAÞLAT (Çok Önemli!)
        // Önceki maçtan veya menüden zaman 0 gelmiþ olabilir.
        Time.timeScale = 1f;

        if (ResultPanel != null) ResultPanel.SetActive(false);

        // 2. KOORDÝNATLARI AYARLA (Uçlara koy)
        if (SpawnPoint1 != null) SpawnPoint1.position = new Vector3(-4.0f, -0.5f, 0f);
        if (SpawnPoint2 != null) SpawnPoint2.position = new Vector3(4.0f, -0.5f, 0f);

        // 3. VERÝLERÝ ÇEK
        var pData = GameManager.I?.PlayerFighter ?? new GladiatorData("TestPlayer");
        var eData = GameManager.I?.EnemyFighter ?? new GladiatorData("TestEnemy");

        // 4. KARAKTERLERÝ YARAT
        GameObject pObj = SpawnFighter(pData, SpawnPoint1, false);
        GameObject eObj = SpawnFighter(eData, SpawnPoint2, true);

        playerAI = pObj.GetComponent<GladiatorAI>();
        enemyAI = eObj.GetComponent<GladiatorAI>();

        // 5. HEDEFLERÝ ÞÝMDÝDEN GÖSTER (Update'i bekleme)
        // Birbirlerini hedef olarak tanýsýnlar
        if (playerAI != null && enemyAI != null)
        {
            playerAI.Target = enemyAI.transform;
            enemyAI.Target = playerAI.transform;
        }
    }

    GameObject SpawnFighter(GladiatorData data, Transform spawnPoint, bool isEnemy)
    {
        // 1. Prefab'ý yarat
        GameObject go = Instantiate(GladiatorPrefab, spawnPoint.position, Quaternion.identity);

        // 2. AI Verisini Ata
        var ai = go.GetComponent<GladiatorAI>();
        ai.Data = data;

        // 3. Unity Hiyerarþi Ýsmi
        go.name = isEnemy ? "Enemy_" + data.Name : "Player_" + data.Name;

        // 4. --- GÖRSELÝ GÜNCELLE (BIND) ---
        var view = go.GetComponent<GladiatorView>();
        if (view != null)
        {
            // Data, ArenaModu=true, DüþmanMý=isEnemy
            view.Bind(data, true, isEnemy);
        }

        // 5. Düþmaný Ters Çevir (Yüzü sola baksýn)
        if (isEnemy)
        {
            go.transform.localScale = new Vector3(-1, 1, 1);
        }

        return go;
    }

    // --- (Kodun geri kalaný ayný: OnFighterDied, EndBattle vs.) ---

    // ÖLÜM HABERÝ
    public void OnFighterDied(GladiatorData victimData)
    {
        if (battleEnded) return;

        Debug.Log("ÖLÜM HABERÝ GELDÝ: " + victimData.Name);

        bool isPlayerDead = (playerAI != null && victimData == playerAI.Data);
        EndBattle(!isPlayerDead);
    }

    void EndBattle(bool playerWon)
    {
        if (battleEnded) return;
        battleEnded = true;
        Time.timeScale = 1f;

        if (ResultPanel != null)
        {
            ResultPanel.SetActive(true);
            ResultPanel.transform.SetAsLastSibling();
        }



        if (playerWon)
        {
            ResultText.text = "VICTORY!";

            if (playerAI != null && playerAI.Data != null)
            {
                playerAI.Data.Wins++; 
                
                // EXTRA GÜVENLÝK: 
                // Eðer maçý kazandýysan ama son anda canýn 0'a indiyse (Kanama vs.),
                // Caný 1 yapalým ki Ludus'a dönünce ölü sayýlýp silinmesin.
                if (playerAI.Data.HP <= 0) playerAI.Data.HP = 1;
            }

            // Campaign Kontrolü
            if (GameManager.I != null && GameManager.I.CurrentMissionIndex != -1)
            {
                var mission = GameManager.I.CampaignMissions[GameManager.I.CurrentMissionIndex];
                GameManager.I.Resources.Gold += mission.GoldReward;
                GameManager.I.GainLudusXP(mission.XPReward);
                GameManager.I.CompleteCurrentMission();
                ResultText.text += $"\n+{mission.GoldReward} Gold\n+{mission.XPReward} XP";
            }
            else
            {
                if (GameManager.I != null) GameManager.I.Resources.Gold += 250;
                if (GameManager.I != null) GameManager.I.GainLudusXP(10);
            }
        }
        else
        {
            ResultText.text = "DEFEAT...";
            if (GameManager.I != null && GameManager.I.PlayerFighter != null)
            {
                var deadGlad = GameManager.I.PlayerFighter;
                deadGlad.HP = -999;
                GameManager.I.Gladiators.Remove(deadGlad);
            }
        }
    }

    public void ReturnToLudus()
    {
        SceneManager.LoadScene("Ludus");
    }
}
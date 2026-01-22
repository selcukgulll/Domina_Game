using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ArenaManager : MonoBehaviour
{
    // --- 1. ERÝÞÝM ÝÇÝN SINGLETON ---
    public static ArenaManager I;

    [Header("Setup")]
    public GameObject[] GladiatorPrefabs; // Array yaptýk
    public Transform SpawnPoint1; // Player için
    public Transform SpawnPoint2; // Enemy için

    [Header("UI")]
    public GameObject ResultPanel;
    public Text ResultText;

    [Header("Arena Movement Bounds (Ellipse)")]
    public Transform ArenaCenter;     // boþ GameObject koyacaðýz
    public float ArenaRadiusX = 6.5f; // elipsin yatay yarýçapý
    public float ArenaRadiusY = 2.2f; // elipsin dikey yarýçapý


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
        // 1. ZAMANI BAÞLAT
        Time.timeScale = 1f;

        if (ResultPanel != null) ResultPanel.SetActive(false);

        // 2. KOORDÝNATLARI AYARLA
        if (SpawnPoint1 != null) SpawnPoint1.position = new Vector3(-4.0f, -0.5f, 0f);
        if (SpawnPoint2 != null) SpawnPoint2.position = new Vector3(4.0f, -0.5f, 0f);

        // 3. VERÝLERÝ ÇEK (DÜZELTME BURADA)
        // Eðer GameManager yoksa (Test için Arena sahnesini açtýysan), 
        // ID olarak -1 veriyoruz ve Ýsmi elle atýyoruz.

        var pData = GameManager.I?.PlayerFighter ?? new GladiatorData(-1) { Name = "TestPlayer" };
        var eData = GameManager.I?.EnemyFighter ?? new GladiatorData(-1) { Name = "TestEnemy" };

        // 4. KARAKTERLERÝ YARAT
        GameObject pObj = SpawnFighter(pData, SpawnPoint1, false);
        GameObject eObj = SpawnFighter(eData, SpawnPoint2, true);

        playerAI = pObj.GetComponent<GladiatorAI>();
        enemyAI = eObj.GetComponent<GladiatorAI>();

        // 5. HEDEFLERÝ ÞÝMDÝDEN GÖSTER
        if (playerAI != null && enemyAI != null)
        {
            playerAI.Target = enemyAI.transform;
            enemyAI.Target = playerAI.transform;
        }
    }

    GameObject SpawnFighter(GladiatorData data, Transform spawnPoint, bool isEnemy)
    {
        // 1. Prefab'ý yarat
        // Eðer BodyTypeIndex negatif veya hatalý gelirse diye Math.Abs ve güvenlik ekledik
        int safeIndex = Mathf.Abs(data.BodyTypeIndex) % GladiatorPrefabs.Length;
        GameObject prefabToUse = GladiatorPrefabs[safeIndex];

        GameObject go = Instantiate(prefabToUse, spawnPoint.position, Quaternion.identity);

        // 2. AI Verisini Ata
        var ai = go.GetComponent<GladiatorAI>();
        ai.Data = data;

        // 3. Unity Hiyerarþi Ýsmi
        go.name = isEnemy ? "Enemy_" + data.Name : "Player_" + data.Name;

        // 4. --- GÖRSELÝ GÜNCELLE (BIND) ---
        var view = go.GetComponent<GladiatorView>();
        if (view != null)
        {
            view.Bind(data, true, isEnemy);
        }

        if (ai != null)
        {
            ai.enabled = true;             // Beyni çalýþtýr
            ai.IsCombatMode = true;        // ARENA MODU (Saldýr!)
            ai.State = AIState.Approach;   // "Koþ" emriyle baþla

            // Arena'da drag kapalý
            foreach (var drag in go.GetComponentsInChildren<GladiatorDrag>(true))
                drag.enabled = false;
        }

        // Collider'ý Trigger yap
        BoxCollider2D col = go.GetComponent<BoxCollider2D>();
        if (col != null) col.isTrigger = true;

        return go; // (Buradaki çift return hatasýný da sildim)
    }

    // --- (Kodun geri kalaný ayný: OnFighterDied, EndBattle vs.) ---

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

    public Vector2 ClampToArena(Vector2 worldPos)
    {
        Vector2 center = (ArenaCenter != null) ? (Vector2)ArenaCenter.position : Vector2.zero;
        Vector2 p = worldPos - center;

        float rx = Mathf.Max(0.01f, ArenaRadiusX);
        float ry = Mathf.Max(0.01f, ArenaRadiusY);

        float nx = p.x / rx;
        float ny = p.y / ry;

        float k = nx * nx + ny * ny;

        if (k <= 1f) return worldPos;

        float scale = 1f / Mathf.Sqrt(k);
        Vector2 clamped = center + new Vector2(p.x * scale, p.y * scale);
        return clamped;
    }

    public Vector2 GetArenaCenter()
    {
        return (ArenaCenter != null) ? (Vector2)ArenaCenter.position : Vector2.zero;
    }
}
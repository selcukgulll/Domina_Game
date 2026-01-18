using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ArenaManager : MonoBehaviour
{
    [Header("Setup")]
    public GameObject GladiatorPrefab;
    public Transform SpawnPoint1;
    public Transform SpawnPoint2;

    [Header("UI")]
    public GameObject ResultPanel;
    public Text ResultText;

    private GladiatorAI playerAI;
    private GladiatorAI enemyAI;
    private bool battleEnded = false;

    void Start()
    {
        if (ResultPanel != null) ResultPanel.SetActive(false);

        var pData = GameManager.I?.PlayerFighter ?? new GladiatorData("TestPlayer");
        var eData = GameManager.I?.EnemyFighter ?? new GladiatorData("TestEnemy");

        playerAI = SpawnFighter(pData, SpawnPoint1, false).GetComponent<GladiatorAI>();
        enemyAI = SpawnFighter(eData, SpawnPoint2, true).GetComponent<GladiatorAI>();
        Time.timeScale = 1f;
    }

    GameObject SpawnFighter(GladiatorData data, Transform spawnPoint, bool isEnemy)
    {
        GameObject go = Instantiate(GladiatorPrefab, spawnPoint.position, Quaternion.identity);
        var ai = go.GetComponent<GladiatorAI>();
        ai.Data = data;

        // Target atamalarýný gecikmeli yapabiliriz veya burada find ile
        // Þu anlýk basitçe býrakalým, Update'de birbirlerini bulurlar ya da
        // Manuel atama:
        go.name = isEnemy ? "Enemy" : "Player";
        if (isEnemy) go.GetComponent<SpriteRenderer>().color = Color.gray;

        return go;
    }

    void Update()
    {
        // Savaþ bittiyse daha fazla iþlem yapma
        if (battleEnded) return;

        // GÜVENLÝK ÖNLEMÝ: Eðer adamlar yoksa (Destroy edildiyse) hata vermesin
        if (playerAI == null || enemyAI == null) return;

        // Hedef atamalarý (Target kaybolursa tekrar bulsun)
        if (playerAI.Target == null && enemyAI != null) playerAI.Target = enemyAI.transform;
        if (enemyAI.Target == null && playerAI != null) enemyAI.Target = playerAI.transform;

        // --- DAHA HIZLI TESPÝT ---
        // Canýn 0 veya daha az olduðunu gördüðümüz an savaþý bitir

        bool playerDead = playerAI.Data.HP <= 0;
        bool enemyDead = enemyAI.Data.HP <= 0;

        if (playerDead)
        {
            Debug.Log("Player Died detected in Update"); // Konsoldan takip et
            EndBattle(false); // Kaybettin
        }
        else if (enemyDead)
        {
            Debug.Log("Enemy Died detected in Update"); // Konsoldan takip et
            EndBattle(true); // Kazandýn
        }
    }

    void EndBattle(bool playerWon)
    {
        // Çift çalýþmayý önle
        if (battleEnded) return;
        battleEnded = true;

        Debug.Log("SAVAÞ BÝTTÝ! Sonuç Paneli Açýlýyor...");

        // 1. ZAMAN GARANTÝSÝ (Oyun yavaþladýysa normale döndür)
        Time.timeScale = 1f;

        if (ResultPanel != null)
        {
            // Paneli Aktif Et
            ResultPanel.SetActive(true);

            // --- KRÝTÝK ÇÖZÜM: EN ÖNE GETÝR ---
            // Bu komut, paneli Canvas hiyerarþisinin en altýna taþýr.
            // Unity'de en alttaki obje, ekranda EN ÖNDE çizilir.
            ResultPanel.transform.SetAsLastSibling();
        }
        else
        {
            Debug.LogError("HATA: ResultPanel Inspector'da atanmamýþ!");
        }

        if (playerWon)
        {
            ResultText.text = "VICTORY!";
            if (GameManager.I != null) GameManager.I.Resources.Gold += 250;
        }
        else
        {
            ResultText.text = "DEFEAT...";
            // Ölüm Ýþlemleri
            if (GameManager.I != null)
            {
                var deadGlad = GameManager.I.PlayerFighter;
                if (deadGlad != null)
                {
                    deadGlad.HP = -999;
                    GameManager.I.Gladiators.Remove(deadGlad);
                }
            }
        }
    }

    // Butona baðlanacak fonksiyon
    public void ReturnToLudus()
    {
        SceneManager.LoadScene("Ludus");
    }
}
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
        // Hedef atamalarý (Start'ta bulamazlarsa diye güvenli yöntem)
        if (playerAI.Target == null) playerAI.Target = enemyAI.transform;
        if (enemyAI.Target == null) enemyAI.Target = playerAI.transform;

        if (battleEnded) return;

        // Biri öldü mü kontrolü
        if (playerAI.Data.HP <= 0)
        {
            EndBattle(false); // Kaybettin
        }
        else if (enemyAI.Data.HP <= 0)
        {
            EndBattle(true); // Kazandýn
        }
    }

    void EndBattle(bool playerWon)
    {
        battleEnded = true;
        ResultPanel.SetActive(true);

        if (playerWon)
        {
            ResultText.text = "VICTORY!";
            // ÖDÜL KAZANMA
            if (GameManager.I != null) GameManager.I.Resources.Gold += 250;
        }
        else
        {
            ResultText.text = "DEFEAT...";
            // ÖLÜMÜ KAYDETME (Kalýcý Silme)
            if (GameManager.I != null)
            {
                var deadGlad = GameManager.I.PlayerFighter;
                if (deadGlad != null)
                {
                    deadGlad.HP = -999; // Ölü olduðunu garantile
                    // Listeden hemen silmiyoruz, Ludus'a dönünce temizleyeceðiz
                    // Veya direkt silebiliriz:
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
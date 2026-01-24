using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GladiatorUI : MonoBehaviour
{
    public static GladiatorUI I;

    [Header("Panels")]
    public GameObject StatsPanel; // Tüm panel

    [Header("Text Info")]
    public Text NameText;
    public Text StatsText;
    public Text StateText;

    [Header("Action Buttons")]
    public Button TrainButton;
    public Button RestButton;
    public Button WineButton;
    public Button FightButton;

    private GladiatorData currentData;
    private Transform targetTransform; // Takip edilecek gladyatör objesi

    void Awake()
    {
        I = this;
        if (StatsPanel != null) StatsPanel.SetActive(false);
    }

    // --- YENÝ EKLENEN KISIM: EKANDA TAKÝP ETME ---
    void Update()
    {
        // Panel açýksa ve bir hedefimiz varsa
        if (StatsPanel.activeSelf && targetTransform != null)
        {
            // Gladyatörün 3D dünyadaki pozisyonunu, 2D ekran koordinatýna çevir
            Vector3 screenPos = Camera.main.WorldToScreenPoint(targetTransform.position);

            // Paneli biraz saða ve yukarý kaydýr (Karakterin tam üstüne binmesin)
            // Bu sayýlarý (150, 50) ekran çözünürlüðüne göre artýrýp azaltabilirsin
            screenPos.x += 70;
            screenPos.y += 20;

            StatsPanel.transform.position = screenPos;
        }
    }
    // ----------------------------------------------

    // Show fonksiyonunu deðiþtirdik: Artýk Transform da istiyor
    public void Show(GladiatorData data, Transform physicalObject)
    {
        currentData = data;
        targetTransform = physicalObject; // Hedefi kaydet

        StatsPanel.SetActive(true);
        RefreshInfo();
    }

    void RefreshInfo()
    {
        if (currentData == null) return;

        NameText.text = currentData.Name + " (" + currentData.Class + ")";

        StatsText.text = $"HP: {currentData.HP:F0}/{currentData.MaxHP:F0}\n" +
                         $"STA: {currentData.Stamina:F0}\n" +
                         $"STR: {currentData.Strength:F1}\n" +
                         $"AGI: {currentData.Agility:F1}\n" +
                         $"AGG: {currentData.Aggression:F2}\n" +
                         $"MEN: {currentData.Mentality:F2}";

        StateText.text = "STATUS: " + currentData.CurrentState.ToString().ToUpper();

        TrainButton.interactable = currentData.CurrentState != GladiatorState.Training;
        RestButton.interactable = currentData.CurrentState != GladiatorState.Resting;

        bool canAffordWine = GameManager.I.Resources.Gold >= 10 && GameManager.I.Resources.Wine > 0;
        WineButton.interactable = canAffordWine;
    }

    // --- BUTON FONKSÝYONLARI ---

    public void OnTrainClicked()
    {
        if (currentData == null) return;
        currentData.CurrentState = GladiatorState.Training;
        RefreshInfo();
    }

    public void OnRestClicked()
    {
        if (currentData == null) return;
        currentData.CurrentState = GladiatorState.Resting;
        RefreshInfo();
    }

    public void OnWineClicked()
    {
        if (currentData == null) return;
        if (GameManager.I == null) return;

        // Yeterli wine var mý?
        if (GameManager.I.Resources.Wine <= 0)
        {
            return;
        }

        // SADECE WINE AZALT
        GameManager.I.Resources.Wine--;

        // Etkiler
        currentData.Mentality += 0.1f;
        currentData.HP = Mathf.Min(currentData.MaxHP, currentData.HP + 10);

        // Sadece gladyatör panelini yenile
        RefreshInfo();

        var ludus = FindObjectOfType<LudusManager>();
        if (ludus != null)
            ludus.UpdateUI();
    }


    public void OnFightClicked()
    {
        if (currentData == null) return;

        if (currentData.HP < 15)
        {
            Debug.Log("Gladiator is too injured to fight!");
            return;
        }

        GameManager.I.PlayerFighter = currentData;
        GameManager.I.EnemyFighter = new GladiatorData(GameManager.I.GetNextGladiatorID());

        SceneManager.LoadScene("Arena");
    }

    public void OnCloseClicked()
    {
        StatsPanel.SetActive(false);
        currentData = null;
        targetTransform = null; // Hedefi sýfýrla
    }
}
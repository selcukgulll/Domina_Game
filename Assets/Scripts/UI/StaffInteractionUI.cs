using UnityEngine;
using UnityEngine.UI;

// Personel Tipleri
public enum StaffType { Medicus, Doctore, Faber }

public class StaffInteractionUI : MonoBehaviour
{
    public LudusShopManager mainShopManager;
    public ArenaSelectorUI ArenaUIScript;

    [Header("UI Elements")]
    public GameObject Panel;
    public Text TitleText;
    public Text LevelText;
    public Text CostText;
    public Button UpgradeButton;

    private StaffType currentType;
    private int currentCost;

    void Start()
    {
        // Otomatik bul
        if (mainShopManager == null) mainShopManager = FindObjectOfType<LudusShopManager>();
        if (ArenaUIScript == null) ArenaUIScript = FindObjectOfType<ArenaSelectorUI>();

        if (Panel != null) Panel.SetActive(false);
    }

    // Bu fonksiyonu prefablar çaðýracak
    public void OpenPanel(StaffType type)
    {
        // 1. Dýþarýdakileri Kapat (Manuel Kodla)
        if (ArenaUIScript != null) ArenaUIScript.ClosePanel();
        if (mainShopManager != null) mainShopManager.CloseAllPanels();

        // 2. Kendini Aç
        currentType = type;
        UpdateUI();
        Panel.SetActive(true);
    }

    void UpdateUI()
    {
        int level = 0;
        string title = "";

        // Hangi personeli seçtiysek onun verisini çek
        switch (currentType)
        {
            case StaffType.Medicus:
                level = GameManager.I.Resources.MedicusLevel;
                title = "MEDICUS";
                break;
            case StaffType.Doctore:
                level = GameManager.I.Resources.DoctoreLevel;
                title = "DOCTORE";
                break;
            case StaffType.Faber:
                level = GameManager.I.Resources.FaberLevel;
                title = "FABER";
                break;
        }

        TitleText.text = title;
        LevelText.text = "Level: " + level;

        // Maliyet Hesabý: Seviye * 500 altýn (Örnek)
        currentCost = level * 500;
        CostText.text = $"Cost: {currentCost} Gold";

        // Para yetiyor mu kontrolü
        UpgradeButton.interactable = GameManager.I.Resources.Gold >= currentCost;
    }

    public void OnUpgradeClicked()
    {
        if (GameManager.I.Resources.Gold >= currentCost)
        {
            // Parayý harca
            GameManager.I.Resources.Gold -= currentCost;

            // Seviyeyi artýr
            switch (currentType)
            {
                case StaffType.Medicus: GameManager.I.Resources.MedicusLevel++; break;
                case StaffType.Doctore: GameManager.I.Resources.DoctoreLevel++; break;
                case StaffType.Faber: GameManager.I.Resources.FaberLevel++; break;
            }

            // Arayüzü güncelle
            UpdateUI();

            // Eðer üst barda altýn yazýsý varsa onu da güncellemek gerekebilir
            var manager = FindObjectOfType<LudusManager>();
            if (manager != null) manager.UpdateUI();
        }
    }

    public void ClosePanel()
    {
        Panel.SetActive(false);
    }
}
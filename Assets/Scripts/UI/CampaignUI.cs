using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CampaignUI : MonoBehaviour
{
    public GameObject CampaignPanel;
    public Transform ListContent; // ScrollView'daki Content
    public GameObject MissionButtonPrefab; // Az önce yaptýðýn buton prefabý
    public LudusShopManager shopManager; // Diðer panelleri kapatmak için

    void OnEnable()
    {
        RefreshMissionList();
    }

    public void RefreshMissionList()
    {
        // Önce listeyi temizle
        foreach (Transform child in ListContent) Destroy(child.gameObject);

        var missions = GameManager.I.CampaignMissions;

        for (int i = 0; i < missions.Count; i++)
        {
            var mission = missions[i];
            int index = i; // Lambda expression için kopyalamak þart

            GameObject btnObj = Instantiate(MissionButtonPrefab, ListContent);
            Button btn = btnObj.GetComponent<Button>();

            // Butonun içindeki textleri bul (Sýrasý önemli veya isimle bul)
            Text[] texts = btnObj.GetComponentsInChildren<Text>();
            // Varsayým: 0->Ýsim, 1->Ödül, 2->Gereksinim (Senin prefab yapýna göre deðiþebilir)

            // Renk ve Durum Ayarý
            if (mission.IsCompleted)
            {
                texts[0].text = mission.MissionName + " (DONE)";
                btn.image.color = Color.green; // Tamamlananlar yeþil
            }
            else if (mission.IsUnlocked)
            {
                texts[0].text = mission.MissionName;
                btn.image.color = Color.white; // Açýk olanlar beyaz
            }
            else
            {
                texts[0].text = "LOCKED";
                btn.image.color = Color.gray; // Kilitli olanlar gri
                btn.interactable = false; // Týklanamaz
            }

            // Bilgileri Yaz
            if (texts.Length > 1) texts[1].text = $"Reward: {mission.GoldReward}g | {mission.XPReward} XP";
            if (texts.Length > 2) texts[2].text = $"Req: Lvl {mission.RequiredLudusLevel}";

            // Level Yetmiyor mu?
            if (GameManager.I.Resources.LudusLevel < mission.RequiredLudusLevel)
            {
                btn.interactable = false;
                texts[0].text += " (Low Level)";
            }

            // TIKLAMA OLAYI
            btn.onClick.AddListener(() => OnMissionClicked(index));
        }
    }

    void OnMissionClicked(int index)
    {
        var mission = GameManager.I.CampaignMissions[index];

        // 1. Dövüþ verilerini hazýrla
        GameManager.I.CurrentMissionIndex = index;
        GameManager.I.EnemyFighter = mission.Enemies[0]; // Þimdilik ilk düþmanla savaþýyoruz

        // Player seçimi için ArenaSelectorUI'daki "FighterSelection" ekranýný açabiliriz
        // Veya direkt en güçlü adamý seçip savaþa sokabiliriz.
        // Þimdilik basit olsun: ArenaSelectorUI'yi açýp oradan devam ettirelim

        // Burasý biraz trickli, direkt savaþa yollamak yerine "Seçim Ekranýna" yollayalým.
        // Ama "Rakip" olarak Campaign düþmanýný set ettik.

        // ArenaSelectorUI scriptine ulaþýp "Campaign Modunu Baþlat" dememiz lazým.
        // Þimdilik en basit yöntem: Hazýrlýk ekranýný aç.

        FindObjectOfType<ArenaSelectorUI>().OpenArenaForCampaign(mission.Enemies[0]);

        CampaignPanel.SetActive(false);
    }

    public void ClosePanel()
    {
        CampaignPanel.SetActive(false);
    }
}
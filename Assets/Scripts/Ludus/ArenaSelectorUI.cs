using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class ArenaSelectorUI : MonoBehaviour
{
    [Header("Main Containers")]
    public GameObject ArenaWrapperPanel; // <--- YENÝ: En dýþtaki ana kutu (ArenaPanel)
    public GameObject MatchSelectionPanel;
    public GameObject FighterSelectionPanel;

    [Header("Phase 1: Match Buttons")]
    // Butonlarýn içindeki Text'leri buraya sürükleyeceksin
    public Button BtnEasy;
    public Text TxtEasyInfo; // Easy butonunun içindeki text

    public Button BtnMedium;
    public Text TxtMediumInfo;

    public Button BtnHard;
    public Text TxtHardInfo;

    public Button BtnPit;
    public Text TxtPitInfo;

    [Header("Phase 2: Fighter Selection")]
    public Text MatchTitleText;      // "Vs. Crixus" gibi baþlýk
    public Transform ListContent;    // ScrollView Content
    public GameObject FighterButtonPrefab; // Listeye konacak buton prefabý
    public Button BtnGoToFight;      // Maça Baþla Butonu
    public Text SelectedCountText;   // "Selected: 1/1" yazýsý

    // Logic Variables
    private GladiatorData selectedEnemy;
    private int difficultyIndex = -1; // 0:Easy, 1:Med, 2:Hard, -1:Pit

    // Çoklu seçim sistemi
    private List<GladiatorData> mySelectedGladiators = new List<GladiatorData>();
    private int maxFightersAllowed = 1; // Þimdilik 1v1, ileride burayý 2, 3 yapabilirsin.

    void Start()
    {
        if (ArenaWrapperPanel != null) ArenaWrapperPanel.SetActive(false);
    }

    public void OpenArena()
    {
        // Önce ana kutuyu aç (Ki içindekiler görünebilsin)
        if (ArenaWrapperPanel != null) ArenaWrapperPanel.SetActive(true);

        MatchSelectionPanel.SetActive(true);
        FighterSelectionPanel.SetActive(false);

        GameManager.I.CheckExhibitionRefresh();
        RefreshMatchButtons();
    }

    // --- AÞAMA 1: RAKÝP BUTONLARINI HAZIRLA ---
    void RefreshMatchButtons()
    {
        // GameManager'dan rakipleri çekip butonlarýn ÝÇÝNE yazýyoruz
        var opponents = GameManager.I.ExhibitionOpponents;
        var defeated = GameManager.I.ExhibitionDefeated;

        // EASY
        SetupButton(BtnEasy, TxtEasyInfo, opponents[0], defeated[0], 0, "Easy Match");

        // MEDIUM
        SetupButton(BtnMedium, TxtMediumInfo, opponents[1], defeated[1], 1, "Medium Match");

        // HARD
        SetupButton(BtnHard, TxtHardInfo, opponents[2], defeated[2], 2, "Boss Match");

        // PIT
        TxtPitInfo.text = "<b>THE PIT</b>\nUnknown Enemy\nRisk: High\nReward: Random";
        BtnPit.onClick.RemoveAllListeners();
        BtnPit.onClick.AddListener(() => OnMatchSelected(null, -1));
    }

    void SetupButton(Button btn, Text txt, GladiatorData enemy, bool isDefeated, int index, string title)
    {
        btn.interactable = !isDefeated;

        string status = isDefeated ? "<color=red>(DEFEATED)</color>" : "<color=green>AVAILABLE</color>";
        int reward = (index + 1) * 250;

        // Burada HTML tagleri ile zengin metin kullanýyoruz
        txt.text = $"<b>{title}</b>\n" +
                   $"Vs: {enemy.Name}\n" +
                   $"STR: {enemy.Strength:F0} | HP: {enemy.MaxHP:F0}\n" +
                   $"Reward: {reward} Gold\n" +
                   $"{status}";

        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => OnMatchSelected(enemy, index));
    }

    // --- GEÇÝÞ: RAKÝP SEÇÝLDÝ, SIRA BÝZDE ---
    void OnMatchSelected(GladiatorData enemy, int index)
    {
        difficultyIndex = index;

        if (index == -1) // Pit ise rastgele yarat
        {
            selectedEnemy = new GladiatorData("Pit Fighter " + Random.Range(10, 99));
            // Pit zorluk ayarlarý...
            float mult = 1.0f + (GameManager.I.Day * 0.1f);
            selectedEnemy.Strength *= mult; selectedEnemy.MaxHP *= mult; selectedEnemy.HP = selectedEnemy.MaxHP;
        }
        else
        {
            selectedEnemy = enemy;
        }

        // Arayüzü Deðiþtir
        MatchSelectionPanel.SetActive(false);
        FighterSelectionPanel.SetActive(true);

        // Deðiþkenleri Sýfýrla
        mySelectedGladiators.Clear();
        maxFightersAllowed = 1; // Ýstersen: if(difficulty == 2) maxFightersAllowed = 2; diyebilirsin.

        MatchTitleText.text = $"Select Fighters vs. {selectedEnemy.Name}";
        RefreshMyFighterList();
        UpdateStartButton();
    }

    // --- AÞAMA 2: LÝSTE VE SEÇÝM ---
    void RefreshMyFighterList()
    {
        foreach (Transform child in ListContent) Destroy(child.gameObject);

        foreach (var g in GameManager.I.Gladiators)
        {
            if (!g.IsAlive || g.CurrentState == GladiatorState.Resting) continue;

            var go = Instantiate(FighterButtonPrefab, ListContent);
            var btn = go.GetComponent<Button>();
            var txt = go.GetComponentInChildren<Text>();

            // Butonun görünümü seçili mi deðil mi?
            UpdateFighterButtonVisual(btn, txt, g);

            btn.onClick.AddListener(() =>
            {
                ToggleFighterSelection(g);
                UpdateFighterButtonVisual(btn, txt, g); // Týklayýnca rengi güncelle
                UpdateStartButton();
            });
        }
    }

    void ToggleFighterSelection(GladiatorData g)
    {
        if (mySelectedGladiators.Contains(g))
        {
            mySelectedGladiators.Remove(g); // Zaten seçiliyse çýkar
        }
        else
        {
            if (mySelectedGladiators.Count < maxFightersAllowed)
            {
                mySelectedGladiators.Add(g); // Yer varsa ekle
            }
        }
    }

    void UpdateFighterButtonVisual(Button btn, Text txt, GladiatorData g)
    {
        bool isSelected = mySelectedGladiators.Contains(g);

        if (isSelected)
        {
            btn.GetComponent<Image>().color = Color.green; // Seçiliyse Yeþil
            txt.text = $"<b>[SELECTED]</b>\n{g.Name}";
        }
        else
        {
            btn.GetComponent<Image>().color = Color.white; // Deðilse Beyaz
            txt.text = $"{g.Name}\nHP: {g.HP:F0} | STR: {g.Strength:F0}";
        }
    }

    void UpdateStartButton()
    {
        SelectedCountText.text = $"Ready: {mySelectedGladiators.Count} / {maxFightersAllowed}";

        // Sadece tam sayýya ulaþtýysak veya en az 1 kiþi varsa buton açýlsýn
        BtnGoToFight.interactable = mySelectedGladiators.Count > 0;
    }

    // --- SON AÞAMA: SAVAÞ ---
    public void OnGoToFightClicked()
    {
        if (mySelectedGladiators.Count == 0) return;

        // Verileri GameManager'a iþle
        GameManager.I.SelectedPlayerFighters = new List<GladiatorData>(mySelectedGladiators);
        GameManager.I.EnemyFighter = selectedEnemy;

        // --- DÜZELTME BURADA ---
        // Eskiden: if (difficultyIndex != -1) diyorduk. 
        // -2 (Campaign) de -1 olmadýðý için içeri girip hata veriyordu.

        // YENÝSÝ: Sadece 0 ve üzeri ise (Yani Easy, Medium, Hard ise) bu iþlemi yap.
        if (difficultyIndex >= 0)
        {
            GameManager.I.ExhibitionDefeated[difficultyIndex] = true;
        }
        // -----------------------

        SceneManager.LoadScene("Arena");
    }

    public void OnBackClicked()
    {
        // Geri tuþuna basarsak baþa dön
        MatchSelectionPanel.SetActive(true);
        FighterSelectionPanel.SetActive(false);
    }

    public void ClosePanel()
    {
        // Hepsini kökten kapat
        if (ArenaWrapperPanel != null) ArenaWrapperPanel.SetActive(false);
    }

    // CAMPAIGN ÝÇÝN ÖZEL AÇILIÞ
    public void OpenArenaForCampaign(GladiatorData campaignEnemy)
    {
        // 1. Ana Paneli Aç
        if (ArenaWrapperPanel != null) ArenaWrapperPanel.SetActive(true);

        // 2. Rakip Seçimini Atla, Direkt Karakter Seçimine Geç
        MatchSelectionPanel.SetActive(false);
        FighterSelectionPanel.SetActive(true);

        // 3. Rakibi Ayarla
        selectedEnemy = campaignEnemy; // Campaign'den gelen düþman
        difficultyIndex = -2; // -2 Campaign demek olsun (Kodlarýn karýþmamasý için)

        // 4. Baþlýk ve Listeyi Yenile
        MatchTitleText.text = $"Campaign: Vs. {selectedEnemy.Name}";

        mySelectedGladiators.Clear();
        RefreshMyFighterList();
        UpdateStartButton();
    }
}
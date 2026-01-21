using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LudusShopManager : MonoBehaviour
{

    public StaffInteractionUI staffPopupScript;

    [Header("Panels")]
    public GameObject MarketPanel;
    public GameObject StaffPanel;
    public GameObject RecruitPanel;

    [Header("Global UI")]
    public Text GoldText;

    [Header("Staff Buttons")]
    public Button BtnDoctore;
    public Button BtnMedicus;
    public Button BtnFaber;

    [Header("Recruit System - Selective Slots")]
    public RecruitSlot[] RecruitSlots;

    private List<GladiatorData> dailyCandidates = new List<GladiatorData>();

    // --- LUDUS MANAGER REFERANSI ---
    private LudusManager visualManager;
    // -----------------------------

    private float lastBuyTime = 0f;

    [System.Serializable]
    public class RecruitSlot
    {
        public GameObject Container;
        public Text NameText;
        public Text StatsText;
        public Button BuyButton;
        public Text PriceText;
    }

    void Start()
    {
        visualManager = GetComponent<LudusManager>();

        // Eðer editörden sürüklemeyi unutursan diye otomatik bulsun
        if (staffPopupScript == null) staffPopupScript = FindObjectOfType<StaffInteractionUI>();

        CloseAllPanels();
    }

    void UpdateGoldUI()
    {
        if (GameManager.I != null)
            GoldText.text = "Gold: " + GameManager.I.Resources.Gold;
    }

    public void CloseAllPanels()
    {
        if (MarketPanel != null) MarketPanel.SetActive(false);
        if (StaffPanel != null) StaffPanel.SetActive(false);
        if (RecruitPanel != null) RecruitPanel.SetActive(false);
        if (staffPopupScript != null) staffPopupScript.ClosePanel();
    }

    // --- MARKET ---
    public void OpenMarket()
    {
        CloseAllPanels();
        MarketPanel.SetActive(true);
        UpdateGoldUI();
    }

    public void BuyFood() => BuyResource("Food", 50, 10);
    public void BuyWater() => BuyResource("Water", 50, 10);
    public void BuyWine() => BuyResource("Wine", 100, 5);

    void BuyResource(string type, int cost, int amount)
    {
        if (GameManager.I.Resources.Gold >= cost)
        {
            GameManager.I.Resources.Gold -= cost;
            if (type == "Food") GameManager.I.Resources.Food += amount;
            if (type == "Water") GameManager.I.Resources.Water += amount;
            if (type == "Wine") GameManager.I.Resources.Wine += amount;
            UpdateGoldUI();

            // Üst barý da güncellemek için
            if (visualManager != null) visualManager.UpdateUI();
        }
    }

    // --- STAFF ---
    public void OpenStaff()
    {
        CloseAllPanels();
        StaffPanel.SetActive(true);
        UpdateGoldUI();
        RefreshStaffButtons();
    }

    void RefreshStaffButtons()
    {
        var res = GameManager.I.Resources;
        BtnDoctore.interactable = !res.HasDoctore;
        BtnMedicus.interactable = !res.HasMedicus;
        BtnFaber.interactable = !res.HasFaber;

        if (res.HasDoctore) BtnDoctore.GetComponentInChildren<Text>().text = "OWNED";
        if (res.HasMedicus) BtnMedicus.GetComponentInChildren<Text>().text = "OWNED";
        if (res.HasFaber) BtnFaber.GetComponentInChildren<Text>().text = "OWNED";
    }

    public void BuyStaff(string type)
    {
        int cost = 500;
        if (GameManager.I.Resources.Gold >= cost)
        {
            var res = GameManager.I.Resources;
            bool bought = false;

            if (type == "Doctore" && !res.HasDoctore) { res.HasDoctore = true; bought = true; }
            if (type == "Medicus" && !res.HasMedicus) { res.HasMedicus = true; bought = true; }
            if (type == "Faber" && !res.HasFaber) { res.HasFaber = true; bought = true; }

            if (bought)
            {
                GameManager.I.Resources.Gold -= cost;
                UpdateGoldUI();
                RefreshStaffButtons();
                if (visualManager != null)
                {
                    visualManager.UpdateUI();

                    // --- KRÝTÝK EKLEME: SATIN ALINCA SAHNEYE KOY ---
                    visualManager.SpawnStaffs();
                    // ----------------------------------------------
                }
            }
        }
    }

    // --- RECRUIT ---
    public void OpenRecruit()
    {
        CloseAllPanels();
        RecruitPanel.SetActive(true);
        UpdateGoldUI();

        if (dailyCandidates.Count == 0)
        {
            GenerateDailyCandidates();
        }

        RefreshRecruitUI();
    }

    public void BuyRandomSlave()
    {
        if (GameManager.I.Gladiators.Count >= 12)
        {
            Debug.Log("Ludus is Full! (Max 12)");
            return;
        }
        int cost = 100;
        if (GameManager.I.Resources.Gold >= cost)
        {
            GameManager.I.Resources.Gold -= cost;
            GameManager.I.Gladiators.Add(new GladiatorData("Slave " + Random.Range(100, 999)));

            UpdateGoldUI();

            if (visualManager != null)
            {
                visualManager.UpdateUI();
                visualManager.SpawnGladiators();
            }
        }
    }

    void GenerateDailyCandidates()
    {
        dailyCandidates.Clear();
        for (int i = 0; i < 3; i++)
        {
            GladiatorData candidate = new GladiatorData("Mercenary " + Random.Range(1, 100));

            // STATLARI ARTIR
            candidate.Strength += Random.Range(2, 8);
            candidate.MaxHP = candidate.Strength * 10;
            candidate.HP = candidate.MaxHP;

            // --- YENÝ WIN ALGORÝTMASI ---
            // Gücü ne kadar fazlaysa o kadar çok kazanmýþ olsun.
            // Örnek: Strength 15 ise -> 15 * (1.5 ile 3.0 arasý) = ~30 Win
            candidate.Wins = Mathf.RoundToInt(candidate.Strength * Random.Range(1.0f, 3.0f));

            // Fiyatýna Win sayýsý da etki etsin (Ýsteðe baðlý, CalculateValue fonksiyonuna da ekleyebilirsin)

            dailyCandidates.Add(candidate);
        }
    }

    void RefreshRecruitUI()
    {
        for (int i = 0; i < RecruitSlots.Length; i++)
        {
            if (i < dailyCandidates.Count)
            {
                var candidate = dailyCandidates[i];
                var slot = RecruitSlots[i];
                int price = candidate.CalculateValue();

                slot.Container.SetActive(true);
                slot.NameText.text = candidate.Name + " (" + candidate.Class + ")";
                slot.StatsText.text = $"STR: {candidate.Strength:F0} | AGI: {candidate.Agility:F0} | HP: {candidate.MaxHP}";
                slot.PriceText.text = price + " Gold";

                slot.BuyButton.onClick.RemoveAllListeners();
                int index = i;
                slot.BuyButton.onClick.AddListener(() => BuyCandidate(index));
            }
            else
            {
                RecruitSlots[i].Container.SetActive(false);
            }
        }
    }

    public void BuyCandidate(int index)
    {
        // 1. ZAMAN KONTROLÜ (Yarým saniyede bir iþlem yapýlabilir)
        if (Time.time < lastBuyTime + 0.5f) return;
        lastBuyTime = Time.time;

        if (GameManager.I.Gladiators.Count >= 12)
        {
            Debug.Log("Ludus is Full! (Max 12)");
            return;
        }

        if (index >= dailyCandidates.Count) return;

        var candidate = dailyCandidates[index];
        int price = candidate.CalculateValue();

        if (GameManager.I.Resources.Gold >= price)
        {
            GameManager.I.Resources.Gold -= price;
            GameManager.I.Gladiators.Add(candidate);
            dailyCandidates.RemoveAt(index);

            UpdateGoldUI();
            RefreshRecruitUI();

            if (visualManager != null)
            {
                visualManager.UpdateUI();
                visualManager.SpawnGladiators();
            }
        }
    }
}
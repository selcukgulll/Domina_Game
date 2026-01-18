using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class LudusManager : MonoBehaviour
{

    [Header("Top Bar UI")]
    public Text GoldText;
    public Text FoodText;
    public Text WaterText;
    public Text DayText;

    [Header("References")]
    public Transform GardenArea;
    public GameObject GladiatorPrefab;

    [Header("Grid Settings")]
    public int Columns = 6;
    public int TotalSlots = 18;   // 3x6 = 18 slot
    public float SpacingX = 1.6f;
    public float SpacingY = 2.0f;
    public float VerticalBias = -0.5f;

    // Slotlarýn dünya üzerindeki pozisyonlarýný tutan liste
    private List<Vector3> slotPositions = new List<Vector3>();

    [Header("Staff Settings")]
    // YENÝ SÝSTEM: 3 AYRI PREFAB
    public GameObject MedicusPrefab;
    public GameObject DoctorePrefab;
    public GameObject FaberPrefab;

    public Transform StaffSpawnPoint;

    private List<GameObject> activeStaff = new List<GameObject>();

    [Header("XP & Level UI")]
    public Slider XPSlider;
    public Text LevelText;

    [Header("Panels")]
    // Campaign panelini de buraya ekleyeceðiz birazdan
    public GameObject CampaignPanel;

    IEnumerator Start()
    {
        yield return null;
        if (GameManager.I == null) yield return null;

        if (GameManager.I != null)
        {
            UpdateUI();
            CalculateSlotPositions(); // Önce slot yerlerini hesapla
            SpawnGladiators();
            SpawnStaffs(); // <-- BUNU EKLEMEYÝ UNUTMA! Stafflar oyun baþýnda doðsun.
        }
    }

    public void UpdateUI()
    {
        if (GameManager.I == null) return;
        GoldText.text = "Gold: " + GameManager.I.Resources.Gold;
        FoodText.text = "Food: " + GameManager.I.Resources.Food;
        WaterText.text = "Water: " + GameManager.I.Resources.Water;
        DayText.text = "Day: " + GameManager.I.Day;

        // --- YENÝ: LEVEL VE XP GÜNCELLEME ---
        LevelText.text = "Lvl " + GameManager.I.Resources.LudusLevel;

        // Slider deðerini 0 ile 1 arasýna orantýla
        float fill = GameManager.I.Resources.CurrentXP / GameManager.I.Resources.MaxXP;
        XPSlider.value = fill;
    }

    // Bu fonksiyon sadece sanal ýzgaranýn noktalarýný hesaplar
    void CalculateSlotPositions()
    {
        slotPositions.Clear();

        Vector3 screenCenter = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 10f));
        screenCenter.z = 0;
        screenCenter.y += VerticalBias;

        float totalGridWidth = (Columns - 1) * SpacingX;
        int totalRows = Mathf.CeilToInt((float)TotalSlots / Columns);
        float totalGridHeight = (totalRows - 1) * SpacingY;

        float startX = screenCenter.x - (totalGridWidth / 2f);
        float startY = screenCenter.y + (totalGridHeight / 2f);

        for (int i = 0; i < TotalSlots; i++)
        {
            int col = i % Columns;
            int row = i / Columns;

            float posX = startX + (col * SpacingX);
            float posY = startY - (row * SpacingY);

            slotPositions.Add(new Vector3(posX, posY, 0));
        }
    }

    public void SpawnGladiators()
    {
        if (GameManager.I == null) return;
        foreach (Transform child in GardenArea) Destroy(child.gameObject);

        var livingGladiators = GameManager.I.Gladiators.Where(g => g.IsAlive).ToList();

        // --- Evi Olmayanlara Yer Bul ---
        List<int> occupiedSlots = livingGladiators
            .Where(g => g.GridIndex != -1)
            .Select(g => g.GridIndex)
            .ToList();

        foreach (var g in livingGladiators)
        {
            if (g.GridIndex == -1)
            {
                for (int i = 0; i < TotalSlots; i++)
                {
                    if (!occupiedSlots.Contains(i))
                    {
                        g.GridIndex = i;
                        occupiedSlots.Add(i);
                        break;
                    }
                }
            }
        }

        foreach (var g in livingGladiators)
        {
            if (g.GridIndex >= 0 && g.GridIndex < slotPositions.Count)
            {
                var go = Instantiate(GladiatorPrefab, GardenArea);
                var view = go.GetComponent<GladiatorView>();
                if (view != null) view.Bind(g);

                go.transform.position = slotPositions[g.GridIndex];

                if (g.CurrentState == GladiatorState.Resting)
                {
                    go.GetComponent<SpriteRenderer>().color = Color.gray;
                }
            }
        }
    }

    // --- DRAG SÝSTEMÝ ÝÇÝN YARDIMCI FONKSÝYONLAR ---

    public int GetNearestSlotIndex(Vector3 position)
    {
        int bestIndex = 0;
        float closestDistanceSqr = Mathf.Infinity;

        for (int i = 0; i < slotPositions.Count; i++)
        {
            float dSqr = (position - slotPositions[i]).sqrMagnitude;
            if (dSqr < closestDistanceSqr)
            {
                closestDistanceSqr = dSqr;
                bestIndex = i;
            }
        }
        return bestIndex;
    }

    public Vector3 GetSlotPosition(int index)
    {
        if (index >= 0 && index < slotPositions.Count)
            return slotPositions[index];
        return Vector3.zero;
    }

    void OnDrawGizmos()
    {
        if (slotPositions != null)
        {
            Gizmos.color = Color.yellow;
            foreach (var pos in slotPositions)
            {
                Gizmos.DrawWireCube(pos, new Vector3(1, 1, 0) * 0.5f);
            }
        }
    }

    public void OnNextDayClicked()
    {
        if (GameManager.I == null) return;
        GameManager.I.EndDay();
        UpdateUI();
        SpawnGladiators();
        // Ýstersen her gün stafflarý da yenilemek için buraya SpawnStaffs() ekleyebilirsin
    }

    public void SpawnStaffs()
    {
        // 1. Temizlik
        foreach (var s in activeStaff)
        {
            if (s != null) Destroy(s);
        }
        activeStaff.Clear();

        // --- SABÝT POZÝSYON AYARLARI ---
        // X = 0.1f (Ekranýn solu)
        // Y deðerlerini her biri için özel veriyoruz (0.0 en alt, 1.0 en üst)

        float xLoc = 0.1f;

        // 2. MEDICUS (Doktor) -> YERÝ: ÜST KAT (0.75)
        if (GameManager.I.Resources.HasMedicus)
        {
            Vector3 pos = Camera.main.ViewportToWorldPoint(new Vector3(xLoc, 0.75f, 10f));
            pos.z = 0;
            SpawnIndividualStaff(MedicusPrefab, pos);
        }

        // 3. DOCTORE (Eðitmen) -> YERÝ: ORTA KAT (0.50)
        if (GameManager.I.Resources.HasDoctore)
        {
            Vector3 pos = Camera.main.ViewportToWorldPoint(new Vector3(xLoc, 0.50f, 10f));
            pos.z = 0;
            SpawnIndividualStaff(DoctorePrefab, pos);
        }

        // 4. FABER (Demirci) -> YERÝ: ALT KAT (0.25)
        if (GameManager.I.Resources.HasFaber)
        {
            Vector3 pos = Camera.main.ViewportToWorldPoint(new Vector3(xLoc, 0.25f, 10f));
            pos.z = 0;
            SpawnIndividualStaff(FaberPrefab, pos);
        }
    }

    // Yeni sistemin yardýmcýsý: Sadece prefabý koyar
    void SpawnIndividualStaff(GameObject prefab, Vector3 pos)
    {
        GameObject go = Instantiate(prefab, transform);
        go.transform.position = pos;
        activeStaff.Add(go);
    }

    public void OpenCampaign()
    {
        // Global UI Manager kullanýyorsan onu çaðýr, yoksa manuel aç
        // Örn: FindObjectOfType<LudusShopManager>().CloseAllPanels();
        CampaignPanel.SetActive(true);
    }

} // <--- SINIF BURADA BÝTÝYOR. ALTINDA BAÞKA KOD OLMAMALI.
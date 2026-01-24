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
    public Text WineText;

    [Header("References")]
    public Transform GardenArea;
    [Header("Prefabs")]
    // Artýk tek bir tane deðil, liste olacak
    public GameObject[] GladiatorPrefabs;

    [Header("Grid Settings")]
    public int Columns = 6;
    public int TotalSlots = 12;   // 3x6 = 18 slot
    public float SpacingX = 2.4f;    // Yan yana açýlma (1.6'ydý, artýrdýk)
    public float SpacingY = 3.5f;    // Alt alta açýlma (2.0'dý, bayaðý artýrdýk)
    public float VerticalBias = -1.5f; // Aþaðý kaydýrma (-0.5'ti, daha aþaðý aldýk)

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

    [Header("Ludus Visual Tuning")]
    public float LudusGladiatorScale = 0.9f;

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
        GoldText.text =  GameManager.I.Resources.Gold + " " ;
        FoodText.text =  GameManager.I.Resources.Food + " ";
        WaterText.text = GameManager.I.Resources.Water + " ";
        WineText.text = GameManager.I.Resources.Wine + " ";
        DayText.text = GameManager.I.Day + "Days";

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

        Vector3 screenCenter = Camera.main.ViewportToWorldPoint(
            new Vector3(0.5f, 0.5f, 10f)
        );

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

        // 0. TAZE POZÝSYON HESABI
        CalculateSlotPositions();

        // 1. SAHNE TEMÝZLÝÐÝ (Eski objeleri sil, veriler GameManager'da duruyor)
        foreach (Transform child in GardenArea) Destroy(child.gameObject);

        var livingGladiators = GameManager.I.Gladiators.Where(g => g.IsAlive).ToList();

        // --- DÜZELTME BURADA: MEVCUT YERLERÝ KÝLÝTLE ---
        // Önce kimlerin yeri zaten var, onlarý bir listeye not et.
        List<int> occupiedSlots = new List<int>();

        foreach (var g in livingGladiators)
        {
            // Eðer geçerli bir slotu varsa (-1 deðilse ve 18'den küçükse)
            if (g.GridIndex >= 0 && g.GridIndex < TotalSlots)
            {
                // Bu koltuk doludur, listeye ekle.
                occupiedSlots.Add(g.GridIndex);
            }
            else
            {
                // Eðer saçma bir sayýysa (örn 500), onu -1 yap ki aþaðýda yeni yer bulalým.
                g.GridIndex = -1;
            }
        }

        // --- ÞÝMDÝ SADECE EVSÝZLERE YER BUL ---
        foreach (var g in livingGladiators)
        {
            // Sadece yeri olmayanlar (-1) için döngüye gir
            if (g.GridIndex == -1)
            {
                for (int i = 0; i < TotalSlots; i++)
                {
                    // Eðer bu koltuk (i) dolu listesinde YOKSA
                    if (!occupiedSlots.Contains(i))
                    {
                        g.GridIndex = i;        // Adamý buraya oturt
                        occupiedSlots.Add(i);   // Koltuðu dolu iþaretle
                        break;                  // Döngüden çýk, sýradaki adama geç
                    }
                }
            }
        }

        // 3. YARATMA (GÖRSELLEÞTÝRME)
        foreach (var g in livingGladiators)
        {
            if (g.GridIndex >= 0 && g.GridIndex < slotPositions.Count)
            {
                GameObject prefabToUse = null;
                if (GladiatorPrefabs != null && GladiatorPrefabs.Length > 0)
                {
                    int index = Mathf.Max(0, g.BodyTypeIndex);
                    prefabToUse = GladiatorPrefabs[index % GladiatorPrefabs.Length];
                }

                if (prefabToUse != null)
                {
                    var go = Instantiate(prefabToUse, GardenArea);
                    // Ludus'a özel scale
                    go.transform.localScale = Vector3.one * LudusGladiatorScale;


                    // A) VERÝYÝ BAÐLA
                    var view = go.GetComponent<GladiatorView>();
                    if (view != null) view.Bind(g);

                    // B) YAPAY ZEKAYI AYARLA (LUDUS MODU)
                    var ai = go.GetComponent<GladiatorAI>();
                    if (ai != null)
                    {
                        ai.enabled = true;
                        ai.IsCombatMode = false;   // Barýþ modu
                        ai.WanderRadius = 2.0f;
                    }

                    // C) ANÝMASYONU "IDLE" BAÞLAT
                    var anim = go.GetComponent<Animator>();
                    if (anim != null)
                    {
                        // "IsRunning" bool'unu false yap, Idle çalsýn
                        anim.SetBool("Run1", false);
                    }

                    // D) FÝZÝÐÝ KAPAT (Kinematic)
                    Rigidbody2D rb = go.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        rb.bodyType = RigidbodyType2D.Kinematic;
                        rb.velocity = Vector2.zero;
                        rb.gravityScale = 0f;
                    }

                    // E) COLLIDER (TRIGGER OLSUN - SÜRÜKLEME ÝÇÝN)
                    BoxCollider2D col = go.GetComponent<BoxCollider2D>();
                    if (col != null) col.isTrigger = true;

                    // F) POZÝSYON
                    go.transform.position = slotPositions[g.GridIndex];

                    // Renk (Resting ise gri)
                    if (g.CurrentState == GladiatorState.Resting)
                    {
                        var sr = go.GetComponent<SpriteRenderer>();
                        if (sr == null) sr = go.GetComponentInChildren<SpriteRenderer>();
                        if (sr != null) sr.color = Color.gray;
                    }
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

    public void ToggleCampaign()
    {
        bool isOpen = (CampaignPanel != null && CampaignPanel.activeSelf);
        if (CampaignPanel != null) CampaignPanel.SetActive(!isOpen);
    }

    public void PlaceOrSwap(GladiatorView draggedView, int targetSlot, int originalSlot)
    {
        if (GameManager.I == null) return;
        if (draggedView == null || draggedView.Data == null) return;

        targetSlot = Mathf.Clamp(targetSlot, 0, TotalSlots - 1);
        originalSlot = Mathf.Clamp(originalSlot, 0, TotalSlots - 1);

        if (targetSlot == originalSlot)
        {
            draggedView.transform.position = GetSlotPosition(originalSlot);
            return;
        }

        var draggedData = draggedView.Data;

        // Hedef slotta baþka biri var mý?
        var otherData = GameManager.I.Gladiators
            .FirstOrDefault(g => g != draggedData && g.IsAlive && g.GridIndex == targetSlot);

        if (otherData != null)
        {
            // Diðerini eski slota taþý
            otherData.GridIndex = originalSlot;

            var otherView = GardenArea
                .GetComponentsInChildren<GladiatorView>(true)
                .FirstOrDefault(v => v != null && v.Data == otherData);

            if (otherView != null)
                otherView.transform.position = GetSlotPosition(originalSlot);
        }

        // Sürüklenen kiþiyi hedef slota koy
        draggedData.GridIndex = targetSlot;
        draggedView.transform.position = GetSlotPosition(targetSlot);
    }



} // <--- SINIF BURADA BÝTÝYOR. ALTINDA BAÞKA KOD OLMAMALI.
using UnityEngine;
using UnityEngine.EventSystems;

public class GladiatorDrag : MonoBehaviour
{
    private bool isDragging = false; // Şu an sürükleniyor mu?
    private bool isClickPotential = true; // Tıklama olma ihtimali var mı?

    private Vector3 offset;
    private Vector3 originalPosition;
    private Vector3 clickStartPosition; // Tıklamanın başladığı yer

    private LudusManager manager;
    private GladiatorView view;

    // Panelleri burada tutacağız
    private GameObject[] BlockingPanels;

    // Sürükleme sayılması için ne kadar hareket etmeli? (0.2 birim)
    private float dragThreshold = 0.2f;

    void Start()
    {
        manager = FindObjectOfType<LudusManager>();
        view = GetComponent<GladiatorView>();

        var shopManager = FindObjectOfType<LudusShopManager>();
        var arenaUI = FindObjectOfType<ArenaSelectorUI>();

        if (shopManager != null && arenaUI != null)
        {
            BlockingPanels = new GameObject[] {
                shopManager.MarketPanel,
                shopManager.RecruitPanel,
                shopManager.StaffPanel,
                arenaUI.ArenaWrapperPanel
            };
        }
        else
        {
            BlockingPanels = new GameObject[0];
        }
    }

    void OnMouseDown()
    {
        // 1. MANAGER KONTROLÜ
        if (manager == null) return;

        // 2. UI KORUMASI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        // 3. PANEL KORUMASI
        if (BlockingPanels != null)
        {
            foreach (GameObject panel in BlockingPanels)
            {
                if (panel != null && panel.activeSelf) return;
            }
        }

        // --- HAZIRLIK ---
        originalPosition = transform.position;
        clickStartPosition = GetMouseWorldPos(); // Başlangıç noktasını kaydet
        offset = transform.position - GetMouseWorldPos();

        isDragging = false;       // Henüz sürüklemiyoruz
        isClickPotential = true;  // Tıklama olabilir
    }

    void OnMouseDrag()
    {
        // Mouse'un şu anki konumu
        Vector3 currentMousePos = GetMouseWorldPos();

        // Eğer henüz "Sürükleme Modu"na girmediysek, mesafeyi ölçelim
        if (!isDragging)
        {
            float distance = Vector3.Distance(currentMousePos, clickStartPosition);

            // Eğer fare yeterince hareket ettiyse, artık bu bir TIKLAMA DEĞİL, SÜRÜKLEMEDİR.
            if (distance > dragThreshold)
            {
                isDragging = true;
                isClickPotential = false; // Artık tıklama olamaz

                // Görsel Efekt: Sürükleme başladığında büyüt
                transform.localScale = Vector3.one * 1.2f;
                GetComponent<SpriteRenderer>().sortingOrder = 10;
            }
        }

        // Eğer sürükleme modundaysak objeyi taşı
        if (isDragging)
        {
            transform.position = currentMousePos + offset;
        }
    }

    void OnMouseUp()
    {
        // Önce görseli düzelt (Her durumda eski boyuta dönecek)
        transform.localScale = Vector3.one;
        GetComponent<SpriteRenderer>().sortingOrder = 0;

        // --- SENARYO 1: SADECE TIKLAMA (HİÇ SÜRÜKLENMEDİ) ---
        if (isClickPotential && !isDragging)
        {
            // İstatistik Panelini Aç!
            if (GladiatorUI.I != null && view != null)
            {
                GladiatorUI.I.Show(view.Data, transform);
            }

            // Pozisyonu bozma, eski yerine oturt (hafif kayma varsa diye)
            transform.position = originalPosition;
        }

        // --- SENARYO 2: SÜRÜKLEME BİTTİ ---
        else if (isDragging)
        {
            isDragging = false;

            if (manager != null)
            {
                int nearestSlot = manager.GetNearestSlotIndex(transform.position);
                Vector3 targetPos = manager.GetSlotPosition(nearestSlot);
                transform.position = targetPos;

                if (view != null && view.Data != null)
                {
                    view.Data.GridIndex = nearestSlot;
                }
            }
            else
            {
                transform.position = originalPosition;
            }
        }
    }

    Vector3 GetMouseWorldPos()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = 10f;
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }
}
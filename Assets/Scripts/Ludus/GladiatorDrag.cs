using UnityEngine;
using UnityEngine.EventSystems;

public class GladiatorDrag : MonoBehaviour
{
    private bool isDragging = false;
    private bool isClickPotential = true;

    private Vector3 offset;
    private Vector3 originalPosition;
    private Vector3 clickStartPosition;
    private float dragThreshold = 0.2f;

    private LudusManager manager;
    private GladiatorView view;

    private int originalSlotIndex = -1;

    bool IsPointerOverUI()
    { // Mouse/touch UI üzerindeyse true döner
     return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    void Start()
    {
        manager = FindObjectOfType<LudusManager>();
        view = GetComponent<GladiatorView>();
    }

    void OnMouseDown()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Arena") return;
        if (IsPointerOverUI()) return;
        


        originalSlotIndex = (view != null && view.Data != null) ? view.Data.GridIndex : -1;

        // 1. MANAGER YOKSA HİÇ ÇALIŞMA
        if (manager == null) return;

        // 2. UI KORUMASI (EventSystem)
        // Eğer fare şu an bir UI elemanının (Buton, Panel vs.) üzerindeyse, alttaki kodları çalıştırma.
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        // 3. PANEL KORUMASI (Manuel Kontrol - Çifte Dikiş)
        // Eğer EventSystem kaçırırsa diye, açık panelleri manuel kontrol et.
        if (IsAnyPanelOpen()) return;

        // --- TIKLAMA İŞLEMLERİ BAŞLIYOR ---
        originalPosition = transform.position;
        clickStartPosition = GetMouseWorldPos();
        offset = transform.position - GetMouseWorldPos();

        isDragging = false;
        isClickPotential = true;
    }

    // Açık panel var mı diye kontrol eden yardımcı fonksiyon
    bool IsAnyPanelOpen()
    {
        // ShopManager'ı her seferinde taze bul (Performans kaybı olmaz, tıklama anında çalışır)
        var shopManager = FindObjectOfType<LudusShopManager>();
        if (shopManager != null)
        {
            if (shopManager.MarketPanel != null && shopManager.MarketPanel.activeSelf) return true;
            if (shopManager.RecruitPanel != null && shopManager.RecruitPanel.activeSelf) return true;
            if (shopManager.StaffPanel != null && shopManager.StaffPanel.activeSelf) return true;
        }

        var arenaUI = FindObjectOfType<ArenaSelectorUI>();
        if (arenaUI != null)
        {
            if (arenaUI.ArenaWrapperPanel != null && arenaUI.ArenaWrapperPanel.activeSelf) return true;
        }

        return false;
    }

    void OnMouseDrag()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Arena") return;
        // UI üzerindeyken sürüklemeye devam etmesin
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        Vector3 currentMousePos = GetMouseWorldPos();

        if (!isDragging)
        {
            float distance = Vector3.Distance(currentMousePos, clickStartPosition);
            if (distance > dragThreshold)
            {
                isDragging = true;
                isClickPotential = false;
                transform.localScale = Vector3.one * 1.2f;
                var sr = GetComponent<SpriteRenderer>();
                if (sr) sr.sortingOrder = 10;
            }
        }

        if (isDragging)
        {
            transform.position = currentMousePos + offset;
        }
    }

    void OnMouseUp()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Arena") return;
        if (IsPointerOverUI()) return;
        transform.localScale = Vector3.one;
        var sr = GetComponent<SpriteRenderer>();
        if (sr) sr.sortingOrder = 0;

        if (isClickPotential && !isDragging)
        {
            // UI üzerine bırakılmadıysa işlem yap
            if (!IsAnyPanelOpen())
            {
                if (GladiatorUI.I != null && view != null)
                {
                    GladiatorUI.I.Show(view.Data, transform);
                }
            }
            transform.position = originalPosition;
        }
        else if (isDragging)
        {
            isDragging = false;

            if (manager != null)
            {
                int nearestSlot = manager.GetNearestSlotIndex(transform.position);

                // Swap / place işlemini manager yapsın
                if (view != null)
                    manager.PlaceOrSwap(view, nearestSlot, originalSlotIndex);
                else
                    transform.position = originalPosition;
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
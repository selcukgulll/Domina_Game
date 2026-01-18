using UnityEngine;
using UnityEngine.EventSystems; // <-- BU KÜTÜPHANE ŞART!

public class GladiatorDrag : MonoBehaviour
{
    private bool isDragging = false;
    private Vector3 offset;
    private Vector3 originalPosition;
    private LudusManager manager;
    private GladiatorView view;

    // Panelleri burada tutacağız
    private GameObject[] BlockingPanels;

    void Start()
    {
        manager = FindObjectOfType<LudusManager>();
        view = GetComponent<GladiatorView>();

        // --- DÜZELTME 1: PANELLERİ GARANTİ YOLDAN BULMA ---
        // GameObject.Find kapalı objeleri bulamaz. O yüzden Manager'lardan istiyoruz.

        // 1. Market, Staff, Recruit panellerini ShopManager'dan alalım
        var shopManager = FindObjectOfType<LudusShopManager>();

        // 2. Arena panelini ArenaUI'dan alalım
        var arenaUI = FindObjectOfType<ArenaSelectorUI>();

        // Listeyi güvenli bir şekilde oluşturuyoruz (Null kontrolü yaparak)
        if (shopManager != null && arenaUI != null)
        {
            BlockingPanels = new GameObject[] {
                shopManager.MarketPanel,
                shopManager.RecruitPanel,
                shopManager.StaffPanel,
                arenaUI.ArenaWrapperPanel // Arena Wrapper Panel (En dıştaki)
            };
        }
        else
        {
            // Eğer managerlar sahnede yoksa boş liste yap ki hata vermesin
            BlockingPanels = new GameObject[0];
        }
    }

    void OnMouseDown()
    {
        // 1. UI KORUMASI: Mouse gerçek bir UI (Buton, Panel vb.) üzerinde mi?
        // (Physics 2D Raycaster silindiği için artık gladyatörde tetiklenmez, sadece UI'da çalışır)
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        // 2. YASAKLI PANEL KORUMASI: Büyük panellerden biri açık mı?
        // (Arena, Market vs. açıksa gladyatöre dokunmayı engeller)
        if (BlockingPanels != null)
        {
            foreach (GameObject panel in BlockingPanels)
            {
                if (panel != null && panel.activeSelf) return; // Panel açıksa işlem yapma
            }
        }

        // --- TIKLAMA BAŞARILI, SÜRÜKLEME BAŞLASIN ---
        isDragging = true;
        originalPosition = transform.position;

        // Mouse ile obje arasındaki farkı hesapla (Tam ortasından tutmasan bile kaymasın diye)
        offset = transform.position - GetMouseWorldPos();

        // Görsel Efekt (Büyüt ve öne al)
        transform.localScale = Vector3.one * 1.2f;
        GetComponent<SpriteRenderer>().sortingOrder = 10;
    }

    void OnMouseDrag()
    {
        if (isDragging)
        {
            transform.position = GetMouseWorldPos() + offset;
        }
    }

    void OnMouseUp()
    {
        if (!isDragging) return; // Sürüklenmiyorsa işlem yapma

        isDragging = false;
        transform.localScale = Vector3.one;
        GetComponent<SpriteRenderer>().sortingOrder = 0;

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

    Vector3 GetMouseWorldPos()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = 10f;
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }
}
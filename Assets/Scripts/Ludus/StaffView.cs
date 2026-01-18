using UnityEngine;
using UnityEngine.EventSystems;

public class StaffView : MonoBehaviour
{
    public StaffType Type; // Editörden seçeceðiz (Medicus mu Doctore mi?)
    private StaffInteractionUI uiManager;

    void Start()
    {
        uiManager = FindObjectOfType<StaffInteractionUI>();
    }

    void OnMouseDown()
    {
        // 1. UI Korumasý (Ayný Gladyatördeki gibi)
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        // 2. Panel açýksa baþka staff'a týklayýnca bilgiler güncellensin
        if (uiManager != null)
        {
            uiManager.OpenPanel(Type);
        }
    }
}
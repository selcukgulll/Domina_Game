using UnityEngine;
using UnityEngine.EventSystems;

public class StaffView : MonoBehaviour
{
    [Header("Which staff is this object?")]
    public StaffType Type; // Inspector'dan Medicus / Doctore / Faber seç

    private StaffInteractionUI uiManager;

    void Start()
    {
        uiManager = FindObjectOfType<StaffInteractionUI>();
    }

    void OnMouseDown()
    {
        // UI üzerindeyken alttaki world týklamasýný yeme
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (uiManager == null)
            return;

        // Týklanan staff tipine göre aç/kapat (toggle)
        uiManager.ToggleStaff(Type);
    }
}

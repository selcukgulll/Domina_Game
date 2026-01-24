using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonClickPunch : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] float punchScale = 1.12f;
    [SerializeField] float duration = 0.08f;

    RectTransform rt;
    Vector3 baseScale;
    Coroutine co;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        baseScale = rt.localScale;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (co != null) StopCoroutine(co);
        co = StartCoroutine(Punch());
    }

    IEnumerator Punch()
    {
        rt.localScale = baseScale * punchScale;
        yield return new WaitForSeconds(duration);
        rt.localScale = baseScale;
        co = null;
    }
}

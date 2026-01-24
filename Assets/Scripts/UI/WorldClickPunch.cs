using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class WorldClickPunch : MonoBehaviour
{
    [SerializeField] float punchScale = 1.12f;
    [SerializeField] float duration = 0.08f;

    Vector3 baseScale;
    Coroutine co;

    void Awake()
    {
        baseScale = transform.localScale;
    }

    void OnMouseDown()
    {
        // UI üstündeyken world týklamasýný yeme
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        PlayPunch();
    }

    public void PlayPunch()
    {
        if (co != null) StopCoroutine(co);
        co = StartCoroutine(Punch());
    }

    IEnumerator Punch()
    {
        transform.localScale = baseScale * punchScale;
        yield return new WaitForSeconds(duration);
        transform.localScale = baseScale;
        co = null;
    }
}

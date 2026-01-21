using UnityEngine;
using UnityEngine.UI;

public class GladiatorView : MonoBehaviour
{
    private GladiatorData currentData;
    public GladiatorData Data { get { return currentData; } }

    [Header("UI Elements")]
    public Transform CanvasObj;
    public Text NameText;
    public Text WinText;
    public Image HPBarFill;
    public GameObject BadgeObj;

    [Header("Visuals")]
    public SpriteRenderer BodyRenderer;

    public Vector3 UIOffset = new Vector3(0, 0.8f, 0);

    // Arena ve Düþman kontrolü
    private bool isArenaMode = false;
    private bool isEnemy = false;

    [Header("Animation")]
    public Animator UnitAnimator; // Inspector'dan Animator'ý buraya sürükle!
    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private static readonly int AttackIndexHash = Animator.StringToHash("AttackIndex");
    private static readonly int HitHash = Animator.StringToHash("Hit");
    private static readonly int DeathHash = Animator.StringToHash("Death");
    private static readonly int Run1Hash = Animator.StringToHash("Run1");


    // --- ÖNEMLÝ: Fonksiyonu 3 parametreli yaptýk ---
    public void Bind(GladiatorData data, bool arenaContext = false, bool enemyFlag = false)
    {
        currentData = data;
        isArenaMode = arenaContext;
        isEnemy = enemyFlag;

        RefreshVisuals();
    }

    void LateUpdate()
    {
        if (CanvasObj != null)
        {
            // 1. DÝK DURUÞ VE POZÝSYON (Standart)
            CanvasObj.rotation = Quaternion.identity;
            CanvasObj.position = transform.position + UIOffset;

            // 2. AYNA DÜZELTME (Yeni Deðiþkensiz Yöntem)
            // Mantýk: Canvas'ýn mevcut boyutu neyse (0.005 falan olabilir) onu al,
            // ama iþaretini (+ veya -) babasýnýn yönüne göre ayarla.

            float currentSizeX = Mathf.Abs(CanvasObj.localScale.x);
            CanvasObj.localScale = new Vector3(currentSizeX, CanvasObj.localScale.y, CanvasObj.localScale.z);

        }
    }

    public void RefreshVisuals()
    {
        if (currentData == null) return;

        // 1. ÝSÝM ve RENK AYARI
        if (NameText != null)
        {
            NameText.text = currentData.Name;

            if (isArenaMode)
            {
                // Arenadaysak: Düþman Kýrmýzý, Bizimki Beyaz
                if (isEnemy) NameText.color = Color.red;
                else NameText.color = Color.white;
            }
            // Ludus'taysak: Prefab rengi (Sarý vs.) kalýr
        }

        // 2. CAN BARI
        if (HPBarFill != null && currentData.MaxHP > 0)
        {
            float hpPercent = currentData.HP / currentData.MaxHP;
            HPBarFill.fillAmount = hpPercent;
            HPBarFill.color = Color.Lerp(Color.red, Color.green, hpPercent);
        }

        // 3. BADGE ve WIN SAYISI
        if (BadgeObj != null)
        {
            BadgeObj.SetActive(true); // Rozet hep görünsün
            if (WinText != null)
            {
                // Verideki Win sayýsýný yaz
                WinText.text = currentData.Wins.ToString();
            }
        }
    }

    // Hasar efekti için (Daha önce konuþtuðumuz animasyon kodu)
    public void PlayDamageEffect(float currentHP, float maxHP)
    {
        if (HPBarFill != null && maxHP > 0)
        {
            float hpPercent = currentHP / maxHP;
            HPBarFill.fillAmount = hpPercent;
            HPBarFill.color = Color.Lerp(Color.red, Color.green, hpPercent);
        }
        // Ýlerde buraya "Hit" animasyonu eklenebilir
    }

    public void PlayHit()
    {
        if (UnitAnimator == null) return;
        UnitAnimator.SetTrigger(HitHash);
    }

    public void PlayDeath()
    {
        if (UnitAnimator == null) return;
        UnitAnimator.SetTrigger(DeathHash);
    }


    // Koþma animasyonunu açýp kapatan fonksiyon
    public void SetRunning(bool isRunning)
    {
        if (UnitAnimator != null)
            UnitAnimator.SetBool(Run1Hash, isRunning);
    }

    public void SetFacing(bool faceRight)
    {
        // BodyRenderer atanmýþsa onu flip et
        if (BodyRenderer != null)
            BodyRenderer.flipX = !faceRight;

        // Eðer bazý variant prefablarýnda sprite BodyRenderer deðil de child'lardaysa,
        // garanti olsun diye hepsini flipleyelim:
        foreach (var sr in GetComponentsInChildren<SpriteRenderer>(true))
            sr.flipX = !faceRight;
    }


    public void PlayAttack(int attackIndex)
    {
        if (UnitAnimator == null) return;
        UnitAnimator.SetInteger(AttackIndexHash, attackIndex);
        UnitAnimator.SetTrigger(AttackHash);
    }


}
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

            float currentSizeX = Mathf.Abs(CanvasObj.localScale.x); // Boyutu pozitif olarak al

            // Eðer karakter sola bakýyorsa (-), Canvas da (-) olsun ki çarpýmlarý (+) çýksýn.
            float direction = (transform.localScale.x < 0) ? -1f : 1f;

            // Sadece X eksenini etkile, Y ve Z olduðu gibi kalsýn
            CanvasObj.localScale = new Vector3(currentSizeX * direction, CanvasObj.localScale.y, CanvasObj.localScale.z);
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
}
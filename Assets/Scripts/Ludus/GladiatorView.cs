using UnityEngine;
using UnityEngine.UI;
using System.Collections; // <-- Coroutine (Yanýp sönme) için bunu ekledik

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

    [Header("Visuals (Yeni)")]
    public SpriteRenderer BodyRenderer; // <-- YENÝ: Gladyatörün resmi (Kýzarmasý için)

    // --- ROTASYON KÝLÝDÝ ---
    public Vector3 UIOffset = new Vector3(0, 0.8f, 0);

    public void Bind(GladiatorData data)
    {
        currentData = data;
        RefreshVisuals();
    }

    void LateUpdate()
    {
        if (CanvasObj != null)
        {
            // Canvas hep dik dursun ve kafanýn üstünde kalsýn
            CanvasObj.rotation = Quaternion.identity;
            CanvasObj.position = transform.position + UIOffset;
        }
    }

    public void RefreshVisuals()
    {
        if (currentData == null) return;

        if (NameText != null)
            NameText.text = currentData.Name;

        if (HPBarFill != null)
        {
            float hpPercent = currentData.HP / currentData.MaxHP;
            HPBarFill.fillAmount = hpPercent;
            HPBarFill.color = Color.Lerp(Color.red, Color.green, hpPercent);
        }

        if (WinText != null && BadgeObj != null)
        {
            BadgeObj.SetActive(true);
            WinText.text = currentData.Wins.ToString();
        }
    }

    // --- YENÝ EKLENEN FONKSÝYONLAR ---
    // Bu fonksiyonu Savaþ Kodun (BattleSystem veya ArenaManager) çaðýracak
    public void PlayDamageEffect(float currentHP, float maxHP)
    {
        // 1. CAN BARINI GÜNCELLE (Anlýk)
        if (HPBarFill != null && maxHP > 0)
        {
            float hpPercent = currentHP / maxHP;
            HPBarFill.fillAmount = hpPercent;
            // Can azaldýkça rengi de deðiþsin (Yeþilden Kýrmýzýya)
            HPBarFill.color = Color.Lerp(Color.red, Color.green, hpPercent);
        }

        // 2. KIZARMA EFEKTÝNÝ BAÞLAT
        if (BodyRenderer != null)
        {
            // Eðer arka arkaya vurulursa eski efekti durdur, yenisini baþlat
            StopAllCoroutines();
            StartCoroutine(RedFlashRoutine());
        }
    }

    // Kýzarma animasyonu (0.1 saniye kýrmýzý kalýr, sonra düzelir)
    IEnumerator RedFlashRoutine()
    {
        // 1. DAMAGE RENGÝ (Hafif Kýrmýzý Tint)
        BodyRenderer.color = new Color(1f, 0.6f, 0.6f, 1f);

        yield return new WaitForSeconds(0.15f); // Biraz daha belirgin olsun diye 0.15s

        // 2. RESET (Boya Silme)
        // Bu komut "Beyaza boya" demek deðildir. "Üzerindeki renk filtresini kaldýr" demektir.
        // Prefabýn orjinali sarýysa sarý, maviyse mavi görünür.
        BodyRenderer.color = Color.white;
    }
}
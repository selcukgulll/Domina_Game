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
        // YENÝSÝ: Yumuþak Kýrmýzý (Tint)
        // Mantýk þu: RGB (Kýrmýzý, Yeþil, Mavi).
        // Tam kýrmýzý (1, 0, 0)'dýr.
        // Biz (1, 0.5, 0.5) yaparak kýrmýzýnýn þiddetini azaltýp, orijinal renklerin
        // alttan %50 oranýnda görünmesini saðlýyoruz.
        // Sondaki '1f' ise opaklýk (Alpha). Karakter hayalet gibi olmasýn diye tam görünür (1) býrakýyoruz.

        BodyRenderer.color = new Color(1f, 0.4f, 0.4f, 1f); // Rengi buradan açýp koyulaþtýrabilirsin

        yield return new WaitForSeconds(0.1f); // 0.1 saniye bekle

        BodyRenderer.color = Color.white;     // Normale dön
    }
}
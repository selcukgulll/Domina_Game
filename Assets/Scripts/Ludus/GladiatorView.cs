using UnityEngine;
using UnityEngine.UI;

public class GladiatorView : MonoBehaviour
{
    private GladiatorData currentData;
    // Dýþarýdan okumak için (Drag sistemi için gerekli)
    public GladiatorData Data { get { return currentData; } }

    [Header("UI Elements")]
    public Transform CanvasObj; // <-- YENÝ: Dönmeyi engellemek için Canvas'ý buraya atacaðýz
    public Text NameText;
    public Text WinText;
    public Image HPBarFill;
    public GameObject BadgeObj;

    public void Bind(GladiatorData data)
    {
        currentData = data;
        RefreshVisuals();
    }

    // --- YENÝ EKLENEN KISIM: ROTASYON KÝLÝDÝ ---
    // Gladyatör (Parent) yana dönse bile, Canvas (Child) hep dik duracak
    // Bu deðiþkeni Inspector'dan ayarlayabilirsin (Y=1.5 iyi bir baþlangýçtýr)
    public Vector3 UIOffset = new Vector3(0, 0.8f, 0);

    void LateUpdate()
    {
        if (CanvasObj != null)
        {
            // 1. DÖNMEYÝ SIFIRLA (Hep Dik Dur)
            CanvasObj.rotation = Quaternion.identity;

            // 2. POZÝSYONU SABÝTLE (Hep Tepede Dur)
            // Gladyatör (transform.position) nerede olursa olsun, 
            // Canvas onun tam 'UIOffset' kadar yukarýsýnda olsun.
            // Böylece Gladyatör yan dönse bile Canvas onunla birlikte yana kaymaz.
            CanvasObj.position = transform.position + UIOffset;
        }
    }

    public void RefreshVisuals()
    {
        if (currentData == null) return;

        // 1. Ýsim
        if (NameText != null)
            NameText.text = currentData.Name;

        // 2. Can Barý 
        if (HPBarFill != null)
        {
            float hpPercent = currentData.HP / currentData.MaxHP;
            HPBarFill.fillAmount = hpPercent;
            HPBarFill.color = Color.Lerp(Color.red, Color.green, hpPercent);
        }

        // 3. Galibiyet Rozeti (GÜNCELLENDÝ)
        // Artýk if(Wins > 0) kontrolü yok. Hep açýk.
        if (WinText != null && BadgeObj != null)
        {
            BadgeObj.SetActive(true); // Her zaman görünür olsun
            WinText.text = currentData.Wins.ToString();
        }
    }
}
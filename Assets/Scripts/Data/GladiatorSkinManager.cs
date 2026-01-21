using UnityEngine;
using System.Collections.Generic;

public class GladiatorSkinManager : MonoBehaviour
{
    [Header("Ana Parçalar (Sprite Deðiþecek)")]
    public SpriteRenderer HeadRenderer;  // 'head' kemiðindeki resim
    public SpriteRenderer ChestRenderer; // 'chest' kemiðindeki resim (Zýrh vs.)
    public SpriteRenderer BellyRenderer; // 'belly' kemiðindeki resim (Kemer/Don)

    [Header("Ten Rengi Parçalarý (Sadece Renk Deðiþecek)")]
    // Kollar, bacaklar, eller ve ayaklarý buraya atacaðýz.
    // Böylece "Zenci gladyatör" veya "Beyaz gladyatör" yapýnca hepsi deðiþecek.
    public SpriteRenderer[] SkinParts;

    [Header("Kütüphane (Library)")]
    public List<Sprite> HeadSprites; // Farklý kafa tipleri
    public List<Sprite> ChestSprites; // Farklý göðüs zýrhlarý
    public List<Sprite> BellySprites; // Farklý kemer/altlýklar

    // Rastgele ten renkleri
    public Color[] SkinColors = new Color[]
    {
        new Color(1f, 0.9f, 0.8f), // Açýk Ten
        new Color(0.8f, 0.6f, 0.5f), // Buðday
        new Color(0.5f, 0.3f, 0.2f)  // Koyu Ten
    };

    void Start()
    {
        // Oyun baþlayýnca otomatik rastgele giyinsin
        RandomizeLook();
    }

    public void RandomizeLook()
    {
        // 1. Rastgele Kafa Seç
        if (HeadSprites.Count > 0 && HeadRenderer != null)
        {
            HeadRenderer.sprite = HeadSprites[Random.Range(0, HeadSprites.Count)];
        }

        // 2. Rastgele Göðüs (Zýrh)
        if (ChestSprites.Count > 0 && ChestRenderer != null)
        {
            ChestRenderer.sprite = ChestSprites[Random.Range(0, ChestSprites.Count)];
        }

        // 3. Rastgele Altlýk (Kemer)
        if (BellySprites.Count > 0 && BellyRenderer != null)
        {
            BellyRenderer.sprite = BellySprites[Random.Range(0, BellySprites.Count)];
        }

        // 4. Rastgele Ten Rengi (Tüm vücuda uygula)
        if (SkinColors.Length > 0 && SkinParts.Length > 0)
        {
            Color randomSkin = SkinColors[Random.Range(0, SkinColors.Length)];
            foreach (var part in SkinParts)
            {
                if (part != null) part.color = randomSkin;
            }
            // Kafanýn rengini de ten rengine uyduralým (Eðer kafa resmin siyah-beyaz ise)
            // Eðer kafa resimlerin zaten renkli çizildiyse bu satýrý sil:
            // if (HeadRenderer != null) HeadRenderer.color = randomSkin; 
        }
    }
}
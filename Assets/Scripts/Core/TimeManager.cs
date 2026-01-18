using UnityEngine;

public class TimeManager : MonoBehaviour
{
    // Butonlara baðlanacak fonksiyonlar
    public void SetSpeed1x() { SetTimeScale(1f); }
    public void SetSpeed2x() { SetTimeScale(2f); }
    public void SetSpeed4x() { SetTimeScale(4f); }

    void SetTimeScale(float scale)
    {
        Time.timeScale = scale;
        // KRÝTÝK NOKTA: Fizik motorunun sapýtmamasý için fixedDeltaTime da ölçeklenmeli
        // Varsayýlan 0.02f'dir.
        Time.fixedDeltaTime = 0.02f * scale;
    }
}
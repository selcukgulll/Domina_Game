using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    public void OnStartCampaign()
    {
        // Yeni oyun baþlatýrken verileri sýfýrlamak iyi olur
        // Þimdilik direkt Ludus'a atýyoruz
        SceneManager.LoadScene("Ludus");
    }

    public void OnQuit()
    {
        Application.Quit();
        Debug.Log("Game Quit");
    }
}
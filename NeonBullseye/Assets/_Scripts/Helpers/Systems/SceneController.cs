using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public void LoadGame()
    {
        SceneManager.LoadScene("LevelTest");
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void RoundOver()
    {
        SceneManager.LoadScene("RoundOver");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}

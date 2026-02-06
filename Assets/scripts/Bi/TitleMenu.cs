using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleMenu : MonoBehaviour
{
    [Header("Scene to load on Start")]
    public int nextSceneBuildIndex = 1;

    public void StartGame()
    {
        SceneManager.LoadScene(nextSceneBuildIndex);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
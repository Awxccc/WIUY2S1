using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // Only declare this once
    bool isPaused = false;
    public GameObject pauseMenu;

    private void Start()
    {
        var pauseAction = InputSystem.actions.FindAction("Pause");
        pauseAction.started += ctx =>
        {
            if(isPaused)
                ResumeGame();
            else
                PauseGame();
        };
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void PauseGame()
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
        Debug.Log("Game Paused");
    }

    public void ResumeGame()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
        Debug.Log("Game Resumed");
    }

    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif

    }
}


using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public string eegDataPath;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void StartSimulation()
    {
        SceneManager.LoadScene("EEGVisualizerScene");
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene("MainMenuScene");
    }

    public void QuitApp()
    {
        Application.Quit();
    }
}
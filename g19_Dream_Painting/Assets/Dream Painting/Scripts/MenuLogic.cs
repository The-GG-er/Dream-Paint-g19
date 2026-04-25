using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    // Analytical Note: Use the exact name of your planet scene
    public string gameSceneName = "OutdoorsScene"; 

    public void StartGame()
    {
        // This clears the menu from memory and loads the EEG environment
        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game is exiting..."); // Only visible in editor
    }
}
using UnityEngine;
using UnityEngine.SceneManagement;

public class QuitOnF12 : MonoBehaviour
{
    public string sceneToLoad = "MainMenu";   
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F12))
        {
            Debug.Log("F12 pressed - returning to MainMenu.");
            // foreach (var spawner in FindObjectsOfType<ObstacleSpawner>())
            //     spawner.CleanupObstacles();
            SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Single);
            // Application.Quit();

        // #if UNITY_EDITOR
        //     UnityEditor.EditorApplication.isPlaying = false;
        // #endif
        }
    }
}
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class MenuUI : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField ipInputField;
    public string planetSceneName = "OutdoorsScene"; // Must match your scene name exactly

    void Start()
    {
        // Fill the input box with the current or saved IP
        if (ipInputField != null)
        {
            ipInputField.text = GameSettings.serverIP;
        }
    }

    public void OnPlayClicked()
    {
        // 1. Save the user's input data first
        if (ipInputField != null)
        {
            GameSettings.serverIP = ipInputField.text;
            PlayerPrefs.SetString("SavedIP", ipInputField.text);
            PlayerPrefs.Save();
        }

        // 2. Load the game safely
        StartCoroutine(SafeLoadScene());
    }

    IEnumerator SafeLoadScene()
    {
        // This wait prevents the TMP Cull bug by letting the UI finish its frame
        yield return new WaitForEndOfFrame();
        SceneManager.LoadScene(planetSceneName);
    }

    public void OnQuitClicked()
    {
        Application.Quit();
        Debug.Log("Game Exited");
    }
}
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using System.IO;
using System;
using System.Linq;

public class MainMenuManager : MonoBehaviour{

    private void Start()
    {

    }

    public void PlayGame()
    {
       

        QualitySettings.vSyncCount = 0; // Disable VSync
        Application.targetFrameRate = 60; // Cap to 60 FPS
        Application.runInBackground = true;

        SceneManager.LoadScene("LoadingScreen");
    }

    public void QuitGame(){
        Application.Quit();
    }
}
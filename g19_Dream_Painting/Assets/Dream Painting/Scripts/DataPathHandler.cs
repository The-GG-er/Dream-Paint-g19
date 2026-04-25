using UnityEngine;
using TMPro; // Required for TextMeshPro

public class DataPathHandler : MonoBehaviour 
{
    public TMP_InputField pathInput;
    public string selectedPath;

    public void OnSubmitPath() 
    {
        selectedPath = pathInput.text;
        Debug.Log("EEG Data Source set to: " + selectedPath);
        
        // Call your EEG initialization function here
        // EEGManager.Instance.StartStream(selectedPath);
    }
}
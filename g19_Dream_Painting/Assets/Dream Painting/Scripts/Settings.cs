using UnityEngine;
using TMPro; // Required for TextMeshPro
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    public TMP_InputField ipInput;
    public TMP_InputField portInput;
    public Slider speedSlider;

    // Call this when the user clicks the "Save/Connect" button
    public void ApplySettings()
    {
        // Update the static GameSettings with user input
        GameSettings.serverIP = ipInput.text;
        
        if (int.TryParse(portInput.text, out int newPort))
        {
            GameSettings.portManagerPort = newPort;
        }

        // Find the visualizer in the scene and update its speed
        PlanetEEGVisualizer visualizer = FindFirstObjectByType<PlanetEEGVisualizer>();
        if (visualizer != null)
        {
            visualizer.rotationSpeed = speedSlider.value;
        }

        Debug.Log($"Settings Applied: {GameSettings.serverIP}:{GameSettings.portManagerPort}");
    }
}
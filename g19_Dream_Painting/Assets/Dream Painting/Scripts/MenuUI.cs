using UnityEngine;
using UnityEngine.UI;

public class MenuUI : MonoBehaviour
{
    public InputField pathInput;
    public Slider speedSlider;

    public void StartSimulation()
    {
        SettingsManager.Instance.dataPath = pathInput.text;
        SettingsManager.Instance.updateSpeed = speedSlider.value;

        GameManager.Instance.StartSimulation();
    }
}
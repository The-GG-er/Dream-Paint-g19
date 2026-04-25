using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public GameObject eegCanvas; // Drag your Canvas here in the Inspector

    void Awake()
    {
        // Awake runs before Start, ensuring it's visible instantly
        if (eegCanvas != null)
        {
            eegCanvas.SetActive(true);
        }
    }
}
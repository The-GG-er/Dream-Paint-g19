using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

    [Header("EEG Settings")]
    public string dataPath = "";
    public float updateSpeed = 0.3f;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
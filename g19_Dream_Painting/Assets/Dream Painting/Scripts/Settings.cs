using UnityEngine;

public class Settings : MonoBehaviour
{
    // These are static so they stay in memory across scenes
    public static string serverIP = "127.0.0.1";
    public static float updateSpeed = 0.1f;

    // PSD Data placeholders for the Visualizer
    public static float[,] psd;
    public static int psdFreqs;
    public static int psdChannels;
    public static object psdLock = new object();

    void Awake()
    {
        // Keep this object alive if it's placed in the scene
        DontDestroyOnLoad(gameObject);
        
        // Load last saved IP if it exists
        if (PlayerPrefs.HasKey("SavedIP"))
        {
            serverIP = PlayerPrefs.GetString("SavedIP");
        }
    }
}
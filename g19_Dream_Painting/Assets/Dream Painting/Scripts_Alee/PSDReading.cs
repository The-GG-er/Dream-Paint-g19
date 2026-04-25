using UnityEngine;

public class psdReading : MonoBehaviour
{
    void Update()
    {
        float[,] psdCopy;
        int freqs;
        int channels;

        lock (GameSettings.psdLock)
        {
            if (GameSettings.psd == null)
                return;

            freqs = GameSettings.psdFreqs;
            channels = GameSettings.psdChannels;

            psdCopy = (float[,])GameSettings.psd.Clone();
        }

        // // Example: alpha at channel 0
        // if (freqs > 8)
        // {
        //     float alphaFz = psdCopy[8, 0];
        //     // Debug.Log("Alpha Fz: " + alphaFz);
        // }

        // // Example: mean power at frequency 10
        // if (freqs > 10)
        // {
        //     float sum = 0f;

        //     for (int ch = 0; ch < channels; ch++)
        //         sum += psdCopy[10, ch];

        //     float mean = sum / channels;

        //     // Debug.Log(mean);
        // }
    }
}
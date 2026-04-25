using System.Collections;
using UnityEngine;

public class PlanetEEGVisualizer : MonoBehaviour
{
    [Header("Planet Settings")]
    public float rotationSpeed = 20f;

    [Header("Material")]
    public Material planetMaterial;

    [Header("Resolution")]
    public int segmentCount = 180; // how many slices (2° each)

    private Color[] segmentColors;
    private float currentAngle = 0f;

    void Start()
    {
        segmentColors = new Color[segmentCount];

        // Initialize all to black
        for (int i = 0; i < segmentCount; i++)
            segmentColors[i] = Color.black;

        StartCoroutine(LivePlayback());
    }

    IEnumerator LivePlayback()
    {
        while (true)
        {
            DataPoint d = GetCurrentDataFromPSD();

            if (d != null)
            {
                UpdatePlanet(d);
            }

            yield return null;
        }
    }

    void UpdatePlanet(DataPoint d)
    {
        // Rotate continuously (time = rotation)
        float step = rotationSpeed * Time.deltaTime;
        transform.Rotate(Vector3.up, step);
        currentAngle += step;

        if (currentAngle >= 360f)
            currentAngle -= 360f;

        // Convert angle → segment index
        int index = Mathf.FloorToInt((currentAngle / 360f) * segmentCount);

        // Determine dominant band
        string band = GetDominantBand(d);

        // Get color for this band
        Color bandColor = GetBandColor(band);

        // OVERWRITE ONLY THIS SEGMENT
        segmentColors[index] = bandColor;

        // Send entire array to shader
        planetMaterial.SetColorArray("_SegmentColors", segmentColors);

        // Extra shader controls
        planetMaterial.SetFloat("_CrackAmount", d.beta);
    }

    DataPoint GetCurrentDataFromPSD()
    {
        float[,] psdCopy;
        int freqs;
        int channels;

        lock (GameSettings.psdLock)
        {
            if (GameSettings.psd == null)
                return null;

            freqs = GameSettings.psdFreqs;
            channels = GameSettings.psdChannels;

            psdCopy = (float[,])GameSettings.psd.Clone();
        }

        float alpha = 0f, beta = 0f, gamma = 0f, delta = 0f;

        for (int f = 0; f < freqs; f++)
        {
            float power = 0f;

            for (int ch = 0; ch < channels; ch++)
                power += psdCopy[f, ch];

            power /= channels;

            float freqHz = f; // replace if real mapping exists

            if (freqHz >= 0.5f && freqHz < 4f)
                delta += power;
            else if (freqHz >= 8f && freqHz < 12f)
                alpha += power;
            else if (freqHz >= 13f && freqHz < 30f)
                beta += power;
            else if (freqHz >= 30f)
                gamma += power;
        }

        float total = alpha + beta + gamma + delta + 0.0001f;

        return new DataPoint
        {
            alpha = alpha / total,
            beta = beta / total,
            gamma = gamma / total,
            delta = delta / total
        };
    }

    string GetDominantBand(DataPoint d)
    {
        float max = Mathf.Max(d.alpha, d.beta, d.gamma, d.delta);

        if (max == d.alpha) return "alpha";
        if (max == d.beta) return "beta";
        if (max == d.gamma) return "gamma";
        return "delta";
    }

    Color GetBandColor(string band)
    {
        switch (band)
        {
            case "alpha": return new Color(0.1f, 0.3f, 1f);   // blue
            case "beta":  return new Color(1f, 0.2f, 0.1f);   // red
            case "gamma": return new Color(0.2f, 1f, 0.3f);   // green
            case "delta": return new Color(0.9f, 0.9f, 0.9f); // white
            default: return Color.black;
        }
    }

    // --- Prefabs intentionally removed for shader-only approach ---
    /*
    public GameObject waterPrefab;
    public GameObject firePrefab;
    public GameObject forestPrefab;
    public GameObject cloudPrefab;
    */
}
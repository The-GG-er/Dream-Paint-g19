using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DataPoint
{
    public float alpha;
    public float beta;
    public float gamma;
    public float delta;
}

public class PlanetEEGVisualizer : MonoBehaviour
{
    [Header("Planet Settings")]
    public float rotationSpeed = 20f;
    public Material planetMaterial;
    public int segmentCount = 180;

    [Header("Moons")]
    public GameObject alphaMoonPrefab;
    public GameObject betaMoonPrefab;
    public GameObject gammaMoonPrefab;
    public GameObject deltaMoonPrefab;

    [Header("Orbit Settings")]
    public float spawnDistance = 18f;
    public float orbitRadius = 8f;
    public float orbitSpeed = 25f;
    public float spawnThreshold = 0.55f;
    public int maxMoons = 12;

    private Color[] segmentColors;
    private Texture2D colorMap;
    private float currentAngle = 0f;

    private List<GameObject> activeMoons = new List<GameObject>();

    void Start()
    {
        // Initialize circular color buffer
        segmentColors = new Color[segmentCount];
        for (int i = 0; i < segmentCount; i++)
            segmentColors[i] = Color.black;

        // Create texture (1D strip)
        colorMap = new Texture2D(segmentCount, 1);
        colorMap.wrapMode = TextureWrapMode.Repeat;
        colorMap.filterMode = FilterMode.Point;

        if (planetMaterial != null)
            planetMaterial.SetTexture("_ColorMap", colorMap);

        StartCoroutine(LiveLoop());
    }

    IEnumerator LiveLoop()
    {
        while (true)
        {
            DataPoint d = GetCurrentDataFromPSD();

            if (d != null)
            {
                UpdatePlanet(d);
                HandleMoons(d);
            }

            yield return null;
        }
    }

    void UpdatePlanet(DataPoint d)
    {
        float step = rotationSpeed * Time.deltaTime;
        transform.Rotate(Vector3.up, step);

        currentAngle += step;
        if (currentAngle >= 360f) currentAngle -= 360f;

        int index = Mathf.FloorToInt((currentAngle / 360f) * segmentCount);
        index = Mathf.Clamp(index, 0, segmentCount - 1);

        // Overwrite ONLY current segment (looping memory effect)
        string dominant = GetDominantBand(d);
        segmentColors[index] = GetBandColor(dominant);

        colorMap.SetPixels(segmentColors);
        colorMap.Apply();

        // Shader parameters
        planetMaterial.SetFloat("_CrackAmount", d.beta);
        planetMaterial.SetFloat("_EmissionStrength", d.gamma * 2f);
    }

    void HandleMoons(DataPoint d)
    {
        if (Time.frameCount % 120 != 0) return;

        float maxVal = Mathf.Max(d.alpha, d.beta, d.gamma, d.delta);
        if (maxVal < spawnThreshold) return;

        if (activeMoons.Count >= maxMoons)
        {
            Destroy(activeMoons[0]);
            activeMoons.RemoveAt(0);
        }

        SpawnMoon(GetDominantBand(d));
    }

    void SpawnMoon(string band)
    {
        GameObject prefab = null;

        switch (band)
        {
            case "alpha": prefab = alphaMoonPrefab; break;
            case "beta": prefab = betaMoonPrefab; break;
            case "gamma": prefab = gammaMoonPrefab; break;
            case "delta": prefab = deltaMoonPrefab; break;
        }

        if (prefab == null) return;

        Vector3 spawnPos = transform.position + Random.onUnitSphere * spawnDistance;
        GameObject moon = Instantiate(prefab, spawnPos, Quaternion.identity);

        activeMoons.Add(moon);

        MoonOrbit orbit = moon.GetComponent<MoonOrbit>();
        if (orbit != null)
        {
            orbit.planet = transform;
            orbit.orbitRadius = orbitRadius + Random.Range(-1.5f, 1.5f);
            orbit.orbitSpeed = orbitSpeed * Random.Range(0.8f, 1.2f);
            orbit.ActivateMoon();
        }

        Destroy(moon, 40f);
    }

    DataPoint GetCurrentDataFromPSD()
    {
        float[,] psdCopy;
        int freqs, channels;

        lock (GameSettings.psdLock)
        {
            if (GameSettings.psd == null || GameSettings.psdFreqs <= 0)
                return null;

            freqs = GameSettings.psdFreqs;
            channels = GameSettings.psdChannels;
            psdCopy = (float[,])GameSettings.psd.Clone();
        }

        float alpha = 0, beta = 0, gamma = 0, delta = 0;

        for (int f = 0; f < freqs; f++)
        {
            float power = 0;

            for (int ch = 0; ch < channels; ch++)
                power += psdCopy[f, ch];

            power /= channels;

            if (f >= 1 && f < 4) delta += power;
            else if (f >= 8 && f < 12) alpha += power;
            else if (f >= 13 && f < 30) beta += power;
            else if (f >= 30) gamma += power;
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
            case "alpha": return new Color(0.1f, 0.4f, 1f);   // blue
            case "beta": return new Color(1f, 0.2f, 0.1f);    // red
            case "gamma": return new Color(0.3f, 1f, 0.5f);   // green
            case "delta": return new Color(0.95f, 0.95f, 1f); // white
            default: return Color.black;
        }
    }
}
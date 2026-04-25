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

    [Header("Moon Prefabs")]
    public GameObject alphaMoonPrefab;
    public GameObject betaMoonPrefab;
    public GameObject gammaMoonPrefab;
    public GameObject deltaMoonPrefab;

    [Header("Orbit Config")]
    public float spawnDistance = 18f; 
    public float moonOrbitRadius = 8f;
    public float moonOrbitSpeed = 25f;
    [Range(0.1f, 1f)] public float spawnThreshold = 0.55f;
    public int maxMoons = 12;

    private Color[] segmentColors;
    private Texture2D colorMapTexture; 
    private float currentAngle = 0f;
    private List<GameObject> activeMoons = new List<GameObject>();

    void Start()
    {
        // Initialize Texture for Shader Graph
        segmentColors = new Color[segmentCount];
        for (int i = 0; i < segmentCount; i++)
            segmentColors[i] = Color.black;

        colorMapTexture = new Texture2D(segmentCount, 1);
        colorMapTexture.wrapMode = TextureWrapMode.Repeat;
        colorMapTexture.filterMode = FilterMode.Point;

        if (planetMaterial == null)
            Debug.LogError("Assign the Map_Shader material in the Inspector!");

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
                CheckForMoonSpawn(d);
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

        string dominant = GetDominantBand(d);
        segmentColors[index] = GetBandColor(dominant);

        colorMapTexture.SetPixels(segmentColors);
        colorMapTexture.Apply();

        if (planetMaterial != null)
        {
            planetMaterial.SetTexture("_ColorMap", colorMapTexture); 
            planetMaterial.SetFloat("_CrackAmount", d.beta);
            planetMaterial.SetFloat("_EmissionStrength", d.gamma * 2.5f);
        }
    }

    void CheckForMoonSpawn(DataPoint d)
    {
        if (Time.frameCount % 100 == 0) // Check every ~1.5 seconds
        {
            float maxVal = Mathf.Max(d.alpha, d.beta, d.gamma, d.delta);
            
            if (maxVal > spawnThreshold)
            {
                if (activeMoons.Count >= maxMoons)
                {
                    GameObject oldest = activeMoons[0];
                    activeMoons.RemoveAt(0);
                    Destroy(oldest);
                }
                SpawnMoon(GetDominantBand(d));
            }
        }
    }

    void SpawnMoon(string band)
    {
        GameObject prefabToSpawn = null;

        // Select the correct prefab based on the brain state
        switch (band)
        {
            case "alpha": prefabToSpawn = alphaMoonPrefab; break;
            case "beta":  prefabToSpawn = betaMoonPrefab;  break;
            case "gamma": prefabToSpawn = gammaMoonPrefab; break;
            case "delta": prefabToSpawn = deltaMoonPrefab; break;
        }

        if (prefabToSpawn == null) return;

        Vector3 spawnPos = transform.position + (Random.onUnitSphere * spawnDistance);
        GameObject moon = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
        activeMoons.Add(moon);
        
        // Setup the MoonOrbit script on the spawned object
        MoonOrbit orbitScript = moon.GetComponent<MoonOrbit>();
        if (orbitScript != null)
        {
            orbitScript.planet = this.transform;
            orbitScript.orbitRadius = moonOrbitRadius + Random.Range(-1.5f, 1.5f);
            orbitScript.orbitSpeed = moonOrbitSpeed * Random.Range(0.8f, 1.3f);
            orbitScript.ActivateMoon();
        }

        Destroy(moon, 40f); // Clean up to keep the build running smooth
    }

    DataPoint GetCurrentDataFromPSD()
    {
        float[,] psdCopy;
        int freqs, channels;

        lock (GameSettings.psdLock)
        {
            if (GameSettings.psd == null || GameSettings.psdFreqs <= 0) return null;
            freqs = GameSettings.psdFreqs;
            channels = GameSettings.psdChannels;
            psdCopy = (float[,])GameSettings.psd.Clone();
        }

        float alpha = 0, beta = 0, gamma = 0, delta = 0;
        for (int f = 0; f < freqs; f++)
        {
            float power = 0;
            for (int ch = 0; ch < channels; ch++) power += psdCopy[f, ch];
            power /= channels;

            if (f >= 1 && f < 4) delta += power;
            else if (f >= 8 && f < 12) alpha += power;
            else if (f >= 13 && f < 30) beta += power;
            else if (f >= 30) gamma += power;
        }

        float total = alpha + beta + gamma + delta + 0.0001f;
        return new DataPoint {
            alpha = alpha / total, beta = beta / total,
            gamma = gamma / total, delta = delta / total
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
            case "alpha": return new Color(0.1f, 0.4f, 1.0f); 
            case "beta":  return new Color(1.0f, 0.2f, 0.1f); 
            case "gamma": return new Color(0.3f, 1.0f, 0.5f); 
            case "delta": return new Color(0.95f, 0.95f, 1.0f); // Airy White
            default: return Color.black;
        }
    }
}
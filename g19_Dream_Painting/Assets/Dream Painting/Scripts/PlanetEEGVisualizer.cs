using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class DataPoint
{
    public float time;
    public float alpha;
    public float beta;
    public float gamma;
    public float delta;
}

[System.Serializable]
public class DataWrapper
{
    public List<DataPoint> data;
}

public class PlanetEEGVisualizer : MonoBehaviour
{
    [Header("EEG Data")]
    public string fileName = "eeg_data.json";
    private List<DataPoint> dataPoints;
    private int currentIndex = 0;

    [Header("Planet Settings")]
    public float radius = 5f;

    [Header("Material")]
    public Material planetMaterial;
    private float reveal = 0f;

    [Header("Moons")]
    public MoonOrbit alphaMoon;
    public MoonOrbit betaMoon;
    public MoonOrbit gammaMoon;
    public MoonOrbit deltaMoon;

    private bool alphaActivated = false;
    private bool betaActivated = false;
    private bool gammaActivated = false;
    private bool deltaActivated = false;

    [Header("Prefabs")]
    public GameObject waterPrefab;
    public GameObject firePrefab;
    public GameObject forestPrefab;
    public GameObject cloudPrefab;

    [Header("Playback")]
    public float stepDelay = 0.2f;

    private float currentAngle = 0f;
    private float rotationPerStep;

    private bool playbackFinished = false;

    void Start()
    {
        LoadData();

        if (dataPoints == null || dataPoints.Count == 0)
        {
            Debug.LogError("No EEG data loaded!");
            return;
        }

        rotationPerStep = 360f / dataPoints.Count;

        StartCoroutine(Playback());
    }

    void LoadData()
    {
        string path = Path.Combine(Application.streamingAssetsPath, fileName);

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            string wrapped = "{ \"data\": " + json + "}";

            DataWrapper wrapper = JsonUtility.FromJson<DataWrapper>(wrapped);
            dataPoints = wrapper.data;
        }
        else
        {
            Debug.LogError("File not found: " + path);
        }
    }

    IEnumerator Playback()
    {
        while (currentIndex < dataPoints.Count)
        {
            DataPoint d = dataPoints[currentIndex];

            // Rotate planet
            transform.Rotate(Vector3.up, rotationPerStep);
            currentAngle += rotationPerStep;

            // Determine dominant band
            string band = GetDominantBand(d);

            // Activate moons
            HandleMoonActivation(band);

            // Spawn terrain features
            SpawnFeature(band);

            // Update reveal shader
            reveal = (float)currentIndex / dataPoints.Count;
            planetMaterial.SetFloat("_Reveal", reveal);

            currentIndex++;
            yield return new WaitForSeconds(stepDelay);
        }

        playbackFinished = true;
        Debug.Log("Playback finished.");
    }

    string GetDominantBand(DataPoint d)
    {
        float max = Mathf.Max(d.alpha, d.beta, d.gamma, d.delta);

        if (max == d.alpha) return "alpha";
        if (max == d.beta) return "beta";
        if (max == d.gamma) return "gamma";
        return "delta";
    }

    void HandleMoonActivation(string band)
    {
        if (band == "alpha" && !alphaActivated)
        {
            alphaMoon.ActivateMoon();
            alphaActivated = true;
        }

        if (band == "beta" && !betaActivated)
        {
            betaMoon.ActivateMoon();
            betaActivated = true;
        }

        if (band == "gamma" && !gammaActivated)
        {
            gammaMoon.ActivateMoon();
            gammaActivated = true;
        }

        if (band == "delta" && !deltaActivated)
        {
            deltaMoon.ActivateMoon();
            deltaActivated = true;
        }
    }

    void SpawnFeature(string band)
    {
        Vector3 direction = Quaternion.Euler(0, currentAngle, 0) * Vector3.forward;
        Vector3 position = transform.position + direction * radius;

        GameObject prefab = null;

        switch (band)
        {
            case "alpha":
                prefab = waterPrefab;
                break;
            case "beta":
                prefab = firePrefab;
                break;
            case "gamma":
                prefab = forestPrefab;
                break;
            case "delta":
                prefab = cloudPrefab;
                position += direction * 1.5f;
                break;
        }

        if (prefab != null)
        {
            GameObject obj = Instantiate(prefab, position, Quaternion.identity);
            obj.transform.up = direction;
            obj.transform.parent = this.transform;

            float scale = Random.Range(0.5f, 1.5f);
            obj.transform.localScale *= scale;
        }
    }

    void Update()
    {
        if (playbackFinished)
        {
            float rotX = Input.GetAxis("Mouse X") * 100f * Time.deltaTime;
            float rotY = Input.GetAxis("Mouse Y") * 100f * Time.deltaTime;

            transform.Rotate(Vector3.up, -rotX, Space.World);
            transform.Rotate(Vector3.right, rotY, Space.World);
        }
    }
}
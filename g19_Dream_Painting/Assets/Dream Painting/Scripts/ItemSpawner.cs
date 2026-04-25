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
public Material planetMaterial;
private float reveal = 0f;

public class PlanetEEGVisualizer : MonoBehaviour
{
    [Header("EEG Data")]
    public string fileName = "eeg_data.json";
    private List<DataPoint> dataPoints;
    private int currentIndex = 0;

    [Header("Planet Settings")]
    public float radius = 5f;
    public float rotationSpeed = 10f;

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

            // Wrap manually because Unity JsonUtility doesn't support top-level arrays
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

            // Spawn feature
            SpawnFeature(band);

            currentIndex++;
            yield return new WaitForSeconds(stepDelay);

            reveal = (float)currentIndex / dataPoints.Count;
            planetMaterial.SetFloat("_Reveal", reveal);
        }

        playbackFinished = true;
        Debug.Log("Playback finished. You can now explore the planet.");
    }

    string GetDominantBand(DataPoint d)
    {
        float max = Mathf.Max(d.alpha, d.beta, d.gamma, d.delta);

        if (max == d.alpha) return "alpha";
        if (max == d.beta) return "beta";
        if (max == d.gamma) return "gamma";
        return "delta";
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
                position += direction * 1.5f; // lift clouds above surface
                break;
        }

        if (prefab != null)
        {
            GameObject obj = Instantiate(prefab, position, Quaternion.identity);

            // Make it stick to surface
            obj.transform.up = direction;

            // Parent to planet so it rotates with it
            obj.transform.parent = this.transform;

            // Optional random scaling
            float scale = Random.Range(0.5f, 1.5f);
            obj.transform.localScale *= scale;
        }
    }

    void Update()
    {
        // Allow manual rotation after playback
        if (playbackFinished)
        {
            float rotX = Input.GetAxis("Mouse X") * 100f * Time.deltaTime;
            float rotY = Input.GetAxis("Mouse Y") * 100f * Time.deltaTime;

            transform.Rotate(Vector3.up, -rotX, Space.World);
            transform.Rotate(Vector3.right, rotY, Space.World);
        }
    }
}
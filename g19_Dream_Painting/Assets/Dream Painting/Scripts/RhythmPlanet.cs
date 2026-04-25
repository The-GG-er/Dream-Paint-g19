using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class RhythmPlanet : MonoBehaviour
{
    public GameObject planet;

    public Material oceanMat;
    public Material volcanoMat;
    public Material landMat;
    public Material atmosphereMat;

    public float rotationSpeed = 10f;

    private List<Vector4> rhythmData = new List<Vector4>();
    private int index = 0;

    void Start()
    {
        LoadRhythmData();
    }

    void LoadRhythmData()
    {
        string path = Application.dataPath + "/Data/unity_rhythm_data.csv";
        string[] lines = File.ReadAllLines(path);

        foreach (string line in lines)
        {
            if (line.Contains("alpha")) continue;

            string[] p = line.Split(',');

            float a = float.Parse(p[0]);
            float b = float.Parse(p[1]);
            float c = float.Parse(p[2]);
            float d = float.Parse(p[3]);

            rhythmData.Add(new Vector4(a,b,c,d));
        }
    }

    void Update()
    {
        if (rhythmData.Count == 0) return;
        if (index >= rhythmData.Count) return;

        Vector4 r = rhythmData[index];

        float alpha = r.x;
        float beta  = r.y;
        float theta = r.z;
        float delta = r.w;

        ApplyRhythms(alpha, beta, theta, delta);

        planet.transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);

        index++;
    }

    void ApplyRhythms(float a, float b, float t, float d)
    {
        // 🌊 OCEAN (alpha)
        oceanMat.SetFloat("_Intensity", a);

        // 🌋 VOLCANO (beta)
        volcanoMat.SetFloat("_Eruption", b);

        // 🌍 LAND (theta)
        landMat.SetFloat("_Growth", t);

        // 🌫 ATMOSPHERE (delta)
        atmosphereMat.SetFloat("_Density", d);
    }
}
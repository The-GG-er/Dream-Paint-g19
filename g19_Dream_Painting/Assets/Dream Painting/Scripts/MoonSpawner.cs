using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MoonSpawner : MonoBehaviour
{
    public GameObject moonPrefab;
    public Transform planet;

    private class MoonEvent
    {
        public int time;
        public int type;
        public float intensity;
    }

    private List<MoonEvent> events = new List<MoonEvent>();
    private int index = 0;
    private List<GameObject> moons = new List<GameObject>();

    void Start()
    {
        LoadEvents();
    }

    void LoadEvents()
    {
        string path = Application.dataPath + "/Data/unity_moon_events.csv";
        string[] lines = File.ReadAllLines(path);

        foreach (string line in lines)
        {
            if (line.Contains("time")) continue;

            string[] p = line.Split(',');

            MoonEvent e = new MoonEvent();
            e.time = int.Parse(p[0]);
            e.type = int.Parse(p[1]);
            e.intensity = float.Parse(p[2]);

            events.Add(e);
        }
    }

    void Update()
    {
        if (index >= events.Count) return;

        MoonEvent e = events[index];

        // spawn at correct time
        if (Time.frameCount >= e.time)
        {
            SpawnMoon(e);
            index++;
        }
    }

    void SpawnMoon(MoonEvent e)
    {
        Vector3 spawnDir = Random.onUnitSphere * 8f;
        Vector3 pos = planet.position + spawnDir;

        GameObject moon = Instantiate(moonPrefab, pos, Quaternion.identity);

        Moon m = moon.AddComponent<Moon>();
        m.planet = planet;
        m.type = e.type;
        m.speed = 2f + e.intensity * 5f;

        moons.Add(moon);
    }
}
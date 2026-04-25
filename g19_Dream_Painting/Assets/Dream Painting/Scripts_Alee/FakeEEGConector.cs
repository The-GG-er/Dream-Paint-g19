using UnityEngine;
using System;
using System.Net.Sockets;
using System.Threading;

public class FakeEEGConnector : MonoBehaviour
{
    [Header("Connection Settings")]
    public string serverIP = "127.0.0.1";
    public int port = 5005;

    [Header("PSD Settings")]
    public int frequencies = 64;   // rows
    public int channels = 8;       // columns
    public float sendInterval = 0.1f; // seconds

    private Thread sendThread;
    private bool running = true;

    void Start()
    {
        sendThread = new Thread(SendFakePSD);
        sendThread.IsBackground = true;
        sendThread.Start();
    }

    void SendFakePSD()
    {
        try
        {
            TcpClient client = new TcpClient();
            client.Connect(serverIP, port);

            NetworkStream stream = client.GetStream();

            System.Random rand = new System.Random();

            Debug.Log("[FakeEEG] Connected to server.");

            while (running)
            {
                // Create fake PSD: [frequencies x channels]
                float[] psdFlat = new float[frequencies * channels];

                for (int i = 0; i < psdFlat.Length; i++)
                {
                    psdFlat[i] = (float)rand.NextDouble();
                }

                // Convert float[] → byte[]
                byte[] bytes = new byte[psdFlat.Length * sizeof(float)];
                Buffer.BlockCopy(psdFlat, 0, bytes, 0, bytes.Length);

                // Send to Unity receiver
                stream.Write(bytes, 0, bytes.Length);

                Thread.Sleep((int)(sendInterval * 1000f));
            }

            stream.Close();
            client.Close();
        }
        catch (Exception e)
        {
            Debug.LogError("[FakeEEG] Error: " + e.Message);
        }
    }

    void OnApplicationQuit()
    {
        running = false;

        if (sendThread != null && sendThread.IsAlive)
            sendThread.Join();

        Debug.Log("[FakeEEG] Stopped.");
    }
}
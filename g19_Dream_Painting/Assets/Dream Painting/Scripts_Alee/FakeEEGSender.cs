using UnityEngine;
using System.Net.Sockets;
using System.Threading;
using System;

public class FakeEEGSender : MonoBehaviour
{
    [Header("Connection")]
    public string serverIP = "127.0.0.1";
    public int port = 5005;

    [Header("PSD Settings")]
    public int nFrequencies = 64;
    public int nChannels = 8;
    public float sendInterval = 0.1f;

    private TcpClient client;
    private NetworkStream stream;
    private Thread sendThread;
    private bool isRunning = false;

    void Start()
    {
        Connect();
    }

    void Connect()
    {
        try
        {
            client = new TcpClient(serverIP, port);
            stream = client.GetStream();

            Debug.Log("Fake EEG Sender Connected to Unity Receiver");

            isRunning = true;
            sendThread = new Thread(SendLoop);
            sendThread.IsBackground = true;
            sendThread.Start();
        }
        catch (Exception e)
        {
            Debug.LogError("Connection failed: " + e.Message);
        }
    }

    void SendLoop()
    {
        System.Random rand = new System.Random();

        while (isRunning)
        {
            try
            {
                int totalValues = nFrequencies * nChannels;
                float[] psdFlat = new float[totalValues];

                for (int i = 0; i < totalValues; i++)
                {
                    psdFlat[i] = (float)rand.NextDouble();
                }

                byte[] bytes = new byte[psdFlat.Length * sizeof(float)];
                Buffer.BlockCopy(psdFlat, 0, bytes, 0, bytes.Length);

                stream.Write(bytes, 0, bytes.Length);

                Thread.Sleep((int)(sendInterval * 1000f));
            }
            catch (Exception e)
            {
                Debug.LogError("Send error: " + e.Message);
                isRunning = false;
            }
        }
    }

    void OnApplicationQuit()
    {
        isRunning = false;

        if (sendThread != null && sendThread.IsAlive)
            sendThread.Join();

        stream?.Close();
        client?.Close();

        Debug.Log("🔌 Fake EEG Sender stopped.");
    }
}
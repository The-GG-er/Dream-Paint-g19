using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;
using System.Collections;
using System.Threading.Tasks;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;



public class UnityNetworkManager : MonoBehaviour
{
    private readonly string serverIP = GameSettings.serverIP;
    private volatile bool isRunning = true;
    public static UnityNetworkManager Instance;
    private Thread udpListenThread;


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Prevent duplicates
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }


    // ============ UDP ============


    public IEnumerator WaitForUDPConnection(UdpClient udpClient, System.Action<bool> onComplete, float timeoutSeconds = 10f)
    {
        ManualResetEvent receivedEvent = new ManualResetEvent(false);
        IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);

        byte[] ping = BuildTimestampedMessage("PING");
        udpClient.Send(ping, ping.Length);

        Thread udpWaitThread = new Thread(() =>
        {
            try
            {
                byte[] data = udpClient.Receive(ref remoteEP);
                int tsLen = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(data, 0));
                string timestamp = Encoding.UTF8.GetString(data, 4, tsLen);
                int msgLen = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(data, 4 + tsLen));
                string msg = Encoding.UTF8.GetString(data, 8 + tsLen, msgLen);

                if (msg.Trim() == "PONG")
                {
                    receivedEvent.Set();
                }
            }
            catch (Exception e)
            {
                Debug.LogError("[UDP-WAIT] Thread error: " + e.Message);
            }
        });

        udpWaitThread.IsBackground = true;
        udpWaitThread.Start();

        float elapsed = 0f;
        while (!receivedEvent.WaitOne(0) && elapsed < timeoutSeconds)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        onComplete?.Invoke(receivedEvent.WaitOne(0));
    }



    public void SendUDPMessage(string message, UdpClient udpClient)
    {
        byte[] fullMessage = BuildTimestampedMessage(message);
        udpClient.Send(fullMessage, fullMessage.Length);
        // Debug.Log($"[UDP] Sent: {message} to port ");
    }

    public void ReceiveUDPMessage(int listenPort)
    {
        udpListenThread = new Thread(() =>
        {
            using (UdpClient listener = new UdpClient(listenPort))
            {
                IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                Debug.Log($"[UDP-RECV] Listening on port {listenPort}");

                try
                {
                    while (isRunning)
                    {
                        byte[] data = listener.Receive(ref remoteEndPoint);
                        ParseAndPrintMessage(data, "[UDP-RECV]");
                    }
                }
                catch (SocketException e)
                {
                    if (isRunning)
                        Debug.LogError("[UDP-RECV] Socket error: " + e.Message);
                }
                catch (Exception e)
                {
                    Debug.LogError("[UDP-RECV] General error: " + e.Message);
                }
                finally
                {
                    listener.Close();
                    Debug.Log($"[UDP-RECV] Listener on port {listenPort} closed.");
                }
            }
        });

        udpListenThread.IsBackground = true;
        udpListenThread.Start();
    }

    public IEnumerator WaitForUDPReply(UdpClient client, System.Action<string> onMessageReceived)
    {
        bool received = false;

        Thread receiveThread = new Thread(() =>
        {
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
            try
            {
                byte[] data = client.Receive(ref remoteEP);
                int tsLen = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(data, 0));
                string timestamp = Encoding.UTF8.GetString(data, 4, tsLen);
                int msgLen = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(data, 4 + tsLen));
                string msg = Encoding.UTF8.GetString(data, 8 + tsLen, msgLen);

                // Debug.Log($"[PortManager] Received at {timestamp}: {msg}");
                onMessageReceived?.Invoke(msg);
                received = true;
            }
            catch (Exception e)
            {
                Debug.LogError("Error receiving UDP response: " + e.Message);
            }
        });

        receiveThread.IsBackground = true;
        receiveThread.Start();

        while (!received)
            yield return null;
    }


    // public void StartUDPListenerForPercPosX(UdpClient client)
    // {
    //     udpListenThread = new(() =>
    //     {
    //         IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
    //         SendUDPMessage("ADD_ME", client);
    //         while (isRunning)
    //         {
    //             // Debug.Log("[UDP Listener] Waiting for messages...");
    //             try
    //             {
    //                 byte[] data = client.Receive(ref remoteEndPoint);
    //                 int tsLen = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(data, 0));
    //                 string timestamp = Encoding.UTF8.GetString(data, 4, tsLen);
    //                 int msgLen = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(data, 4 + tsLen));
    //                 string msg = Encoding.UTF8.GetString(data, 8 + tsLen, msgLen);

    //                 if (float.TryParse(msg, NumberStyles.Float, CultureInfo.InvariantCulture, out float newPercX))
    //                 {
    //                     // Debug.Log($"[UDP Listener] Received new percX: {newPercX}");
    //                     GameSettings.percPosX = newPercX;
    //                 }

    //             }
    //             catch (Exception ex)
    //             {
    //                 Debug.LogError($"[UDP Listener] Error: {ex.Message}");
    //             }
    //         }
    //     })
    //     {
    //         IsBackground = true
    //     };
    //     udpListenThread.Start();
    // }

    public void CloseUDPListener()
    {
        isRunning = false;
        GameSettings.psdClientTCP?.Close();  // This unblocks Receive
        if (udpListenThread != null && udpListenThread.IsAlive)
        {
            udpListenThread.Join(); // Clean exit instead of Abort
            Debug.Log("[UDP-RECV] Listener thread stopped.");
        }
    }

    



    // ============ TCP ============
    public IEnumerator WaitForTCPConnection(TcpClient client, int port, System.Action<bool> onComplete, float timeoutSeconds = 10f, string serverIP = null)
    {
        serverIP ??= this.serverIP;

        // if (client == null || !client.Connected)    client = new TcpClient();


        if (port <= 0)
        {
            Debug.LogError("[TCP-CONNECT] Invalid port number.");
            onComplete?.Invoke(false);
            yield break;
        }


        float elapsed = 0f;
        bool connected = false;

        while (elapsed < timeoutSeconds && !connected)
        {
            Task connectTask;

            try
            {
                connectTask = client.ConnectAsync(serverIP, port);
            }
            catch (Exception e)
            {
                Debug.LogError("[TCP-CONNECT] Exception starting connection: " + e.Message);
                yield break;
            }

            float attemptTimeout = 0.5f;
            float attemptElapsed = 0f;
            while (!connectTask.IsCompleted && attemptElapsed < attemptTimeout)
            {
                attemptElapsed += Time.deltaTime;
                yield return null;
            }

            if (connectTask.IsCompleted && client.Connected)
            {
                // Debug.Log("[TCP-CONNECT] Connected successfully.");
                onComplete?.Invoke(true);
                yield break;
            }
            else
            {
                // Debug.LogWarning("[TCP-CONNECT] Connect attempt timed out or failed, retrying...");
                client.Close();
                client = new TcpClient();
            }
            elapsed += attemptTimeout;
        }
        onComplete?.Invoke(false);
        // if (!connected)
        //     Debug.LogError("[TCP-CONNECT] Failed to connect within timeout.");
    }


    public IEnumerator WaitForTCPReply(TcpClient client, Action<string> onMessageReceived)
    {
        bool received = false;

        Thread receiveThread = new Thread(() =>
        {
            try
            {
                NetworkStream stream = client.GetStream();
                string timestamp = ReadUTF8String(stream);
                string message = ReadUTF8String(stream);

                Debug.Log($"[TCP-REPLY] Received at {timestamp}: {message}");
                onMessageReceived?.Invoke(message);
                received = true;
            }
            catch (Exception e)
            {
                Debug.LogError("[TCP-REPLY] Error: " + e.Message);
            }
        });

        receiveThread.IsBackground = true;
        receiveThread.Start();

        while (!received)
            yield return null;
    }


     public void StartTCPListenerForPSD(TcpClient client, int nChannels)
{
    Thread tcpListenThread = new Thread(() =>
    {
        try
        {
            NetworkStream stream = client.GetStream();

            SendTCPMessage("ADD_ME", client);

            while (isRunning)
            {
                // First: read full packet size (we assume fixed frame size is unknown,
                // so we read in chunks safely using buffer growth approach)

                List<byte> receivedBytes = new List<byte>();
                byte[] buffer = new byte[4096];

                // read until no more data available (simple framing assumption)
                do
                {
                    int read = stream.Read(buffer, 0, buffer.Length);
                    if (read == 0) throw new Exception("Disconnected");

                    byte[] chunk = new byte[read];
                    Array.Copy(buffer, chunk, read);
                    receivedBytes.AddRange(chunk);

                } while (stream.DataAvailable);

                // convert to float array
                int floatCount = receivedBytes.Count / sizeof(float);

                float[] psdFlat = new float[floatCount];
                Buffer.BlockCopy(receivedBytes.ToArray(), 0, psdFlat, 0, receivedBytes.Count);

                // infer frequencies
                if (floatCount % nChannels != 0)
                {
                    Debug.LogError($"[PSD] Invalid packet size: {floatCount} not divisible by {nChannels}");
                    continue;
                }

                int nFrequencies = floatCount / nChannels;

                float[,] psd = new float[nFrequencies, nChannels];

                for (int f = 0; f < nFrequencies; f++)
                {
                    for (int c = 0; c < nChannels; c++)
                    {
                        psd[f, c] = psdFlat[f * nChannels + c];
                    }
                }

                lock (GameSettings.psdLock)
                {
                    GameSettings.psd = psd;
                    GameSettings.psdFreqs = nFrequencies;
                    GameSettings.psdChannels = nChannels;
                }
            }
        }
        catch (Exception ex)
        {
            if (isRunning)
                Debug.LogError($"[TCP PSD] Error: {ex.Message}");
        }
    });

    tcpListenThread.IsBackground = true;
    tcpListenThread.Start();
}



    // private void OnTCPConnected(IAsyncResult ar)
    // {
    //     try
    //     {
    //         tcpClient.EndConnect(ar);
    //         Debug.Log("Connected to TCP server");

    //         tcpReceiveThread = new Thread(() => HandleTCPClient(tcpClient));
    //         tcpReceiveThread.IsBackground = true;
    //         tcpReceiveThread.Start();

    //         // Send test
    //         SendTCPMessage("Hello from Unity via TCP!");
    //     }
    //     catch (Exception e)
    //     {
    //         Debug.LogError("TCP Connection Error: " + e.Message);
    //     }
    // }


    public void SendTCPMessage(string message, TcpClient client)
    {
        try
        {
            byte[] fullMessage = BuildTimestampedMessage(message);
            client.GetStream().Write(fullMessage, 0, fullMessage.Length);
            // Debug.Log($"[TCP-SEND] Sent: {message} to {client.Client.RemoteEndPoint}");
        }
        catch (Exception e)
        {
            Debug.LogError("[TCP-SEND] Error: " + e.Message);
        }
    }

    public void StartTCPReceiver(TcpClient client)
    {
        if (client == null || !client.Connected)
        {
            Debug.LogError("TCP client not connected.");
            return;
        }

        Thread tcpListenThread = new Thread(() =>
        {
            try
            {
                NetworkStream stream = client.GetStream();
                while (isRunning)
                {
                    string timestamp = ReadUTF8String(stream);
                    string message = ReadUTF8String(stream);
                    Debug.Log($"[TCP-RECV] Received at {timestamp}: {message}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError("[TCP-RECV] Error: " + e.Message);
            }
        });

        tcpListenThread.IsBackground = true;
        tcpListenThread.Start();
    }




    private void HandleTCPClient(TcpClient client)
    {
        NetworkStream stream = client.GetStream();
        try
        {
            while (isRunning)
            {
                string timestamp = ReadUTF8String(stream);
                string message = ReadUTF8String(stream);
                Debug.Log($"[TCP] Received at {timestamp}: {message}");
            }
        }
        catch (IOException e)
        {
            Debug.LogError("TCP Client error: " + e.Message);
        }
        finally
        {
            stream.Close();
            client.Close();
            Debug.Log("TCP Client disconnected.");
        }
    }

    // ============ Message Protocol ============

    public byte[] BuildTimestampedMessage(string msg)
    {
        string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        byte[] tsBytes = Encoding.UTF8.GetBytes(timestamp);
        byte[] msgBytes = Encoding.UTF8.GetBytes(msg);

        byte[] tsLen = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(tsBytes.Length));
        byte[] msgLen = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(msgBytes.Length));

        using (MemoryStream ms = new MemoryStream())
        {
            ms.Write(tsLen, 0, 4);
            ms.Write(tsBytes, 0, tsBytes.Length);
            ms.Write(msgLen, 0, 4);
            ms.Write(msgBytes, 0, msgBytes.Length);
            return ms.ToArray();
        }
    }

    private string ReadUTF8String(NetworkStream stream)
    {
        byte[] lenBytes = ReadExact(stream, 4);
        int len = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(lenBytes, 0));
        byte[] data = ReadExact(stream, len);
        return Encoding.UTF8.GetString(data);
    }

    private byte[] ReadExact(NetworkStream stream, int length)
    {
        byte[] buffer = new byte[length];
        int read = 0;
        while (read < length)
        {
            int r = stream.Read(buffer, read, length - read);
            if (r <= 0) throw new IOException("Disconnected");
            read += r;
        }
        return buffer;
    }

    private void ParseAndPrintMessage(byte[] data, string prefix)
    {
        int tsLen = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(data, 0));
        string timestamp = Encoding.UTF8.GetString(data, 4, tsLen);

        int msgLen = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(data, 4 + tsLen));
        string msg = Encoding.UTF8.GetString(data, 8 + tsLen, msgLen);

        Debug.Log($"{prefix} Received at {timestamp}: {msg}");
    }
    
    public string DictToPythonString(Dictionary<string, object> dict)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("{");
        bool first = true;

        foreach (var kvp in dict)
        {
            if (!first) sb.Append(", ");
            first = false;

            sb.Append($"\"{kvp.Key}\": {ValueToPythonString(kvp.Value)}");
        }

        sb.Append("}");
        return sb.ToString();
    }


    string ValueToPythonString(object value)
    {
        if (value is int i) 
            return i.ToString(CultureInfo.InvariantCulture);
        else if (value is float f) 
            return f.ToString(CultureInfo.InvariantCulture);
        else if (value is double d) 
            return d.ToString(CultureInfo.InvariantCulture);
        else if (value is bool b)
            return b ? "True" : "False";
        else if (value is string s)
            return $"\"{s}\"";
        else if (value is IList<int> intList)
            return "[" + string.Join(", ", intList) + "]";
        else if (value is IList<float> floatList)
            return "[" + string.Join(", ", floatList.Select(x => x.ToString(CultureInfo.InvariantCulture))) + "]";
        else if (value is IList<double> doubleList)
            return "[" + string.Join(", ", doubleList.Select(x => x.ToString(CultureInfo.InvariantCulture))) + "]";
        else if (value is IList<string> stringList)
            return "[\"" + string.Join("\", \"", stringList) + "\"]";
        else
            return "\"UnsupportedType\"";
    }


    // ============ Cleanup ============

    private void OnApplicationQuit()
    {
        if (!isRunning) return;
        isRunning = false;
        GameSettings.Cleanup();
        Debug.Log("NetworkManager shut down.");
    }
    
    private void OnDestroy(){
        if (!isRunning) return;
        isRunning = false;
        GameSettings.Cleanup();
        Debug.Log("NetworkManager destroyed.");
    }

}
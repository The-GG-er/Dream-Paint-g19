using System.Net.Sockets;
using UnityEngine;

public static class GameSettings
{
    public static string serverIP = "127.0.0.1";
    public static int portManagerPort = 25798;

    public static TcpClient psdClientTCP;

    public static int trialsStarted = 0;
    public static int trialsEnded = 0;

    // PSD
    public static float[,] psd;
    public static int psdFreqs;
    public static int psdChannels = 8;

    public static readonly object psdLock = new object();

    public static void Cleanup()
    {
        psdClientTCP?.Close();
        UnityNetworkManager.Instance?.CloseUDPListener();
    }
}
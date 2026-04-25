using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
using System.Net.Sockets;
using System.Collections.Generic;


public class LoadingScreenManager : MonoBehaviour{
    public string sceneToLoad = "GameScene";     // Set in Inspector or hardcode
    public TMP_Text loadingText; // Assign in Inspector

    void Start(){

        // if (GameSettings.simulateSeed){
        //     return;
        // }


        StartCoroutine(LoadSceneAsync());
    }


    IEnumerator LoadSceneAsync()
    {
            loadingText.text = "Connecting to port manager...";

            int portManagerPort = GameSettings.portManagerPort;
            string serverIP = GameSettings.serverIP;


            // ------ Connect to Port Manager
            // Debug.Log($"Connecting to Port Manager at {serverIP}:{portManagerPort}");
            UdpClient portManagerClient = new(0);
            portManagerClient.Connect(serverIP, portManagerPort);
            bool udpConnected = false;
            yield return StartCoroutine(UnityNetworkManager.Instance.WaitForUDPConnection(portManagerClient, success => udpConnected = success));
            if (!udpConnected)
            {
                loadingText.text = "Error: Failed to connect to Port Manager (UDP).";
                yield break;
            }


            // ------ Getting UDP PercPosX from Port Manager
            // Debug.Log($"Getting UDP PercPosX");
            UnityNetworkManager.Instance.SendUDPMessage("GET_PORT/FilteredData", portManagerClient);
            int udpPort = -1;
            yield return StartCoroutine(UnityNetworkManager.Instance.WaitForUDPReply(portManagerClient, (msg) =>
            {
                int.TryParse(msg, out udpPort);
            }));
            loadingText.text = $"TCP PSD port received: {udpPort}";


            // -------------------------------------------------------------------------------------- Connections

            // ------ Connect to Mapper
            // Debug.Log($"Connecting to Mapper");
            // ------ Connect to Mapper (TCP)

                loadingText.text = "Connecting to the psd (TCP)...";

                GameSettings.psdClientTCP = new TcpClient();   // NEW VARIABLE
                bool mapperConnected = false;

                yield return StartCoroutine(
                    UnityNetworkManager.Instance.WaitForTCPConnection(
                        GameSettings.psdClientTCP,
                        udpPort,   // same port, now TCP
                        success => mapperConnected = success
                    )
                );

                if (!mapperConnected)
                {
                    Debug.LogError("Failed to connect to Output Mapper (TCP).");
                    loadingText.text = "Error: Failed to connect to Output Mapper (TCP).";
                    yield break;
                }

                UnityNetworkManager.Instance.StartTCPListenerForPSD(GameSettings.psdClientTCP, GameSettings.psdChannels);
            


            portManagerClient.Close();
            loadingText.text = "All connections established. Loading scene...";
      

        SceneManager.LoadScene(sceneToLoad);
    
    }

}
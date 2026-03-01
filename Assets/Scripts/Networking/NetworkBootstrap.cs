using FishNet;
using FishNet.Managing;
using FishNet.Transporting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpaceFighter
{
    public class NetworkBootstrap : MonoBehaviour
    {
        private NetworkManager _networkManager;
        private string _ipAddress = "127.0.0.1";
        private string _status = "";
        private bool _connecting;

        private void Awake()
        {
            _networkManager = FindObjectOfType<NetworkManager>();
            if (_networkManager == null)
            {
                Debug.LogError("[NetworkBootstrap] NetworkManager not found in scene.");
                return;
            }

            _networkManager.ClientManager.OnClientConnectionState += OnClientConnectionState;
            _networkManager.ServerManager.OnServerConnectionState += OnServerConnectionState;
        }

        private void OnDestroy()
        {
            if (_networkManager == null)
                return;

            _networkManager.ClientManager.OnClientConnectionState -= OnClientConnectionState;
            _networkManager.ServerManager.OnServerConnectionState -= OnServerConnectionState;
        }

        private void OnServerConnectionState(ServerConnectionStateArgs args)
        {
            if (args.ConnectionState == LocalConnectionState.Started)
            {
                _status = "Server started.";
                Debug.Log("[NetworkBootstrap] Server started.");
            }
            else if (args.ConnectionState == LocalConnectionState.Stopped)
            {
                _status = "Server stopped.";
                _connecting = false;
            }
        }

        private void OnClientConnectionState(ClientConnectionStateArgs args)
        {
            if (args.ConnectionState == LocalConnectionState.Started)
            {
                _status = "Connected!";
                _connecting = false;
                Debug.Log("[NetworkBootstrap] Client connected. Loading Lobby scene.");
                SceneManager.LoadScene("Lobby");
            }
            else if (args.ConnectionState == LocalConnectionState.Stopped)
            {
                _status = "Disconnected.";
                _connecting = false;
            }
            else if (args.ConnectionState == LocalConnectionState.Starting)
            {
                _status = "Connecting...";
            }
        }

        private void OnGUI()
        {
            if (_networkManager == null)
            {
                GUILayout.Label("Error: NetworkManager not found.");
                return;
            }

            float centerX = Screen.width * 0.5f - 120f;
            float startY = Screen.height * 0.3f;

            GUILayout.BeginArea(new Rect(centerX, startY, 240f, 300f));

            GUILayout.Label("<size=24><b>SpaceFighter</b></size>", new GUIStyle(GUI.skin.label) { richText = true, alignment = TextAnchor.MiddleCenter });
            GUILayout.Space(20f);

            bool active = _networkManager.ServerManager.Started || _networkManager.ClientManager.Started;

            GUI.enabled = !active && !_connecting;

            if (GUILayout.Button("Host Game", GUILayout.Height(36f)))
            {
                _connecting = true;
                _status = "Starting host...";
                _networkManager.ServerManager.StartConnection();
                _networkManager.ClientManager.StartConnection();
            }

            GUILayout.Space(10f);
            GUILayout.Label("Server IP:");
            _ipAddress = GUILayout.TextField(_ipAddress, 45);

            GUILayout.Space(4f);
            if (GUILayout.Button("Join Game", GUILayout.Height(36f)))
            {
                _connecting = true;
                _status = "Connecting to " + _ipAddress + "...";
                _networkManager.ClientManager.StartConnection(_ipAddress);
            }

            GUI.enabled = true;
            GUILayout.Space(10f);
            GUILayout.Label(_status);

            GUILayout.EndArea();
        }
    }
}

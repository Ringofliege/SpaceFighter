using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpaceFighter
{
    public class LobbyManager : NetworkBehaviour
    {
        public static LobbyManager Instance { get; private set; }

        public readonly SyncList<LobbyPlayerData> Players = new();

        private Vector2 _scrollPos;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            ServerManager.OnRemoteConnectionState += OnRemoteConnectionState;
        }

        public override void OnStopServer()
        {
            base.OnStopServer();
            ServerManager.OnRemoteConnectionState -= OnRemoteConnectionState;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            CmdRegisterPlayer();
        }

        private void OnRemoteConnectionState(NetworkConnection conn, RemoteConnectionStateArgs args)
        {
            if (args.ConnectionState == RemoteConnectionState.Stopped)
                RemovePlayer(conn.ClientId);
        }

        [Server]
        private void RemovePlayer(int clientId)
        {
            for (int i = Players.Count - 1; i >= 0; i--)
            {
                if (Players[i].ConnectionId == clientId)
                {
                    Players.RemoveAt(i);
                    break;
                }
            }
        }

        private int FindPlayerIndex(int clientId)
        {
            for (int i = 0; i < Players.Count; i++)
            {
                if (Players[i].ConnectionId == clientId)
                    return i;
            }
            return -1;
        }

        [ServerRpc(RequireOwnership = false)]
        private void CmdRegisterPlayer(NetworkConnection conn = null)
        {
            int clientId = conn.ClientId;

            if (FindPlayerIndex(clientId) >= 0)
                return;

            var data = new LobbyPlayerData
            {
                ConnectionId = clientId,
                Team = Team.None,
                ShipClass = ShipClass.Striker,
                IsReady = false,
                PlayerName = "Player " + clientId
            };

            Players.Add(data);
            Debug.Log("[LobbyManager] Player registered: " + data.PlayerName);
        }

        [ServerRpc(RequireOwnership = false)]
        public void CmdSelectTeam(Team team, NetworkConnection conn = null)
        {
            int clientId = conn.ClientId;
            int idx = FindPlayerIndex(clientId);
            if (idx < 0)
                return;

            if (team == Team.None)
                return;

            var data = Players[idx];
            data.Team = team;
            Players[idx] = data;
        }

        [ServerRpc(RequireOwnership = false)]
        public void CmdSelectClass(ShipClass cls, NetworkConnection conn = null)
        {
            int clientId = conn.ClientId;
            int idx = FindPlayerIndex(clientId);
            if (idx < 0)
                return;

            var data = Players[idx];

            if (data.Team == Team.None)
            {
                Debug.LogWarning("[LobbyManager] Player " + clientId + " must select a team first.");
                return;
            }

            int classCount = 0;
            for (int i = 0; i < Players.Count; i++)
            {
                if (i == idx)
                    continue;
                if (Players[i].Team == data.Team && Players[i].ShipClass == cls)
                    classCount++;
            }

            if (cls == ShipClass.Vanguard && classCount >= GameConstants.MaxVanguards)
            {
                Debug.LogWarning("[LobbyManager] Max Vanguards reached for team " + data.Team);
                return;
            }
            if (cls == ShipClass.Disruptor && classCount >= GameConstants.MaxDisruptors)
            {
                Debug.LogWarning("[LobbyManager] Max Disruptors reached for team " + data.Team);
                return;
            }

            data.ShipClass = cls;
            Players[idx] = data;
        }

        [ServerRpc(RequireOwnership = false)]
        public void CmdToggleReady(NetworkConnection conn = null)
        {
            int clientId = conn.ClientId;
            int idx = FindPlayerIndex(clientId);
            if (idx < 0)
                return;

            var data = Players[idx];
            data.IsReady = !data.IsReady;
            Players[idx] = data;
        }

        [ServerRpc(RequireOwnership = false)]
        public void CmdStartMatch(NetworkConnection conn = null)
        {
            if (!IsServerStarted)
                return;

            // Only the host (ClientId 0) can start the match
            if (conn.ClientId != 0)
            {
                Debug.LogWarning("[LobbyManager] Only the host can start the match.");
                return;
            }

            int blueCount = 0;
            int redCount = 0;
            bool allReady = true;

            for (int i = 0; i < Players.Count; i++)
            {
                if (!Players[i].IsReady)
                {
                    allReady = false;
                    break;
                }
                if (Players[i].Team == Team.Blue)
                    blueCount++;
                else if (Players[i].Team == Team.Red)
                    redCount++;
            }

            if (!allReady)
            {
                Debug.LogWarning("[LobbyManager] Cannot start: not all players are ready.");
                return;
            }

            if (blueCount < 1 || redCount < 1)
            {
                Debug.LogWarning("[LobbyManager] Cannot start: need at least 1 player per team.");
                return;
            }

            StartMatch();
        }

        [Server]
        private void StartMatch()
        {
            Debug.Log("[LobbyManager] Starting match.");

            if (GameManager.Instance != null)
                GameManager.Instance.StartMatch();

            SceneManager.LoadScene("Game");
        }

        private int LocalClientId
        {
            get
            {
                if (ClientManager != null && ClientManager.Connection != null)
                    return ClientManager.Connection.ClientId;
                return -1;
            }
        }

        private void OnGUI()
        {
            float areaWidth = 700f;
            float areaHeight = 500f;
            float x = (Screen.width - areaWidth) * 0.5f;
            float y = 40f;

            GUILayout.BeginArea(new Rect(x, y, areaWidth, areaHeight));

            GUILayout.Label("<size=20><b>Lobby</b></size>",
                new GUIStyle(GUI.skin.label) { richText = true, alignment = TextAnchor.MiddleCenter });
            GUILayout.Space(10f);

            int blueCount = 0;
            int redCount = 0;
            for (int i = 0; i < Players.Count; i++)
            {
                if (Players[i].Team == Team.Blue) blueCount++;
                else if (Players[i].Team == Team.Red) redCount++;
            }
            GUILayout.Label("Blue: " + blueCount + "  |  Red: " + redCount);
            GUILayout.Space(6f);

            // Header
            GUILayout.BeginHorizontal();
            GUILayout.Label("Name", GUILayout.Width(100f));
            GUILayout.Label("Team", GUILayout.Width(130f));
            GUILayout.Label("Class", GUILayout.Width(300f));
            GUILayout.Label("Ready", GUILayout.Width(60f));
            GUILayout.EndHorizontal();

            _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.Height(300f));

            int localId = LocalClientId;

            for (int i = 0; i < Players.Count; i++)
            {
                var p = Players[i];
                bool isLocal = p.ConnectionId == localId;

                if (isLocal)
                    GUI.color = Color.yellow;

                GUILayout.BeginHorizontal("box");

                GUILayout.Label(p.PlayerName, GUILayout.Width(100f));

                // Team buttons
                if (isLocal)
                {
                    if (GUILayout.Button(p.Team == Team.Blue ? "[Blue]" : "Blue", GUILayout.Width(60f)))
                        CmdSelectTeam(Team.Blue);
                    if (GUILayout.Button(p.Team == Team.Red ? "[Red]" : "Red", GUILayout.Width(60f)))
                        CmdSelectTeam(Team.Red);
                }
                else
                {
                    GUILayout.Label(p.Team.ToString(), GUILayout.Width(130f));
                }

                // Class buttons
                if (isLocal)
                {
                    ShipClass[] classes = { ShipClass.Vanguard, ShipClass.Striker, ShipClass.Disruptor, ShipClass.Flanker };
                    foreach (var cls in classes)
                    {
                        string label = p.ShipClass == cls ? "[" + cls + "]" : cls.ToString();
                        if (GUILayout.Button(label, GUILayout.Width(72f)))
                            CmdSelectClass(cls);
                    }
                }
                else
                {
                    GUILayout.Label(p.ShipClass.ToString(), GUILayout.Width(300f));
                }

                // Ready status
                if (isLocal)
                {
                    string readyLabel = p.IsReady ? "Ready!" : "Not Ready";
                    if (GUILayout.Button(readyLabel, GUILayout.Width(70f)))
                        CmdToggleReady();
                }
                else
                {
                    GUILayout.Label(p.IsReady ? "Ready" : "—", GUILayout.Width(60f));
                }

                GUILayout.EndHorizontal();

                if (isLocal)
                    GUI.color = Color.white;
            }

            GUILayout.EndScrollView();

            // Class restriction warnings
            for (int i = 0; i < Players.Count; i++)
            {
                var p = Players[i];
                if (p.ConnectionId != localId)
                    continue;

                if (p.Team == Team.None)
                {
                    GUILayout.Label("<color=orange>Select a team before choosing a class.</color>",
                        new GUIStyle(GUI.skin.label) { richText = true });
                }
                break;
            }

            GUILayout.Space(10f);

            // Host-only start button
            if (IsServerStarted && localId == 0)
            {
                if (GUILayout.Button("Start Match", GUILayout.Height(36f)))
                    CmdStartMatch();
            }

            GUILayout.EndArea();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}

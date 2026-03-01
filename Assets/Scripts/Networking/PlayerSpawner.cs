using FishNet.Connection;
using FishNet.Object;
using UnityEngine;

namespace SpaceFighter
{
    public class PlayerSpawner : NetworkBehaviour
    {
        private const float SpawnOffsetX = 15f;
        private const float SpawnSpacingY = 3f;

        public override void OnStartServer()
        {
            base.OnStartServer();
            SpawnAllShips();
        }

        [Server]
        private void SpawnAllShips()
        {
            if (LobbyManager.Instance == null)
            {
                Debug.LogError("[PlayerSpawner] LobbyManager instance not found. Cannot spawn ships.");
                return;
            }

            var players = LobbyManager.Instance.Players;
            if (players.Count == 0)
            {
                Debug.LogWarning("[PlayerSpawner] No players in lobby data.");
                return;
            }

            int blueIndex = 0;
            int redIndex = 0;

            for (int i = 0; i < players.Count; i++)
            {
                var data = players[i];

                NetworkConnection conn = null;
                if (!ServerManager.Clients.TryGetValue(data.ConnectionId, out conn))
                {
                    Debug.LogWarning("[PlayerSpawner] Connection not found for ClientId " + data.ConnectionId);
                    continue;
                }

                Vector3 spawnPos;
                if (data.Team == Team.Blue)
                {
                    float yOffset = (blueIndex - (CountTeam(players, Team.Blue) - 1) * 0.5f) * SpawnSpacingY;
                    spawnPos = new Vector3(-SpawnOffsetX, yOffset, 0f);
                    blueIndex++;
                }
                else
                {
                    float yOffset = (redIndex - (CountTeam(players, Team.Red) - 1) * 0.5f) * SpawnSpacingY;
                    spawnPos = new Vector3(SpawnOffsetX, yOffset, 0f);
                    redIndex++;
                }

                GameObject go = CreateShipObject(data, spawnPos);
                if (go == null)
                    continue;

                ServerManager.Spawn(go, conn);

                var ship = go.GetComponent<PlayerShip>();
                if (ship != null)
                    ship.Initialize(data.ShipClass, data.Team);

                Debug.Log("[PlayerSpawner] Spawned " + data.ShipClass + " for " + data.PlayerName
                          + " on team " + data.Team + " at " + spawnPos);
            }
        }

        private static int CountTeam(SyncList<LobbyPlayerData> players, Team team)
        {
            int count = 0;
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].Team == team)
                    count++;
            }
            return count;
        }

        private GameObject CreateShipObject(LobbyPlayerData data, Vector3 position)
        {
            var go = new GameObject("Ship_" + data.PlayerName);
            go.transform.position = position;

            // Networking
            go.AddComponent<NetworkObject>();

            // Physics
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.linearDamping = 3f;
            rb.freezeRotation = true;

            go.AddComponent<CircleCollider2D>();

            // Visuals
            var sr = go.AddComponent<SpriteRenderer>();
            sr.color = data.Team == Team.Blue ? Color.cyan : Color.red;

            // Core player systems
            go.AddComponent<PlayerShip>();
            go.AddComponent<PlayerMovement>();
            go.AddComponent<DashSystem>();
            go.AddComponent<EnergySystem>();
            go.AddComponent<HeatSystem>();
            go.AddComponent<WeaponController>();
            go.AddComponent<AbilityController>();

            // Class-specific abilities
            switch (data.ShipClass)
            {
                case ShipClass.Vanguard:
                    go.AddComponent<VanguardBarrier>();
                    go.AddComponent<ShieldBash>();
                    break;
                case ShipClass.Striker:
                    go.AddComponent<StrikerOverdrive>();
                    go.AddComponent<RicochetAbility>();
                    break;
                case ShipClass.Disruptor:
                    go.AddComponent<PierceCannonAbility>();
                    go.AddComponent<EMPMineAbility>();
                    break;
                case ShipClass.Flanker:
                    go.AddComponent<FlankerBlink>();
                    go.AddComponent<CloakAbility>();
                    break;
            }

            // StabilizeSystem for non-Vanguard classes
            if (data.ShipClass != ShipClass.Vanguard)
                go.AddComponent<StabilizeSystem>();

            return go;
        }
    }
}

using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

namespace SpaceFighter
{
    public class GameManager : NetworkBehaviour
    {
        public static GameManager Instance { get; private set; }

        [field: SyncVar]
        public GameState CurrentState { get; [server] private set; }

        [field: SyncVar]
        public int BlueAlive { get; [server] private set; }

        [field: SyncVar]
        public int RedAlive { get; [server] private set; }

        [SerializeField] private RoundManager roundManager;
        [SerializeField] private TeamManager teamManager;

        public RoundManager RoundManager => roundManager;
        public TeamManager TeamManager => teamManager;

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
            CurrentState = GameState.WaitingForPlayers;
            BlueAlive = 0;
            RedAlive = 0;
        }

        [Server]
        public void StartMatch()
        {
            CurrentState = GameState.Lobby;
        }

        [Server]
        public void StartRound()
        {
            BlueAlive = teamManager.GetTeamCount(Team.Blue);
            RedAlive = teamManager.GetTeamCount(Team.Red);
            CurrentState = GameState.RoundActive;
            roundManager.StartRound();
        }

        [Server]
        public void OnPlayerDied(Team team)
        {
            if (team == Team.Blue)
                BlueAlive--;
            else if (team == Team.Red)
                RedAlive--;

            CheckElimination();
        }

        [Server]
        private void CheckElimination()
        {
            if (BlueAlive <= 0 && RedAlive <= 0)
            {
                EndRound(RoundEndReason.Elimination, Team.None);
            }
            else if (BlueAlive <= 0)
            {
                EndRound(RoundEndReason.Elimination, Team.Red);
            }
            else if (RedAlive <= 0)
            {
                EndRound(RoundEndReason.Elimination, Team.Blue);
            }
        }

        [Server]
        public void EndRound(RoundEndReason reason, Team winner)
        {
            CurrentState = GameState.RoundEnd;
        }

        [Server]
        public void SetOvertime()
        {
            CurrentState = GameState.Overtime;
        }

        [Server]
        public void EndMatch()
        {
            CurrentState = GameState.MatchEnd;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}

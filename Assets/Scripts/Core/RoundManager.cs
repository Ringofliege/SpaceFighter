using System;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

namespace SpaceFighter
{
    public class RoundManager : NetworkBehaviour
    {
        [field: SyncVar]
        public float RoundTimer { get; [server] private set; }

        [field: SyncVar]
        public bool OvertimeActive { get; [server] private set; }

        [field: SyncVar]
        public bool CoreActive { get; [server] private set; }

        private bool _overtimeDelayStarted;
        private float _overtimeDelayTimer;

        public static event Action OnOvertimeStarted;
        public static event Action OnCoreActivated;

        public override void OnStartServer()
        {
            base.OnStartServer();
            RoundTimer = GameConstants.RoundDuration;
            OvertimeActive = false;
            CoreActive = false;
            _overtimeDelayStarted = false;
            _overtimeDelayTimer = 0f;
        }

        private void Update()
        {
            if (!IsServerInitialized)
                return;

            if (GameManager.Instance == null)
                return;

            if (GameManager.Instance.CurrentState != GameState.RoundActive &&
                GameManager.Instance.CurrentState != GameState.Overtime)
                return;

            if (!OvertimeActive)
            {
                RoundTimer -= Time.deltaTime;
                if (RoundTimer <= 0f)
                {
                    RoundTimer = 0f;
                    StartOvertime();
                }
            }

            if (_overtimeDelayStarted)
            {
                _overtimeDelayTimer -= Time.deltaTime;
                if (_overtimeDelayTimer <= 0f)
                {
                    _overtimeDelayStarted = false;
                    ActivateCore();
                }
            }
        }

        [Server]
        public void StartRound()
        {
            RoundTimer = GameConstants.RoundDuration;
            OvertimeActive = false;
            CoreActive = false;
            _overtimeDelayStarted = false;
            _overtimeDelayTimer = 0f;
        }

        [Server]
        public void OnPlayerDied(Team team)
        {
            GameManager.Instance.OnPlayerDied(team);
        }

        [Server]
        public void StartOvertime()
        {
            OvertimeActive = true;
            _overtimeDelayStarted = true;
            _overtimeDelayTimer = GameConstants.OvertimeDelay;
            GameManager.Instance.SetOvertime();
            OnOvertimeStarted?.Invoke();
        }

        [Server]
        public void ActivateCore()
        {
            CoreActive = true;
            OnCoreActivated?.Invoke();
        }

        [Server]
        public void OnCoreCaptured(Team team)
        {
            CoreActive = false;
            GameManager.Instance.EndRound(RoundEndReason.CoreCapture, team);
        }
    }
}

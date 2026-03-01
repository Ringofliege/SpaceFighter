using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

namespace SpaceFighter
{
    public class StabilizeSystem : NetworkBehaviour
    {
        [SyncVar]
        public bool HasUsedStabilize;

        [SyncVar]
        public bool IsChanneling;

        private float _channelTimer;
        private PlayerShip _ship;
        private EnergySystem _energy;

        private void Awake()
        {
            _ship = GetComponent<PlayerShip>();
            _energy = GetComponent<EnergySystem>();
        }

        private void Update()
        {
            if (IsOwner && Input.GetKeyDown(KeyCode.E))
            {
                CmdStabilize();
            }

            if (!IsServerInitialized) return;

            if (IsChanneling)
            {
                _channelTimer -= Time.deltaTime;
                if (_channelTimer <= 0f)
                {
                    CompleteChannel();
                }
            }
        }

        [ServerRpc]
        private void CmdStabilize()
        {
            if (_ship == null || !_ship.IsAlive) return;
            if (_ship.ShipClassType == ShipClass.Vanguard) return;
            if (HasUsedStabilize) return;
            if (IsChanneling) return;

            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Overtime)
                return;

            if (_energy == null || _energy.CurrentEnergy < GameConstants.StabilizeEnergyCost) return;

            _energy.UseEnergy(GameConstants.StabilizeEnergyCost);
            IsChanneling = true;
            _channelTimer = GameConstants.StabilizeChannelTime;
        }

        [Server]
        private void CompleteChannel()
        {
            IsChanneling = false;
            HasUsedStabilize = true;

            if (_ship != null && _ship.IsAlive)
            {
                int healAmount = Mathf.RoundToInt(_ship.MaxHP * GameConstants.StabilizeHealPercent);
                _ship.Heal(healAmount);
            }
        }

        [Server]
        public void InterruptChannel()
        {
            if (!IsChanneling) return;
            IsChanneling = false;
            _channelTimer = 0f;
        }
    }
}

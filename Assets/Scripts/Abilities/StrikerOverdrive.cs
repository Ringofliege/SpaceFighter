using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

namespace SpaceFighter
{
    public class StrikerOverdrive : NetworkBehaviour
    {
        [SyncVar] public bool IsOverdriveActive;
        [SyncVar] public float Cooldown;

        private float _durationTimer;
        private PlayerShip _ship;
        private EnergySystem _energy;
        private WeaponController _weapon;
        private AbilityController _abilityController;

        private void Awake()
        {
            _ship = GetComponent<PlayerShip>();
            _energy = GetComponent<EnergySystem>();
            _weapon = GetComponent<WeaponController>();
            _abilityController = GetComponent<AbilityController>();
        }

        private void Update()
        {
            if (!IsServerInitialized) return;

            if (Cooldown > 0f)
                Cooldown -= Time.deltaTime;

            if (!IsOverdriveActive) return;

            _durationTimer -= Time.deltaTime;
            if (_durationTimer <= 0f)
                Deactivate();
        }

        [Server]
        public void Execute()
        {
            if (Cooldown > 0f) return;
            if (IsOverdriveActive) return;
            if (_ship == null || !_ship.IsAlive) return;
            if (_energy == null || !_energy.UseEnergy(GameConstants.StrikerOverdriveEnergyCost)) return;

            IsOverdriveActive = true;
            _durationTimer = GameConstants.StrikerOverdriveDuration;
            Cooldown = GameConstants.StrikerOverdriveCooldown;
            if (_abilityController != null)
                _abilityController.Ability1Cooldown = GameConstants.StrikerOverdriveCooldown;

            if (_weapon != null)
            {
                _weapon.OverdriveFireRateMult = 1f - GameConstants.StrikerOverdriveFireRateBonus;
                _weapon.OverdriveHeatMult = 1f + GameConstants.StrikerOverdriveHeatPenalty;
            }

            RpcOverdriveState(true);
        }

        [Server]
        private void Deactivate()
        {
            IsOverdriveActive = false;

            if (_weapon != null)
            {
                _weapon.OverdriveFireRateMult = 1f;
                _weapon.OverdriveHeatMult = 1f;
            }

            RpcOverdriveState(false);
        }

        [ObserversRpc]
        private void RpcOverdriveState(bool active)
        {
            // Glow effect handled by client-side systems
        }
    }
}

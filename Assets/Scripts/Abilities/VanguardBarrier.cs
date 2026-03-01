using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

namespace SpaceFighter
{
    public class VanguardBarrier : NetworkBehaviour
    {
        [SyncVar] public bool IsBarrierActive;
        [SyncVar] public int RearModuleHP = GameConstants.VanguardBarrierRearModuleHP;
        [SyncVar] public bool IsShieldSystemsOffline;

        // Legacy alias so existing code referencing IsActive still compiles
        public bool IsActive => IsBarrierActive;

        private float _barrierTimer;
        private float _offlineTimer;
        private float _empDisableTimer;

        private PlayerShip _ship;
        private PlayerMovement _movement;
        private EnergySystem _energy;
        private DashSystem _dash;

        private void Awake()
        {
            _ship = GetComponent<PlayerShip>();
            _movement = GetComponent<PlayerMovement>();
            _energy = GetComponent<EnergySystem>();
            _dash = GetComponent<DashSystem>();
        }

        [Server]
        public void ActivateBarrier()
        {
            if (IsShieldSystemsOffline) return;
            if (_empDisableTimer > 0f) return;
            if (IsBarrierActive) return;
            if (_energy == null || _energy.CurrentEnergy < GameConstants.VanguardBarrierEnergyCostPerSec) return;

            IsBarrierActive = true;
            _barrierTimer = 0f;
            RearModuleHP = GameConstants.VanguardBarrierRearModuleHP;

            if (_movement != null)
                _movement.SpeedMultiplier = GameConstants.VanguardBarrierSpeedMult;

            float drainRate = GameConstants.VanguardBarrierEnergyCostPerSec;
            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Overtime)
                drainRate = 30f;
            if (_energy != null)
                _energy.SetContinuousDrain(drainRate);

            RpcBarrierStateChanged(true);
        }

        private void Update()
        {
            if (!IsServerInitialized) return;

            if (IsShieldSystemsOffline)
            {
                _offlineTimer -= Time.deltaTime;
                if (_offlineTimer <= 0f)
                {
                    IsShieldSystemsOffline = false;
                    _offlineTimer = 0f;
                }
            }

            if (_empDisableTimer > 0f)
                _empDisableTimer -= Time.deltaTime;

            if (!IsBarrierActive) return;

            _barrierTimer += Time.deltaTime;

            if (_barrierTimer >= GameConstants.VanguardBarrierDuration ||
                (_energy != null && _energy.CurrentEnergy <= 0f))
            {
                DeactivateBarrier();
            }
        }

        [Server]
        public void DeactivateBarrier()
        {
            if (!IsBarrierActive) return;
            IsBarrierActive = false;

            if (_movement != null)
                _movement.SpeedMultiplier = 1f;

            if (_energy != null)
                _energy.ClearContinuousDrain();

            // Cooldown starts after deactivation
            AbilityController ac = GetComponent<AbilityController>();
            if (ac != null)
                ac.Ability1Cooldown = GameConstants.VanguardBarrierCooldown;

            RpcBarrierStateChanged(false);
        }

        [Server]
        public void DamageRearModule(int damage)
        {
            if (!IsBarrierActive) return;
            RearModuleHP -= damage;
            if (RearModuleHP <= 0)
            {
                RearModuleHP = 0;
                ForceDropBarrier();
                ApplyShieldOffline(3f);
            }
        }

        [Server]
        public void ForceDropBarrier()
        {
            if (!IsBarrierActive) return;
            IsBarrierActive = false;

            if (_movement != null)
                _movement.SpeedMultiplier = 1f;

            if (_energy != null)
                _energy.ClearContinuousDrain();

            RpcBarrierStateChanged(false);
        }

        [Server]
        public void ApplyShieldOffline(float duration)
        {
            IsShieldSystemsOffline = true;
            _offlineTimer = duration;
            if (IsBarrierActive)
                ForceDropBarrier();
        }

        [Server]
        public void ApplyEMPDisable(float duration)
        {
            _empDisableTimer = duration;
            if (IsBarrierActive)
                ForceDropBarrier();
        }

        [ObserversRpc]
        private void RpcBarrierStateChanged(bool active)
        {
            // Visual updates handled by client-side systems
        }
    }
}

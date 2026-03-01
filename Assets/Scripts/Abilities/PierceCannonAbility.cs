using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

namespace SpaceFighter
{
    public class PierceCannonAbility : NetworkBehaviour
    {
        [SyncVar] public bool IsCharging;
        [SyncVar] public float Cooldown;

        [SerializeField] private GameObject _pierceProjectilePrefab;

        private float _chargeTimer;
        private PlayerShip _ship;
        private PlayerMovement _movement;
        private EnergySystem _energy;
        private AbilityController _abilityController;

        private void Awake()
        {
            _ship = GetComponent<PlayerShip>();
            _movement = GetComponent<PlayerMovement>();
            _energy = GetComponent<EnergySystem>();
            _abilityController = GetComponent<AbilityController>();
        }

        private void Update()
        {
            if (!IsServerInitialized) return;

            if (Cooldown > 0f)
                Cooldown -= Time.deltaTime;

            if (!IsCharging) return;

            // Cancel if player dies during charge
            if (_ship == null || !_ship.IsAlive)
            {
                CancelCharge();
                return;
            }

            _chargeTimer += Time.deltaTime;
            RpcChargeState(true, _chargeTimer / GameConstants.DisruptorPierceCannonChargeTime);

            if (_chargeTimer >= GameConstants.DisruptorPierceCannonChargeTime)
                Fire();
        }

        [Server]
        public void StartCharge()
        {
            if (Cooldown > 0f) return;
            if (IsCharging) return;
            if (_ship == null || !_ship.IsAlive) return;
            if (_energy == null || !_energy.UseEnergy(GameConstants.DisruptorPierceCannonEnergyCost)) return;

            IsCharging = true;
            _chargeTimer = 0f;

            if (_movement != null)
                _movement.SpeedMultiplier = GameConstants.DisruptorPierceCannonMoveSpeedMult;

            RpcChargeState(true, 0f);
        }

        [Server]
        private void Fire()
        {
            IsCharging = false;
            _chargeTimer = 0f;
            Cooldown = GameConstants.DisruptorPierceCannonCooldown;
            if (_abilityController != null)
                _abilityController.Ability1Cooldown = GameConstants.DisruptorPierceCannonCooldown;

            if (_movement != null)
                _movement.SpeedMultiplier = 1f;

            Vector2 aimDir = (Vector2)transform.up;
            Vector2 spawnPos = (Vector2)transform.position + aimDir * 0.6f;

            if (_pierceProjectilePrefab != null)
            {
                GameObject go = Instantiate(_pierceProjectilePrefab, spawnPos, Quaternion.identity);
                PierceProjectile proj = go.GetComponent<PierceProjectile>();
                if (proj != null)
                    proj.Initialize(_ship.ShipTeam, aimDir, Owner.ClientId);
                ServerManager.Spawn(go);
            }

            RpcChargeState(false, 0f);
        }

        [Server]
        private void CancelCharge()
        {
            IsCharging = false;
            _chargeTimer = 0f;

            if (_movement != null)
                _movement.SpeedMultiplier = 1f;

            RpcChargeState(false, 0f);
        }

        [ObserversRpc]
        private void RpcChargeState(bool charging, float progress)
        {
            // Visual/audio handled by client-side systems
        }
    }
}

using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

namespace SpaceFighter
{
    public class RicochetAbility : NetworkBehaviour
    {
        [SyncVar] public float Cooldown;

        [SerializeField] private GameObject _ricochetProjectilePrefab;

        private PlayerShip _ship;
        private EnergySystem _energy;
        private AbilityController _abilityController;

        private void Awake()
        {
            _ship = GetComponent<PlayerShip>();
            _energy = GetComponent<EnergySystem>();
            _abilityController = GetComponent<AbilityController>();
        }

        private void Update()
        {
            if (!IsServerInitialized) return;

            if (Cooldown > 0f)
                Cooldown -= Time.deltaTime;
        }

        [Server]
        public void Execute(Vector2 aimDir)
        {
            if (Cooldown > 0f) return;
            if (_ship == null || !_ship.IsAlive) return;
            if (_energy == null || !_energy.UseEnergy(GameConstants.StrikerRicochetShotEnergyCost)) return;

            Cooldown = GameConstants.StrikerRicochetShotCooldown;
            if (_abilityController != null)
                _abilityController.Ability2Cooldown = GameConstants.StrikerRicochetShotCooldown;

            aimDir.Normalize();
            Vector2 spawnPos = (Vector2)transform.position + aimDir * 0.6f;

            if (_ricochetProjectilePrefab == null) return;

            GameObject go = Instantiate(_ricochetProjectilePrefab, spawnPos, Quaternion.identity);
            RicochetProjectile proj = go.GetComponent<RicochetProjectile>();
            if (proj != null)
                proj.Initialize(_ship.ShipTeam, aimDir, GameConstants.ProjectileSpeed, Owner.ClientId);
            ServerManager.Spawn(go);
        }
    }
}

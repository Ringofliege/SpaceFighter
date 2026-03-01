using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

namespace SpaceFighter
{
    public class ShieldBash : NetworkBehaviour
    {
        [SyncVar] public float Cooldown;

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
        }

        [Server]
        public void Execute()
        {
            if (Cooldown > 0f) return;
            if (_ship == null || !_ship.IsAlive) return;
            if (_energy == null || !_energy.UseEnergy(GameConstants.VanguardShieldBashEnergyCost)) return;

            Cooldown = GameConstants.VanguardShieldBashCooldown;
            if (_abilityController != null)
                _abilityController.Ability2Cooldown = GameConstants.VanguardShieldBashCooldown;

            Vector2 bashDir = (Vector2)transform.up;

            // Apply forward dash burst
            if (_movement != null)
                _movement.ApplyForce(bashDir * GameConstants.ShieldBashForce);

            // Overlap check for targets in front
            Vector2 checkPos = (Vector2)transform.position + bashDir * 1.5f;
            Collider2D[] hits = Physics2D.OverlapCircleAll(checkPos, 1.5f);

            foreach (Collider2D hit in hits)
            {
                if (hit.gameObject == gameObject) continue;

                PlayerShip target = hit.GetComponent<PlayerShip>();
                if (target == null) continue;
                if (target.ShipTeam == _ship.ShipTeam) continue;

                // Apply damage and knockback
                Vector2 knockbackDir = ((Vector2)target.transform.position - (Vector2)transform.position).normalized;
                target.TakeDamage(
                    GameConstants.VanguardShieldBashDamage,
                    ignoresBarrier: false,
                    ignoresDR: false,
                    knockbackDir: knockbackDir,
                    knockbackForce: GameConstants.ShieldBashForce
                );
            }

            RpcBashEffect((Vector2)transform.position, bashDir);
        }

        [ObserversRpc]
        private void RpcBashEffect(Vector2 pos, Vector2 dir)
        {
            // VFX handled by client-side systems
        }
    }
}

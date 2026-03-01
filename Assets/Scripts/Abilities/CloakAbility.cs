using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

namespace SpaceFighter
{
    public class CloakAbility : NetworkBehaviour
    {
        [SyncVar] public bool IsCloaked;
        [SyncVar] public float Cooldown;

        private float _durationTimer;
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

            if (!IsCloaked) return;

            _durationTimer -= Time.deltaTime;
            if (_durationTimer <= 0f)
                Deactivate();
        }

        [Server]
        public void Execute()
        {
            if (Cooldown > 0f) return;
            if (IsCloaked) return;
            if (_ship == null || !_ship.IsAlive) return;
            if (_energy == null || !_energy.UseEnergy(GameConstants.FlankerCloakEnergyCost)) return;

            IsCloaked = true;
            _durationTimer = GameConstants.FlankerCloakDuration;
            Cooldown = GameConstants.FlankerCloakCooldown;
            if (_abilityController != null)
                _abilityController.Ability2Cooldown = GameConstants.FlankerCloakCooldown;

            RpcCloakState(true);
        }

        [Server]
        private void Deactivate()
        {
            IsCloaked = false;
            RpcCloakState(false);
        }

        [ObserversRpc]
        private void RpcCloakState(bool cloaked)
        {
            if (_ship == null) return;

            SpriteRenderer sr = _ship.Sprite;
            if (sr == null) return;

            if (cloaked)
            {
                Color c = sr.color;
                c.a = IsOwner ? 0.3f : 0.1f;
                sr.color = c;
            }
            else
            {
                Color c = sr.color;
                c.a = 1f;
                sr.color = c;
            }
        }
    }
}

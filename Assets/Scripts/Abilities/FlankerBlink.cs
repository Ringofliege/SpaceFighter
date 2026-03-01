using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

namespace SpaceFighter
{
    public class FlankerBlink : NetworkBehaviour
    {
        [SyncVar] public int BlinkCharges = GameConstants.FlankerBlinkCharges;

        private float[] _rechargeTimers = new float[GameConstants.FlankerBlinkCharges];
        private const float BlinkDistance = 6f;

        private PlayerShip _ship;
        private EnergySystem _energy;

        private void Awake()
        {
            _ship = GetComponent<PlayerShip>();
            _energy = GetComponent<EnergySystem>();
        }

        private void Update()
        {
            if (!IsServerInitialized) return;

            for (int i = 0; i < _rechargeTimers.Length; i++)
            {
                if (_rechargeTimers[i] > 0f)
                {
                    _rechargeTimers[i] -= Time.deltaTime;
                    if (_rechargeTimers[i] <= 0f)
                    {
                        _rechargeTimers[i] = 0f;
                        if (BlinkCharges < GameConstants.FlankerBlinkCharges)
                            BlinkCharges++;
                    }
                }
            }
        }

        [Server]
        public void Execute(Vector2 direction)
        {
            if (BlinkCharges <= 0) return;
            if (_ship == null || !_ship.IsAlive) return;
            if (_energy == null || !_energy.UseEnergy(GameConstants.FlankerBlinkEnergyCost)) return;

            direction.Normalize();
            if (direction.sqrMagnitude < 0.01f)
                direction = (Vector2)transform.up;

            BlinkCharges--;

            // Start recharge for the consumed charge
            for (int i = 0; i < _rechargeTimers.Length; i++)
            {
                if (_rechargeTimers[i] <= 0f)
                {
                    _rechargeTimers[i] = GameConstants.FlankerBlinkRechargeTime;
                    break;
                }
            }

            Vector2 origin = (Vector2)transform.position;
            Vector2 targetPos = origin + direction * BlinkDistance;

            // Raycast for wall collision
            RaycastHit2D hit = Physics2D.Raycast(origin, direction, BlinkDistance);
            if (hit.collider != null)
            {
                // Check if the hit object is a wall (not a player)
                PlayerShip hitShip = hit.collider.GetComponent<PlayerShip>();
                if (hitShip == null)
                    targetPos = hit.point - direction * 0.2f;
            }

            Vector2 fromPos = origin;
            transform.position = (Vector3)targetPos;

            RpcBlinkEffect(fromPos, targetPos);
        }

        [ObserversRpc]
        private void RpcBlinkEffect(Vector2 from, Vector2 to)
        {
            // VFX handled by client-side systems
        }
    }
}

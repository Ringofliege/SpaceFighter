using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

namespace SpaceFighter
{
    public class DashSystem : NetworkBehaviour
    {
        [SyncVar]
        public int DashCharges = GameConstants.DashMaxCharges;

        private float[] _rechargeTimers = new float[GameConstants.DashMaxCharges];
        private PlayerShip _ship;
        private PlayerMovement _movement;
        private EnergySystem _energy;

        public bool CanDash
        {
            get
            {
                if (_ship == null || !_ship.IsAlive) return false;
                if (DashCharges <= 0) return false;
                if (_energy == null || _energy.CurrentEnergy < GameConstants.DashEnergyCost) return false;
                if (_ship.HasBarrierActive) return false;
                return true;
            }
        }

        private void Awake()
        {
            _ship = GetComponent<PlayerShip>();
            _movement = GetComponent<PlayerMovement>();
            _energy = GetComponent<EnergySystem>();
        }

        private void Update()
        {
            if (IsOwner && Input.GetKeyDown(KeyCode.Space))
            {
                Vector2 moveInput = new Vector2(
                    Input.GetKey(KeyCode.D) ? 1 : Input.GetKey(KeyCode.A) ? -1 : 0,
                    Input.GetKey(KeyCode.W) ? 1 : Input.GetKey(KeyCode.S) ? -1 : 0
                );
                CmdDash(moveInput.normalized);
            }

            if (!IsServerInitialized) return;

            // Recharge timers
            for (int i = 0; i < _rechargeTimers.Length; i++)
            {
                if (_rechargeTimers[i] > 0f)
                {
                    _rechargeTimers[i] -= Time.deltaTime;
                    if (_rechargeTimers[i] <= 0f)
                    {
                        _rechargeTimers[i] = 0f;
                        if (DashCharges < GameConstants.DashMaxCharges)
                            DashCharges++;
                    }
                }
            }
        }

        [ServerRpc]
        private void CmdDash(Vector2 clientMoveInput)
        {
            if (!CanDash) return;

            _energy.UseEnergy(GameConstants.DashEnergyCost);
            DashCharges--;

            // Start recharge for the consumed charge
            for (int i = 0; i < _rechargeTimers.Length; i++)
            {
                if (_rechargeTimers[i] <= 0f)
                {
                    _rechargeTimers[i] = GameConstants.DashRechargeTime;
                    break;
                }
            }

            // Dash in movement direction if provided, otherwise aim direction (transform.up)
            Vector2 dashDir = clientMoveInput.sqrMagnitude > 0.01f
                ? clientMoveInput.normalized
                : (Vector2)transform.up;

            _movement.ApplyForce(dashDir * GameConstants.DashBurstSpeed);
        }
    }
}

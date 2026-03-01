using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

namespace SpaceFighter
{
    public class EnergySystem : NetworkBehaviour
    {
        [SyncVar]
        public float CurrentEnergy = GameConstants.EnergyMax;

        private bool _isDamaged;
        private float _damageTimer;
        private float _continuousDrain;

        private void Update()
        {
            if (!IsServerInitialized) return;

            // Damage lockout timer
            if (_isDamaged)
            {
                _damageTimer -= Time.deltaTime;
                if (_damageTimer <= 0f)
                    _isDamaged = false;
            }

            // Continuous drain (e.g. barrier)
            if (_continuousDrain > 0f)
            {
                CurrentEnergy -= _continuousDrain * Time.deltaTime;
            }

            // Regen energy when not damaged
            if (!_isDamaged)
            {
                float regenRate = GameConstants.EnergyRegen;

                if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Overtime)
                    regenRate *= GameConstants.OvertimeRegenMult;

                CurrentEnergy += regenRate * Time.deltaTime;
            }

            CurrentEnergy = Mathf.Clamp(CurrentEnergy, 0f, GameConstants.EnergyMax);
        }

        [Server]
        public bool UseEnergy(float amount)
        {
            if (CurrentEnergy < amount)
                return false;

            CurrentEnergy -= amount;
            CurrentEnergy = Mathf.Max(CurrentEnergy, 0f);
            return true;
        }

        [Server]
        public void SetDamaged()
        {
            _isDamaged = true;
            _damageTimer = GameConstants.DamageLockoutDuration;
        }

        [Server]
        public void SetContinuousDrain(float rate)
        {
            _continuousDrain = rate;
        }

        [Server]
        public void ClearContinuousDrain()
        {
            _continuousDrain = 0f;
        }
    }
}

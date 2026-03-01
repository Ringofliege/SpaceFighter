using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

namespace SpaceFighter
{
    public class HeatSystem : NetworkBehaviour
    {
        [SyncVar]
        public float CurrentHeat;

        [SyncVar]
        public bool IsOverheated;

        private float _lockoutTimer;
        private bool _isFiring;

        public bool CanFire
        {
            get { return !IsOverheated; }
        }

        private void Update()
        {
            if (!IsServerInitialized) return;

            if (IsOverheated)
            {
                _lockoutTimer -= Time.deltaTime;
                if (_lockoutTimer <= 0f)
                {
                    IsOverheated = false;
                    CurrentHeat = 0f;
                }
                return;
            }

            // Decay heat when not firing
            if (!_isFiring && CurrentHeat > 0f)
            {
                CurrentHeat -= GameConstants.HeatDecayRate * Time.deltaTime;
                if (CurrentHeat < 0f)
                    CurrentHeat = 0f;
            }
        }

        [Server]
        public void AddHeat(float amount)
        {
            if (IsOverheated) return;

            CurrentHeat += amount;

            if (CurrentHeat >= GameConstants.HeatMax)
            {
                CurrentHeat = GameConstants.HeatMax;
                IsOverheated = true;
                _lockoutTimer = GameConstants.OverheatLockout;
            }
        }

        [Server]
        public void SetFiring(bool firing)
        {
            _isFiring = firing;
        }
    }
}

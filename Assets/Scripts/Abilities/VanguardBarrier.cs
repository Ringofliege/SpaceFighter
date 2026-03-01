using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

namespace SpaceFighter
{
    public class VanguardBarrier : NetworkBehaviour
    {
        [SyncVar] public bool IsActive;

        private int _rearModuleHP;
        private float _offlineTimer;
        private bool _isOffline;

        [Server]
        public void Activate()
        {
            if (_isOffline) return;
            IsActive = true;
            _rearModuleHP = GameConstants.VanguardBarrierRearModuleHP;
        }

        [Server]
        public void Deactivate()
        {
            IsActive = false;
        }

        [Server]
        public void DamageRearModule(int damage)
        {
            if (!IsActive) return;
            _rearModuleHP -= damage;
            if (_rearModuleHP <= 0)
            {
                _rearModuleHP = 0;
                ForceDropBarrier();
            }
        }

        [Server]
        public void ForceDropBarrier()
        {
            IsActive = false;
        }

        [Server]
        public void ApplyShieldOffline(float duration)
        {
            _isOffline = true;
            _offlineTimer = duration;
            IsActive = false;
        }

        private void Update()
        {
            if (!IsServerInitialized) return;

            if (_isOffline)
            {
                _offlineTimer -= Time.deltaTime;
                if (_offlineTimer <= 0f)
                    _isOffline = false;
            }
        }
    }
}

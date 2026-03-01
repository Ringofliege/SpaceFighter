using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

namespace SpaceFighter
{
    public class PlayerShip : NetworkBehaviour
    {
        [SyncVar]
        public int CurrentHP;

        [SyncVar]
        public int MaxHP;

        [SyncVar]
        public Team ShipTeam;

        [SyncVar]
        public ShipClass ShipClassType;

        [SyncVar]
        public bool IsAlive = true;

        public PlayerMovement Movement { get; private set; }
        public EnergySystem Energy { get; private set; }
        public HeatSystem Heat { get; private set; }
        public DashSystem Dash { get; private set; }
        public WeaponController Weapon { get; private set; }
        public Rigidbody2D Rb { get; private set; }
        public SpriteRenderer Sprite { get; private set; }

        private Collider2D _collider;
        private VanguardBarrier _barrier;

        private void Awake()
        {
            Movement = GetComponent<PlayerMovement>();
            Energy = GetComponent<EnergySystem>();
            Heat = GetComponent<HeatSystem>();
            Dash = GetComponent<DashSystem>();
            Weapon = GetComponent<WeaponController>();
            Rb = GetComponent<Rigidbody2D>();
            Sprite = GetComponent<SpriteRenderer>();
            _collider = GetComponent<Collider2D>();
            _barrier = GetComponent<VanguardBarrier>();
        }

        [Server]
        public void Initialize(ShipClass cls, Team team)
        {
            ShipClassType = cls;
            ShipTeam = team;
            IsAlive = true;

            MaxHP = cls == ShipClass.Vanguard ? GameConstants.VanguardHP : GameConstants.DefaultHP;
            CurrentHP = MaxHP;
        }

        public bool HasBarrierActive
        {
            get { return _barrier != null && _barrier.IsActive; }
        }

        public float DamageReduction
        {
            get { return ShipClassType == ShipClass.Vanguard ? GameConstants.VanguardDR : 0f; }
        }

        public float KnockbackResist
        {
            get { return ShipClassType == ShipClass.Vanguard ? GameConstants.KnockbackResist : 0f; }
        }

        [Server]
        public void TakeDamage(int amount, bool ignoresBarrier = false, bool ignoresDR = false,
            Vector2 knockbackDir = default, float knockbackForce = 0f)
        {
            if (!IsAlive) return;

            float damage = amount;

            // Barrier damage reduction (99%)
            if (HasBarrierActive && !ignoresBarrier)
            {
                // Check if hit from behind while barrier is active
                if (knockbackDir != default && DamageSystem.IsHitFromBehind(transform, knockbackDir))
                {
                    if (_barrier != null)
                        _barrier.DamageRearModule(amount);
                }

                damage *= 0.01f;
            }

            // Vanguard passive DR
            if (ShipClassType == ShipClass.Vanguard && !ignoresDR)
            {
                damage *= (1f - GameConstants.VanguardDR);
            }

            int finalDamage = Mathf.CeilToInt(damage);
            CurrentHP = Mathf.Max(CurrentHP - finalDamage, 0);

            // Apply micro-knockback
            if (knockbackDir != default && knockbackForce > 0f && Movement != null)
            {
                float resistMult = 1f - KnockbackResist;
                Movement.ApplyForce(knockbackDir.normalized * knockbackForce * resistMult);
            }

            // Flag energy system as damaged
            if (Energy != null)
                Energy.SetDamaged();

            // Interrupt stabilize channel
            StabilizeSystem stabilize = GetComponent<StabilizeSystem>();
            if (stabilize != null)
                stabilize.InterruptChannel();

            RpcTakeDamage();

            if (CurrentHP <= 0)
                Die();
        }

        [ObserversRpc]
        private void RpcTakeDamage()
        {
            if (Sprite != null)
            {
                Sprite.color = Color.red;
                Invoke(nameof(ResetColor), 0.1f);
            }
        }

        private void ResetColor()
        {
            if (Sprite != null)
                Sprite.color = Color.white;
        }

        [Server]
        public void Heal(int amount)
        {
            if (!IsAlive) return;
            CurrentHP = Mathf.Min(CurrentHP + amount, MaxHP);
        }

        [Server]
        private void Die()
        {
            IsAlive = false;

            if (_collider != null)
                _collider.enabled = false;

            if (GameManager.Instance != null)
                GameManager.Instance.OnPlayerDied(ShipTeam);

            RpcDie();
        }

        [ObserversRpc]
        private void RpcDie()
        {
            if (Sprite != null)
                Sprite.enabled = false;

            if (_collider != null)
                _collider.enabled = false;
        }
    }
}

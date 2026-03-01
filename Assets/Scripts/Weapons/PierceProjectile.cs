using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

namespace SpaceFighter
{
    public class PierceProjectile : NetworkBehaviour
    {
        [SyncVar] public Team OwnerTeam;
        [SyncVar] public int OwnerConnectionId;

        [SyncVar] private Vector2 _direction;

        private float _speed = GameConstants.PierceSpeed;
        private float _lifetime = 2f;
        private float _spawnTime;

        public void Initialize(Team team, Vector2 direction, int ownerId)
        {
            OwnerTeam = team;
            OwnerConnectionId = ownerId;
            _direction = direction.normalized;
            _spawnTime = Time.time;
        }

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();
            _spawnTime = Time.time;
            CreateVisuals();
        }

        private void Update()
        {
            transform.Translate(_direction * _speed * Time.deltaTime, Space.World);

            if (!IsServerInitialized) return;

            if (Time.time - _spawnTime >= _lifetime)
            {
                ServerManager.Despawn(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!IsServerInitialized) return;
            if (other.gameObject.layer == GameConstants.ProjectileLayer) return;

            // Pass through barrier colliders
            if (other.GetComponentInParent<VanguardBarrier>() != null
                && other.GetComponent<PlayerShip>() == null)
                return;

            PlayerShip target = other.GetComponent<PlayerShip>();
            if (target != null)
            {
                if (target.Owner != null && target.Owner.ClientId == OwnerConnectionId) return;

                int damage;
                if (target.ShipClassType != ShipClass.Vanguard)
                {
                    damage = Mathf.RoundToInt(target.MaxHP *
                        GameConstants.DisruptorPierceCannonStandardDmgPercent);

                    PlayerMovement movement = target.GetComponent<PlayerMovement>();
                    if (movement != null)
                        movement.ApplyStun(GameConstants.DisruptorPierceCannonStunDuration);
                }
                else
                {
                    damage = Mathf.RoundToInt(target.MaxHP *
                        GameConstants.DisruptorPierceCannonVanguardDmgPercent);

                    if (target.HasBarrierActive
                        && DamageSystem.IsHitFromBehind(target.transform, _direction))
                    {
                        VanguardBarrier barrier = target.GetComponent<VanguardBarrier>();
                        if (barrier != null)
                        {
                            barrier.ForceDropBarrier();
                            barrier.ApplyShieldOffline(3f);
                        }
                    }
                }

                DamageSystem.ApplyDamage(target, damage, true, false, _direction,
                    GameConstants.MicroKnockback);
                ServerManager.Despawn(gameObject);
                return;
            }

            // Wall or asteroid – destroy
            ServerManager.Despawn(gameObject);
        }

        private void CreateVisuals()
        {
            gameObject.layer = GameConstants.ProjectileLayer;

            Rigidbody2D rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.isKinematic = true;

            CircleCollider2D col = gameObject.AddComponent<CircleCollider2D>();
            col.radius = 0.2f;
            col.isTrigger = true;

            SpriteRenderer sr = gameObject.AddComponent<SpriteRenderer>();
            Texture2D tex = new Texture2D(8, 8);
            Color[] pixels = new Color[64];
            for (int i = 0; i < 64; i++) pixels[i] = Color.white;
            tex.SetPixels(pixels);
            tex.Apply();
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0.5f), 6f);

            Color pierceColor = new Color(0.8f, 1f, 1f);
            sr.color = pierceColor;
            sr.sortingOrder = 5;

            TrailRenderer trail = gameObject.AddComponent<TrailRenderer>();
            trail.startWidth = 0.2f;
            trail.endWidth = 0f;
            trail.time = 0.3f;
            trail.material = new Material(Shader.Find("Sprites/Default"));
            trail.startColor = pierceColor;
            trail.endColor = new Color(pierceColor.r, pierceColor.g, pierceColor.b, 0f);
            trail.sortingOrder = 4;
        }
    }
}

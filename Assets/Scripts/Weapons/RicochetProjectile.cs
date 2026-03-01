using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

namespace SpaceFighter
{
    public class RicochetProjectile : NetworkBehaviour
    {
        [SyncVar] public Team OwnerTeam;
        [SyncVar] public int OwnerConnectionId;
        [SyncVar] public int BounceCount;

        [SyncVar] private Vector2 _direction;
        [SyncVar] private float _speed;

        private int _maxBounces = 1;
        private float _lifetime = 3f;
        private float _spawnTime;

        public void Initialize(Team team, Vector2 direction, float speed, int ownerId)
        {
            OwnerTeam = team;
            OwnerConnectionId = ownerId;
            _direction = direction.normalized;
            _speed = speed;
            BounceCount = 0;
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
            if (other.gameObject.layer == 8) return;

            PlayerShip target = other.GetComponent<PlayerShip>();
            if (target != null)
            {
                if (target.Owner != null && target.Owner.ClientId == OwnerConnectionId) return;

                int damage = Mathf.RoundToInt(target.MaxHP *
                    GameConstants.StrikerRicochetShotDamagePercent);
                DamageSystem.ApplyDamage(target, damage, false, false, _direction,
                    GameConstants.MicroKnockback);
                ServerManager.Despawn(gameObject);
                return;
            }

            // Wall or asteroid – attempt bounce
            if (BounceCount < _maxBounces)
            {
                int layerMask = ~(1 << 8);
                RaycastHit2D hit = Physics2D.Raycast(
                    (Vector2)transform.position - _direction * 0.5f,
                    _direction, 2f, layerMask);

                if (hit.collider != null)
                {
                    _direction = Vector2.Reflect(_direction, hit.normal).normalized;
                    BounceCount++;
                    return;
                }
            }

            ServerManager.Despawn(gameObject);
        }

        private void CreateVisuals()
        {
            gameObject.layer = 8;

            Rigidbody2D rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.isKinematic = true;

            CircleCollider2D col = gameObject.AddComponent<CircleCollider2D>();
            col.radius = 0.15f;
            col.isTrigger = true;

            SpriteRenderer sr = gameObject.AddComponent<SpriteRenderer>();
            Texture2D tex = new Texture2D(6, 6);
            Color[] pixels = new Color[36];
            for (int i = 0; i < 36; i++) pixels[i] = Color.white;
            tex.SetPixels(pixels);
            tex.Apply();
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, 6, 6), new Vector2(0.5f, 0.5f), 6f);

            Color teamColor = OwnerTeam == Team.Blue ? new Color(0.4f, 0.8f, 1f) : new Color(1f, 0.5f, 0.3f);
            sr.color = teamColor;
            sr.sortingOrder = 4;

            TrailRenderer trail = gameObject.AddComponent<TrailRenderer>();
            trail.startWidth = 0.15f;
            trail.endWidth = 0f;
            trail.time = 0.25f;
            trail.material = new Material(Shader.Find("Sprites/Default"));
            trail.startColor = teamColor;
            trail.endColor = new Color(teamColor.r, teamColor.g, teamColor.b, 0f);
            trail.sortingOrder = 3;
        }
    }
}

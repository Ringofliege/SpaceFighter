using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

namespace SpaceFighter
{
    public class Projectile : NetworkBehaviour
    {
        [SyncVar] public int Damage;
        [SyncVar] public Team OwnerTeam;
        [SyncVar] public int OwnerConnectionId;

        [SyncVar] private Vector2 _direction;
        [SyncVar] private float _speed = 15f;

        private float _knockbackForce = 2f;
        private float _lifetime = 3f;
        private float _spawnTime;

        public void Initialize(int damage, Team team, Vector2 direction, float speed, int ownerId,
            float knockback = 2f)
        {
            Damage = damage;
            OwnerTeam = team;
            OwnerConnectionId = ownerId;
            _direction = direction.normalized;
            _speed = speed;
            _knockbackForce = knockback;
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

                DamageSystem.ApplyDamage(target, Damage, false, false, _direction, _knockbackForce);
                ServerManager.Despawn(gameObject);
                return;
            }

            // Wall or asteroid
            ServerManager.Despawn(gameObject);
        }

        private void CreateVisuals()
        {
            gameObject.layer = 8;

            Rigidbody2D rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.isKinematic = true;

            CircleCollider2D col = gameObject.AddComponent<CircleCollider2D>();
            col.radius = 0.1f;
            col.isTrigger = true;

            SpriteRenderer sr = gameObject.AddComponent<SpriteRenderer>();
            Texture2D tex = new Texture2D(4, 4);
            Color[] pixels = new Color[16];
            for (int i = 0; i < 16; i++) pixels[i] = Color.white;
            tex.SetPixels(pixels);
            tex.Apply();
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 8f);

            Color teamColor = OwnerTeam == Team.Blue ? Color.cyan : Color.red;
            sr.color = teamColor;
            sr.sortingOrder = 3;

            TrailRenderer trail = gameObject.AddComponent<TrailRenderer>();
            trail.startWidth = 0.08f;
            trail.endWidth = 0f;
            trail.time = 0.15f;
            trail.material = new Material(Shader.Find("Sprites/Default"));
            trail.startColor = teamColor;
            trail.endColor = new Color(teamColor.r, teamColor.g, teamColor.b, 0f);
            trail.sortingOrder = 2;
        }
    }
}

using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

namespace SpaceFighter
{
    public class EMPMine : NetworkBehaviour
    {
        [SyncVar] public bool IsArmed;
        [SyncVar] public Team MineTeam;

        private float _armTimer;
        private const float TriggerRadius = 2f;

        public override void OnStartServer()
        {
            base.OnStartServer();
            _armTimer = GameConstants.DisruptorEMPMineArmTime;
            IsArmed = false;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            CreateVisuals();
        }

        private void Update()
        {
            if (!IsServerInitialized) return;

            if (!IsArmed)
            {
                _armTimer -= Time.deltaTime;
                if (_armTimer <= 0f)
                    IsArmed = true;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!IsServerInitialized) return;
            if (!IsArmed) return;

            PlayerShip target = other.GetComponent<PlayerShip>();
            if (target == null) return;
            if (target.ShipTeam == MineTeam) return;
            if (!target.IsAlive) return;

            // Apply EMP effect to the target
            VanguardBarrier barrier = target.GetComponent<VanguardBarrier>();
            if (barrier != null)
                barrier.ApplyEMPDisable(GameConstants.DisruptorEMPMineBarrierDisableDuration);

            // Apply stun to movement
            PlayerMovement movement = target.GetComponent<PlayerMovement>();
            if (movement != null)
                movement.ApplyStun(GameConstants.DisruptorEMPMineBarrierDisableDuration);

            ServerManager.Despawn(gameObject);
        }

        private void CreateVisuals()
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr == null)
                sr = gameObject.AddComponent<SpriteRenderer>();

            Texture2D tex = new Texture2D(8, 8);
            Color fillColor = MineTeam == Team.Blue ? Color.cyan : Color.red;
            Color[] pixels = new Color[64];
            for (int i = 0; i < 64; i++)
                pixels[i] = fillColor;
            tex.SetPixels(pixels);
            tex.Apply();
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0.5f), 8f);
            sr.sortingOrder = 3;

            // Ensure trigger collider exists
            CircleCollider2D col = GetComponent<CircleCollider2D>();
            if (col == null)
            {
                col = gameObject.AddComponent<CircleCollider2D>();
                col.isTrigger = true;
                col.radius = TriggerRadius;
            }
        }
    }
}

using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

namespace SpaceFighter
{
    public class OvertimeCore : NetworkBehaviour
    {
        [SyncVar] public bool IsActive;
        [SyncVar] public Team CapturingTeam;
        [SyncVar] public float CaptureProgress;

        private CircleCollider2D _trigger;
        private SpriteRenderer _ringRenderer;
        private SpriteRenderer _fillRenderer;

        private void Awake()
        {
            _trigger = gameObject.GetComponent<CircleCollider2D>();
            if (_trigger == null) _trigger = gameObject.AddComponent<CircleCollider2D>();
            _trigger.isTrigger = true;
            _trigger.radius = 3f;

            _ringRenderer = GetComponent<SpriteRenderer>();
            if (_ringRenderer == null) _ringRenderer = gameObject.AddComponent<SpriteRenderer>();
            _ringRenderer.sprite = ProceduralSpriteGenerator.GenerateCoreSprite();
            _ringRenderer.sortingOrder = 2;
            _ringRenderer.enabled = false;

            // Inner fill indicator
            var fillGo = new GameObject("CoreFill");
            fillGo.transform.SetParent(transform, false);
            _fillRenderer = fillGo.AddComponent<SpriteRenderer>();
            _fillRenderer.sprite = ProceduralSpriteGenerator.GenerateCoreSprite();
            _fillRenderer.sortingOrder = 3;
            _fillRenderer.color = new Color(1f, 0.9f, 0.3f, 0.3f);
            _fillRenderer.enabled = false;
            fillGo.transform.localScale = Vector3.zero;
        }

        [Server]
        public void Activate()
        {
            IsActive = true;
            CaptureProgress = 0f;
            CapturingTeam = Team.None;
            RpcCoreStateChanged(true);
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            UpdateVisuals();
        }

        private void FixedUpdate()
        {
            if (!IsActive) return;

            if (IsServerInitialized)
                ServerCaptureTick();

            UpdateVisuals();
        }

        [Server]
        private void ServerCaptureTick()
        {
            int blueCount = 0;
            int redCount = 0;

            var hits = Physics2D.OverlapCircleAll(transform.position, _trigger.radius);
            foreach (var hit in hits)
            {
                var ship = hit.GetComponent<PlayerShip>();
                if (ship == null || !ship.IsAlive) continue;

                if (ship.ShipTeam == Team.Blue) blueCount++;
                else if (ship.ShipTeam == Team.Red) redCount++;
            }

            bool onlyBlue = blueCount > 0 && redCount == 0;
            bool onlyRed = redCount > 0 && blueCount == 0;

            if (onlyBlue)
            {
                if (CapturingTeam != Team.Blue)
                {
                    CapturingTeam = Team.Blue;
                    CaptureProgress = 0f;
                }
                CaptureProgress += Time.fixedDeltaTime;
            }
            else if (onlyRed)
            {
                if (CapturingTeam != Team.Red)
                {
                    CapturingTeam = Team.Red;
                    CaptureProgress = 0f;
                }
                CaptureProgress += Time.fixedDeltaTime;
            }
            else
            {
                CaptureProgress = 0f;
                CapturingTeam = Team.None;
            }

            if (CaptureProgress >= GameConstants.CoreCaptureTime)
            {
                if (GameManager.Instance != null && GameManager.Instance.RoundManager != null)
                    GameManager.Instance.RoundManager.OnCoreCaptured(CapturingTeam);
            }
        }

        private void UpdateVisuals()
        {
            _ringRenderer.enabled = IsActive;

            if (_fillRenderer != null)
            {
                _fillRenderer.enabled = IsActive && CaptureProgress > 0f;

                if (IsActive && CaptureProgress > 0f)
                {
                    float t = Mathf.Clamp01(CaptureProgress / GameConstants.CoreCaptureTime);
                    _fillRenderer.transform.localScale = Vector3.one * t;

                    Color c = CapturingTeam == Team.Blue
                        ? new Color(0.3f, 0.5f, 1f, 0.5f)
                        : new Color(1f, 0.4f, 0.3f, 0.5f);
                    _fillRenderer.color = c;
                }
            }
        }

        [ObserversRpc]
        private void RpcCoreStateChanged(bool active)
        {
            UpdateVisuals();
        }
    }
}

using FishNet.Object;
using System.Collections;
using UnityEngine;

namespace SpaceFighter
{
    public class WeaponController : NetworkBehaviour
    {
        [SerializeField] private GameObject _projectilePrefab;

        private PlayerShip _ship;
        private HeatSystem _heat;
        private EnergySystem _energy;

        private float _fireCooldown;
        private bool _overdriveActive;
        public float OverdriveFireRateMult = 1f;
        public float OverdriveHeatMult = 1f;

        private void Awake()
        {
            _ship = GetComponent<PlayerShip>();
            _heat = GetComponent<HeatSystem>();
            _energy = GetComponent<EnergySystem>();
        }

        private void Update()
        {
            if (IsServerInitialized && _fireCooldown > 0f)
                _fireCooldown -= Time.deltaTime;

            if (!IsOwner) return;

            if (Input.GetMouseButton(0))
            {
                Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2 aimDir = ((Vector2)mouseWorld - (Vector2)transform.position).normalized;
                if (aimDir.sqrMagnitude > 0.01f)
                    CmdFire(aimDir);
            }
        }

        [ServerRpc]
        private void CmdFire(Vector2 aimDirection)
        {
            if (!_ship.IsAlive) return;
            if (!_heat.CanFire) return;
            if (_fireCooldown > 0f) return;
            if (_ship.Movement != null && _ship.Movement.IsStunned) return;

            aimDirection.Normalize();

            switch (_ship.ShipClassType)
            {
                case ShipClass.Vanguard:
                    FireVanguard(aimDirection);
                    break;
                case ShipClass.Striker:
                    StartCoroutine(FireStrikerBurst(aimDirection));
                    break;
                case ShipClass.Disruptor:
                    FireDisruptor(aimDirection);
                    break;
                case ShipClass.Flanker:
                    FireFlanker(aimDirection);
                    break;
            }
        }

        [Server]
        private void FireVanguard(Vector2 dir)
        {
            _fireCooldown = GameConstants.VanguardFireRate * OverdriveFireRateMult;
            _heat.AddHeat(GameConstants.VanguardHeatPerShot * OverdriveHeatMult);
            float knockback = GameConstants.MicroKnockback * GameConstants.VanguardKnockbackMult;
            Vector2 spawnPos = (Vector2)transform.position + dir * 0.6f;
            SpawnProjectile(spawnPos, dir, GameConstants.VanguardDamage,
                GameConstants.ProjectileSpeed, knockback);
        }

        private IEnumerator FireStrikerBurst(Vector2 dir)
        {
            _fireCooldown = GameConstants.StrikerBurstInterval * OverdriveFireRateMult;
            _heat.AddHeat(GameConstants.StrikerHeatPerBurst * OverdriveHeatMult);

            for (int i = 0; i < GameConstants.StrikerShotsPerBurst; i++)
            {
                if (!_ship.IsAlive) yield break;
                Vector2 spawnPos = (Vector2)transform.position + dir * 0.6f;
                SpawnProjectile(spawnPos, dir, GameConstants.StrikerBurstDamage,
                    GameConstants.ProjectileSpeed, GameConstants.MicroKnockback);
                if (i < GameConstants.StrikerShotsPerBurst - 1)
                    yield return new WaitForSeconds(GameConstants.StrikerBurstDelay);
            }
        }

        [Server]
        private void FireDisruptor(Vector2 dir)
        {
            _fireCooldown = GameConstants.DisruptorFireRate * OverdriveFireRateMult;
            _heat.AddHeat(GameConstants.DisruptorHeatPerShot * OverdriveHeatMult);
            Vector2 spawnPos = (Vector2)transform.position + dir * 0.6f;
            SpawnProjectile(spawnPos, dir, GameConstants.DisruptorDamage,
                GameConstants.ProjectileSpeed, GameConstants.MicroKnockback);
        }

        [Server]
        private void FireFlanker(Vector2 dir)
        {
            _fireCooldown = GameConstants.FlankerFireRate * OverdriveFireRateMult;
            _heat.AddHeat(GameConstants.FlankerHeatPerShot * OverdriveHeatMult);

            float halfSpread = GameConstants.FlankerSpreadAngle * 0.5f;
            float step = GameConstants.FlankerSpreadAngle / (GameConstants.FlankerPellets - 1);

            for (int i = 0; i < GameConstants.FlankerPellets; i++)
            {
                float angle = -halfSpread + step * i;
                Vector2 pelletDir = RotateVector(dir, angle);
                Vector2 spawnPos = (Vector2)transform.position + pelletDir * 0.6f;
                SpawnProjectile(spawnPos, pelletDir, GameConstants.FlankerPelletDamage,
                    GameConstants.ProjectileSpeed, GameConstants.MicroKnockback);
            }
        }

        [Server]
        private void SpawnProjectile(Vector2 pos, Vector2 dir, int damage, float speed, float knockback)
        {
            if (_projectilePrefab == null) return;

            GameObject go = Instantiate(_projectilePrefab, pos, Quaternion.identity);
            Projectile proj = go.GetComponent<Projectile>();
            proj.Initialize(damage, _ship.ShipTeam, dir, speed, Owner.ClientId, knockback);
            ServerManager.Spawn(go);

            RpcSpawnProjectileVisual(pos, dir, _ship.ShipTeam);
        }

        [ObserversRpc]
        private void RpcSpawnProjectileVisual(Vector2 pos, Vector2 dir, Team team)
        {
            GameObject flash = new GameObject("MuzzleFlash");
            flash.transform.position = pos;

            SpriteRenderer sr = flash.AddComponent<SpriteRenderer>();
            Texture2D tex = new Texture2D(4, 4);
            Color[] pixels = new Color[16];
            for (int i = 0; i < 16; i++) pixels[i] = Color.white;
            tex.SetPixels(pixels);
            tex.Apply();
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
            sr.color = team == Team.Blue ? Color.cyan : Color.red;
            sr.sortingOrder = 5;

            Destroy(flash, 0.1f);
        }

        private static Vector2 RotateVector(Vector2 v, float degrees)
        {
            float rad = degrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);
            return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
        }
    }
}

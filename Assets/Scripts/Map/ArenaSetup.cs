using UnityEngine;

namespace SpaceFighter
{
    public class ArenaSetup : MonoBehaviour
    {
        [SerializeField] private OvertimeCore _overtimeCore;

        private void Start()
        {
            SetupCamera();
            CreateBoundaryWalls();
            CreateLaneDividers();
            CreateCenterFeature();
            CreateCore();
        }

        private void SetupCamera()
        {
            var cam = Camera.main;
            if (cam == null) return;
            cam.transform.position = new Vector3(0f, 0f, -10f);
            cam.orthographic = true;
            cam.orthographicSize = 14f;
            cam.backgroundColor = new Color(0.04f, 0.04f, 0.08f);
        }

        private void CreateBoundaryWalls()
        {
            // Top / Bottom
            CreateWall(new Vector2(0f, 12.5f), new Vector2(42f, 1f), 0f);
            CreateWall(new Vector2(0f, -12.5f), new Vector2(42f, 1f), 0f);
            // Left / Right
            CreateWall(new Vector2(-20f, 0f), new Vector2(1f, 26f), 0f);
            CreateWall(new Vector2(20f, 0f), new Vector2(1f, 26f), 0f);
        }

        private void CreateLaneDividers()
        {
            // Upper lane divider: asteroids at y=6, spread x=-5..5 with gaps
            CreateAsteroid(new Vector2(-5f, 6f), 1.2f);
            CreateAsteroid(new Vector2(-2f, 6.5f), 0.9f);
            CreateAsteroid(new Vector2(2f, 5.8f), 1.0f);
            CreateAsteroid(new Vector2(5f, 6.2f), 1.1f);

            // Lower lane divider: asteroids at y=-6
            CreateAsteroid(new Vector2(-5f, -6f), 1.1f);
            CreateAsteroid(new Vector2(-2f, -6.3f), 1.0f);
            CreateAsteroid(new Vector2(2f, -5.7f), 0.9f);
            CreateAsteroid(new Vector2(5f, -6.1f), 1.2f);
        }

        private void CreateCenterFeature()
        {
            // Two angled walls near center for ricochet gameplay
            CreateWall(new Vector2(-2f, 0f), new Vector2(4f, 0.5f), 45f);
            CreateWall(new Vector2(2f, 0f), new Vector2(4f, 0.5f), -45f);
        }

        private void CreateCore()
        {
            var coreGo = new GameObject("OvertimeCore");
            coreGo.transform.position = Vector3.zero;

            var sr = coreGo.AddComponent<SpriteRenderer>();
            sr.sprite = ProceduralSpriteGenerator.GenerateCoreSprite();
            sr.sortingOrder = 2;
            sr.enabled = false;

            var core = coreGo.AddComponent<OvertimeCore>();
            _overtimeCore = core;

            coreGo.SetActive(true);
        }

        private void CreateWall(Vector2 pos, Vector2 size, float angle)
        {
            var go = new GameObject("Wall");
            go.transform.position = pos;
            go.transform.rotation = Quaternion.Euler(0f, 0f, angle);
            go.layer = 6;

            var col = go.AddComponent<BoxCollider2D>();
            col.size = size;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = ProceduralSpriteGenerator.GenerateWallSprite();
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.size = size;
            sr.color = new Color(0.5f, 0.5f, 0.5f);
            sr.sortingOrder = 1;

            go.transform.SetParent(transform);
        }

        private void CreateAsteroid(Vector2 pos, float radius)
        {
            var go = new GameObject("Asteroid");
            go.transform.position = pos;
            go.layer = 9;

            var col = go.AddComponent<CircleCollider2D>();
            col.radius = radius;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = ProceduralSpriteGenerator.GenerateAsteroidSprite();
            sr.sortingOrder = 1;

            float scale = radius / 0.5f;
            go.transform.localScale = new Vector3(scale, scale, 1f);

            go.transform.SetParent(transform);
        }
    }
}

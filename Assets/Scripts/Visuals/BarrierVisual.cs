using UnityEngine;

namespace SpaceFighter
{
    public class BarrierVisual : MonoBehaviour
    {
        private LineRenderer _line;
        private VanguardBarrier _barrier;
        private PlayerShip _ship;

        private const int Segments = 20;
        private const float Radius = 2f;
        private const float ArcDegrees = 180f;

        private void Start()
        {
            _ship = GetComponent<PlayerShip>();
            _barrier = GetComponent<VanguardBarrier>();

            _line = gameObject.AddComponent<LineRenderer>();
            _line.useWorldSpace = false;
            _line.positionCount = Segments + 1;
            _line.startWidth = 0.12f;
            _line.endWidth = 0.12f;
            _line.material = new Material(Shader.Find("Sprites/Default"));
            _line.sortingOrder = 5;
            _line.enabled = false;
        }

        private void Update()
        {
            if (_barrier == null || _ship == null)
                return;

            bool active = _barrier.IsBarrierActive;
            _line.enabled = active;
            if (!active) return;

            Color teamColor = _ship.ShipTeam == Team.Blue
                ? new Color(0.3f, 0.5f, 1f, 0.6f)
                : new Color(1f, 0.4f, 0.3f, 0.6f);

            // Pulse effect when energy is low
            if (_ship.Energy != null && _ship.Energy.CurrentEnergy < 30f)
            {
                float pulse = Mathf.PingPong(Time.time * 3f, 1f);
                teamColor.a = Mathf.Lerp(0.2f, 0.7f, pulse);
            }

            _line.startColor = teamColor;
            _line.endColor = teamColor;

            float startAngle = 90f - ArcDegrees / 2f;
            float step = ArcDegrees / Segments;

            for (int i = 0; i <= Segments; i++)
            {
                float angle = (startAngle + step * i) * Mathf.Deg2Rad;
                float x = Mathf.Cos(angle) * Radius;
                float y = Mathf.Sin(angle) * Radius;
                _line.SetPosition(i, new Vector3(x, y, 0f));
            }
        }
    }
}

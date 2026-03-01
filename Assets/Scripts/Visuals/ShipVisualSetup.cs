using System.Collections;
using UnityEngine;

namespace SpaceFighter
{
    public class ShipVisualSetup : MonoBehaviour
    {
        private PlayerShip _ship;
        private SpriteRenderer _outlineRenderer;
        private CloakAbility _cloak;
        private SpriteRenderer _mainRenderer;

        private void Start()
        {
            StartCoroutine(WaitForInit());
        }

        private IEnumerator WaitForInit()
        {
            _ship = GetComponent<PlayerShip>();
            while (_ship == null || _ship.ShipTeam == Team.None)
            {
                yield return null;
                if (_ship == null) _ship = GetComponent<PlayerShip>();
            }

            _mainRenderer = _ship.Sprite;
            if (_mainRenderer == null)
                _mainRenderer = GetComponent<SpriteRenderer>();

            Sprite shipSprite = ProceduralSpriteGenerator.GenerateShipSprite(
                _ship.ShipClassType, _ship.ShipTeam);
            _mainRenderer.sprite = shipSprite;
            _mainRenderer.sortingOrder = 10;

            // Outline child
            var outlineGo = new GameObject("Outline");
            outlineGo.transform.SetParent(transform, false);
            outlineGo.transform.localScale = Vector3.one * 1.15f;

            _outlineRenderer = outlineGo.AddComponent<SpriteRenderer>();
            _outlineRenderer.sprite = shipSprite;

            Color outlineColor = _ship.ShipTeam == Team.Blue
                ? new Color(0.1f, 0.2f, 0.5f, 0.8f)
                : new Color(0.5f, 0.15f, 0.1f, 0.8f);
            _outlineRenderer.color = outlineColor;
            _outlineRenderer.sortingOrder = 9;

            _cloak = GetComponent<CloakAbility>();
        }

        private void Update()
        {
            if (_cloak == null || _mainRenderer == null) return;

            float targetAlpha = _cloak.IsCloaked ? (_ship.IsOwner ? 0.3f : 0f) : 1f;

            Color c = _mainRenderer.color;
            c.a = Mathf.MoveTowards(c.a, targetAlpha, Time.deltaTime * 5f);
            _mainRenderer.color = c;

            if (_outlineRenderer != null)
            {
                Color oc = _outlineRenderer.color;
                oc.a = c.a * 0.8f;
                _outlineRenderer.color = oc;
            }
        }
    }
}

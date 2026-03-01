using UnityEngine;
using UnityEngine.UI;

namespace SpaceFighter
{
    public class GameHUD : MonoBehaviour
    {
        private PlayerShip _localShip;
        private AbilityController _abilityController;
        private bool _initialized;

        // HP / Energy / Heat bars
        private Image _hpFill;
        private Image _energyFill;
        private Image _heatFill;

        // Ability cooldowns
        private Text _ability1Text;
        private Image _ability1Icon;
        private Text _ability2Text;
        private Image _ability2Icon;

        // Dash
        private Text _dashText;

        // Stabilize
        private Text _stabilizeText;
        private GameObject _stabilizeRoot;

        // Round info
        private Text _roundTimerText;
        private Text _aliveCounterText;

        // Overtime
        private Text _overtimeText;
        private float _overtimeFlashTimer;
        private bool _overtimeVisible;

        // Overheat
        private Text _overheatText;
        private float _overheatFlashTimer;
        private bool _overheatVisible;

        private Canvas _canvas;

        private static readonly Color PanelColor = new Color(0f, 0f, 0f, 0.6f);
        private static readonly Color HpColor = new Color(0.9f, 0.15f, 0.15f, 1f);
        private static readonly Color EnergyColor = new Color(0.2f, 0.5f, 1f, 1f);
        private static readonly Color HeatColor = new Color(1f, 0.55f, 0.1f, 1f);
        private static readonly Color BrightText = new Color(1f, 1f, 1f, 0.95f);
        private static readonly Color DimText = new Color(0.7f, 0.7f, 0.7f, 0.8f);

        private void OnEnable()
        {
            SetupCanvas();
            CreateAllElements();
            _initialized = true;
        }

        private void SetupCanvas()
        {
            _canvas = GetComponent<Canvas>();
            if (_canvas == null) _canvas = gameObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 100;

            if (GetComponent<CanvasScaler>() == null)
            {
                var scaler = gameObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
            }

            if (GetComponent<GraphicRaycaster>() == null)
                gameObject.AddComponent<GraphicRaycaster>();
        }

        private void CreateAllElements()
        {
            // HP bar - left side
            var hpPanel = CreatePanel("HPPanel", new Vector2(220, 28), new Vector2(130, -30), TextAnchor.UpperLeft);
            _hpFill = CreateFillBar(hpPanel, "HPFill", HpColor);

            // Energy bar - below HP
            var energyPanel = CreatePanel("EnergyPanel", new Vector2(220, 22), new Vector2(130, -65), TextAnchor.UpperLeft);
            _energyFill = CreateFillBar(energyPanel, "EnergyFill", EnergyColor);

            // Heat bar - below Energy
            var heatPanel = CreatePanel("HeatPanel", new Vector2(220, 18), new Vector2(130, -95), TextAnchor.UpperLeft);
            _heatFill = CreateFillBar(heatPanel, "HeatFill", HeatColor);

            // Ability 1 - right of center-bottom
            var ab1Root = CreatePanel("Ability1", new Vector2(140, 50), new Vector2(80, 60), TextAnchor.LowerCenter);
            _ability1Icon = CreateIcon(ab1Root, "Ab1Icon", new Vector2(-40, 0), new Vector2(36, 36));
            _ability1Text = CreateTextElement(ab1Root, "Ab1Text", "Ready", 14, BrightText, TextAnchor.MiddleCenter,
                new Vector2(60, 36), new Vector2(20, 0));

            // Ability 2 - right of ability 1
            var ab2Root = CreatePanel("Ability2", new Vector2(140, 50), new Vector2(230, 60), TextAnchor.LowerCenter);
            _ability2Icon = CreateIcon(ab2Root, "Ab2Icon", new Vector2(-40, 0), new Vector2(36, 36));
            _ability2Text = CreateTextElement(ab2Root, "Ab2Text", "Ready", 14, BrightText, TextAnchor.MiddleCenter,
                new Vector2(60, 36), new Vector2(20, 0));

            // Dash charges - bottom left-ish
            var dashPanel = CreatePanel("DashPanel", new Vector2(140, 30), new Vector2(-80, 60), TextAnchor.LowerCenter);
            _dashText = CreateTextElement(dashPanel, "DashText", "Dash: 2/2", 16, BrightText, TextAnchor.MiddleCenter,
                new Vector2(130, 26), Vector2.zero);

            // Stabilize indicator
            _stabilizeRoot = CreatePanel("StabilizePanel", new Vector2(160, 30), new Vector2(-80, 100), TextAnchor.LowerCenter);
            _stabilizeText = CreateTextElement(_stabilizeRoot, "StabText", "Available", 14, DimText,
                TextAnchor.MiddleCenter, new Vector2(150, 26), Vector2.zero);

            // Round timer - top center
            var timerPanel = CreatePanel("TimerPanel", new Vector2(160, 50), new Vector2(0, -30), TextAnchor.UpperCenter);
            _roundTimerText = CreateTextElement(timerPanel, "TimerText", "1:15", 28, BrightText,
                TextAnchor.MiddleCenter, new Vector2(150, 44), Vector2.zero);

            // Alive counter - top left
            var alivePanel = CreatePanel("AlivePanel", new Vector2(140, 40), new Vector2(90, -30), TextAnchor.UpperLeft);
            _aliveCounterText = CreateTextElement(alivePanel, "AliveText", "5 v 5", 20, BrightText,
                TextAnchor.MiddleCenter, new Vector2(130, 34), Vector2.zero);

            // Overtime indicator - center screen
            var otPanel = CreatePanel("OvertimePanel", new Vector2(320, 60), new Vector2(0, 80), TextAnchor.MiddleCenter);
            _overtimeText = CreateTextElement(otPanel, "OTText", "OVERTIME", 36, new Color(1f, 0.85f, 0.1f, 1f),
                TextAnchor.MiddleCenter, new Vector2(300, 54), Vector2.zero);
            otPanel.SetActive(false);

            // Overheat warning
            var ohPanel = CreatePanel("OverheatPanel", new Vector2(300, 50), new Vector2(0, 140), TextAnchor.MiddleCenter);
            _overheatText = CreateTextElement(ohPanel, "OHText", "OVERHEATED", 30, new Color(1f, 0.3f, 0f, 1f),
                TextAnchor.MiddleCenter, new Vector2(280, 44), Vector2.zero);
            ohPanel.SetActive(false);
        }

        private void Update()
        {
            CacheLocalPlayer();
            if (_localShip == null) return;

            UpdateBars();
            UpdateAbilities();
            UpdateDash();
            UpdateStabilize();
            UpdateRoundInfo();
            UpdateOvertime();
            UpdateOverheat();
        }

        private void CacheLocalPlayer()
        {
            if (_localShip != null && _localShip.IsAlive) return;

            var ships = FindObjectsOfType<PlayerShip>();
            foreach (var s in ships)
            {
                if (s.IsOwner)
                {
                    _localShip = s;
                    _abilityController = s.GetComponent<AbilityController>();
                    UpdateStabilizeVisibility();
                    return;
                }
            }
        }

        private void UpdateBars()
        {
            float hpRatio = _localShip.MaxHP > 0 ? (float)_localShip.CurrentHP / _localShip.MaxHP : 0f;
            _hpFill.fillAmount = Mathf.Clamp01(hpRatio);

            float energyRatio = _localShip.Energy != null
                ? _localShip.Energy.CurrentEnergy / GameConstants.EnergyMax
                : 0f;
            _energyFill.fillAmount = Mathf.Clamp01(energyRatio);

            float heatRatio = _localShip.Heat != null
                ? _localShip.Heat.CurrentHeat / GameConstants.HeatMax
                : 0f;
            _heatFill.fillAmount = Mathf.Clamp01(heatRatio);
        }

        private void UpdateAbilities()
        {
            if (_abilityController == null) return;

            float cd1 = _abilityController.Ability1Cooldown;
            _ability1Text.text = cd1 > 0 ? cd1.ToString("F1") : "Ready";
            _ability1Text.color = cd1 > 0 ? DimText : BrightText;

            float cd2 = _abilityController.Ability2Cooldown;
            _ability2Text.text = cd2 > 0 ? cd2.ToString("F1") : "Ready";
            _ability2Text.color = cd2 > 0 ? DimText : BrightText;
        }

        private void UpdateDash()
        {
            if (_localShip.Dash == null) return;
            int charges = _localShip.Dash.DashCharges;
            _dashText.text = $"Dash: {charges}/{GameConstants.DashMaxCharges}";
            _dashText.color = charges > 0 ? BrightText : DimText;
        }

        private void UpdateStabilize()
        {
            if (_localShip.ShipClassType == ShipClass.Vanguard) return;

            var stab = _localShip.GetComponent<StabilizeSystem>();
            if (stab == null) return;

            if (stab.IsChanneling)
                _stabilizeText.text = "Channeling...";
            else if (stab.HasUsedStabilize)
                _stabilizeText.text = "Used";
            else
                _stabilizeText.text = "Available";

            _stabilizeText.color = stab.HasUsedStabilize ? DimText : BrightText;
        }

        private void UpdateStabilizeVisibility()
        {
            if (_stabilizeRoot == null) return;
            _stabilizeRoot.SetActive(_localShip != null && _localShip.ShipClassType != ShipClass.Vanguard);
        }

        private void UpdateRoundInfo()
        {
            if (GameManager.Instance == null) return;

            var rm = GameManager.Instance.RoundManager;
            if (rm != null)
            {
                float t = Mathf.Max(0f, rm.RoundTimer);
                int min = Mathf.FloorToInt(t / 60f);
                int sec = Mathf.FloorToInt(t % 60f);
                _roundTimerText.text = $"{min}:{sec:D2}";
            }

            int blue = GameManager.Instance.BlueAlive;
            int red = GameManager.Instance.RedAlive;
            _aliveCounterText.text = $"{blue} v {red}";
        }

        private void UpdateOvertime()
        {
            if (GameManager.Instance == null) return;
            var rm = GameManager.Instance.RoundManager;
            bool isOvertime = rm != null && rm.OvertimeActive;

            var otGo = _overtimeText.transform.parent.gameObject;
            if (isOvertime && !otGo.activeSelf)
                otGo.SetActive(true);
            else if (!isOvertime && otGo.activeSelf)
                otGo.SetActive(false);

            if (!isOvertime) return;

            _overtimeFlashTimer += Time.deltaTime;
            if (_overtimeFlashTimer >= 0.5f)
            {
                _overtimeFlashTimer = 0f;
                _overtimeVisible = !_overtimeVisible;
                var c = _overtimeText.color;
                c.a = _overtimeVisible ? 1f : 0.3f;
                _overtimeText.color = c;
            }
        }

        private void UpdateOverheat()
        {
            bool overheated = _localShip.Heat != null && _localShip.Heat.IsOverheated;

            var ohGo = _overheatText.transform.parent.gameObject;
            if (overheated && !ohGo.activeSelf)
                ohGo.SetActive(true);
            else if (!overheated && ohGo.activeSelf)
                ohGo.SetActive(false);

            if (!overheated) return;

            _overheatFlashTimer += Time.deltaTime;
            if (_overheatFlashTimer >= 0.35f)
            {
                _overheatFlashTimer = 0f;
                _overheatVisible = !_overheatVisible;
                var c = _overheatText.color;
                c.a = _overheatVisible ? 1f : 0.2f;
                _overheatText.color = c;
            }
        }

        // ── Helper Methods ───────────────────────────────────────────────

        private GameObject CreatePanel(string name, Vector2 size, Vector2 offset, TextAnchor anchor)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(_canvas.transform, false);

            var rt = go.GetComponent<RectTransform>();
            SetAnchorFromAlignment(rt, anchor);
            rt.anchoredPosition = offset;
            rt.sizeDelta = size;

            var img = go.GetComponent<Image>();
            img.color = PanelColor;
            img.raycastTarget = false;

            return go;
        }

        private Image CreateFillBar(GameObject parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent.transform, false);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(2, 2);
            rt.offsetMax = new Vector2(-2, -2);

            var img = go.GetComponent<Image>();
            img.color = color;
            img.type = Image.Type.Filled;
            img.fillMethod = Image.FillMethod.Horizontal;
            img.fillAmount = 1f;
            img.raycastTarget = false;

            return img;
        }

        private Text CreateTextElement(GameObject parent, string name, string text, int fontSize, Color color,
            TextAnchor alignment, Vector2 size, Vector2 offset)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent.transform, false);

            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = offset;
            rt.sizeDelta = size;

            var t = go.GetComponent<Text>();
            t.text = text;
            t.fontSize = fontSize;
            t.color = color;
            t.alignment = alignment;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.raycastTarget = false;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;

            return t;
        }

        private Image CreateIcon(GameObject parent, string name, Vector2 offset, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent.transform, false);

            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = offset;
            rt.sizeDelta = size;

            var img = go.GetComponent<Image>();
            img.color = new Color(1f, 1f, 1f, 0.4f);
            img.raycastTarget = false;

            return img;
        }

        private static void SetAnchorFromAlignment(RectTransform rt, TextAnchor anchor)
        {
            float ax = 0.5f, ay = 0.5f;
            switch (anchor)
            {
                case TextAnchor.UpperLeft:    ax = 0f; ay = 1f; break;
                case TextAnchor.UpperCenter:  ax = 0.5f; ay = 1f; break;
                case TextAnchor.UpperRight:   ax = 1f; ay = 1f; break;
                case TextAnchor.MiddleLeft:   ax = 0f; ay = 0.5f; break;
                case TextAnchor.MiddleCenter: ax = 0.5f; ay = 0.5f; break;
                case TextAnchor.MiddleRight:  ax = 1f; ay = 0.5f; break;
                case TextAnchor.LowerLeft:    ax = 0f; ay = 0f; break;
                case TextAnchor.LowerCenter:  ax = 0.5f; ay = 0f; break;
                case TextAnchor.LowerRight:   ax = 1f; ay = 0f; break;
            }
            rt.anchorMin = new Vector2(ax, ay);
            rt.anchorMax = new Vector2(ax, ay);
            rt.pivot = new Vector2(0.5f, 0.5f);
        }
    }
}

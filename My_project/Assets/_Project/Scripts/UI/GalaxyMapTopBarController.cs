using UnityEngine;
using UnityEngine.UIElements;

namespace _Project.Scripts.UI
{
    /// <summary>UI Toolkit верхняя панель режимов галактической карты.</summary>
    public class GalaxyMapTopBarController : MonoBehaviour
    {
        [Header("Text")]
        [SerializeField] private string title = "GALAXY MAP MODES";
        [SerializeField] private string[] toggleLabels = new string[5]
        {
            "Overview",
            "Resources",
            "Routes",
            "Influence",
            "Signals"
        };

        [Header("Layout")]
        [SerializeField] private Vector2 panelSize = new Vector2(0f, 44f);
        [SerializeField] private Vector4 panelPadding = new Vector4(12f, 12f, 8f, 8f); // left, right, top, bottom
        [SerializeField] private float toggleSpacing = 8f;
        [SerializeField] private Vector2 toggleSize = new Vector2(120f, 26f);
        [SerializeField] private float toggleCornerRadius = 8f;

        [Header("Colors")]
        [SerializeField] private Color panelColor = new Color(0.07f, 0.10f, 0.16f, 0.90f);
        [SerializeField, Range(0f, 1f)] private float panelOpacity = 1f;
        [SerializeField] private Color toggleOnColor = new Color(0.20f, 0.45f, 0.70f, 0.95f);
        [SerializeField] private Color toggleOffColor = new Color(0.12f, 0.15f, 0.20f, 0.90f);
        [SerializeField] private Color toggleBorderColor = new Color(0.55f, 0.65f, 0.80f, 0.35f);
        [SerializeField] private Color labelColor = new Color(0.88f, 0.93f, 1f, 1f);
        [SerializeField] private Color checkmarkColor = new Color(0.95f, 0.98f, 1f, 1f);
        [SerializeField] private int labelFontSize = 12;

        private UIDocument _doc;
        private VisualElement _root;
        private VisualElement _panel;
        private VisualElement _toggleRow;
        private Label _titleLabel;
        private Toggle[] _toggles;
        private EventCallback<ChangeEvent<bool>> _toggleCallback;

        private void OnEnable()
        {
            if (!TryResolveElements())
                return;

            _toggleCallback = OnToggleChanged;
            RegisterToggleCallbacks();
            ApplyAllSettings();
        }

        private void OnDisable()
        {
            UnregisterToggleCallbacks();
        }

        private void OnValidate()
        {
            if (!isActiveAndEnabled)
                return;

            if (!TryResolveElements())
                return;

            ApplyAllSettings();
        }

        private bool TryResolveElements()
        {
            _doc = GetComponent<UIDocument>();
            if (_doc == null)
            {
                Debug.LogError("[GalaxyMapTopBar] UIDocument not found.");
                return false;
            }

            _root = _doc.rootVisualElement;
            if (_root.childCount == 0 && _doc.visualTreeAsset != null)
            {
                _root.Clear();
                _doc.visualTreeAsset.CloneTree(_root);
            }

            _panel = _root.Q<VisualElement>("GalaxyMapTopBar");
            _toggleRow = _root.Q<VisualElement>("ToggleRow");
            _titleLabel = _root.Q<Label>("TitleLabel");

            _toggles = new Toggle[5];
            for (int i = 0; i < _toggles.Length; i++)
                _toggles[i] = _root.Q<Toggle>($"ModeToggle{i + 1}");

            return _panel != null && _toggleRow != null;
        }

        private void RegisterToggleCallbacks()
        {
            if (_toggles == null)
                return;

            foreach (var toggle in _toggles)
            {
                if (toggle == null)
                    continue;

                toggle.UnregisterValueChangedCallback(_toggleCallback);
                toggle.RegisterValueChangedCallback(_toggleCallback);
            }
        }

        private void UnregisterToggleCallbacks()
        {
            if (_toggles == null || _toggleCallback == null)
                return;

            foreach (var toggle in _toggles)
            {
                if (toggle == null)
                    continue;

                toggle.UnregisterValueChangedCallback(_toggleCallback);
            }
        }

        private void ApplyAllSettings()
        {
            ApplyPanelSettings();
            ApplyTitleSettings();
            ApplyToggleText();
            ApplyToggleVisuals();
        }

        private void ApplyPanelSettings()
        {
            if (_panel == null)
                return;

            _panel.style.backgroundColor = panelColor;
            _panel.style.opacity = Mathf.Clamp01(panelOpacity);

            _panel.style.paddingLeft = panelPadding.x;
            _panel.style.paddingRight = panelPadding.y;
            _panel.style.paddingTop = panelPadding.z;
            _panel.style.paddingBottom = panelPadding.w;

            _panel.style.width = panelSize.x > 0f ? panelSize.x : StyleKeyword.Auto;
            _panel.style.height = panelSize.y > 0f ? panelSize.y : StyleKeyword.Auto;
        }

        private void ApplyTitleSettings()
        {
            if (_titleLabel == null)
                return;

            if (string.IsNullOrWhiteSpace(title))
            {
                _titleLabel.style.display = DisplayStyle.None;
            }
            else
            {
                _titleLabel.style.display = DisplayStyle.Flex;
                _titleLabel.text = title;
            }
        }

        private void ApplyToggleText()
        {
            if (_toggles == null)
                return;

            for (int i = 0; i < _toggles.Length; i++)
            {
                var toggle = _toggles[i];
                if (toggle == null)
                    continue;

                if (toggleLabels != null && i < toggleLabels.Length && !string.IsNullOrWhiteSpace(toggleLabels[i]))
                    toggle.text = toggleLabels[i];
            }
        }

        private void ApplyToggleVisuals()
        {
            if (_toggles == null)
                return;

            for (int i = 0; i < _toggles.Length; i++)
            {
                var toggle = _toggles[i];
                if (toggle == null)
                    continue;

                ApplyToggleVisuals(toggle, i == _toggles.Length - 1);
            }
        }

        private void ApplyToggleVisuals(Toggle toggle, bool isLast)
        {
            var isOn = toggle.value;
            var bgColor = isOn ? toggleOnColor : toggleOffColor;

            toggle.style.backgroundColor = bgColor;
            toggle.style.borderTopColor = toggleBorderColor;
            toggle.style.borderRightColor = toggleBorderColor;
            toggle.style.borderBottomColor = toggleBorderColor;
            toggle.style.borderLeftColor = toggleBorderColor;
            toggle.style.borderTopWidth = 1f;
            toggle.style.borderRightWidth = 1f;
            toggle.style.borderBottomWidth = 1f;
            toggle.style.borderLeftWidth = 1f;

            toggle.style.marginRight = isLast ? 0f : toggleSpacing;

            if (toggleSize.x > 0f)
                toggle.style.width = toggleSize.x;
            if (toggleSize.y > 0f)
                toggle.style.height = toggleSize.y;

            if (toggleCornerRadius > 0f)
            {
                toggle.style.borderTopLeftRadius = toggleCornerRadius;
                toggle.style.borderTopRightRadius = toggleCornerRadius;
                toggle.style.borderBottomLeftRadius = toggleCornerRadius;
                toggle.style.borderBottomRightRadius = toggleCornerRadius;
            }

            var label = toggle.Q<Label>(className: "unity-toggle__text");
            if (label != null)
            {
                label.style.color = labelColor;
                if (labelFontSize > 0)
                    label.style.fontSize = labelFontSize;
            }

            var input = toggle.Q<VisualElement>(className: "unity-toggle__input");
            if (input != null)
            {
                input.style.width = 16f;
                input.style.height = 16f;
                input.style.marginRight = 6f;
            }

            var checkmark = toggle.Q<VisualElement>(className: "unity-toggle__checkmark");
            if (checkmark != null)
                checkmark.style.unityBackgroundImageTintColor = checkmarkColor;
        }

        private void OnToggleChanged(ChangeEvent<bool> evt)
        {
            if (evt.target is Toggle toggle)
            {
                var index = _toggles != null ? System.Array.IndexOf(_toggles, toggle) : -1;
                var isLast = _toggles != null && index == _toggles.Length - 1;
                ApplyToggleVisuals(toggle, isLast);
            }
        }
    }
}

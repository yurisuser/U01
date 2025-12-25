using _Project.Scripts.Core;
using _Project.Scripts.Core.GameState;
using UnityEngine;
using UnityEngine.UIElements;

namespace _Project.Scripts.UI
{
    /// <summary>UI Toolkit верхняя панель режимов карты.</summary>
    public sealed class GalaxyMapTopBarController : MonoBehaviour
    {
        [Header("Colors")]
        [SerializeField] private Color panelBackground = new Color(0.07f, 0.10f, 0.16f, 0.90f);
        [SerializeField] private Color linkOnColor = new Color(0.20f, 0.45f, 0.70f, 0.95f);
        [SerializeField] private Color linkOffColor = new Color(0.12f, 0.15f, 0.20f, 0.90f);
        [SerializeField] private Color linkDisabledColor = new Color(0.25f, 0.25f, 0.28f, 0.6f);

        private UIDocument _doc;
        private VisualElement _root;
        private VisualElement _panel;
        private VisualElement _linkButton;
        private Label _linkLabel;
        private GameStateService _state;

        private EventCallback<ClickEvent> _onClick;

        private void OnEnable()
        {
            if (!TryResolveElements())
                return;

            _state = GameBootstrap.GameState;
            if (_state != null)
                _state.StateChanged += OnStateChanged;

            _onClick = OnLinkClicked;
            _linkButton?.UnregisterCallback(_onClick);
            _linkButton?.RegisterCallback(_onClick);

            ApplyPanelBackground();
            RefreshLinkVisual();
        }

        private void OnDisable()
        {
            if (_state != null)
                _state.StateChanged -= OnStateChanged;
            _state = null;

            if (_linkButton != null && _onClick != null)
                _linkButton.UnregisterCallback(_onClick);
        }

        private void OnValidate()
        {
            if (!isActiveAndEnabled)
                return;

            if (!TryResolveElements())
                return;

            ApplyPanelBackground();
            RefreshLinkVisual();
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
            _linkButton = _root.Q<VisualElement>("VisualElement1");
            _linkLabel = _linkButton?.Q<Label>();

            return _panel != null && _linkButton != null;
        }

        private void ApplyPanelBackground()
        {
            if (_panel == null)
                return;

            _panel.style.backgroundColor = panelBackground;
        }

        private void OnStateChanged()
        {
            RefreshLinkVisual();
        }

        private void OnLinkClicked(ClickEvent evt)
        {
            if (_state == null)
                return;

            _state.SetShowHyperlinks(!_state.ShowHyperlinks);
        }

        private void RefreshLinkVisual()
        {
            if (_linkButton == null)
                return;

            if (_state == null)
            {
                if (_linkLabel != null)
                    _linkLabel.style.color = linkDisabledColor;
                return;
            }

            if (_linkLabel != null)
                _linkLabel.style.color = _state.ShowHyperlinks ? linkOnColor : linkOffColor;
        }
    }
}

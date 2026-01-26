using _Project.Scripts.Core;
using _Project.Scripts.Core.GameState;
using UnityEngine;
using UnityEngine.UIElements;

namespace _Project.Scripts.UI
{
    /// <summary>UI Toolkit панель режимов отображения галактики.</summary>
    public sealed class GalaxyViewModesController : MonoBehaviour
    {
        [Header("Colors")]
        [SerializeField] private Color panelBackground = new Color(0.07f, 0.10f, 0.16f, 0.90f);
        [SerializeField] private Color linkOnColor = new Color(0.10f, 0.85f, 0.10f, 0.95f);
        [SerializeField] private Color linkOffColor = new Color(1f, 1f, 1f, 0.95f);
        [SerializeField] private Color linkDisabledColor = new Color(0.25f, 0.25f, 0.28f, 0.6f);
        [SerializeField] private Color stellarisOnColor = new Color(0.10f, 0.85f, 0.10f, 0.95f);
        [SerializeField] private Color stellarisOffColor = new Color(1f, 1f, 1f, 0.95f);

        private UIDocument _doc;
        private VisualElement _root;
        private VisualElement _panel;
        [SerializeField] private VisualTreeAsset objectDataAsset;
        private VisualElement _objectDataHost;
        private VisualElement _linkButton;
        private Label _linkLabel;
        private VisualElement _fractionsButton;
        private Label _fractionsLabel;
        private VisualElement _constellationsButton;
        private Label _constellationsLabel;
        private GameStateService _state;
        private GalaxyMapStarInfoController _starInfoController;
        private GalaxyMapStarInfoTabsController _tabsController;

        private EventCallback<ClickEvent> _onClick;
        private EventCallback<ClickEvent> _onFractionsClick;
        private EventCallback<ClickEvent> _onConstellationsClick;

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

            _onFractionsClick = OnFractionsClicked;
            _fractionsButton?.UnregisterCallback(_onFractionsClick);
            _fractionsButton?.RegisterCallback(_onFractionsClick);

            _onConstellationsClick = OnConstellationsClicked;
            _constellationsButton?.UnregisterCallback(_onConstellationsClick);
            _constellationsButton?.RegisterCallback(_onConstellationsClick);

            ApplyPanelBackground();
            RefreshLinkVisual();
            RefreshFractionsVisual();
            RefreshConstellationsVisual();
            EnsureObjectData();
            HideDefaultObjectData();
            RebindTabs();
            RebindStarInfo();
        }

        private void OnDisable()
        {
            if (_state != null)
                _state.StateChanged -= OnStateChanged;
            _state = null;

            if (_linkButton != null && _onClick != null)
                _linkButton.UnregisterCallback(_onClick);
            if (_fractionsButton != null && _onFractionsClick != null)
                _fractionsButton.UnregisterCallback(_onFractionsClick);
            if (_constellationsButton != null && _onConstellationsClick != null)
                _constellationsButton.UnregisterCallback(_onConstellationsClick);
        }

        private void OnValidate()
        {
            if (!isActiveAndEnabled)
                return;

            var doc = GetComponent<UIDocument>();
            if (doc == null || doc.rootVisualElement == null)
                return;

            if (!TryResolveElements())
                return;

            ApplyPanelBackground();
            RefreshLinkVisual();
            RefreshFractionsVisual();
            RefreshConstellationsVisual();
            EnsureObjectData();
            HideDefaultObjectData();
            RebindTabs();
            RebindStarInfo();
        }

        private bool TryResolveElements()
        {
            _doc = GetComponent<UIDocument>();
            if (_doc == null)
            {
                Debug.LogError("[GalaxyViewModes] UIDocument not found.");
                return false;
            }

            _root = _doc.rootVisualElement;
            if (_root == null)
                return false;
            if (_root.childCount == 0 && _doc.visualTreeAsset != null)
            {
                _root.Clear();
                _doc.visualTreeAsset.CloneTree(_root);
            }

            _panel = _root.Q<VisualElement>("GalaxyViewModes");
            _objectDataHost = _root.Q<VisualElement>("ObjectDataHost");
            _linkButton = _root.Q<VisualElement>("VisualElement1");
            _linkLabel = _linkButton?.Q<Label>();
            _fractionsButton = _root.Q<VisualElement>("VisualElement2");
            _fractionsLabel = _fractionsButton?.Q<Label>();
            _constellationsButton = _root.Q<VisualElement>("VisualElement3");
            _constellationsLabel = _constellationsButton?.Q<Label>();

            return _panel != null && _linkButton != null && _fractionsButton != null && _constellationsButton != null;
        }

        private void EnsureObjectData()
        {
            if (_objectDataHost == null || objectDataAsset == null)
                return;

            if (_objectDataHost.childCount > 0)
                return;

            objectDataAsset.CloneTree(_objectDataHost);
        }

        private void RebindTabs()
        {
            if (_tabsController == null)
                _tabsController = GetComponent<GalaxyMapStarInfoTabsController>();

            if (_tabsController != null)
                _tabsController.Rebind();
        }

        private void RebindStarInfo()
        {
            if (_starInfoController == null)
                _starInfoController = GetComponent<GalaxyMapStarInfoController>();

            if (_starInfoController != null)
                _starInfoController.Rebind();
        }

        private void HideDefaultObjectData()
        {
            if (_starInfoController == null)
                _starInfoController = GetComponent<GalaxyMapStarInfoController>();

            if (_starInfoController != null)
                _starInfoController.ClearStarInfo();
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
            RefreshFractionsVisual();
            RefreshConstellationsVisual();
        }

        private void OnLinkClicked(ClickEvent evt)
        {
            var settings = SettingsService.Instance;
            settings.SetShowHyperlinks(!settings.ShowHyperlinks);
        }

        private void OnFractionsClicked(ClickEvent evt)
        {
            var settings = SettingsService.Instance;
            bool newValue = !settings.UseFractionColoring;
            settings.SetUseFractionColoring(newValue);
            if (newValue && settings.UseHyperlinkColoring)
                settings.SetUseHyperlinkColoring(false);
        }

        private void OnConstellationsClicked(ClickEvent evt)
        {
            var settings = SettingsService.Instance;
            bool newValue = !settings.UseHyperlinkColoring;
            settings.SetUseHyperlinkColoring(newValue);
            if (newValue && settings.UseFractionColoring)
                settings.SetUseFractionColoring(false);
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

        private void RefreshFractionsVisual()
        {
            if (_fractionsButton == null)
                return;

            if (_state == null)
            {
                if (_fractionsLabel != null)
                    _fractionsLabel.style.color = linkDisabledColor;
                return;
            }

            if (_fractionsLabel != null)
                _fractionsLabel.style.color = _state.UseFractionColoring ? linkOnColor : linkOffColor;
        }

        private void RefreshConstellationsVisual()
        {
            if (_constellationsButton == null)
                return;

            if (_state == null)
            {
                if (_constellationsLabel != null)
                    _constellationsLabel.style.color = linkDisabledColor;
                return;
            }

            if (_constellationsLabel != null)
                _constellationsLabel.style.color = _state.UseHyperlinkColoring ? linkOnColor : linkOffColor;
        }
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace _Project.Scripts.UI
{
    /// <summary>Переключатели вкладок в блоке StarInfo.</summary>
    public sealed class GalaxyMapStarInfoTabsController : MonoBehaviour
    {
        [SerializeField] private string tabsContainerName = "buttons";
        [SerializeField] private string defaultTabLabel = "star";
        [SerializeField] private Color activeColor = new Color(0.1f, 0.85f, 0.3f, 1f);
        [SerializeField] private Color inactiveColor = new Color(1f, 0.698f, 0f, 1f);

        private UIDocument _doc;
        private VisualElement _root;
        private VisualElement _tabsContainer;
        private readonly List<VisualElement> _tabs = new List<VisualElement>();
        private readonly Dictionary<Label, Color> _inactiveColors = new Dictionary<Label, Color>();

        private void OnEnable()
        {
            if (!TryResolveElements())
                return;

            CacheTabs();
            ApplyDefaultTab();
        }

        private void OnValidate()
        {
            if (!isActiveAndEnabled)
                return;

            var doc = GetComponent<UIDocument>();
            if (doc == null || doc.rootVisualElement == null)
                return;

            TryResolveElements();
        }

        private bool TryResolveElements()
        {
            _doc = GetComponent<UIDocument>();
            if (_doc == null)
            {
                Debug.LogError("[GalaxyMapStarInfoTabs] UIDocument not found.");
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

            _tabsContainer = _root.Q<VisualElement>(tabsContainerName);
            return _tabsContainer != null;
        }

        private void CacheTabs()
        {
            _tabs.Clear();
            _inactiveColors.Clear();

            foreach (var child in _tabsContainer.Children())
            {
                if (child == null)
                    continue;

                var label = child.Q<Label>();
                if (label != null)
                    _inactiveColors[label] = inactiveColor;

                child.pickingMode = PickingMode.Position;
                child.UnregisterCallback<ClickEvent>(OnTabClicked);
                child.RegisterCallback<ClickEvent>(OnTabClicked);
                _tabs.Add(child);
            }
        }

        private void ApplyDefaultTab()
        {
            if (_tabs.Count == 0)
                return;

            VisualElement defaultTab = null;
            for (int i = 0; i < _tabs.Count; i++)
            {
                var label = _tabs[i].Q<Label>();
                if (label == null)
                    continue;

                if (label.text != null && label.text.ToLowerInvariant() == defaultTabLabel.ToLowerInvariant())
                {
                    defaultTab = _tabs[i];
                    break;
                }
            }

            SetActiveTab(defaultTab ?? _tabs[0]);
        }

        private void OnTabClicked(ClickEvent evt)
        {
            if (evt.currentTarget is VisualElement tab)
                SetActiveTab(tab);
        }

        private void SetActiveTab(VisualElement activeTab)
        {
            for (int i = 0; i < _tabs.Count; i++)
            {
                var label = _tabs[i].Q<Label>();
                if (label == null)
                    continue;

                if (_tabs[i] == activeTab)
                {
                    label.style.color = activeColor;
                }
                else if (_inactiveColors.TryGetValue(label, out var color))
                {
                    label.style.color = color;
                }
            }
        }
    }
}

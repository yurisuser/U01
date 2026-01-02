using System.Collections.Generic;
using System.Text;
using _Project.Scripts.Galaxy.Data;
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
        private Label _starNameLabel;
        private Label _constellationNameLabel;
        private VisualElement _starParamValue;
        private VisualElement _systemParamValue;
        private Label _starParamLabel;
        private Label _starValueLabel;
        private Label _systemParamLabel;
        private Label _systemValueLabel;
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
            _starNameLabel = _root.Q<Label>("StarName");
            _constellationNameLabel = _root.Q<Label>("ConstellationName");
            _starParamValue = _root.Q<VisualElement>("StarParamValue") ?? _root.Q<VisualElement>("ParamValue");
            _systemParamValue = _root.Q<VisualElement>("SystemParamValue");
            _starParamLabel = _starParamValue?.Q<Label>("Param");
            _starValueLabel = _starParamValue?.Q<Label>("Value");
            _systemParamLabel = _systemParamValue?.Q<Label>("Param");
            _systemValueLabel = _systemParamValue?.Q<Label>("Value");
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

            var activeLabel = activeTab?.Q<Label>();
            UpdateParamPanels(activeLabel?.text);
        }

        public void ApplyStarInfo(StarSys starSys)
        {
            if (_root == null && !TryResolveElements())
                return;

            if (_starNameLabel == null || _starParamLabel == null || _starValueLabel == null)
                TryResolveElements();

            var starName = string.IsNullOrWhiteSpace(starSys.Name) ? "Unknown" : starSys.Name;
            starName = $"{starName} (х: {starSys.OldX:F1} / у: {starSys.OldY:F1} / хн: {starSys.GalaxyPosition.x:F1} / ун: {starSys.GalaxyPosition.y:F1} / r: {starSys.DistanceToCenter:F1})";
            int planetsCount = starSys.PlanetSysArr != null ? starSys.PlanetSysArr.Length : 0;

            if (_starNameLabel != null)
                _starNameLabel.text = starName;
            if (_constellationNameLabel != null)
                _constellationNameLabel.text = "ConstellationName   ";

            if (_starParamLabel == null || _starValueLabel == null)
                return;

            var paramList = new StringBuilder();
            var valueList = new StringBuilder();

            paramList.Append("star type:").Append('\n');
            valueList.Append(starSys.Star.type).Append('\n');

            paramList.Append("star size:").Append('\n');
            valueList.Append(starSys.Star.size).Append('\n');

            paramList.Append("temperature:").Append('\n');
            valueList.Append(FormatWithUnit(starSys.Star.temperature, "0", "K", treatZeroAsMissing: true)).Append('\n');

            paramList.Append("mass:").Append('\n');
            valueList.Append(FormatWithUnit(starSys.Star.mass, "0.0000", "solar", treatZeroAsMissing: true)).Append('\n');

            paramList.Append("radius:").Append('\n');
            valueList.Append(FormatWithUnit(starSys.Star.radius, "0.0000", "solar", treatZeroAsMissing: true)).Append('\n');

            paramList.Append("luminosity:").Append('\n');
            valueList.Append(FormatWithUnit(starSys.Star.luminosity, "0.0000", "Lsun", treatZeroAsMissing: true)).Append('\n');

            paramList.Append("age:").Append('\n');
            valueList.Append(FormatWithUnit(starSys.Star.age, "0.0", "Gyr", treatZeroAsMissing: true)).Append('\n');

            paramList.Append("metallicity:").Append('\n');
            valueList.Append(FormatWithUnit(starSys.Star.metallicity, "0.00", string.Empty, treatZeroAsMissing: false)).Append('\n');

            paramList.Append("stability:").Append('\n');
            valueList.Append(FormatFloat(starSys.Star.stability, treatZeroAsMissing: false)).Append('\n');

            _starParamLabel.text = paramList.ToString();
            _starValueLabel.text = valueList.ToString();

            if (_systemParamLabel == null || _systemValueLabel == null)
                return;

            var systemParamList = new StringBuilder();
            var systemValueList = new StringBuilder();
            var planets = starSys.PlanetSysArr;

            if (planets == null || planets.Length == 0)
            {
                systemParamList.Append("");
                systemValueList.Append("no objects");
            }
            else
            {
                for (int i = 0; i < planets.Length; i++)
                {
                    systemParamList.Append(planets[i].Planet.Type).Append(':');
                    var planetName = string.IsNullOrWhiteSpace(planets[i].Planet.Name) ? "no info" : planets[i].Planet.Name;
                    systemValueList.Append(planetName);

                    if (i < planets.Length - 1)
                    {
                        systemParamList.Append('\n');
                        systemValueList.Append('\n');
                    }
                }
            }

            _systemParamLabel.text = systemParamList.ToString();
            _systemValueLabel.text = systemValueList.ToString();
        }

        private static string FormatFloat(float value, bool treatZeroAsMissing)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                return "no info";
            if (treatZeroAsMissing && Mathf.Approximately(value, 0f))
                return "no info";

            return value.ToString("0.0000");
        }

        private static string FormatWithUnit(float value, string format, string unit, bool treatZeroAsMissing)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                return "no info";
            if (treatZeroAsMissing && Mathf.Approximately(value, 0f))
                return "no info";

            var text = value.ToString(format);
            return string.IsNullOrWhiteSpace(unit) ? text : $"{text} {unit}";
        }

        private void UpdateParamPanels(string activeLabelText)
        {
            var active = string.IsNullOrWhiteSpace(activeLabelText)
                ? string.Empty
                : activeLabelText.ToLowerInvariant();

            bool showStar = active == "star";
            bool showSystem = active == "system";

            if (_starParamValue != null)
                _starParamValue.style.display = showStar ? DisplayStyle.Flex : DisplayStyle.None;
            if (_systemParamValue != null)
                _systemParamValue.style.display = showSystem ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}

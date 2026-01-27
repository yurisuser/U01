using System.Collections.Generic;
using System.Text;
using _Project.DataAccess;
using _Project.Scripts.Galaxy.Constellations;
using _Project.Scripts.Galaxy.Data;
using UnityEngine;
using UnityEngine.UIElements;

namespace _Project.Scripts.UI
{
    /// <summary>Переключатели вкладок в блоке ObjectInfo.</summary>
    public sealed class ObjectInfoTabsController : MonoBehaviour
    {
        public enum ObjectInfoTabScope
        {
            GalaxyStar,
            SysStar,
            SysPlanet,
            SysMoon,
            SysShip,
            SysStation
        }

        [SerializeField] private string tabsContainerName = "buttons";
        [SerializeField] private string defaultTabLabel = "star";
        [SerializeField] private Color activeColor = new Color(0.1f, 0.85f, 0.3f, 1f);
        [SerializeField] private Color inactiveColor = new Color(1f, 0.698f, 0f, 1f);

        private UIDocument _doc;
        private VisualElement _root;
        private VisualElement _tabsContainer;
        private Label _objectNameLabel;
        private Label _upStringObjectNameLabel;
        private VisualElement _paramValueBlock;
        private Label _paramLabel;
        private Label _valueLabel;
        private string _starParamText = string.Empty;
        private string _starValueText = string.Empty;
        private string _systemParamText = string.Empty;
        private string _systemValueText = string.Empty;
        private string _activeTabText = string.Empty;
        private ObjectInfoTabScope _currentScope = ObjectInfoTabScope.GalaxyStar;
        private readonly List<VisualElement> _tabs = new List<VisualElement>();
        private readonly Dictionary<Label, Color> _inactiveColors = new Dictionary<Label, Color>();
        private readonly Dictionary<ObjectInfoTabScope, string> _lastTabByScope = new Dictionary<ObjectInfoTabScope, string>();

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

        public void Rebind()
        {
            if (!TryResolveElements())
                return;

            CacheTabs();
            ApplyDefaultTab();
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
            _objectNameLabel = _root.Q<Label>("ObjectName");
            _upStringObjectNameLabel = _root.Q<Label>("UpStringObjectName");
            _paramValueBlock = _root.Q<VisualElement>("ParamValueBlock") ?? _root.Q<VisualElement>("ParamValue");
            _paramLabel = _paramValueBlock?.Q<Label>("Param");
            _valueLabel = _paramValueBlock?.Q<Label>("Value");
            if (_paramLabel != null)
                _paramLabel.enableRichText = true;
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

            VisualElement targetTab = null;
            if (_lastTabByScope.TryGetValue(_currentScope, out var savedTab) && !string.IsNullOrWhiteSpace(savedTab))
                targetTab = FindTabByLabel(savedTab);

            if (targetTab == null)
                targetTab = FindTabByLabel(defaultTabLabel);

            SetActiveTab(targetTab ?? _tabs[0]);
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
            _activeTabText = activeLabel?.text;
            if (!string.IsNullOrWhiteSpace(_activeTabText))
                _lastTabByScope[_currentScope] = _activeTabText;
            UpdateParamPanels(_activeTabText);
        }

        public void SetScope(ObjectInfoTabScope scope)
        {
            _currentScope = scope;
        }

        public void ApplyScopeTab()
        {
            if (_tabs.Count == 0)
                return;

            ApplyDefaultTab();
        }

        private VisualElement FindTabByLabel(string labelText)
        {
            if (string.IsNullOrWhiteSpace(labelText))
                return null;

            var target = labelText.ToLowerInvariant();
            for (int i = 0; i < _tabs.Count; i++)
            {
                var label = _tabs[i].Q<Label>();
                if (label?.text == null)
                    continue;

                if (label.text.ToLowerInvariant() == target)
                    return _tabs[i];
            }

            return null;
        }

        public void ApplyStarInfo(StarSys starSys)
        {
            if (_root == null && !TryResolveElements())
                return;

            if (_objectNameLabel == null || _paramLabel == null || _valueLabel == null)
                TryResolveElements();

            var starName = string.IsNullOrWhiteSpace(starSys.Name) ? "Unknown" : starSys.Name;
            int planetsCount = starSys.PlanetSysArr != null ? starSys.PlanetSysArr.Length : 0;

            if (_objectNameLabel != null)
                _objectNameLabel.text = starName;
            if (_upStringObjectNameLabel != null)
                _upStringObjectNameLabel.text = ConstellationService.GetNameById(starSys.ConstellationId);

            if (_paramLabel == null || _valueLabel == null)
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

            _starParamText = paramList.ToString();
            _starValueText = valueList.ToString();

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

            _systemParamText = systemParamList.ToString();
            _systemValueText = systemValueList.ToString();
            UpdateParamPanels(GetActiveTabTextOrDefault());
        }

        public void ApplyPlanetInfo(in StarSys system, in PlanetSys planetSys)
        {
            if (_root == null && !TryResolveElements())
                return;

            if (_objectNameLabel == null || _paramLabel == null || _valueLabel == null)
                TryResolveElements();

            if (_objectNameLabel != null)
            {
                var planetName = string.IsNullOrWhiteSpace(planetSys.Planet.Name) ? "Unknown planet" : planetSys.Planet.Name;
                _objectNameLabel.text = planetName;
            }

            if (_upStringObjectNameLabel != null)
            {
                var starName = string.IsNullOrWhiteSpace(system.Star.Name) ? "Unknown star" : system.Star.Name;
                _upStringObjectNameLabel.text = starName;
            }

            if (_paramLabel == null || _valueLabel == null)
                return;

            var paramList = new StringBuilder();
            var valueList = new StringBuilder();

            paramList.Append("planet type:").Append('\n');
            valueList.Append(planetSys.Planet.Type).Append('\n');

            paramList.Append("orbit index:").Append('\n');
            valueList.Append(planetSys.OrbitIndex).Append('\n');

            paramList.Append("orbit dist:").Append('\n');
            valueList.Append(FormatWithUnit(planetSys.Planet.OrbitalDistance, "0.00", "AU", treatZeroAsMissing: true)).Append('\n');

            paramList.Append("temperature:").Append('\n');
            valueList.Append(FormatWithUnit(planetSys.Planet.Temperature, "0", "K", treatZeroAsMissing: true)).Append('\n');

            paramList.Append("mass:").Append('\n');
            valueList.Append(FormatWithUnit(planetSys.Planet.Mass, "0.0000", "M⊕", treatZeroAsMissing: true)).Append('\n');

            paramList.Append("radius:").Append('\n');
            valueList.Append(FormatWithUnit(planetSys.Planet.Radius, "0.0000", "R⊕", treatZeroAsMissing: true)).Append('\n');

            int moonsCount = planetSys.Moons != null ? planetSys.Moons.Length : 0;
            paramList.Append("moons:").Append('\n');
            valueList.Append(moonsCount).Append('\n');

            _starParamText = paramList.ToString();
            _starValueText = valueList.ToString();
            BuildPlanetResourceInfo(planetSys.Planet.ResourceDeposits);
            UpdateParamPanels(GetActiveTabTextOrDefault());
        }

        public void ApplyMoonInfo(in StarSys system, in PlanetSys planetSys, in Moon moon)
        {
            if (_root == null && !TryResolveElements())
                return;

            if (_objectNameLabel == null || _paramLabel == null || _valueLabel == null)
                TryResolveElements();

            if (_objectNameLabel != null)
            {
                var moonName = string.IsNullOrWhiteSpace(moon.Name) ? "Unknown moon" : moon.Name;
                _objectNameLabel.text = moonName;
            }

            if (_upStringObjectNameLabel != null)
            {
                var planetName = string.IsNullOrWhiteSpace(planetSys.Planet.Name) ? "Unknown planet" : planetSys.Planet.Name;
                _upStringObjectNameLabel.text = planetName;
            }

            if (_paramLabel == null || _valueLabel == null)
                return;

            var paramList = new StringBuilder();
            var valueList = new StringBuilder();

            paramList.Append("moon type:").Append('\n');
            valueList.Append(moon.Type).Append('\n');

            paramList.Append("orbit index:").Append('\n');
            valueList.Append(moon.OrbitIndex).Append('\n');

            paramList.Append("orbit dist:").Append('\n');
            valueList.Append(FormatWithUnit(moon.OrbitDistance, "0.00", "AU", treatZeroAsMissing: true)).Append('\n');

            paramList.Append("temperature:").Append('\n');
            valueList.Append(FormatWithUnit(moon.Temperature, "0", "K", treatZeroAsMissing: true)).Append('\n');

            paramList.Append("mass:").Append('\n');
            valueList.Append(FormatWithUnit(moon.Mass, "0.0000", "M⊕", treatZeroAsMissing: true)).Append('\n');

            paramList.Append("radius:").Append('\n');
            valueList.Append(FormatWithUnit(moon.Radius, "0.0000", "R⊕", treatZeroAsMissing: true)).Append('\n');

            _starParamText = paramList.ToString();
            _starValueText = valueList.ToString();
            BuildPlanetResourceInfo(moon.ResourceDeposits);
            UpdateParamPanels(GetActiveTabTextOrDefault());
        }

        public void ApplyStationInfo(in StarSys system, in _Project.Scripts.Stations.Station station)
        {
            if (_root == null && !TryResolveElements())
                return;

            if (_objectNameLabel == null || _paramLabel == null || _valueLabel == null)
                TryResolveElements();

            if (_objectNameLabel != null)
            {
                var stationName = string.IsNullOrWhiteSpace(station.TypeKey) ? "Unknown station" : station.TypeKey;
                _objectNameLabel.text = stationName;
            }

            if (_upStringObjectNameLabel != null)
            {
                var systemName = string.IsNullOrWhiteSpace(system.Name) ? "Unknown system" : system.Name;
                _upStringObjectNameLabel.text = systemName;
            }

            if (_paramLabel == null || _valueLabel == null)
                return;

            var paramList = new StringBuilder();
            var valueList = new StringBuilder();

            paramList.Append("type key:").Append('\n');
            valueList.Append(string.IsNullOrWhiteSpace(station.TypeKey) ? "no info" : station.TypeKey).Append('\n');

            paramList.Append("owner:").Append('\n');
            valueList.Append(string.IsNullOrWhiteSpace(station.Owner.Name) ? "no info" : station.Owner.Name).Append('\n');

            paramList.Append("hull:").Append('\n');
            valueList.Append(FormatFloat2(station.Hull, treatZeroAsMissing: false)).Append('\n');

            paramList.Append("power:").Append('\n');
            valueList.Append($"{FormatFloat2(station.PowerStored, treatZeroAsMissing: false)}/{FormatFloat2(station.PowerCapacity, treatZeroAsMissing: false)}").Append('\n');

            int modulesCount = station.Modules != null ? station.Modules.Length : 0;
            paramList.Append("modules:").Append('\n');
            valueList.Append(modulesCount).Append('\n');

            _starParamText = paramList.ToString();
            _starValueText = valueList.ToString();
            BuildStationModulesInfo(station.Modules);

            UpdateParamPanels(GetActiveTabTextOrDefault());
        }

        public void ApplyShipInfo(in StarSys system, in _Project.Scripts.Ships.Ship ship)
        {
            if (_root == null && !TryResolveElements())
                return;

            if (_objectNameLabel == null || _paramLabel == null || _valueLabel == null)
                TryResolveElements();

            if (_objectNameLabel != null)
            {
                _objectNameLabel.text = ship.Type.ToString();
            }

            if (_upStringObjectNameLabel != null)
            {
                var systemName = string.IsNullOrWhiteSpace(system.Name) ? "Unknown system" : system.Name;
                _upStringObjectNameLabel.text = systemName;
            }

            if (_paramLabel == null || _valueLabel == null)
                return;

            var paramList = new StringBuilder();
            var valueList = new StringBuilder();

            paramList.Append("type:").Append('\n');
            valueList.Append(ship.Type).Append('\n');

            paramList.Append("maker:").Append('\n');
            valueList.Append(string.IsNullOrWhiteSpace(ship.MakerFraction.Name) ? "no info" : ship.MakerFraction.Name).Append('\n');

            paramList.Append("hp:").Append('\n');
            valueList.Append(ship.Stats.Hp).Append('\n');

            paramList.Append("speed:").Append('\n');
            valueList.Append($"{FormatFloat2(ship.CurrentSpeed, treatZeroAsMissing: false)}/{FormatFloat2(ship.Stats.MaxSpeed, treatZeroAsMissing: false)}").Append('\n');

            paramList.Append("agility:").Append('\n');
            valueList.Append(FormatFloat2(ship.Stats.Agility, treatZeroAsMissing: false)).Append('\n');

            _starParamText = paramList.ToString();
            _starValueText = valueList.ToString();
            BuildShipEquipmentInfo(ship.Equipment);

            UpdateParamPanels(GetActiveTabTextOrDefault());
        }

        private void BuildStationModulesInfo(_Project.Scripts.Stations.StationModule[] modules)
        {
            if (modules == null || modules.Length == 0)
            {
                _systemParamText = "modules:";
                _systemValueText = "none";
                return;
            }

            var paramList = new StringBuilder();
            var valueList = new StringBuilder();

            for (int i = 0; i < modules.Length; i++)
            {
                var module = modules[i];
                paramList.Append("<b>").Append(module.Type).Append("</b>").Append('\n');
                valueList.Append($"lvl {module.Level}").Append('\n');

                if (i < modules.Length - 1)
                {
                    paramList.Append('\n');
                    valueList.Append('\n');
                }
            }

            _systemParamText = paramList.ToString();
            _systemValueText = valueList.ToString();
        }

        private void BuildShipEquipmentInfo(_Project.Scripts.Ships.InstalledEquip equipment)
        {
            var paramList = new StringBuilder();
            var valueList = new StringBuilder();

            int weaponSlots = equipment.WeaponSlotsCount;
            paramList.Append("weapons:").Append('\n');
            valueList.Append(weaponSlots).Append('\n');

            for (int i = 0; i < weaponSlots; i++)
            {
                var slot = equipment.GetWeaponSlot(i);
                paramList.Append($"weapon {i + 1}:").Append('\n');
                valueList.Append(slot.IsEmpty ? "empty" : $"id {slot.Id}").Append('\n');
            }

            if (weaponSlots > 0)
            {
                paramList.Append('\n');
                valueList.Append('\n');
            }

            paramList.Append("engine:").Append('\n');
            valueList.Append(equipment.Engine.IsEmpty ? "empty" : $"id {equipment.Engine.Id}").Append('\n');

            paramList.Append("shield:").Append('\n');
            valueList.Append(equipment.Shield.IsEmpty ? "empty" : $"id {equipment.Shield.Id}").Append('\n');

            paramList.Append("scanner:").Append('\n');
            valueList.Append(equipment.Scanner.IsEmpty ? "empty" : $"id {equipment.Scanner.Id}").Append('\n');

            _systemParamText = paramList.ToString();
            _systemValueText = valueList.ToString();
        }

        private string GetActiveTabTextOrDefault()
        {
            if (!string.IsNullOrWhiteSpace(_activeTabText))
                return _activeTabText;

            if (_lastTabByScope.TryGetValue(_currentScope, out var savedTab) && !string.IsNullOrWhiteSpace(savedTab))
                return savedTab;

            return defaultTabLabel;
        }

        private void BuildPlanetResourceInfo(ResourceDeposit[] deposits)
        {
            if (deposits == null || deposits.Length == 0)
            {
                _systemParamText = "resources:";
                _systemValueText = "none";
                return;
            }

            var paramList = new StringBuilder();
            var valueList = new StringBuilder();

            for (int i = 0; i < deposits.Length; i++)
            {
                var d = deposits[i];
                var resourceName = GetResourceName(d.ResourceId);
                paramList.Append("<b>").Append(resourceName).Append("</b>").Append('\n');
                valueList.Append(string.Empty).Append('\n');

                paramList.Append("purity:").Append('\n');
                valueList.Append(FormatFloat2(d.ResourcePurity, treatZeroAsMissing: false)).Append('\n');

                paramList.Append("availability:").Append('\n');
                valueList.Append(FormatFloat2(d.Availability, treatZeroAsMissing: false)).Append('\n');

                if (i < deposits.Length - 1)
                {
                    paramList.Append('\n');
                    valueList.Append('\n');
                }
            }

            _systemParamText = paramList.ToString();
            _systemValueText = valueList.ToString();
        }

        private static string GetResourceName(int resourceId)
        {
            if (CATALOG.SkuById != null && CATALOG.SkuById.TryGetValue(resourceId, out var sku))
            {
                if (!string.IsNullOrWhiteSpace(sku.Name))
                    return sku.Name;
            }

            return $"resource {resourceId}";
        }

        private static string FormatFloat(float value, bool treatZeroAsMissing)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                return "no info";
            if (treatZeroAsMissing && Mathf.Approximately(value, 0f))
                return "no info";

            return value.ToString("0.0000");
        }

        private static string FormatFloat2(float value, bool treatZeroAsMissing)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                return "no info";
            if (treatZeroAsMissing && Mathf.Approximately(value, 0f))
                return "no info";

            return value.ToString("0.00");
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

            bool showPrimary = active == "star" || active == "info";
            bool showSecondary = active == "system" || active == "resources";

            if (_paramValueBlock == null || _paramLabel == null || _valueLabel == null)
                return;

            if (showPrimary)
            {
                _paramValueBlock.style.display = DisplayStyle.Flex;
                _paramLabel.text = _starParamText;
                _valueLabel.text = _starValueText;
            }
            else if (showSecondary)
            {
                _paramValueBlock.style.display = DisplayStyle.Flex;
                _paramLabel.text = _systemParamText;
                _valueLabel.text = _systemValueText;
            }
            else
            {
                _paramValueBlock.style.display = DisplayStyle.None;
            }
        }
    }
}

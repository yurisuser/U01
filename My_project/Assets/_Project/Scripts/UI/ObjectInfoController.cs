using _Project.Scripts.Core;
using _Project.Scripts.Core.GameState.GameStateMembers.SelectedObj;
using _Project.Scripts.Galaxy.Data;
using _Project.Scripts.Selection;
using UnityEngine;
using UnityEngine.UIElements;

namespace _Project.Scripts.UI
{
    /// <summary>Показывает информацию о выбранной звезде на галкарте.</summary>
    public sealed class ObjectInfoController : MonoBehaviour
    {
        [SerializeField] private bool hideWhenEmpty = true;
        [Header("ObjectData Templates")]
        [SerializeField] private VisualTreeAsset galaxyObjectData;
        [Header("System Map Templates")]
        [SerializeField] private VisualTreeAsset sysStarObjectData;
        [SerializeField] private VisualTreeAsset sysPlanetObjectData;
        [SerializeField] private VisualTreeAsset sysMoonObjectData;
        [SerializeField] private VisualTreeAsset sysShipObjectData;
        [SerializeField] private VisualTreeAsset sysStationObjectData;

        private UIDocument _doc;
        private VisualElement _root;
        private VisualElement _objectDataElement;
        private VisualElement _closer;
        private EventCallback<ClickEvent> _onCloserClick;
        private StarSys starSys;
        private ObjectInfoTabsController _tabsController;
        private GalaxyViewModesController _viewModesController;
        private SelectableData _selectedData;

        private void Update()
        {
            if (_selectedData == null || !_selectedData.HasData || _selectedData.SelectedType != ESelectedObjectType.Ship)
                return;

            int systemIndex = _selectedData.SystemIndex >= 0
                ? _selectedData.SystemIndex
                : GameBootstrap.GameState.SelectedSystemIndex;
            if (!TryGetSystem(systemIndex, out var system) || !TryFindShip(system, _selectedData.Uid, out var ship))
                return;

            _tabsController?.ApplyShipInfo(system, ship);
        }
        private void OnEnable()
        {
            if (!TryResolveElements())
                return;

            BindCloser();

            if (hideWhenEmpty)
                Hide();
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

            BindCloser();
        }

        public void ShowStarInfo(UID starUid)
        {
            _selectedData = null;
            if (!TryResolveElements())
                return;

            if (_tabsController == null)
                _tabsController = GetComponent<ObjectInfoTabsController>();
            _tabsController?.SetScope(ObjectInfoTabsController.ObjectInfoTabScope.GalaxyStar);

            ApplyObjectDataTemplate(galaxyObjectData);

            if (!TryFindStarSys(starUid))
            {
                Hide();
                return;
            }

            SetDataToUI();
            _objectDataElement.style.display = DisplayStyle.Flex;
        }

        public void ShowObjectInfo(SelectableData data)
        {
            if (data == null || !data.HasData)
            {
                _selectedData = null;
                ClearStarInfo();
                return;
            }

            _selectedData = data;

            if (!TryResolveElements())
                return;

            if (_tabsController == null)
                _tabsController = GetComponent<ObjectInfoTabsController>();
            _tabsController?.SetScope(GetScopeForType(data.SelectedType));

            ApplyObjectDataTemplate(GetSysObjectDataTemplate(data.SelectedType));

            int systemIndex = data.SystemIndex >= 0 ? data.SystemIndex : GameBootstrap.GameState.SelectedSystemIndex;
            if (!TryGetSystem(systemIndex, out var system))
            {
                _selectedData = null;
                ClearStarInfo();
                return;
            }

            switch (data.SelectedType)
            {
                case ESelectedObjectType.Star:
                    _tabsController?.ApplyStarInfo(system);
                    break;
                case ESelectedObjectType.Planet:
                    if (TryFindPlanet(system, data.Uid, out var planetSys))
                        _tabsController?.ApplyPlanetInfo(system, planetSys);
                    else
                        ClearStarInfo();
                    break;
                case ESelectedObjectType.Moon:
                    if (TryFindMoon(system, data.Uid, out var moonPlanetSys, out var moon))
                        _tabsController?.ApplyMoonInfo(system, moonPlanetSys, moon);
                    else
                        ClearStarInfo();
                    break;
                case ESelectedObjectType.Station:
                    if (TryFindStation(system, data.Uid, out var station))
                        _tabsController?.ApplyStationInfo(system, station);
                    else
                        ClearStarInfo();
                    break;
                case ESelectedObjectType.Ship:
                    if (TryFindShip(system, data.Uid, out var ship))
                        _tabsController?.ApplyShipInfo(system, ship);
                    else
                        ClearStarInfo();
                    break;
                default:
                    ClearStarInfo();
                    return;
            }

            _objectDataElement.style.display = DisplayStyle.Flex;
        }

        public void ClearStarInfo()
        {
            _selectedData = null;
            if (!TryResolveElements())
                return;

            Hide();
        }

        private void Hide()
        {
            if (_objectDataElement != null)
                _objectDataElement.style.display = DisplayStyle.None;
        }

        private bool TryResolveElements()
        {
            _doc = GetComponent<UIDocument>();
            if (_doc == null)
            {
                Debug.LogError("[GalaxyMapStarInfo] UIDocument not found.");
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

            _objectDataElement = _root.Q<VisualElement>("ObjectData");
            _closer = _root.Q<VisualElement>("Closer");
            if (_tabsController == null)
                _tabsController = GetComponent<ObjectInfoTabsController>();

            return _objectDataElement != null;
        }

        private void OnDisable()
        {
            if (_closer != null && _onCloserClick != null)
                _closer.UnregisterCallback(_onCloserClick);
        }

        private void OnCloserClicked(ClickEvent evt)
        {
            ClearStarInfo();
        }

        private void BindCloser()
        {
            _onCloserClick = OnCloserClicked;
            if (_closer != null)
            {
                _closer.UnregisterCallback(_onCloserClick);
                _closer.RegisterCallback(_onCloserClick);
            }
        }

        private void ApplyObjectDataTemplate(VisualTreeAsset template)
        {
            if (template == null)
                return;

            if (_viewModesController == null)
                _viewModesController = GetComponent<GalaxyViewModesController>();

            _viewModesController?.ApplyObjectData(template);
        }

        private VisualTreeAsset GetSysObjectDataTemplate(ESelectedObjectType type)
        {
            switch (type)
            {
                case ESelectedObjectType.Star:
                    return sysStarObjectData;
                case ESelectedObjectType.Planet:
                    return sysPlanetObjectData;
                case ESelectedObjectType.Moon:
                    return sysPlanetObjectData != null ? sysPlanetObjectData : sysMoonObjectData;
                case ESelectedObjectType.Ship:
                    return sysShipObjectData;
                case ESelectedObjectType.Station:
                    return sysStationObjectData;
                default:
                    return null;
            }
        }

        private static ObjectInfoTabsController.ObjectInfoTabScope GetScopeForType(ESelectedObjectType type)
        {
            switch (type)
            {
                case ESelectedObjectType.Star:
                    return ObjectInfoTabsController.ObjectInfoTabScope.SysStar;
                case ESelectedObjectType.Planet:
                    return ObjectInfoTabsController.ObjectInfoTabScope.SysPlanet;
                case ESelectedObjectType.Moon:
                    return ObjectInfoTabsController.ObjectInfoTabScope.SysMoon;
                case ESelectedObjectType.Ship:
                    return ObjectInfoTabsController.ObjectInfoTabScope.SysShip;
                case ESelectedObjectType.Station:
                    return ObjectInfoTabsController.ObjectInfoTabScope.SysStation;
                default:
                    return ObjectInfoTabsController.ObjectInfoTabScope.SysStar;
            }
        }

        private static bool TryFindStation(in StarSys system, UID uid, out _Project.Scripts.Stations.Station station)
        {
            if (system.Stations != null)
            {
                for (int i = 0; i < system.Stations.Length; i++)
                {
                    if (system.Stations[i].Uid.Type == uid.Type &&
                        system.Stations[i].Uid.Id == uid.Id)
                    {
                        station = system.Stations[i];
                        return true;
                    }
                }
            }

            station = default;
            return false;
        }

        private static bool TryFindShip(in StarSys system, UID uid, out _Project.Scripts.Ships.Ship ship)
        {
            var runtime = system.State;
            if (runtime != null)
            {
                var ships = runtime.Ships;
                for (int i = 0; i < ships.Count; i++)
                {
                    var candidate = ships[i];
                    if (candidate.Uid.Type == uid.Type && candidate.Uid.Id == uid.Id)
                    {
                        ship = candidate;
                        return true;
                    }
                }

                var snapshot = runtime.CurrShipSnapshots;
                for (int i = 0; i < snapshot.Count; i++)
                {
                    var candidate = snapshot[i];
                    if (candidate.Uid.Type == uid.Type && candidate.Uid.Id == uid.Id)
                    {
                        ship = candidate;
                        return true;
                    }
                }
            }

            ship = default;
            return false;
        }

        private bool TryFindStarSys(UID starUid)
        {
            var galaxy = GameBootstrap.GameState.Galaxy;
            if (galaxy == null || galaxy.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < galaxy.Length; i++)
            {
                if (galaxy[i].Uid.Type != starUid.Type || galaxy[i].Uid.Id != starUid.Id)
                    continue;

                starSys = galaxy[i];
                return true;
            }
            return false;
        }

        private void SetDataToUI()
        {
            if (_tabsController != null)
                _tabsController.ApplyStarInfo(starSys);
        }

        private bool TryGetSystem(int index, out StarSys system)
        {
            var galaxy = GameBootstrap.GameState.Galaxy;
            if (galaxy == null || galaxy.Length == 0)
            {
                system = default;
                return false;
            }

            if (index < 0 || index >= galaxy.Length)
            {
                system = default;
                return false;
            }

            system = galaxy[index];
            return true;
        }

        private static bool TryFindPlanet(in StarSys system, UID uid, out PlanetSys planetSys)
        {
            if (system.PlanetSysArr != null)
            {
                for (int i = 0; i < system.PlanetSysArr.Length; i++)
                {
                    if (system.PlanetSysArr[i].Planet.Uid.Type == uid.Type &&
                        system.PlanetSysArr[i].Planet.Uid.Id == uid.Id)
                    {
                        planetSys = system.PlanetSysArr[i];
                        return true;
                    }
                }
            }

            planetSys = default;
            return false;
        }

        private static bool TryFindMoon(in StarSys system, UID uid, out PlanetSys planetSys, out Moon moon)
        {
            if (system.PlanetSysArr != null)
            {
                for (int i = 0; i < system.PlanetSysArr.Length; i++)
                {
                    var candidatePlanetSys = system.PlanetSysArr[i];
                    if (candidatePlanetSys.Moons == null)
                        continue;

                    for (int k = 0; k < candidatePlanetSys.Moons.Length; k++)
                    {
                        var candidateMoon = candidatePlanetSys.Moons[k];
                        if (candidateMoon.Uid.Type == uid.Type && candidateMoon.Uid.Id == uid.Id)
                        {
                            planetSys = candidatePlanetSys;
                            moon = candidateMoon;
                            return true;
                        }
                    }
                }
            }

            planetSys = default;
            moon = default;
            return false;
        }
    }
}

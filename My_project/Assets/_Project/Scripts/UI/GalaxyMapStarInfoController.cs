using _Project.Scripts.Core;
using _Project.Scripts.Core.GameState.GameStateMembers.SelectedObj;
using _Project.Scripts.Galaxy.Data;
using _Project.Scripts.Selection;
using UnityEngine;
using UnityEngine.UIElements;

namespace _Project.Scripts.UI
{
    /// <summary>Показывает информацию о выбранной звезде на галкарте.</summary>
    public sealed class GalaxyMapStarInfoController : MonoBehaviour
    {
        [SerializeField] private bool hideWhenEmpty = true;
        [Header("ObjectData Templates")]
        [SerializeField] private VisualTreeAsset galaxyObjectData;
        [Header("System Map Templates")]
        [SerializeField] private VisualTreeAsset sysStarObjectData;
        [SerializeField] private VisualTreeAsset sysPlanetObjectData;
        [SerializeField] private VisualTreeAsset sysMoonObjectData;
        [SerializeField] private VisualTreeAsset sysShipObjectData;

        private UIDocument _doc;
        private VisualElement _root;
        private VisualElement _objectDataElement;
        private VisualElement _closer;
        private EventCallback<ClickEvent> _onCloserClick;
        private StarSys starSys;
        private ObjectInfoTabsController _tabsController;
        private GalaxyViewModesController _viewModesController;
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
            if (!TryResolveElements())
                return;

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
                ClearStarInfo();
                return;
            }

            if (!TryResolveElements())
                return;

            ApplyObjectDataTemplate(GetSysObjectDataTemplate(data.SelectedType));

            int systemIndex = data.SystemIndex >= 0 ? data.SystemIndex : GameBootstrap.GameState.SelectedSystemIndex;
            if (!TryGetSystem(systemIndex, out var system))
            {
                ClearStarInfo();
                return;
            }

            if (_tabsController == null)
                _tabsController = GetComponent<ObjectInfoTabsController>();

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
                default:
                    ClearStarInfo();
                    return;
            }

            _objectDataElement.style.display = DisplayStyle.Flex;
        }

        public void ClearStarInfo()
        {
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
                    return sysMoonObjectData;
                case ESelectedObjectType.Ship:
                    return sysShipObjectData;
                default:
                    return null;
            }
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
    }
}

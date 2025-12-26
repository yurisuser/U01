using _Project.Scripts.Core;
using _Project.Scripts.Galaxy.Data;
using UnityEngine;
using UnityEngine.UIElements;

namespace _Project.Scripts.UI
{
    /// <summary>Показывает информацию о выбранной звезде на галкарте.</summary>
    public sealed class GalaxyMapStarInfoController : MonoBehaviour
    {
        [SerializeField] private bool hideWhenEmpty = true;

        private UIDocument _doc;
        private VisualElement _root;
        private VisualElement _starInfoElement;
        private VisualElement _closer;
        private EventCallback<ClickEvent> _onCloserClick;
        private StarSys starSys;
        private GalaxyMapStarInfoTabsController _tabsController;
        private void OnEnable()
        {
            if (!TryResolveElements())
                return;

            _onCloserClick = OnCloserClicked;
            if (_closer != null)
            {
                _closer.UnregisterCallback(_onCloserClick);
                _closer.RegisterCallback(_onCloserClick);
            }

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

        public void ShowStarInfo(UID starUid)
        {
            if (!TryResolveElements())
                return;

            if (!TryFindStarSys(starUid))
            {
                Hide();
                return;
            }

            SetDataToUI();
            _starInfoElement.style.display = DisplayStyle.Flex;
        }

        public void ClearStarInfo()
        {
            if (!TryResolveElements())
                return;

            Hide();
        }

        private void Hide()
        {
            if (_starInfoElement != null)
                _starInfoElement.style.display = DisplayStyle.None;
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

            _starInfoElement = _root.Q<VisualElement>("StarInfoElement");
            _closer = _root.Q<VisualElement>("Closer");
            if (_tabsController == null)
                _tabsController = GetComponent<GalaxyMapStarInfoTabsController>();

            return _starInfoElement != null;
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
    }
}

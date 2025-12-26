using _Project.Scripts.Core;
using _Project.Scripts.Galaxy.Data;
using System.Text;
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
        private Label _starNameLabel;
        private Label _constellationName;
        private Label _paramLabel;
        private Label _valueLabel;
        private VisualElement _closer;
        private EventCallback<ClickEvent> _onCloserClick;
        private StarSys starSys;
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
            _starNameLabel = _root.Q<Label>("StarName");
            _paramLabel = _root.Q<Label>("Param");
            _valueLabel = _root.Q<Label>("Value");
            _closer = _root.Q<VisualElement>("Closer");
            _constellationName =  _root.Q<Label>("ConstellationName");

            return _starInfoElement != null && _starNameLabel != null;
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
            var starName = string.IsNullOrWhiteSpace(starSys.Name) ? "Unknown" : starSys.Name;
            int planetsCount = starSys.PlanetSysArr != null ? starSys.PlanetSysArr.Length : 0;

            _starNameLabel.text = starName;
            _constellationName.text = "ConstellationName   ";
            if (_paramLabel == null || _valueLabel == null)
                return;

            var paramList = new StringBuilder();
            var valueList = new StringBuilder();

            paramList.Append("star type:").Append('\n');
            valueList.Append(starSys.Star.type).Append('\n');

            paramList.Append("star size:").Append('\n');
            valueList.Append(starSys.Star.size).Append('\n');

            paramList.Append("planets:");
            valueList.Append(planetsCount);

            _paramLabel.text = paramList.ToString();
            _valueLabel.text = valueList.ToString();
        }
    }
}

using System.Collections.Generic;
using _Project.Scripts.Core;
using _Project.Scripts.Core.GameState;
using _Project.Scripts.Galaxy.Data;
using UnityEngine;

namespace _Project.Scripts.GalaxyMap.Runtime
{
    /// <summary>Окрашивает звёзды на карте галактики в те же цвета, что и ссылки (фракции/созвездия).</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(StarGalaxyMapClick))]
    public sealed class StarGalaxyColorizer : MonoBehaviour
    {
        [Tooltip("Если оставить пустым, будут использованы все Renderer на объекте и детях.")]
        [SerializeField] private Renderer[] renderers;

        private GameStateService _state;
        private StarGalaxyMapClick _click;
        private ConstellationLinksRenderer _links;
        private StarSys[] _lastGalaxy;
        private int _systemIndex = -1;
        private Color[] _constellationColors;
        private bool _lastUseFractionColoring;
        private MaterialPropertyBlock _mpb;

        private void Awake()
        {
            _click = GetComponent<StarGalaxyMapClick>();
            if (renderers == null || renderers.Length == 0)
                renderers = GetComponentsInChildren<Renderer>(includeInactive: true);
        }

        private void OnEnable()
        {
            _state = GameBootstrap.GameState;
            if (_state != null)
                _state.StateChanged += OnStateChanged;

            _links = FindFirstObjectByType<ConstellationLinksRenderer>();
            ApplyColor();
        }

        private void OnDisable()
        {
            if (_state != null)
                _state.StateChanged -= OnStateChanged;
            _state = null;
            _lastGalaxy = null;
            _constellationColors = null;
            _systemIndex = -1;
        }

        private void OnStateChanged()
        {
            ApplyColor();
        }

        public void RefreshColor()
        {
            ApplyColor();
        }

        private void ApplyColor()
        {
            if (_state == null || !_click || !_click.System.HasValue)
                return;

            var systems = _state.Galaxy;
            if (systems == null || systems.Length == 0)
                return;

            if (_lastGalaxy != systems)
            {
                _lastGalaxy = systems;
                _systemIndex = -1;
                _constellationColors = null;
            }

            if (_systemIndex < 0)
                _systemIndex = FindSystemIndex(systems, _click.System.Value.Uid);

            if (_systemIndex < 0 || _systemIndex >= systems.Length)
                return;

            bool coloringOn = _state.UseHyperlinkColoring || _state.UseFractionColoring;
            if (!coloringOn)
            {
                ClearRendererOverrides();
                return;
            }

            bool useFractionColoring = _state.UseFractionColoring;
            if (_links)
            {
                _constellationColors = _links.GetConstellationColors();
                _lastUseFractionColoring = useFractionColoring;
            }
            else if (_constellationColors == null || _lastUseFractionColoring != useFractionColoring)
            {
                var settings = GetColorSettings();
                _constellationColors = GalaxyMapColorProvider.BuildConstellationColors(
                    systems,
                    useFractionColoring,
                    settings.ConstellationSaturation,
                    settings.ConstellationValue,
                    settings.ConstellationAlpha,
                    settings.EmptyConstellationColor);
                _lastUseFractionColoring = useFractionColoring;
            }

            var colorSettings = GetColorSettings();
            var finalColor = GalaxyMapColorProvider.GetSystemColor(
                systems,
                _systemIndex,
                _state.UseHyperlinkColoring,
                _state.UseFractionColoring,
                _constellationColors,
                colorSettings.ConstellationSaturation,
                colorSettings.ConstellationValue,
                colorSettings.ConstellationAlpha,
                colorSettings.EmptyConstellationColor,
                colorSettings.NoColoringColor,
                colorSettings.DefaultColor);

            ApplyToRenderers(finalColor);
        }

        private int FindSystemIndex(StarSys[] systems, UID uid)
        {
            for (int i = 0; i < systems.Length; i++)
            {
                var sid = systems[i].Uid;
                if (sid.Type == uid.Type && sid.Id == uid.Id)
                    return i;
            }
            return -1;
        }

        private void ApplyToRenderers(Color color)
        {
            if (renderers == null || renderers.Length == 0)
                return;

            _mpb ??= new MaterialPropertyBlock();
            for (int i = 0; i < renderers.Length; i++)
            {
                var r = renderers[i];
                if (!r)
                    continue;

                r.GetPropertyBlock(_mpb);
                _mpb.SetColor("_BaseColor", color);
                _mpb.SetColor("_Color", color);
                _mpb.SetColor("_EmissionColor", new Color(color.r, color.g, color.b, 1f));
                r.SetPropertyBlock(_mpb);
            }
        }

        private void ClearRendererOverrides()
        {
            if (renderers == null || renderers.Length == 0)
                return;

            for (int i = 0; i < renderers.Length; i++)
            {
                var r = renderers[i];
                if (!r)
                    continue;

                r.SetPropertyBlock(null);
            }
        }

        private ColorSettings GetColorSettings()
        {
            if (_links)
            {
                return new ColorSettings(
                    _links.ConstellationSaturation,
                    _links.ConstellationValue,
                    _links.ConstellationAlpha,
                    _links.EmptyConstellationColor,
                    _links.NoColoringLinkColor,
                    _links.LineColor);
            }

            // запасные значения совпадают со значениями по умолчанию в ConstellationLinksRenderer
            return new ColorSettings(
                0.75f,
                0.9f,
                0.65f,
                new Color(0.6f, 0.85f, 1f, 0.65f),
                new Color(1f, 1f, 1f, 0.85f),
                new Color(0.4f, 0.8f, 1f, 0.5f));
        }

        private readonly struct ColorSettings
        {
            public readonly float ConstellationSaturation;
            public readonly float ConstellationValue;
            public readonly float ConstellationAlpha;
            public readonly Color EmptyConstellationColor;
            public readonly Color NoColoringColor;
            public readonly Color DefaultColor;

            public ColorSettings(
                float constellationSaturation,
                float constellationValue,
                float constellationAlpha,
                Color emptyConstellationColor,
                Color noColoringColor,
                Color defaultColor)
            {
                ConstellationSaturation = constellationSaturation;
                ConstellationValue = constellationValue;
                ConstellationAlpha = constellationAlpha;
                EmptyConstellationColor = emptyConstellationColor;
                NoColoringColor = noColoringColor;
                DefaultColor = defaultColor;
            }
        }
    }
}

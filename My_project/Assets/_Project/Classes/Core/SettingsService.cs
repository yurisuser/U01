using UnityEngine;

namespace _Project.Scripts.Core
{
    /// <summary>Сервис пользовательских настроек.</summary>
    public sealed class SettingsService
    {
        private const string ShowHyperlinksKey = "Settings_ShowHyperlinks";
        private const string UseHyperlinkColoringKey = "Settings_UseHyperlinkColoring";
        private const string UseFractionColoringKey = "Settings_UseFractionColoring";
        private const string UseSecurityColoringKey = "Settings_UseSecurityColoring";
        private static SettingsService _instance;

        private bool _showHyperlinks;
        private bool _useHyperlinkColoring;
        private bool _useFractionColoring;
        private bool _useSecurityColoring;

        public static SettingsService Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new SettingsService();

                return _instance;
            }
        }

        private SettingsService()
        {
            _showHyperlinks = PlayerPrefs.GetInt(ShowHyperlinksKey, 1) == 1;
            _useHyperlinkColoring = PlayerPrefs.GetInt(UseHyperlinkColoringKey, 1) == 1;
            _useFractionColoring = PlayerPrefs.GetInt(UseFractionColoringKey, 1) == 1;
            _useSecurityColoring = PlayerPrefs.GetInt(UseSecurityColoringKey, 0) == 1;
        }

        public bool ShowHyperlinks => _showHyperlinks;
        public bool UseHyperlinkColoring => _useHyperlinkColoring;
        public bool UseFractionColoring => _useFractionColoring;
        public bool UseSecurityColoring => _useSecurityColoring;

        public void SetShowHyperlinks(bool show)
        {
            if (_showHyperlinks == show)
                return;

            _showHyperlinks = show;
            PlayerPrefs.SetInt(ShowHyperlinksKey, show ? 1 : 0);
            PlayerPrefs.Save();

            var gameState = GameBootstrap.GameState;
            if (gameState != null)
                gameState.ApplyShowHyperlinks(show);
        }

        public void SetUseHyperlinkColoring(bool use)
        {
            if (_useHyperlinkColoring == use)
                return;

            _useHyperlinkColoring = use;
            PlayerPrefs.SetInt(UseHyperlinkColoringKey, use ? 1 : 0);
            PlayerPrefs.Save();

            var gameState = GameBootstrap.GameState;
            if (gameState != null)
                gameState.ApplyUseHyperlinkColoring(use);
        }

        public void SetUseFractionColoring(bool use)
        {
            if (_useFractionColoring == use)
                return;

            _useFractionColoring = use;
            PlayerPrefs.SetInt(UseFractionColoringKey, use ? 1 : 0);
            PlayerPrefs.Save();

            var gameState = GameBootstrap.GameState;
            if (gameState != null)
                gameState.ApplyUseFractionColoring(use);
        }

        public void SetUseSecurityColoring(bool use)
        {
            if (_useSecurityColoring == use)
                return;

            _useSecurityColoring = use;
            PlayerPrefs.SetInt(UseSecurityColoringKey, use ? 1 : 0);
            PlayerPrefs.Save();

            var gameState = GameBootstrap.GameState;
            if (gameState != null)
                gameState.ApplyUseSecurityColoring(use);
        }
    }
}

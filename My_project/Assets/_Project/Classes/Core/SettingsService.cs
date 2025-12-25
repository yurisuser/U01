using UnityEngine;

namespace _Project.Scripts.Core
{
    /// <summary>Сервис пользовательских настроек.</summary>
    public sealed class SettingsService
    {
        private const string ShowHyperlinksKey = "Settings_ShowHyperlinks";
        private static SettingsService _instance;

        private bool _showHyperlinks;

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
        }

        public bool ShowHyperlinks => _showHyperlinks;

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
    }
}

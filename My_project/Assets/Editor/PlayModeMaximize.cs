using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;

[InitializeOnLoad]
public static class PlayModeMaximize
{
    private const string PrefKey = "PlayModeMaximize_EditorWasMaximized";

    static PlayModeMaximize()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            var gameView = GetGameView();
            if (gameView == null)
                return;

            EditorPrefs.SetBool(PrefKey, gameView.maximized);
            if (!gameView.maximized)
                gameView.maximized = true;
        }
        else if (state == PlayModeStateChange.EnteredEditMode)
        {
            var gameView = GetGameView();
            if (gameView == null)
                return;

            if (!EditorPrefs.GetBool(PrefKey, false) && gameView.maximized)
                gameView.maximized = false;
        }
    }

    private static EditorWindow GetGameView()
    {
        var gameViewType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameView");
        if (gameViewType == null)
            return null;

        return EditorWindow.GetWindow(gameViewType);
    }
}

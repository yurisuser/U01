#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace _Project.Editor
{
    [InitializeOnLoad]
    public static class PlayFromBoot
    {
        // ⬇️ укажи путь к своей Boot-сцене
        private const string BootScenePath = "Assets/_Project/Scenes/Boot.unity";

        static PlayFromBoot()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.ExitingEditMode) return;

            var boot = AssetDatabase.LoadAssetAtPath<SceneAsset>(BootScenePath);
            if (boot == null) return;

            // Гарантируем старт именно из Boot
            if (EditorSceneManager.playModeStartScene != boot)
            {
                EditorSceneManager.playModeStartScene = boot;
                Debug.Log($"[PlayFromBoot] Play Mode start scene = {BootScenePath}");
            }
        }

        [MenuItem("Tools/Play/Play From Boot _F5")]
        public static void PlayNow()
        {
            var boot = AssetDatabase.LoadAssetAtPath<SceneAsset>(BootScenePath);
            if (boot == null) { Debug.LogWarning($"[PlayFromBoot] Not found: {BootScenePath}"); return; }
            EditorSceneManager.playModeStartScene = boot;
            EditorApplication.isPlaying = true;
        }

        [MenuItem("Tools/Play/Clear Play Start Scene")]
        public static void ClearStartScene() => EditorSceneManager.playModeStartScene = null;
    }
}
#endif
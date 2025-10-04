using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Project.Scripts
{
    [DefaultExecutionOrder(-1000)]
    public class SceneBanner : MonoBehaviour
    {
        public bool show = true;

        private void OnGUI()
        {
            if (!show) return;
            var name = SceneManager.GetActiveScene().name;
            var rect = new Rect(10, 10, 480, 24);
            GUI.color = new Color(0,0,0,0.5f);
            GUI.Box(new Rect(8, 8, 484, 28), GUIContent.none);
            GUI.color = Color.white;
            GUI.Label(rect, $"Scene: {name}");
        }
    }
}
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

namespace _Project
{
    public class DebugViewer : MonoBehaviour
    {
        private static DebugViewer _instance;

        [SerializeField] private TextMeshProUGUI[] lines = new TextMeshProUGUI[10];
        [SerializeField] private GameObject panel;

        private void Awake()
        {
            // Очистим все строки при старте
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i] != null)
                    lines[i].text = "";
            }
        }
        private void Start()
        {
            if (panel != null) panel.SetActive(false); // панель изначально выключена
            if (!Application.isPlaying) return;
            if (Core.Instance != null) 
                Core.Instance.Input.Subscribe(Key.F1, TogglePanel);
        }

        private void OnDisable()
        {
            if (!Application.isPlaying) return;
            if (Core.Instance != null)
                Core.Instance.Input.Unsubscribe(Key.F1, TogglePanel);
        }

        public void ShowMe(int num, string text, object val)
        {
            if (num < 0 || num >= lines.Length) return;
            if (lines[num] != null)
                lines[num].text = text + "\t" + val;
        }

        private void TogglePanel()
        {
            if (panel != null)
                panel.SetActive(!panel.activeSelf);
        }
    }
}
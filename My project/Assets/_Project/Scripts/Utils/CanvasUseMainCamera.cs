using UnityEngine;

namespace _Project.Scripts.Utils
{
    [ExecuteAlways, RequireComponent(typeof(Canvas))]
    public class CanvasUseMainCamera : MonoBehaviour
    {
        void OnEnable()
        {
            var canvas = GetComponent<Canvas>();
            if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                if (canvas.worldCamera == null)
                {
                    var cam = Camera.main;
                    if (cam != null) canvas.worldCamera = cam;
                }
            }
        }
    }
}
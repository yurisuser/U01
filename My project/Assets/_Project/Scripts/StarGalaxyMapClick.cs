using UnityEngine;
using _Project.Galaxy.Obj; // StarType

namespace Scripts
{
    [DisallowMultipleComponent]
    public class StarGalaxyMapClick : MonoBehaviour
    {
        [HideInInspector] public StarType Type;
        [HideInInspector] public string SystemName;

        private void OnMouseUpAsButton()
        {
            Debug.Log($"[Star] {SystemName} → {Type}");
        }
    }
}
using UnityEngine;
using _Project.Scripts.Simulation.Ships;
using _Project.Scripts.Ships;

namespace _Project.Scripts.Simulation.Local.Stages.Movement
{
    /// <summary>Отвечает за расчёт нового курса корабля (ориентации/направления).</summary>
    public sealed class CourseChanger
    {
        /// <summary>Линейное движение: нос в сторону цели.</summary>
        public Vector3 GetDirection(in Vector3 shipPosition, Vector3 currentForward, in Vector3 destinationPos, float agility, float deltaTime)
        {
            var toTarget = destinationPos - shipPosition;
            if (toTarget.sqrMagnitude <= 0f)
                return currentForward;

            var desired = toTarget.normalized;
            var current = currentForward.sqrMagnitude > 0f ? currentForward.normalized : Vector3.up;

            float maxAngle = Mathf.Max(0f, agility) * Mathf.Rad2Deg * Mathf.Max(0f, deltaTime);
            if (maxAngle <= 0f)
                return desired;

            float angleBetween = Vector3.SignedAngle(current, desired, Vector3.forward);
            float clamped = Mathf.Clamp(angleBetween, -maxAngle, maxAngle);
            return (Quaternion.AngleAxis(clamped, Vector3.forward) * current).normalized;
        }

        /// <summary>Орбита: нос по касательной вокруг центра с заданным радиусом.</summary>
        public Vector3 GetOrbitDirection(in Vector3 shipPosition, Vector3 currentForward, in Vector3 orbitCenter, float orbitRadius, float agility, float deltaTime)
        {
            var toCenter = orbitCenter - shipPosition;
            if (toCenter.sqrMagnitude <= 0f || orbitRadius <= 0f)
                return currentForward;

            var tangent = GetOrbitTangent(shipPosition, orbitCenter, currentForward);
            return GetDirection(shipPosition, currentForward, shipPosition + tangent, agility, deltaTime);
        }

        private static Vector3 GetOrbitTangent(in Vector3 shipPosition, in Vector3 orbitCenter, in Vector3 currentForward)
        {
            var toCenter = orbitCenter - shipPosition;
            if (toCenter.sqrMagnitude <= 0f)
                return currentForward;

            // две касательные: вправо и влево от радиального вектора
            var tangentRight = new Vector3(-toCenter.y, toCenter.x, 0f).normalized;
            var tangentLeft = new Vector3(toCenter.y, -toCenter.x, 0f).normalized;

            // выбираем ту, которая ближе по углу к текущему forward
            float dotRight = Vector3.Dot(currentForward.normalized, tangentRight);
            float dotLeft = Vector3.Dot(currentForward.normalized, tangentLeft);

            if (Mathf.Approximately(dotRight, dotLeft))
                return tangentRight; // равны — берём вправо по умолчанию

            return dotRight > dotLeft ? tangentRight : tangentLeft;
        }
    }
}

using System;
using UnityEngine;

namespace _Project.Scripts.Core
{
    /// <summary>Простой логгер сборок мусора по поколениям.</summary>
    public sealed class GCLogger : MonoBehaviour
    {
        [Tooltip("Логировать генерацию 0")] public bool logGen0 = true;
        [Tooltip("Логировать генерацию 1")] public bool logGen1 = true;
        [Tooltip("Логировать генерацию 2")] public bool logGen2 = true;

        private int _gen0;
        private int _gen1;
        private int _gen2;

        private void Awake()
        {
            _gen0 = GC.CollectionCount(0);
            _gen1 = GC.CollectionCount(1);
            _gen2 = GC.CollectionCount(2);
        }

        private void Update()
        {
            bool changed = false;
            if (logGen0)
                changed |= Check(ref _gen0, 0);
            if (logGen1)
                changed |= Check(ref _gen1, 1);
            if (logGen2)
                changed |= Check(ref _gen2, 2);

            if (changed)
            {
                UnityEngine.Debug.Log($"[GC] gen0={_gen0}, gen1={_gen1}, gen2={_gen2}, t={Time.realtimeSinceStartup:0.000}");
            }
        }

        private bool Check(ref int last, int gen)
        {
            int now = GC.CollectionCount(gen);
            if (now != last)
            {
                last = now;
                return true;
            }
            return false;
        }
    }
}

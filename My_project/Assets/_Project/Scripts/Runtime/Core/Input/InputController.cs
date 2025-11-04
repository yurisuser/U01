using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace _Project.Scripts.Core.Input
{
    public sealed class InputController
    {
        // Подписки на "нажатие в этот кадр"
        private readonly Dictionary<Key, Action> _keyDownHandlers = new();
        private readonly List<Key> _keysSnapshot = new(); // чтобы не ловить "collection modified"

        public void Subscribe(Key key, Action handler)
        {
            if (handler == null) return;

            if (_keyDownHandlers.TryGetValue(key, out var existing))
                _keyDownHandlers[key] = existing + handler;
            else
                _keyDownHandlers.Add(key, handler);
        }

        public void Unsubscribe(Key key, Action handler)
        {
            if (!_keyDownHandlers.TryGetValue(key, out var existing)) return;

            existing -= handler;
            if (existing == null) _keyDownHandlers.Remove(key);
            else _keyDownHandlers[key] = existing;
        }

        public void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            _keysSnapshot.Clear();
            _keysSnapshot.AddRange(_keyDownHandlers.Keys);

            foreach (var key in _keysSnapshot)
            {
                if (!_keyDownHandlers.TryGetValue(key, out var action) || action == null) continue;

                var keyCtrl = kb[key];
                if (keyCtrl != null && keyCtrl.wasPressedThisFrame)
                {
                    try { action.Invoke(); }
                    catch (Exception e) { UnityEngine.Debug.LogException(e); }
                }
            }
        }
    }
}
using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace _Project.Inputs
{
    public class InputController
    {
        private readonly Dictionary<Key, Action> _handlers = new();

        public void Update()
        { 
            //UnityEngine.Debug.Log("VAR");
            var kb = Keyboard.current;
            if (kb == null) return;

            foreach (var kvp in _handlers)
            {
                if (kb[kvp.Key].wasPressedThisFrame)
                    kvp.Value?.Invoke();
            }
        }

        public void Subscribe(Key key, Action handler)
        {
            if (!_handlers.ContainsKey(key))
                _handlers[key] = null;

            _handlers[key] += handler;
        }

        public void Unsubscribe(Key key, Action handler)
        {
            if (!_handlers.ContainsKey(key)) return;

            _handlers[key] -= handler;
            if (_handlers[key] == null)
                _handlers.Remove(key);
        }
    }
}
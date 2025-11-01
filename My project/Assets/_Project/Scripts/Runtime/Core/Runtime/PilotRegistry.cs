using System.Collections.Generic;
using _Project.Scripts.Core;
using _Project.Scripts.Simulation;

namespace _Project.Scripts.Core.Runtime
{
    /// <summary>
    /// Хранилище состояний пилотов по их UID.
    /// </summary>
    public sealed class PilotRegistry
    {
        private readonly Dictionary<UID, PilotMotiv> _data = new(128);

        public void Reset()
        {
            _data.Clear();
        }

        public void SetMotiv(UID pilotUid, in PilotMotiv motiv)
        {
            _data[pilotUid] = motiv;
        }

        public bool TryGetMotiv(UID pilotUid, out PilotMotiv motiv)
        {
            return _data.TryGetValue(pilotUid, out motiv);
        }

        public bool TryUpdateMotiv(UID pilotUid, in PilotMotiv motiv)
        {
            if (!_data.ContainsKey(pilotUid))
                return false;

            _data[pilotUid] = motiv;
            return true;
        }
    }
}

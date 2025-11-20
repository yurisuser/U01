using System.Collections.Generic;
using _Project.Scripts.Core;
using _Project.Scripts.Simulation.PilotMotivation;

namespace _Project.Scripts.Core.Runtime
{
    /// <summary>Хранилище состояний пилотов по их UID.</summary>
    public sealed class PilotRegistry
    {
        private readonly Dictionary<UID, PilotMotive> _data = new(128); // Быстрый доступ к мотивам.

        /// <summary>Очищает все сохранённые мотивы.</summary>
        public void Reset()
        {
            _data.Clear();
        }

        /// <summary>Сохраняет мотив для пилота (вставка или замена).</summary>
        public void SetMotiv(UID pilotUid, in PilotMotive motiv)
        {
            _data[pilotUid] = motiv;
        }

        /// <summary>Пытается получить мотив пилота.</summary>
        public bool TryGetMotiv(UID pilotUid, out PilotMotive motiv)
        {
            return _data.TryGetValue(pilotUid, out motiv);
        }

        /// <summary>Обновляет мотив, если пилот уже зарегистрирован.</summary>
        public bool TryUpdateMotiv(UID pilotUid, in PilotMotive motiv)
        {
            if (!_data.ContainsKey(pilotUid))
                return false;

            _data[pilotUid] = motiv;
            return true;
        }
    }
}

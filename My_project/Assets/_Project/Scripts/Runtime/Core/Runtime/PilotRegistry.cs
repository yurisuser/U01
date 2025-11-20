using System.Collections.Generic;
using _Project.Scripts.Core;
using _Project.Scripts.Simulation.PilotMotivation;

namespace _Project.Scripts.Core.Runtime
{
    // Хранилище состояний пилотов по их UID.
    public sealed class PilotRegistry
    {
        private readonly Dictionary<UID, PilotMotive> _data = new(128); // Быстрый доступ к мотивам.

        // Очищаем все данные.
        public void Reset()
        {
            _data.Clear();
        }

        // Сохраняем мотив для пилота (вставка или замена).
        public void SetMotiv(UID pilotUid, in PilotMotive motiv)
        {
            _data[pilotUid] = motiv;
        }

        // Пытаемся получить мотив.
        public bool TryGetMotiv(UID pilotUid, out PilotMotive motiv)
        {
            return _data.TryGetValue(pilotUid, out motiv);
        }

        // Обновляем мотив, если пилот зарегистрирован.
        public bool TryUpdateMotiv(UID pilotUid, in PilotMotive motiv)
        {
            if (!_data.ContainsKey(pilotUid))
                return false;

            _data[pilotUid] = motiv;
            return true;
        }
    }
}

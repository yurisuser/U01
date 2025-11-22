# План рефакторинга цепочки мотивации и движения

1) **Зафиксировать поведение и эталоны**
   - Выписать список действий: MoveToCoordinates, AttackTarget, AcquireTarget (позже Patrol/AttackAll).
   - Снять короткие эталонные сценарии (логи/скрины) для простых кейсов: одиночный move, атака неподвижной цели, потеря цели.

2) **Определить новые контракты данных**
   - Описать `ShipState` (позиция, скорость, вращение, активность, статы), `PilotState` (мотив, стек действий), `WorldSlice` (буфер систем/целей на тик).
   - Уточнить роли буферов RenderSnapshot: `Prev`, `Curr`, `Next`, кто и когда их обновляет.
   - Интерфейс трассировки: `ITraceSink.AddSample(uid, tFrac, pos, rot)`.

3) **Создать центральный обработчик пилота**
   - Новый `PilotStepProcessor.ProcessPilotStep(ref Ship, ref PilotMotive, StarSystemState, float dt, ITraceSink trace, List<ShotEvent> shots)`.
   - Внутри линейно: `UpdateMotive` → `TryPeekAction` → `ExecuteAction` (switch) → `ApplyOutcome`.
   - Никаких статических SetTraceWriter/ClearTraceWriter; все зависимости в аргументах.

4) **Разделить исполнителей по ответственности**
   - `MovementExecutor.Execute(ref Ship, MovementPlan plan, float dt, ITraceSink trace)` — движение/сабстепы.
   - `CombatExecutor.Execute(ref Ship attacker, ref Ship target, float distance, List<ShotEvent>)` — урон/события.
   - Targeting/Positioning — утилиты без состояния.

5) **Упростить behaviors в планировщики**
   - `RunMove` → MovementExecutor, возвращает `ActionResult`.
   - `RunAttack` → точка подхода/MovementExecutor → CombatExecutor; флаги `Completed/TargetLost/Fired`.
   - `RunAcquire` → поиск цели и завершение действия.
   - Все switch/ветки в одном месте, математика — в приватных функциях без побочных мутаций.

6) **Переподключить ExecutorShipUpdater**
   - В цикле по кораблям вызвать `PilotStepProcessor.ProcessPilotStep(...)`.
   - Обновление реестров (Systems/Pilots) оставить как единственное место записи состояния после шага.

7) **Привести MoveToPosition к новому интерфейсу**
   - Убрать статический trace writer, принимать `ITraceSink`.
   - Сабстеп-логика сохраняется, вывод — через интерфейс; возврат bool reached.

8) **Документировать снапшоты и Render**
   - Явно описать, кто/когда обновляет `Prev/Curr/Next` и как `StepProgress` связан с симуляцией/пауза/step-by-step.
   - Проверить, что рендер системной карты интерполирует одну пару снапшотов последовательно.

9) **Тесты/проверки**
   - Юниты/интеграция: MovementExecutor без скачков, AttackTarget метит TargetLost при смерти цели.
   - Сравнить новые логи с эталонами из шага 1; при расхождениях корректировать.

10) **Миграция по шагам**
   - Реализовать `PilotStepProcessor` и `MovementExecutor`, подменить в ExecutorShipUpdater.
   - Перенести Move/Attack/Acquire в новые функции.
   - Удалить старые статические SetTraceWriter/использования.

11) **Дополнительно для надёжности**
   - Единый владелец `dt`/`TickIndex` (Executor) для синхронизации симуляции и рендера.
   - На время миграции — флаг переключения старого/нового пайплайна для сравнения.
   - Лёгкое логирование в центральном методе: action, цель, start/end pos, reached/targetLost.

12) **Очистка и финал**
   - Удалить/деактивировать неиспользуемые старые функции, обновить комментарии.
   - Пересобрать и визуально проверить отсутствие дерганий/потери целей.


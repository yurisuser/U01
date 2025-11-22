# AI-map (снимок состояния ИИ)

## 1. Текущий маршрут тика
 - `Executor.Execute` → первичный спавн (`ShipSpawnService.EnsureInitialShips`), `Tasks.Tick`, `Ships.Tick`, `ExecutorShipUpdater.UpdateShips`, маркеры `GameStateService`.
- `ExecutorShipUpdater.UpdateShips` → цикл систем → цикл кораблей: получить `Ship` + `PilotMotive`, `Motivator.Update`, `SetTraceWriter` (только активная система), `ExecuteAction` (switch), `OnActionCompleted` при `Completed`, запись обратно в `Systems`/`Pilots`, `ClearTraceWriter`.
- `Behaviors` (Move/Attack/Acquire) напрямую меняют `Ship`, иногда трогают цель/список выстрелов; работают с `StarSystemState`.
- Данные: `ShotEvent` общим списком, `SubstepTraceBuffer` через статический `MoveToPosition` (трассы только в активной системе).

## 2. Роли объектов (как сейчас)
- Intent: `PilotMotive.Order` (`EPilotOrder` + `ActionParam`).
- Plan: стек `PilotAction` в `PilotMotive`, поддерживается через `Ensure*` (в `Motivator` и самом `PilotMotive`).
- Behavior: `MoveToCoordinatesBehavior`, `AttackTargetBehavior`, `ChoiceTargetBehavior` — чистые статики, вызываются из `ExecutorShipUpdater`.
- Sensors: нет отдельного слоя; выбор цели/проверки зашиты в поведения.
- Actuators: изменение `Ship`, `PilotMotive`, события `ShotEvent`, трассы в `SubstepTraceBuffer`.

## 3. Узкие места/источники путаницы
- Контекст течёт насквозь: `Behaviors` знают о `StarSystemState` и иногда о `RuntimeContext`.
- Планировщик размазан: `Motivator.Update`, `PilotMotive.Ensure*`, `ExecutorShipUpdater.OnActionCompleted` — сложно проследить, кто и когда формирует/закрывает действие.
- Трассировка через статический `MoveToPosition.SetTraceWriter/ClearTraceWriter`.
- Нет явного `WorldView`/сенсоров: поиск целей и валидации вшиты в поведения.

## 4. Целевая схема (якорь)
- Intent: приказ в `PilotMotive` (`SetOrder`), хранится отдельно от плана.
- Plan: единый «обновить план» метод (планировщик) в одном месте, формирует стек `PilotAction`.
- Behavior: интерфейсный слой `Execute(action, worldView, dt)`; реестр поведений вместо switch, контекст — только нужные данные.
- Sensors/Queries: отдельные функции/класс для поиска цели, проверки дистанции, валидации UID — поведения дергают только их.
- Actuators: всё, что мутирует мир, проходит через один выход: изменения `Ship`, `PilotMotive`, `ShotEvent`, `TraceSink` (не статический).

## 5. Ближайшие шаги по внедрению схемы
- Завести сенсоры/квери (папка `Simulation/Sensors`): вынести логику поиска/валидации цели из `ChoiceTargetBehavior`.
- Собрать планировщик в одном месте (`PilotMotive.UpdatePlan` или отдельный сервис) и убрать дубли `Ensure*` между `Motivator`/`PilotMotive`.
- Сделать интерфейс для поведений + реестр, чтобы `ExecutorShipUpdater` выбирал поведение без switch.
 - Убрать статический trace writer: `MoveToPosition` принимает `ITraceSink` + `UID` в аргументах, активная система даёт буфер, остальные — `null`; цель — убрать глобал `Set/ClearTraceWriter`, сделать трассировку явной и тестируемой.

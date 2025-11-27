Current task.md

Минимальный каркас:
LocalSimulationContext (readonly struct): ссылки на GameStateService, идентификатор активной системы/снэпшот, float dt, int day, буферы (шоты, события). Без аллокаций.
LocalSimulationPipeline : держит список стадий List<ISimulationStage>, Name = "Local", RunStep(in ctx) проходит стадии по порядку.
Стадии по плану (можно пустые заглушки):
LocalInputStage — собирает команды игрока/триггеры (пока noop).
LocalPerceptionStage — строит видимый мир для юнитов (пока noop).
LocalAiStage — принятие решений (noop).
LocalMovementStage — движение (noop).
LocalInteractionStage — стыковка/ремонт (noop).
LocalCombatStage — бой (noop).
LocalEventsStage — события (noop).
LocalSnapshotStage — сбор среза для рендера (noop или заполняет пустой снапшот).
Проведение через SimulationRootController: RunLocal создаёт LocalSimulationContext и вызывает LocalSimulationPipeline.RunStep. Контекст можно собрать из SimulationStepContext + активной системы (например, GetSelectedSystem()).

Чтобы не плодить мусор: стадиям передавать in LocalSimulationContext; пайплайн — один список стадий (классы-заглушки без аллокаций внутри). Логи — по флагу.

Папки/файлы:

Simulation/Local/
  LocalSimulationPipeline.cs
  LocalSimulationContext.cs
  Stages/
    01_InputStage.cs
    02_PerceptionStage.cs
    03_AiStage.cs
    04_MovementStage.cs
    05_InteractionStage.cs
    06_CombatStage.cs
    07_EventsStage.cs
    08_SnapshotStage.cs
Ступени пока Noop (шаблон), чтобы собрать структуру.

Если устраивает — могу накинуть каркас с noop-стадиями и подключить к SimulationRootController вместо NoopSimulationPipeline.
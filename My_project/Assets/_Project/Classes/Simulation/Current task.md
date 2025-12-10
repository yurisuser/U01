## План патруля для кораблей (черновик)

1. **Где живёт логика**
   - Сохраняем всё в локальной симуляции: 
         `LocalPerceptionStage` собирает цели/опасности, 
         `LocalAiStage` выдает приказы, 
         `LocalMovementStage` исполняет.
   - В `SystemState` добавляем списки `ShipAgents` (содержит UID корабля, актуальную команду и целевую точку патруля).

2. **Данные агента**
   - Поля: `UID ShipId`, `Vector2 PatrolTarget`, `AiCommandType CurrentCommand`, `float RepathCooldown`.
   - Дополнительно флажок «нужен ли новый патруль» если корабль достиг цели или потерял задачу.

3. **Примитивы поведения**
   - `MoveToPositionCommand` (корабль должен дойти до `PatrolTarget`).
   - `PatrolCommand` – обертка, которая проверяет дистанцию, выбирает новую точку в радиусе (например, 100–200 условных единиц) и создаёт `MoveToPosition`.
   - Все команды – простые структуры с таймером и статусом (`Pending`, `Running`, `Completed`).

4. **Пайплайн шага**
   1. Perception обновляет `ShipAgents`: дополняет списком кораблей, обновляет их позиции.
   2. AiStage:
      - Для каждого агента: если `CurrentCommand` null или выполнена — генерируем новую точку (случайная позиция в плоскости системы, ограничиваемся заданным радиусом). Записываем в `PatrolTarget`.
      - Создаём `MoveToPositionCommand`, кладём в очередь команд (например `SystemState.PendingShipCommands`).
   3. MovementStage:
      - Берет `PendingShipCommands`, сдвигает корабль к точке (линейное движение в той же плоскости).
      - При достижении расстояния <= порога (например, 2ед.) отправляет событие `CommandCompleted`, чтобы AiStage в следующий проход сгенерил новую цель.

5. **Хранение данных**
   - `SystemState`: 
     - `List<Ship> Ships` (как сейчас).
     - `List<ShipAgentState> ShipAgents`.
     - `Queue<ShipCommand> PendingShipCommands`.
   - `ShipAgentState` ищем по UID. Если корабль уничтожен/покинул систему — удаляем запись.

6. **Прочие детали**
   - Все расчёты в одной плоскости (Z = 0).
   - Новая цель патруля = `Random.insideUnitCircle * patrolRadius`.
   - Расстояние проверки – `Vector2.Distance(currentPos, target)`.
   - На будущее: когда появятся другие задачи, `ShipAgentState` уже будет расширяемым (можно добавить `AiRole`, `Priority`).
   - Никаких статических синглтонов: всё держим в `SystemState` и передаём через контексты стадий.

ContinuumPlan.md

# План: межсистемный переход через Continuum

Цель: включить прыжки между системами и не ломать будущую торговлю/логистику.

## Шаги
1. **Структуры транзита**  
   - `ContinuumTransit`: `Ship` (оригинальный объект на время прыжка), `FromSystemIndex`, `ToSystemIndex`, `RemainingTurns`.  
   - Направление храним в самом `Ship.Rotation`/скорости при выходе; в транзите не дублируем.  
   - Для появления используем зону между 1–3 орбитой по линии from→to (радиус в этом диапазоне, отступ + `EntryZoneOffset`).  

2. **Вход в зону (локальная симуляция)**  
   - Локальный Movement ищет активный `ShipTask.JumpToSystem` и `ContinuumZone` нужного линка.  
   - Условие входа: `distance <= EntryZoneRadius` и вектор от звезды ≈ направления линка (можно допуск по углу).  
   - Действия (в таком порядке, чтобы не дёргать визуал):  
     1) Удалить корабль из `LocalSysRuntimeContext.Ships` (чтобы не обновлялся/не рендерился).  
     2) Выставить кораблю `Rotation` и скорость по линии прыжка уже вне сцены.  
     3) Создать `ContinuumTransit` (Ship, from, to, RemainingTurns=ContinuumConsts.JumpDurationTurns) и `Enqueue`.  

3. **Прилёт (глобальная симуляция, ContinuumService.Tick)**  
   - `RemainingTurns--`; на 0:  
     - `dirBA = normalize(fromPos - toPos)` для точки появления.  
     - Радиус появления: фиксированная 3-я орбита целевой звезды, позиция = звезда + `dirBA * orbit3`.  
     - `Rotation`/скорость оставляем как были (по линии прыжка); всегда форсим `CurrentSpeed = MaxSpeed`.  
     - Добавить корабль в `targetStarSys.State.Ships`.

4. **Зоны**  
   - Строить по `HyperlinkEdge`: направление берём с галкарты (A→B), центр = позиция звезды + `dirAB * (outerOrbit + EntryZoneOffset)` в локальных координатах системы, радиус `EntryZoneRadius` (20), отступ 30.  
   - Уточнение: `outerOrbit` = max(PlanetOrbits, Star.radius). Пересчитывать при смене галактики/линков.

5. **Задачи корабля**  
   - Добавить `ShipTask.JumpToSystem(targetSystemId)`. Планнер разворачивает: `MoveToPoint(zone.Center, tolerance=EntryZoneRadius) -> Jump`.  
   - После прилёта AI сам выберет следующую задачу (например, `MoveToPoint` из сохранённого приказа).

6. **Отрисовка зон (сцена)**  
   - Всегда отображать `ContinuumZone` активной системы в сцене через `LineRenderer` (loop) как тонкий контур круга, данные — из `ContinuumService.ZonesBySystem`.  
   - Цвет линии = цвет гиперлинка/системы назначения.

7. **Расширение для торговли**  
   - Continuum остаётся узким: только перемещение, без экономики.  
   - Логистика/торговля срабатывает после вставки корабля (без обязательных событий).

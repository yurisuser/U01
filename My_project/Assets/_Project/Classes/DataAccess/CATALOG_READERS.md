# Ридеры каталогов (DataAccess)

Назначение: единые правила чтения профильных таблиц БД и выдачи данных в код.

## Базовая идея
- Общей таблицы `items` нет.
- Каждый тип хранится в своей таблице (eq-weapons/goods/quest/eq-engines/eq-scanners/eq-shields).
- Ридеры читают профильные таблицы и кешируют результаты.
- Оркестратор даёт базовую карточку предмета по `ItemType + Id`.
- Источник истины: SQLite через `GameDatabaseLite` + ридеры. Других кешей/фасадов не плодим.
- CatalogTypes — DTO под таблицы БД. Игровые модели (например, NPC.Fraction) живут отдельно, чтобы не смешивать уровни.
- Каталоги прогреваются один раз на старте игры и живут весь рантайм без пересозданий, чтобы не плодить GC.
- Внешние файлы данных (не из БД) храним в `Assets/_Project/Data`; каталоги читают оттуда при необходимости.

## Ридеры
- Папка: `Assets/_Project/Classes/DataAccess/Readers`
- `WeaponCatalogReader`
- `GoodsCatalogReader`
- `QuestCatalogReader`
- `EngineCatalogReader`
- `ScannerCatalogReader`
- `ShieldCatalogReader`
- `ShipCatalogReader`

Каждый ридер:
- Читает свою таблицу через `GameDatabaseLite`.
- Кеширует `Dictionary<int, T>`.
- Отдаёт `GetAll()` и `TryGet(id, out item)`.

## Оркестратор
`ItemCatalogService`:
- Метод: `TryGetInfo(ItemType type, int id, out CatalogItemInfo info)`.
- Возвращает базовые поля: `id/key/name/description/price/weight/stackable/max_stack`.
- Не возвращает спец‑поля (урон, радиус и т.д.) — они берутся из профильных ридеров.

## Замечания
- Кеш живёт в статике, перезагрузка возможна только через `GameDatabaseLite` с `forceReload`.
- Для спец‑данных использовать профильные ридеры напрямую.
- `ShipCatalogReader` используется для создания кораблей и берёт данные из таблицы `ships`.

## Базовые поля таблиц предметов
- `id`, `key`, `display_name`, `description`
- `price`, `weight`, `stackable`, `max_stack`
- `stackable` хранится как 0/1 (BOOLEAN в SQLite).

## Спец‑поля
- `eq-weapons`: `tech_level`, `damage`, `rate_per_second`, `range`
- `eq-engines`: `tech_level`, `speed`
- `eq-scanners`: `tech_level`, `radius`
- `eq-shields`: `tech_level`, `radius`, `volume`, `regen`

## Как добавить новый тип
1) Создать таблицу в `GameDatabaseLite.CreateSchema`.
2) Добавить модель каталога в `CatalogTypes.cs`.
3) Добавить `GetXxx()` в `GameDatabaseLite`.
4) Добавить `XxxCatalogReader`.
5) Добавить маппер из `CatalogXxx` в игровую модель, если нужна отдельная runtime-структура.
6) Добавить кейс в `ItemCatalogService`.
7) Обновить `ItemType` и `ITEMS_ARCHITECTURE.md`.

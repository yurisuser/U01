# Ридеры каталогов (DataAccess)

Назначение: единые правила чтения профильных таблиц БД и выдачи данных в код.

## Базовая идея
- Общая таблица `items` есть и используется для всех торговых предметов.
- Остальные типы хранятся в своих таблицах (eq-weapons/quest/eq-engines/eq-scanners/eq-shields).
- Ридеры используются только CATALOG, наружу не торчат.
- Оркестратор даёт базовую карточку предмета по `ItemType + Id`.
- Источник истины: SQLite через `GameDatabaseLite` + ридеры. Других кешей/фасадов не плодим.
- CatalogTypes — DTO под таблицы БД. Игровые модели (например, NPC.Fraction) живут отдельно, чтобы не смешивать уровни.
- Каталоги прогреваются один раз на старте игры и живут весь рантайм без пересозданий, чтобы не плодить GC.
- Внешние файлы данных (не из БД) храним в `Assets/_Project/Data`; каталоги читают оттуда при необходимости.

## Ридеры
- Папка: `Assets/_Project/Classes/DataAccess/Readers`
- `WeaponCatalogReader`
- `ItemCatalogReader`
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

## Базовые поля таблицы items
- `id`, `name`, `description`, `img`
- `price`, `weight`, `stackable`, `max_stack`
- флаги: `isMineable`, `isIndustrial`, `isConsumable`, `isLootOnly`
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

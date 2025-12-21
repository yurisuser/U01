# Ридеры каталогов (DataAccess)

Назначение: единые правила чтения профильных таблиц БД и выдачи данных в код.

## Базовая идея
- Каждый тип хранится в своей таблице (weapons/goods/ammo/quest/engines/scanners/shields и корабли).
- Ридеры читают профильные таблицы и кешируют результаты.
- Оркестратор даёт базовую карточку предмета по `ItemType + Id`.

## Ридеры
- `WeaponCatalogReader`
- `GoodsCatalogReader`
- `AmmoCatalogReader`
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

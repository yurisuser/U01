# Архитектура предметов и оборудования (черновик)

Назначение: зафиксировать минимальную архитектуру предметов и оборудования, с таблицей `items`.

## Ключевая идея
- Предмет в трюме — это тупая коробка.
- В коде хранится только `ItemStack`: `Id`, `Type`, `Quantity`.
- В БД есть таблица `items` для торговых предметов.

## Типы (ItemType)
- Item
- Weapon
- Quest
- Engine
- Scanner
- Shield

## Источник данных (БД)
Таблица `items` хранит торговые предметы, остальное — в профильных таблицах:
- `items`
- `eq-weapons`
- `quest`
- `eq-engines`
- `eq-scanners`
- `eq-shields`

## Базовые поля таблицы items
- `id`, `name`, `description`, `img`
- `price`, `weight`, `stackable`, `max_stack`
- флаги: `isMineable`, `isIndustrial`, `isConsumable`, `isLootOnly`

## Поля для оборудования
- Общее поле для всего оборудования: `tech_level`
- Двигатели: `speed`
- Сканеры: `radius`
- Щиты: `radius`, `volume`, `regen`
- Плотность щита зависит от радиуса и силы, влияет на урон

## Поля для оружия
- `damage`, `rate_per_second`, `range`

## Правило использования
- `ItemStack.Type` выбирает таблицу.
- `ItemStack.Id` — ключ внутри этой таблицы.
- `stackable` хранится как 0/1 (BOOLEAN в SQLite).

## Чтение из БД
- Профильные ридеры: `Weapon/Item/Quest/Engine/Scanner/Shield` (каждый читает свою таблицу).
- Оркестратор: `ItemCatalogService` даёт базовую информацию по `ItemStack` (без спец. полей).

## Как добавить новый тип
1) Добавить таблицу в БД (через `GameDatabaseLite.CreateSchema`).
2) Завести модель каталога в `CatalogTypes.cs`.
3) Добавить метод чтения в `GameDatabaseLite`.
4) Создать профильный ридер.
5) Добавить кейс в `ItemCatalogService`.
6) Обновить `ItemType`.

## Что точно НЕ делаем сейчас
- Логику трюма/переноса.
- Крафт, торговлю, модификаторы.
- Прочие типы, которых нет в списке выше.

-- Переименование таблиц оборудования: старые -> eq-*
ALTER TABLE weapons RENAME TO "eq-weapons";
ALTER TABLE engines RENAME TO "eq-engines";
ALTER TABLE scanners RENAME TO "eq-scanners";
ALTER TABLE shields RENAME TO "eq-shields";

using _Project.Items;

namespace _Project.DataAccess
{
    public static class ItemCatalogService
    {
        public static bool TryGetInfo(ItemType type, int id, out CatalogItemInfo info)
        {
            switch (type)
            {
                case ItemType.Weapon:
                    if (WeaponCatalogReader.TryGet(id, out var weapon))
                    {
                        info = new CatalogItemInfo(
                            weapon.Id, weapon.Key, weapon.DisplayName, weapon.Description,
                            weapon.Price, weapon.Weight, weapon.Stackable, weapon.MaxStack);
                        return true;
                    }
                    break;
                case ItemType.Goods:
                    if (GoodsCatalogReader.TryGet(id, out var goods))
                    {
                        info = new CatalogItemInfo(
                            goods.Id, goods.Key, goods.DisplayName, goods.Description,
                            goods.Price, goods.Weight, goods.Stackable, goods.MaxStack);
                        return true;
                    }
                    break;
                case ItemType.Ammo:
                    if (AmmoCatalogReader.TryGet(id, out var ammo))
                    {
                        info = new CatalogItemInfo(
                            ammo.Id, ammo.Key, ammo.DisplayName, ammo.Description,
                            ammo.Price, ammo.Weight, ammo.Stackable, ammo.MaxStack);
                        return true;
                    }
                    break;
                case ItemType.Quest:
                    if (QuestCatalogReader.TryGet(id, out var quest))
                    {
                        info = new CatalogItemInfo(
                            quest.Id, quest.Key, quest.DisplayName, quest.Description,
                            quest.Price, quest.Weight, quest.Stackable, quest.MaxStack);
                        return true;
                    }
                    break;
                case ItemType.Engine:
                    if (EngineCatalogReader.TryGet(id, out var engine))
                    {
                        info = new CatalogItemInfo(
                            engine.Id, engine.Key, engine.DisplayName, engine.Description,
                            engine.Price, engine.Weight, engine.Stackable, engine.MaxStack);
                        return true;
                    }
                    break;
                case ItemType.Scanner:
                    if (ScannerCatalogReader.TryGet(id, out var scanner))
                    {
                        info = new CatalogItemInfo(
                            scanner.Id, scanner.Key, scanner.DisplayName, scanner.Description,
                            scanner.Price, scanner.Weight, scanner.Stackable, scanner.MaxStack);
                        return true;
                    }
                    break;
                case ItemType.Shield:
                    if (ShieldCatalogReader.TryGet(id, out var shield))
                    {
                        info = new CatalogItemInfo(
                            shield.Id, shield.Key, shield.DisplayName, shield.Description,
                            shield.Price, shield.Weight, shield.Stackable, shield.MaxStack);
                        return true;
                    }
                    break;
            }

            info = default;
            return false;
        }
    }
}

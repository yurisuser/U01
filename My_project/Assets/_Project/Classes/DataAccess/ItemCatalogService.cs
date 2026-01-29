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
                    if (CATALOG.WeaponsById != null && CATALOG.WeaponsById.TryGetValue(id, out var weapon))
                    {
                        info = new CatalogItemInfo(
                            weapon.Id, weapon.Key, weapon.DisplayName, weapon.Description,
                            weapon.Price, weapon.Weight, weapon.Stackable, weapon.MaxStack);
                        return true;
                    }
                    break;
                case ItemType.Goods:
                    if (CATALOG.GoodsById != null && CATALOG.GoodsById.TryGetValue(id, out var goods))
                    {
                        info = new CatalogItemInfo(
                            goods.Id, goods.Key, goods.DisplayName, goods.Description,
                            goods.Price, goods.Weight, goods.Stackable, goods.MaxStack);
                        return true;
                    }
                    break;
                case ItemType.Sku:
                    if (CATALOG.SkuById != null && CATALOG.SkuById.TryGetValue(id, out var sku))
                    {
                        var key = string.IsNullOrWhiteSpace(sku.Name) ? $"sku_{sku.Id}" : sku.Name;
                        var displayName = string.IsNullOrWhiteSpace(sku.Name) ? $"sku {sku.Id}" : sku.Name;
                        info = new CatalogItemInfo(
                            sku.Id,
                            key,
                            displayName,
                            sku.Description,
                            (int)sku.Price,
                            1f,
                            true,
                            100000);
                        return true;
                    }
                    break;
                case ItemType.Quest:
                    if (CATALOG.QuestItemsById != null && CATALOG.QuestItemsById.TryGetValue(id, out var quest))
                    {
                        info = new CatalogItemInfo(
                            quest.Id, quest.Key, quest.DisplayName, quest.Description,
                            quest.Price, quest.Weight, quest.Stackable, quest.MaxStack);
                        return true;
                    }
                    break;
                case ItemType.Engine:
                    if (CATALOG.EnginesById != null && CATALOG.EnginesById.TryGetValue(id, out var engine))
                    {
                        info = new CatalogItemInfo(
                            engine.Id, engine.Key, engine.DisplayName, engine.Description,
                            engine.Price, engine.Weight, engine.Stackable, engine.MaxStack);
                        return true;
                    }
                    break;
                case ItemType.Scanner:
                    if (CATALOG.ScannersById != null && CATALOG.ScannersById.TryGetValue(id, out var scanner))
                    {
                        info = new CatalogItemInfo(
                            scanner.Id, scanner.Key, scanner.DisplayName, scanner.Description,
                            scanner.Price, scanner.Weight, scanner.Stackable, scanner.MaxStack);
                        return true;
                    }
                    break;
                case ItemType.Shield:
                    if (CATALOG.ShieldsById != null && CATALOG.ShieldsById.TryGetValue(id, out var shield))
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

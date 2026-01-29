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
                case ItemType.Item:
                    if (CATALOG.ItemsById != null && CATALOG.ItemsById.TryGetValue(id, out var item))
                    {
                        var key = string.IsNullOrWhiteSpace(item.Name) ? $"item_{item.Id}" : item.Name;
                        var displayName = string.IsNullOrWhiteSpace(item.Name) ? $"item {item.Id}" : item.Name;
                        info = new CatalogItemInfo(
                            item.Id,
                            key,
                            displayName,
                            item.Description,
                            (int)item.Price,
                            item.Weight,
                            item.Stackable,
                            item.MaxStack);
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

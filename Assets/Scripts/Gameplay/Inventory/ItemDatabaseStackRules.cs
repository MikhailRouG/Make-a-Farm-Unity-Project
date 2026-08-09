using System;

/// <summary>
/// Adapts ItemDatabase to what InventoryModel needs.
/// </summary>
public sealed class ItemDatabaseStackRules : IItemStackRules
{
    private readonly ItemDatabase _database;

    public ItemDatabaseStackRules(ItemDatabase database)
    {
        _database = database;
    }

    public bool TryGet(int itemId, out ItemStackRule rule)
    {
        ItemConfig item = _database != null ? _database.Get(itemId) : null;

        if (item == null)
        {
            rule = default;
            return false;
        }

        rule = new ItemStackRule(item.IsStackable, Math.Max(1, item.MaxStackSize));
        return true;
    }
}

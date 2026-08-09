/// <summary>
/// The only thing the inventory logic needs to know about an item.
/// </summary>
public readonly struct ItemStackRule
{
    public readonly bool IsStackable;
    public readonly int MaxStackSize;

    public ItemStackRule(bool isStackable, int maxStackSize)
    {
        IsStackable = isStackable;
        MaxStackSize = maxStackSize;
    }

    /// <summary>How many of this item a single slot can hold.</summary>
    public int SlotCapacity => IsStackable ? MaxStackSize : 1;
}

public interface IItemStackRules
{
    bool TryGet(int itemId, out ItemStackRule rule);
}
public readonly struct ItemStackRule
{
    public readonly bool IsStackable;
    public readonly int MaxStackSize;

    public ItemStackRule(bool isStackable, int maxStackSize)
    {
        IsStackable = isStackable;
        MaxStackSize = maxStackSize;
    }

    public int SlotCapacity => IsStackable ? MaxStackSize : 1;
}

public interface IItemStackRules
{
    bool TryGet(int itemId, out ItemStackRule rule);
}
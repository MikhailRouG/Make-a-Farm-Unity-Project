using System;

[Serializable]
public struct InventorySlot
{
    public int ItemId;
    public int Amount;

    public bool IsEmpty => ItemId <= 0 || Amount <= 0;

    public static InventorySlot Empty => new InventorySlot(0, 0);

    public InventorySlot(int itemId, int amount)
    {
        ItemId = itemId;
        Amount = amount;
    }
}

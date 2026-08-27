using System;

[Serializable]
public struct InventorySlot
{
    public int ItemId;
    public int Amount;
    public float Weight;

    public bool IsEmpty => ItemId <= 0 || Amount <= 0;

    public static InventorySlot Empty => new InventorySlot(0, 0);

    public InventorySlot(int itemId, int amount, float weight = 1f)
    {
        ItemId = itemId;
        Amount = amount;
        Weight = weight;
    }

    public InventorySlot WithAmount(int amount) => new InventorySlot(ItemId, amount, Weight);
}

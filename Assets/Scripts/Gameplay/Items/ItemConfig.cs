using Mirror;
using UnityEngine;
public abstract class ItemConfig : ScriptableObject
{
    [field: SerializeField] public int Id { get; private set; }
    [field: SerializeField] public string Name { get; private set; }
    [field: SerializeField] public ItemRarity Rarity { get; private set; }
    [field: SerializeField] public int Price { get; private set; }
    [field: SerializeField] public ItemType Category { get; private set; }

    [field: SerializeField] public Sprite Icon { get; private set; }
    [field: SerializeField] public GameObject Model { get; private set; }
    [field: SerializeField] public bool IsStackable { get; private set; } = true;

    [field: SerializeField] public int MaxStackSize { get; private set; } = 32;
    /// <summary>
    /// Applies the item on the server.
    /// Returns true if the item was consumed and should be removed from its slot.
    /// Seeds return false: they are spent when the planting is confirmed, not on use.
    /// </summary>
    public abstract bool UseServer(
        NetworkIdentity owner,
        InventorySlot slot);
}

public enum ItemType
{
    Seed = 0,
    Harvest = 1,
}
public enum ItemRarity
{
    Common = 0,
    Rare = 1,
}
using Mirror;
using UnityEngine;
public abstract class ItemConfig : ScriptableObject
{
    [field: SerializeField] public int Id { get; private set; }
    [field: SerializeField] public string Name { get; private set; }
    [field: SerializeField] public int Price { get; private set; }
    [field: SerializeField] public Sprite Icon { get; private set; }
    [field: SerializeField] public GameObject Model { get; private set; }
    [field: SerializeField] public bool IsStackable { get; private set; } = true;
    [field: SerializeField] public int MaxStackSize { get; private set; } = 32;
    [field: SerializeField] public bool ConsumeOnUse { get; private set; } = true;
    public abstract bool UseServer(
        NetworkIdentity owner,
        InventorySlot slot);
}
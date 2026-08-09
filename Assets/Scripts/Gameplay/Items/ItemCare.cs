using Mirror;
using UnityEngine;

[CreateAssetMenu(menuName = "Item/Care Tool")]
public class ItemCare : ItemConfig
{
    [field: Tooltip("Health the plant regains when this tool closes an overdue need.")]
    [field: SerializeField] public float HealthRestored { get; private set; } = 25f;

    [field: Tooltip("Fertilizer is spent on use, a watering can is not.")]
    [field: SerializeField] public bool ConsumedOnUse { get; private set; }

    // Care tools are applied by interacting with a plant, so the use button does
    // nothing here and must not spend the tool.
    public override bool UseServer(NetworkIdentity owner, InventorySlot slot) => false;
}

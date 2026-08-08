using Mirror;
using UnityEngine;

[CreateAssetMenu(menuName = "Item/Food")]
public class ItemFood : ItemConfig
{
    [field: SerializeField] public float CommonWeight { get; private set; }

    public override bool UseServer(NetworkIdentity owner, InventorySlot slot)
    {
        // TODO: restore hunger/health proportionally to slot.Weight once the
        // project has such a system.
        return true;
    }
}

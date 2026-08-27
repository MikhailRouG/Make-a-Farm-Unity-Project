using Mirror;
using UnityEngine;

[CreateAssetMenu(menuName = "Item/Care Tool")]
public class ItemCare : ItemConfig
{
    public override bool UseServer(NetworkIdentity owner, InventorySlot slot) => false;
}

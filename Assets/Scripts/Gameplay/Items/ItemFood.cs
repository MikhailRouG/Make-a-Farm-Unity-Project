using Mirror;
using UnityEngine;

[CreateAssetMenu(menuName = "Item/Food")]
public class ItemFood : ItemConfig
{
    [field: SerializeField] public float commonWeight { get; private set; }

    [Server]
    public override bool UseServer(
        NetworkIdentity owner,
        InventorySlot slot)
    {
        Debug.Log($"You eat a {name} with weight ");
        return true;
    }
}

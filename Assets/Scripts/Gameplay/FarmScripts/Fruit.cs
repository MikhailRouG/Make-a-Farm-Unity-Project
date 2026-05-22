using Mirror;
using UnityEngine;

public class Fruit : NetworkBehaviour, IInteractable
{
    [SyncVar]
    private uint _ownerId;
    private Plant _parent;
    private ItemConfig _item;

    public string InteractionPrompt =>"Collect the plant";
    public override void OnStartClient()
    {
        base.OnStartClient();
    }
    public void Init(uint ownerId, ItemConfig item)
    {
        if (!isServer) return;
        _ownerId = ownerId;
        _item = item;
    }
    [Server]
    public void Interact(GameObject interactor)
    {
        if (_item == null)
        {
            Debug.Log("Item == null", this);
            return;
        }

            if (interactor.TryGetComponent<Inventory>(out Inventory inventory))
        {
            if (inventory.TryAddItem(_item.Id, 1))
            {
                NetworkServer.Destroy(gameObject);
            }
        }
    }
}

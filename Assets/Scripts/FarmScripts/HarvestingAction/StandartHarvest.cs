using Mirror;
using UnityEngine;

public class StandartHarvest : NetworkBehaviour, IHarvestable,IInteractable
{
    private uint _ownerId;
    private ItemSeed _seed;
    private bool isInit;
    public string InteractionPrompt => "Collect the plant";

    public void StartHarvesting(uint ownerId, ItemSeed seed)
    {
        if (!isServer) return;
        _ownerId = ownerId;
        _seed = seed;
        isInit= true;
    }

    public void Interact(GameObject interactor)
    {
        if(!isInit) return;
        if (interactor.TryGetComponent<Inventory>(out Inventory inventory))
        {
            if (inventory.TryAddItem(_seed.HarvestItem[0].Id, 1))
            {
                NetworkServer.Destroy(gameObject);
            }
        }
    }
}

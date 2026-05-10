using Mirror;
using UnityEngine;

[CreateAssetMenu(menuName = "Farm/Harvest/Standard")]
public class StandartHarvestAction : HarvestAction
{
    public override void Harvest(uint ownerId, Plant plant, ItemSeed seed)
    {
        if (plant == null || seed == null)
            return;

        if (seed.HarvestItem == null || seed.HarvestItem.Length == 0)
            return;
        if (!NetworkServer.spawned.TryGetValue(ownerId, out NetworkIdentity identity))
            return;

        if (!identity.TryGetComponent(out Player player))
            return;

        if (player == null)
            return;

         player.Inventory.TryAddItem(seed.HarvestItem[0].Id,1);
        NetworkServer.Destroy(plant.gameObject);
    }
}

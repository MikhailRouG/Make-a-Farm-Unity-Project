using Mirror;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Seed")]
public class ItemSeed : ItemConfig
{
    [Header("Seed Settings")]
    [field: SerializeField] public GameObject GhostObject { get; private set; }
    [field: SerializeField] public GameObject[] Stages { get; private set; }
    [field: SerializeField] public ItemConfig[] HarvestItem { get; private set; }
    [field: SerializeField] public float TimePerStage { get; private set; } = 5f;

[Header("Plant")]
    [field: SerializeField] public HarvestAction harvestAction { get; private set; }

    [Server]
    public override bool UseServer(
        NetworkIdentity owner,
        InventorySlot slot)
    {
        owner.GetComponent<PlayerPlacement>().StartPlanting(Id);
        return true;
    }
}

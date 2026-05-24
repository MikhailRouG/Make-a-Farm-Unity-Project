using UnityEngine;
using Mirror;
[CreateAssetMenu(menuName = "Items/Seed")]
public class ItemSeed : ItemConfig
{
    [Header("Seed Settings")]
    [field: SerializeField] public PlantCheckPosition GhostObject { get; private set; }
    [field:SerializeField] public Plant PlantStartObject { get; private set; }
    [field: SerializeField] public GameObject[] Stages { get; private set; }
    [field: SerializeField] public ItemConfig[] HarvestItem { get; private set; }
    [field: SerializeField] public float TimePerStage { get; private set; } = 5f;

    public override bool UseServer(
        NetworkIdentity owner,
        InventorySlot slot)
    {
        PlayerPlacement i = owner.GetComponent<PlayerPlacement>();
        if(i.enabled == true) i.enabled = false;
        i.enabled = true;
        i.TargetStartPlanting(owner.connectionToClient, Id);
        return true;
    }
}

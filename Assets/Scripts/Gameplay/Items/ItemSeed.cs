using Gameplay.Farm;
using Gameplay.Player;
using Mirror;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Seed")]
public class ItemSeed : ItemConfig
{
    [Header("Seed Settings")]
    [field: SerializeField] public PlantCheckPosition GhostObject { get; private set; }
    [field: SerializeField] public Plant PlantStartObject { get; private set; }
    [field: SerializeField] public GameObject[] Stages { get; private set; }
    [field: SerializeField] public ItemConfig[] HarvestItem { get; private set; }
    [field: SerializeField] public float TimePerStage { get; private set; } = 5f;
    [field: SerializeField] public EffectConfig Effect { get; private set; }

    public override bool UseServer(NetworkIdentity owner, InventorySlot slot)
    {
        if (owner.TryGetComponent(out PlayerPlacement placement))
            placement.TargetStartPlanting(owner.connectionToClient, Id);

        // Spent in ServerConfirmPlacement rather than here: the player can still
        // cancel while the ghost is being placed.
        return false;
    }
}

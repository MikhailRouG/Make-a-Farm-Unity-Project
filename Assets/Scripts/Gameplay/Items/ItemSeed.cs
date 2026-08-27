using Gameplay.Farm;
using Gameplay.Player;
using Mirror;
using UnityEngine;

[CreateAssetMenu(menuName = "Item/Seed")]
public class ItemSeed : ItemConfig
{
    [Header("Seed Settings")]
    [field: SerializeField] public GameObject[] Stages { get; private set; }
    [field: SerializeField] public ItemConfig[] HarvestItem { get; private set; }
    [field: SerializeField] public float TimePerStage { get; private set; } = 5f;
    [field: SerializeField] public EffectConfig Effect { get; private set; }

    [Header("Requirement")]
    [field: SerializeField] public float MaxHealth { get; private set; } = 100f;
    [field: SerializeField] public float DrainPerSecond { get; private set; } = 2f;
    [field: SerializeField] public float RegenPerSecond { get; private set; } = 1f;
    [field: SerializeField] public float RepeatEvery { get; private set; } = 60f;
    [field: SerializeField] public PlantRequirement[] Requirements { get; private set; }
    public bool HasRequirements => Requirements != null && Requirements.Length > 0;

    public override bool UseServer(NetworkIdentity owner, InventorySlot slot)
    {
        if (owner.TryGetComponent(out PlayerPlacement placement))
            placement.TargetStartPlanting(owner.connectionToClient, Id);
        return false;
    }
}

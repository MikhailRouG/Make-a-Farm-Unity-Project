using Mirror;
using UnityEngine;

[CreateAssetMenu(menuName = "Item/Food")]
public class ItemFood : ItemConfig
{
    [field: SerializeField] public float CommonWeight { get; private set; } = 1f;

    /// <summary>Weight of a single item grown at <paramref name="scale"/>.</summary>
    public float GetWeight(float scale) => CommonWeight * scale;

    public override bool UseServer(NetworkIdentity owner, InventorySlot slot)
    {
        // TODO: restore hunger/health proportionally to slot.Weight once the
        // project has such a system.
        return true;
    }
}

public static class ItemWeight
{
    /// <summary>
    /// Weight a harvested item is stored with: its common weight scaled by how
    /// big the plant grew. Items with no weight data fall back to the raw scale.
    /// </summary>
    public static float Resolve(ItemConfig item, float scale)
    {
        if (item is ItemFood food)
        {
            if (food.CommonWeight > 0f)
                return food.GetWeight(scale);

            Debug.LogWarning($"[ItemWeight] {food.name}: CommonWeight is not set.", food);
        }

        return scale;
    }

    /// <summary>
    /// Inverse of <see cref="Resolve"/>: how big the item is relative to its
    /// common weight. Prices are per common-weight item, so payouts multiply by
    /// this rather than by the raw kilograms.
    /// </summary>
    public static float ResolveScale(ItemConfig item, float weight)
    {
        if (item is ItemFood food && food.CommonWeight > 0f)
            return weight / food.CommonWeight;

        return weight;
    }
}

using UnityEngine;

public abstract class HarvestAction : ScriptableObject
{
    public abstract void Harvest(uint ownerId, Plant plant, ItemSeed seed);
}
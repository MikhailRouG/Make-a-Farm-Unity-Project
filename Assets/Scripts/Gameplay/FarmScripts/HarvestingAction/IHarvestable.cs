using UnityEngine;

public interface IHarvestable
{
    public void StartHarvesting(uint ownerId,ItemSeed seed);
}

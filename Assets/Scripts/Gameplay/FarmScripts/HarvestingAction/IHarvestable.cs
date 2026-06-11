using System;
using UnityEngine;

public interface IHarvestable
{
    public void StartHarvesting(uint ownerId,ItemSeed seed);
    public event Action OnDestroyedServer;
}

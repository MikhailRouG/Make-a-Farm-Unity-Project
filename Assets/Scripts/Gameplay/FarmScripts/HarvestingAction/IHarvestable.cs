using System;

namespace Gameplay.Farm
{
    public interface IHarvestable
    {
        public void StartHarvesting(uint ownerId, ItemSeed seed);
        public event Action OnDestroyedServer;
    }
}
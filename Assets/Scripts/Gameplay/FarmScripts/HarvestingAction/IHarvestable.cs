using System;

namespace Gameplay.Farm
{
    public interface IHarvestable
    {
        public void StartHarvesting(uint ownerId, int seedId, float size);
        public event Action OnDestroyedServer;
    }
}
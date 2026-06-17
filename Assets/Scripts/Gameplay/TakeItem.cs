using Mirror;
using UnityEngine;

namespace Gameplay.Farm
{
    public class TakeItem : NetworkBehaviour, IInteractable
    {
        private uint _ownerNetId;
        [SerializeField] private ItemConfig _data;
        private Plant _plant;
        public string InteractionPrompt => _data != null ? _data.name : "";


        public void Init(uint ownerNetId, ItemConfig data, Plant plant)
        {
            _ownerNetId = ownerNetId;
            _data = data;
            _plant = plant;
        }
        [Server]
        public void Interact(GameObject interactor)
        {
            Debug.Log("item");
            interactor.GetComponent<Inventory>().TryAddItem(_data.Id, 1);
            NetworkServer.Destroy(gameObject);
        }
    }
}
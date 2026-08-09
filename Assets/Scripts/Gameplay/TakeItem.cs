using Mirror;
using UnityEngine;

namespace Gameplay.Farm
{
    public class TakeItem : NetworkBehaviour, IInteractable
    {
        [SerializeField] private ItemConfig _data;

        public string InteractionPrompt => _data != null ? _data.Name : string.Empty;

        [Server]
        public void Init(ItemConfig data)
        {
            _data = data;
        }

        [Server]
        public void Interact(GameObject interactor)
        {
            if (_data == null)
            {
                Debug.LogError($"[TakeItem] {name}: item is not assigned.", this);
                return;
            }

            if (!interactor.TryGetComponent(out Inventory inventory))
                return;

            if (inventory.TryAddItem(_data.Id, 1))
                NetworkServer.Destroy(gameObject);
        }
    }
}

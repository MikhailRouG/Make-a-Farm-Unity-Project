using Mirror;
using System;
using UnityEngine;

namespace Gameplay.Farm
{
    public class Fruit : NetworkBehaviour, IInteractable
    {
        [SyncVar]
        private uint _ownerId;

        [SyncVar(hook = nameof(OnSizeChanged))]
        private float _size = 1f;

        private Plant _parent;
        private ItemConfig _item;
        private Vector3 _baseScale;
        public event Action OnDestroyedServer;
        public string InteractionPrompt => "Collect the plant";

        private void Awake()
        {
            _baseScale = transform.localScale;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            InitializeVisual(_size);
        }
        [Server]
        public void Init(uint ownerId, ItemConfig item, float size)
        {
            _ownerId = ownerId;
            _item = item;
            _size = size;
        }

        private void OnSizeChanged(float oldSize, float newSize)
        {
            InitializeVisual(newSize);
        }

        [Client]
        private void InitializeVisual(float size)
        {
            if (!isActiveAndEnabled) return;

            if (TryGetComponent<AppearAnimation>(out var animation))
                animation.Initialize(size);
            else
                transform.localScale = _baseScale * size;
        }

        [Server]
        public void Interact(GameObject interactor)
        {
            if (_item == null)
            {
                Debug.Log("Item == null", this);
                return;
            }

            if (interactor.TryGetComponent<Inventory>(out Inventory inventory))
            {
                if (inventory.TryAddItem(_item.Id, 1, _size))
                {
                    NetworkServer.Destroy(gameObject);
                }
            }
        }
        public override void OnStopServer()
        {
            OnDestroyedServer?.Invoke();
            OnDestroyedServer = null;

            base.OnStopServer();
        }
    }
}

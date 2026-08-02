using Mirror;
using System;
using UnityEngine;

namespace Gameplay.Farm
{
    public class StandartHarvest : NetworkBehaviour, IHarvestable, IInteractable
    {
        [SyncVar]
        private uint _ownerId;

        [SyncVar]
        private int _seedId = -1;

        [SyncVar(hook = nameof(OnSizeChanged))]
        private float _size = 0;


        private ItemSeed _seed;
        private ItemDatabase _database;

        public event Action OnDestroyedServer;

        public string InteractionPrompt => "Collect the plant";

        private ItemSeed Seed
        {
            get
            {
                if (_seed != null)
                    return _seed;

                if (_seedId < 0 || _database == null)
                    return null;

                _seed = _database.Get(_seedId) as ItemSeed;
                return _seed;
            }
        }
        private void Awake()
        {
            _database = ItemDatabase.Instance;
        }
        private void OnSizeChanged(float oldSize, float newSize)
{
    InitializeVisual(newSize);
}
     
        [Server]
        public void StartHarvesting(uint ownerId, int seed, float size)
        {
            if (seed < 0)
            {
                Debug.LogWarning("[StandartHarvest] Seed is null.", this);
                return;
            }

            _ownerId = ownerId;
            _seedId = seed;
            _seed = Seed;
            _size = size;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

                InitializeVisual(_size);
        }
        [Client]
        private void InitializeVisual(float size)
        {
            if (!isActiveAndEnabled)
                return;

            AppearAnimation animation =
                GetComponent<AppearAnimation>();

            if (animation == null)
                return;
            animation.Initialize(size);
        }

        public void Interact(GameObject interactor)
        {
            if (interactor == null)
                return;

            NetworkIdentity identity =
                interactor.GetComponent<NetworkIdentity>();

            if (identity == null)
                return;

            ServerCollect(identity);
        }

        // Interact() is only ever invoked server-side (StandartHarvest has a
        // NetworkIdentity, so PlayerInteraction routes it through
        // CmdExecuteInteraction). Authority over the interacting player was
        // already verified there — no need for a second network hop / re-check.
        [Server]
        private void ServerCollect(NetworkIdentity interactorIdentity)
        {
            if (interactorIdentity == null)
                return;

            ItemSeed seed = Seed;

            if (seed == null ||
                seed.HarvestItem == null ||
                seed.HarvestItem.Length == 0)
            {
                Debug.LogWarning(
                    $"[StandartHarvest] Invalid seed data: {_seedId}",
                    this
                );

                return;
            }

            if (!interactorIdentity.TryGetComponent(
                    out Inventory inventory))
            {
                return;
            }

            int itemId = seed.HarvestItem[0].Id;

            if (!inventory.TryAddItem(itemId, 1, _size))
            {
                Debug.LogError("Error on Add");
                return;
            }

            OnDestroyedServer?.Invoke();
            NetworkServer.Destroy(gameObject);
        }
    }
}
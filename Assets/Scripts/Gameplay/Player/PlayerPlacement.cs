using Gameplay.Farm;
using Mirror;
using UnityEngine;

namespace Gameplay.Player
{
    public class PlayerPlacement : NetworkBehaviour
    {
        [SerializeField] private PlantCheckPosition _ghost;
        [SerializeField] private Plant _plantPrefab;

        private ItemDatabase _database;
        private PlayerInteraction _interaction;
        private Inventory _inventory;

        private ItemSeed _currentSeed;
        private PlantCheckPosition _currentGhost;
        private bool _canPlant;

        public bool IsPlanting { get; private set; }

        private void Awake()
        {
            _database = ItemDatabase.Instance;
            _interaction = GetComponent<PlayerInteraction>();
            _inventory = GetComponent<Inventory>();

            IsPlanting = false;
            _canPlant = false;
        }

        private void OnDestroy()
        {
            CleanUpGhost();
        }

        private void LateUpdate()
        {
            if (!isLocalPlayer || !IsPlanting) return;
            if (_currentGhost == null || _interaction == null) return;

            _currentGhost.transform.position = _interaction.LookPoint;
            _canPlant = _currentGhost.CheckZone();
        }

        [TargetRpc]
        public void TargetStartPlanting(NetworkConnection target, int seedId)
        {
            StartPlanting(seedId);
        }

        public void StartPlanting(int seedId)
        {
            if (!isLocalPlayer) return;

            if (_ghost == null)
            {
                Debug.LogError($"[{nameof(PlayerPlacement)}] Ghost prefab is not assigned.", this);
                return;
            }

            if (_database.Get(seedId) is not ItemSeed seed)
                return;

            CleanUpGhost();

            _currentSeed = seed;

            _currentGhost = Instantiate(_ghost);
            _currentGhost.Show(seed);

            IsPlanting = true;
        }

        public void ConfirmPlacement()
        {
            if (!isLocalPlayer || !IsPlanting) return;

            if (!_canPlant)
            {
                StopPlanting();
                return;
            }

            CmdConfirmPlacement(_currentSeed.Id, _interaction.LookPoint);

            StopPlanting();
        }

        public void CancelPlacement()
        {
            StopPlanting();
        }

        [Command]
        private void CmdConfirmPlacement(int seedId, Vector3 spawnPosition)
        {
            if (_database.Get(seedId) is ItemSeed seedConfig)
                ServerConfirmPlacement(seedConfig, spawnPosition);
        }

        [Server]
        private void ServerConfirmPlacement(ItemSeed seedConfig, Vector3 position)
        {
            if (_plantPrefab == null)
            {
                Debug.LogError($"[{nameof(PlayerPlacement)}] Plant prefab is not assigned.", this);
                return;
            }

            int id = seedConfig.Id;

            if (!_inventory.HasItem(id))
                return;

            Plant plant = Instantiate(_plantPrefab, position, Quaternion.identity);
            plant.Init(netId, id);

            GameObject instance = plant.gameObject;
            NetworkServer.Spawn(instance, connectionToClient);

            if (!_inventory.TryRemoveItem(id, 1))
                NetworkServer.Destroy(instance);
        }

        private void StopPlanting()
        {
            CleanUpGhost();

            _currentSeed = null;
            IsPlanting = false;
            _canPlant = false;
        }

        private void CleanUpGhost()
        {
            if (_currentGhost == null) return;

            Destroy(_currentGhost.gameObject);
            _currentGhost = null;
        }
    }
}

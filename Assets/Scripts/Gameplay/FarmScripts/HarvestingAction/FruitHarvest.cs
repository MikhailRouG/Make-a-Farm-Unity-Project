using System;
using Mirror;
using UnityEngine;

namespace Gameplay.Farm
{
    public class FruitHarvest : NetworkBehaviour, IHarvestable
    {
        [SerializeField] private Transform _stem;
        [SerializeField] private Transform[] _fruitPoint;
        [SerializeField] private GameObject _fruitPrefab;
        [SerializeField] private int _fruitCount = 3;
        private Vector3[] _fruitPointBaseOffset;
        private int _currentCount;
        private uint _ownerId;
        private int _seedId = -1;
        private ItemSeed _seed;
        private ItemDatabase _database;

        [SyncVar(hook = nameof(OnSizeChanged))]
        private float _size = 0f;

        public event Action OnDestroyedServer;

        private ItemSeed Seed
        {
            get
            {
                if (_seed != null) return _seed;
                if (_seedId < 0 || _database == null) return null;

                _seed = _database.Get(_seedId) as ItemSeed;
                return _seed;
            }
        }

        private void Awake()
        {
            _database = ItemDatabase.Instance;

            if (_stem != null && _stem.childCount > 0)
            {
                _fruitPoint = new Transform[_stem.childCount];
                for (int i = 0; i < _stem.childCount; i++)
                    _fruitPoint[i] = _stem.GetChild(i);
            }
            else if (_fruitPoint == null || _fruitPoint.Length == 0)
            {
                _fruitPoint = new Transform[] { _stem != null ? _stem : transform };
            }

            // Captured once, before any scaling, so spawn points can be
            // re-projected for the plant's actual size at spawn time
            // instead of relying on the (client-only) visual scale.
            _fruitPointBaseOffset = new Vector3[_fruitPoint.Length];
            for (int i = 0; i < _fruitPoint.Length; i++)
                _fruitPointBaseOffset[i] = transform.InverseTransformPoint(_fruitPoint[i].position);
        }

        [Server]
        public void StartHarvesting(uint ownerId, int seed, float size)
        {
            _ownerId = ownerId;
            _seedId = seed;
            _size = size;
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            SpawnFruits();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            InitializeVisual(_size);
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
        }

        [Server]
        private void SpawnFruits()
        {
            ItemSeed seed = Seed;
            if (seed == null || seed.HarvestItem == null || seed.HarvestItem.Length == 0)
            {
                Debug.LogWarning($"[FruitHarvest] Invalid seed data: {_seedId}", this);
                DestroySelf();
                return;
            }
            if (_fruitPrefab == null || _fruitPoint == null || _fruitPoint.Length == 0)
            {
                Debug.LogError("[FruitHarvest] Fruit prefab or spawn points not set.", this);
                DestroySelf();
                return;
            }

            ItemConfig harvestItem = seed.HarvestItem[0];

            if (harvestItem == null)
            {
                Debug.LogError($"[FruitHarvest] {seed.name}: harvest item is not assigned.", this);
                DestroySelf();
                return;
            }

            _currentCount = 0;

            for (int i = 0; i < _fruitCount; i++)
            {
                float randomAngle = UnityEngine.Random.Range(0f, 360f);
                int pointIndex = UnityEngine.Random.Range(0, _fruitPoint.Length);
                Vector3 targetPosition = transform.TransformPoint(_fruitPointBaseOffset[pointIndex] * _size);
                Quaternion targetRotation = Quaternion.Euler(0f, randomAngle, 0f);

                GameObject fruitInstance = Instantiate(_fruitPrefab, targetPosition, targetRotation);
                if (!fruitInstance.TryGetComponent(out Fruit fruitComponent))
                {
                    Debug.LogError("[FruitHarvest] Fruit component missing on prefab.", this);
                    Destroy(fruitInstance);
                    continue;
                }

                fruitComponent.Init(_ownerId, harvestItem, _size);
                fruitComponent.OnDestroyedServer += HandleFruitDestroyed;
                NetworkServer.Spawn(fruitInstance);
                _currentCount++;
            }

            if (_currentCount <= 0)
                DestroySelf();
        }

        [Server]
        private void HandleFruitDestroyed()
        {
            _currentCount--;

            if (_currentCount <= 0)
                DestroySelf();
        }

        [Server]
        private void DestroySelf()
        {
            OnDestroyedServer?.Invoke();
            NetworkServer.Destroy(gameObject);
        }
    }
}

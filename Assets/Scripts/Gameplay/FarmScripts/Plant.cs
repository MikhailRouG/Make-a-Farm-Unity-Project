using Mirror;
using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gameplay.Farm
{
    public class Plant : NetworkBehaviour
    {
        [SerializeField, Min(0.01f)] private float _growthSpeed = 1f;

        [SyncVar(hook = nameof(OnSeedSynced))]
        private int _seedId = -1;

        [SyncVar(hook = nameof(OnStageChanged))]
        private int _stageIndex = -1;

        [SyncVar] private uint _ownerNetId;
        [SyncVar] private float _size;
        [SyncVar] private double _stageEndTime;

        private ItemDatabase _database;
        private ItemSeed _seed;
        private GameObject _currentVisual;
        private Coroutine _growCoroutine;
        private BoxCollider _collider;

        public event Action<float, EffectConfig> OnInitialized;
        public event Action<EffectState> OnUpdateStage;

        public bool HasStageDeadline => _stageEndTime > 0d;

        public float GrowthSpeed => _growthSpeed;

        public bool IsFullyGrown
        {
            get
            {
                ItemSeed seed = Seed;

                if (seed == null || seed.Stages == null || seed.Stages.Length == 0)
                    return false;

                return _stageIndex >= seed.Stages.Length - 1;
            }
        }

        public float RemainingStageTime =>
            IsFullyGrown ? 0f : Mathf.Max(0f, (float)(_stageEndTime - NetworkTime.time));

        public ItemSeed Seed
        {
            get
            {
                if (_seed != null) return _seed;
                if (_seedId < 0) return null;

                _seed = _database.Get(_seedId) as ItemSeed;

                return _seed;
            }
        }

        private void Awake()
        {
            _database = ItemDatabase.Instance;
            TryGetComponent(out _collider);
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            RefreshCollider();
            TryUpdateVisual();

            ItemSeed seed = Seed;

            if (seed == null)
                return;

            OnInitialized?.Invoke(_size, seed.Effect);
        }

        [Server]
        public void Init(uint ownerId, int id)
        {
            _ownerNetId = ownerId;
            _seedId = id;
            _stageIndex = 0;
            _size = Random.Range(0.5f, 1.5f);

            ItemSeed seed = Seed;
            if (seed != null)
                _stageEndTime = NetworkTime.time + StageDuration(seed);

            if (_growCoroutine != null) StopCoroutine(_growCoroutine);
            _growCoroutine = StartCoroutine(GrowRoutine());
        }

        [Server]
        public void SetGrowthSpeed(float value)
        {
            _growthSpeed = Mathf.Max(0.01f, value);
        }

        [Server]
        public void Kill()
        {
            OnUpdateStage?.Invoke(EffectState.Destroy);
            NetworkServer.Destroy(gameObject);
        }

        private float StageDuration(ItemSeed seed) => seed.TimePerStage / _growthSpeed;

        [Server]
        private IEnumerator GrowRoutine()
        {
            yield return new WaitUntil(() => _seedId >= 0);
            yield return new WaitUntil(() => Seed != null);

            if (Seed.Stages == null || Seed.Stages.Length == 0)
            {
                Debug.LogError($"[Plant] {Seed.name}: Stages are not assigned.", this);
                yield break;
            }

            int lastStage = Seed.Stages.Length - 1;

            while (_stageIndex < lastStage)
            {
                float duration = StageDuration(Seed);
                _stageEndTime = NetworkTime.time + duration;

                yield return new WaitForSeconds(duration);

                _stageIndex++;
            }

            _stageEndTime = 0d;

            OnLastStage();
        }

        [Client]
        private void TryUpdateVisual()
        {
            if (!isClient) return;
            if (_seedId < 0 || _stageIndex < 0) return;

            UpdateVisual(_stageIndex);
        }

        [Client]
        private void UpdateVisual(int stage)
        {
            if (_seedId < 0 || _stageIndex < 0)
                return;

            ItemSeed seed = Seed;
            if (seed == null || seed.Stages == null || stage < 0 || stage >= seed.Stages.Length)
            {
                Debug.LogWarning($"[Plant] Cannot update visual. Seed or stage invalid. Stage: {stage}", this);
                return;
            }

            if (_currentVisual != null)
                Destroy(_currentVisual);

            if (stage >= seed.Stages.Length - 1)
                return;

            if (seed.Stages[stage] == null)
            {
                Debug.LogWarning($"[Plant] {seed.name}: stage {stage} prefab is not assigned.", this);
                return;
            }

            _currentVisual = Instantiate(
                seed.Stages[stage],
                transform.position,
                Quaternion.identity,
                transform
            );

            if (stage == 0)
            {
                OnInitialized?.Invoke(_size, seed.Effect);
                OnUpdateStage?.Invoke(EffectState.Start);
            }
            else
            {
                OnUpdateStage?.Invoke(EffectState.Upgrade);
            }

            if (_currentVisual.TryGetComponent(out AppearAnimation animation))
                animation.Initialize(_size);
        }

        private void OnStageChanged(int oldStage, int newStage)
        {
            RefreshCollider();
            TryUpdateVisual();

            ItemSeed seed = Seed;
            if (seed != null && seed.Stages != null && newStage >= seed.Stages.Length - 1)
                OnUpdateStage?.Invoke(EffectState.Grow);
        }

        private void OnSeedSynced(int oldId, int newId)
        {
            _seed = null;
            RefreshCollider();
            TryUpdateVisual();
        }

        // The harvest object spawns on top of the plant, and a ray only reports its
        // nearest hit: while this collider is up it shadows the harvest completely.
        // Driven off the synced stage rather than switched off in OnLastStage, which
        // is server-only - remote clients kept aiming at the plant and got the care
        // prompt where the host already saw the harvest.
        private void RefreshCollider()
        {
            if (_collider == null)
                return;

            _collider.enabled = !IsFullyGrown;
        }

        [Server]
        private void OnLastStage()
        {
            ItemSeed seed = Seed;

            if (seed == null || seed.Stages == null || seed.Stages.Length == 0)
                return;

            GameObject lastStagePrefab = seed.Stages[seed.Stages.Length - 1];

            if (lastStagePrefab == null)
            {
                Debug.LogError($"[Plant] {seed.name}: last stage prefab is not assigned.", this);
                return;
            }

            GameObject obj = Instantiate(
                lastStagePrefab,
                transform.position,
                Quaternion.identity
            );

            RefreshCollider();

            if (obj.TryGetComponent(out IHarvestable component))
            {
                component.StartHarvesting(_ownerNetId, seed.Id, _size);
                component.OnDestroyedServer += OnDestroyedPlant;
            }
            else
            {
                Debug.LogWarning($"[Plant] {obj.name} does not implement IHarvestable.", this);
            }
            NetworkServer.Spawn(obj);
        }

        private void OnDestroy()
        {
            if (!isServer) return;
            if (_growCoroutine != null) StopCoroutine(_growCoroutine);
        }

        private void OnDestroyedPlant() => Kill();
    }
}
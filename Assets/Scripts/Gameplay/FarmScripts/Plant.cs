using Mirror;
using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gameplay.Farm
{
    public class Plant : NetworkBehaviour
    {
        [SyncVar(hook = nameof(OnSeedSynced))]
        private int _seedId = -1;

        [SyncVar(hook = nameof(OnStageChanged))]
        private int _stageIndex = -1;

        [SyncVar] private uint _ownerNetId;
        [SyncVar] private float _size;
        private ItemDatabase _database;
        private ItemSeed _seed;
        private GameObject _currentVisual;
        private Coroutine _growCoroutine;

        public event Action<float,EffectConfig> OnInitialized;
        public event Action<EffectState, string> OnUpdateStage;

        private ItemSeed Seed
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
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

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
            _size = Random.Range(0.2f, 2.0f);

            if (_growCoroutine != null) StopCoroutine(_growCoroutine);
            _growCoroutine = StartCoroutine(GrowRoutine());
        }

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
                yield return new WaitForSeconds(Seed.TimePerStage);
                _stageIndex++;
            }

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
                OnUpdateStage?.Invoke(EffectState.Start, seed.TimePerStage.ToString());
            }
            else
            {
                OnUpdateStage?.Invoke(EffectState.Upgrade, seed.TimePerStage.ToString());
            }

            if (_currentVisual.TryGetComponent(out AppearAnimation animation))
                animation.Initialize(_size);
        }

        // Mirror assigns the SyncVar before invoking the hook, so the field
        // already holds newStage/newId — no need to assign it again here.
        private void OnStageChanged(int oldStage, int newStage)
        {
            TryUpdateVisual();

            ItemSeed seed = Seed;
            if (seed != null && seed.Stages != null && newStage >= seed.Stages.Length - 1)
                OnUpdateStage?.Invoke(EffectState.Grow, "Can be collected");
        }

        private void OnSeedSynced(int oldId, int newId)
        {
            _seed = null;
            TryUpdateVisual();
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

        private void OnDestroyedPlant()
        {
            OnUpdateStage?.Invoke(EffectState.Destroy, string.Empty);
            NetworkServer.Destroy(gameObject);
        }
    }
}
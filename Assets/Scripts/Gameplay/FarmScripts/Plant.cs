using Mirror;
using System;
using System.Collections;
using UnityEngine;
using Zenject;

namespace Gameplay.Farm
{
    public class Plant : NetworkBehaviour
    {
        [SyncVar(hook = nameof(OnSeedSynced))]
        private int _seedId = -1;

        [SyncVar(hook = nameof(OnStageChanged))]
        private int _stageIndex = -1;

        [SyncVar] private uint _ownerNetId;

        private ItemDatabase _database;
        private ItemSeed _seed;
        private GameObject _currentVisual;
        private Coroutine _growCoroutine;

        public event Action<EffectConfig> OnInitialized;
        public event Action<EffectState, string> OnUpdateStage;
        [Inject]
        private void Construct(ItemDatabase database)
        {
            _database = database;
        }
        private ItemSeed Seed
        {
            get
            {
                if (_seed != null) return _seed;
                if (_seedId < 0) return null;

                if (_database == null)
                {
                    var container = ProjectContext.Instance.Container;
                    container.Inject(this);

                }
                if (_database != null)
                    _seed = _database.Get(_seedId) as ItemSeed;

                return _seed;
            }
        }

        private void Awake()
        {
            var container = ProjectContext.Instance.Container;
            container.Inject(this);
        }
        public override void OnStartClient()
        {
            base.OnStartClient();
            TryUpdateVisual();
            ItemSeed seed = Seed;

            if (seed == null)
                return;
            OnInitialized?.Invoke(seed.Effect);
        }
        [Server]
        public void Init(uint ownerId, int id)
        {
            Debug.Log($"[Plant Init] isServer={isServer}, itemId={id}, netId={netId}", this);
            _ownerNetId = ownerId;
            _seedId = id;
            _stageIndex = 0;

            if (_growCoroutine != null) StopCoroutine(_growCoroutine);
            _growCoroutine = StartCoroutine(GrowRoutine());

            TryUpdateVisual();
        }
        [Server]
        private IEnumerator GrowRoutine()
        {
            yield return new WaitUntil(() => _seedId >= 0);
            yield return new WaitUntil(() => _database != null || ItemDatabase.Instance != null);
            yield return new WaitUntil(() => Seed != null);

            while (_stageIndex < Seed.Stages.Length - 1)
            {
                yield return new WaitForSeconds(Seed.TimePerStage);

                _stageIndex++;
                TryUpdateVisual();
                if (_stageIndex >= Seed.Stages.Length - 1)
                {
                    OnLastStage();
                    yield break;
                }
            }
        }

        private void TryUpdateVisual()
        {
            if (!isClient) return;
            if (_seedId < 0 || _stageIndex < 0)
            {
                Debug.Log($"[Plant] Waiting sync. seedId={_seedId}, stage={_stageIndex}", this);
                return;
            }
            UpdateVisual(_stageIndex);
        }
        [Client]
        private void UpdateVisual(int stage)
        {
            if (_seedId < 0 || _stageIndex < 0)
            {
                Debug.Log(_seedId + _stageIndex);
                return;
            }
            ItemSeed seed = Seed;
            if (seed == null || seed.Stages == null || stage < 0 || stage >= seed.Stages.Length)
            {
                Debug.LogWarning($"[Plant] Cannot update visual. Seed or stage invalid. Stage: {stage}", this);
                return;
            }


            if (_currentVisual != null)
                Destroy(_currentVisual);
            if (stage >= seed.Stages.Length - 1)
            {
                return;
            }
            _currentVisual = Instantiate(
                seed.Stages[stage],
                transform.position,
                Quaternion.identity,
                transform
            );
            if (!isClient) return;
            if (_stageIndex == 0)
            {
                OnInitialized?.Invoke(_seed.Effect);
                OnUpdateStage?.Invoke(EffectState.Start, _seed.TimePerStage.ToString());
            }
            else OnUpdateStage?.Invoke(EffectState.Upgrade, _seed.TimePerStage.ToString());
        }
        private void OnStageChanged(int oldStage, int newStage)
        {
            Debug.Log($"{oldStage}, {newStage}");
            _stageIndex = newStage;
            TryUpdateVisual();

        }

        private void OnSeedSynced(int oldId, int newId)
        {
            _seedId = newId;
            _seed = null;
            TryUpdateVisual();
        }
        private void OnLastStage()
        {
            if (!isServer) return;
            GameObject obj = Instantiate(
               _seed.Stages[_seed.Stages.Length - 1],
               transform.position,
               Quaternion.identity
           );
            NetworkServer.Spawn(obj);
            if (obj.TryGetComponent<IHarvestable>(out IHarvestable component))
            {
                component.StartHarvesting(_ownerNetId, _seed);
                component.OnDestroyedServer += OnDestroyedPlant;
                OnUpdateStage?.Invoke(EffectState.Harvest, "Can be collected");
            }
            else
            {
                Debug.LogWarning($"[Plant] {obj.name} IHarvestable! .");
            }
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
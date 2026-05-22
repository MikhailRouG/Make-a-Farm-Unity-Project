using Mirror;
using System.Collections;
using UnityEngine;
using Zenject;

public class Plant : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnSeedSynced))]
    private int _seedId = -1;

    [SyncVar(hook = nameof(OnStageChanged))]
    private int _stageIndex;

    [SyncVar] private uint _ownerNetId;

    private ItemDatabase _database;
    private ItemSeed _seed;
    private GameObject _currentVisual;
    private Coroutine _growCoroutine;
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
                ProjectContext.Instance.Container.Inject(this);

            if (_database != null)
                _seed = _database.Get(_seedId) as ItemSeed;

            return _seed;
        }
    }
    public override void OnStartClient()
    {
        base.OnStartClient();
            UpdateVisual(_stageIndex);
    }
    public void Init(uint ownerId, ItemSeed item)
    {
        if (!isServer) return;
        _ownerNetId = ownerId;
        _seed = item;
        _seedId = item.Id;
        _stageIndex = 0;
        if (_growCoroutine != null) StopCoroutine(_growCoroutine);
        _growCoroutine = StartCoroutine(GrowRoutine());

        UpdateVisual(_stageIndex);
    }
    private IEnumerator GrowRoutine()
    {
        while (_seed != null && _stageIndex < _seed.Stages.Length - 1)
        {
            yield return new WaitForSeconds(_seed.TimePerStage);
            if (_seed == null)
            {
                Debug.LogError($"[Plant] Cannot grow! Either _seed is null on GameObject: {gameObject.name}", this);
                Debug.LogError(_stageIndex);
                yield break;
            }

            _stageIndex++;
            UpdateVisual(_stageIndex);
        }
    }
    private void UpdateVisual(int stage)
    {
        ItemSeed seed = Seed;
        if (_seed == null || _seed.Stages == null || stage >= _seed.Stages.Length) return;

        if (_currentVisual != null)
            Destroy(_currentVisual);
        if (stage >= _seed.Stages.Length - 1 && isServer)
        {
            {
                if (isServer)
                {
                    OnLastStage();
                }
                return;
            }
        }
        _currentVisual = Instantiate(
            _seed.Stages[stage],
            transform.position,
            Quaternion.identity,
            transform
        );
    }
    private void OnStageChanged(int oldStage, int newStage)
    {
        if (!isServer)
        {
            UpdateVisual(newStage);
        }
    }
    private void OnLastStage()
    {
       GameObject obj =  Instantiate(
           _seed.Stages[_seed.Stages.Length - 1],
           transform.position,
           Quaternion.identity
       );
        NetworkServer.Spawn( obj );
        if (obj.TryGetComponent<IHarvestable>(out IHarvestable component))
            {
                component.StartHarvesting(_ownerNetId,_seed);
            NetworkServer.Destroy(gameObject);
            }
        
    }

    private void OnSeedSynced(int oldId, int newId)
    {
        if (!isServer)
        {
            _seed = null;
            UpdateVisual(_stageIndex);
        }
    }
    private void OnDestroy()
    {
        if (_growCoroutine != null) StopCoroutine(_growCoroutine);
    }
}


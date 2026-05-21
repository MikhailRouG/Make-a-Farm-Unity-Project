using Mirror;
using System.Collections;
using UnityEngine;

public class Plant : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnSeedSynced))]
    private ItemSeed _seed;

    [SyncVar(hook = nameof(OnStageChanged))]
    private int _stageIndex;

    [SyncVar] private uint _ownerNetId;

    private GameObject _currentVisual;
    private Coroutine _growCoroutine;
    public override void OnStartClient()
    {
        base.OnStartClient();
        if (_seed != null)
        {
            UpdateVisual(_stageIndex);
        }
    }
    public void Init(uint ownerId, ItemSeed item)
    {
        if (!isServer) return;
        _ownerNetId = ownerId;
        _seed = item;
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
        if (_seed == null || _seed.Stages == null || stage >= _seed.Stages.Length) return;

        if (_currentVisual != null)
            Destroy(_currentVisual);
        if (stage >= _seed.Stages.Length - 1 && isServer)
        {
            OnLastStage();
            return;
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

    private void OnSeedSynced(ItemSeed oldSeed, ItemSeed newSeed)
    {
    }
    private void OnDestroy()
    {
        if (_growCoroutine != null) StopCoroutine(_growCoroutine);
    }
}


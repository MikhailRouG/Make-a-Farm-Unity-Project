using Castle.Core.Internal;
using Mirror;
using System.Linq;
using UnityEngine;

public class Plant : NetworkBehaviour
{
    [SyncVar] private uint ownerNetId;
    private ItemSeed itemSeed;
    [SyncVar(hook = nameof(OnStageChanged))]
    private int stageIndex;
    [SyncVar] private float randomedWeight;

    private bool isBlockedCached;
    private float stageTimer;
    private GameObject currentVisual;

    private void EnsureSeedData()
    {
        if (itemSeed == null)
        {
            return;
            itemSeed = Resources.Load<ItemSeed>($"Seeds/{itemSeed.Name}");
        }
    }
    public override void OnStartClient()
    {
        base.OnStartClient();
        OnStageChanged(0, stageIndex);
    }
    public void Init(uint ownerId, ItemSeed item)
    {
        ownerNetId = ownerId;
        itemSeed = item;
        randomedWeight = Random.Range(0.5f, 1.5f);
        ItemConfig[] config = item.HarvestItem;
        transform.localScale = new Vector3(randomedWeight, randomedWeight, randomedWeight);
        itemSeed = item;

        stageIndex = 0;
        stageTimer = 0f;
        SpawnStageServer();
    }

    private void Update()
    {
        if (!isServer) return;
        Grow();
    }


    private void Grow()
    {
        if (itemSeed == null || itemSeed.Stages.Length == 0) return;
        if (stageIndex >= itemSeed.Stages.Length - 1) return;
        stageTimer += Time.deltaTime;

        if (stageTimer >= itemSeed.TimePerStage && stageIndex < itemSeed.Stages.Length - 1)
        {
            stageIndex++;
            stageTimer = 0f;
            SpawnStageServer();
            OnStageChanged(stageIndex, stageIndex);
            Debug.Log(stageIndex);
        }
    }
    private void SpawnStageServer()
    {
        if (stageIndex >= itemSeed.Stages.Length - 1)
        {
            if (!GetComponent<TakeItem>())
            {
                gameObject.AddComponent<TakeItem>().Init(ownerNetId, itemSeed.HarvestItem[0], this);
            }
        }
    }

    public void OnTaking()
    {
        itemSeed.harvestAction.Harvest(ownerNetId,this,itemSeed);
    }
    private void OnStageChanged(int oldStage, int newStage)
    {
        EnsureSeedData();
        if (itemSeed == null) return;
        if (currentVisual != null)
            Destroy(currentVisual);
        if (newStage < itemSeed.Stages.Length)
        {
            currentVisual = Instantiate(
            itemSeed.Stages[newStage],
            transform.position,
            Quaternion.identity,
            transform);
        }
    }
}


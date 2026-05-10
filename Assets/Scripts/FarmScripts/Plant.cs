using Mirror;
using System.Linq;
using UnityEngine;

public class Plant : NetworkBehaviour
{
    [SyncVar] private uint ownerNetId;
    private ItemSeed itemSeed;

    [SyncVar] private int stageIndex;
    [SyncVar] private float randomedWeight;

    private bool isBlockedCached;
    private float stageTimer;
    private GameObject currentVisual;

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

        SpawnStage();
    }

    private void Update()
    {
        if (!isServer) return;
        Grow();
    }

    private void PlantConfirm()
    {
      //  player.Inventory?.RemoveItem(itemSeed);
        stageTimer = 0f;
        stageIndex++;
        SpawnStage();
    }

    private void Grow()
    {
        if (itemSeed == null || itemSeed.Stages.Length == 0) return;

        stageTimer += Time.deltaTime;

        if (stageTimer >= itemSeed.TimePerStage && stageIndex < itemSeed.Stages.Length - 1)
        {
            stageIndex++;
            stageTimer = 0f;
            SpawnStage();
        }
    }
    private void SpawnStage()
    {
        if (currentVisual != null)
            Destroy(currentVisual);

        currentVisual = Instantiate(
            itemSeed.Stages[stageIndex],
            transform.position,
            Quaternion.identity,
            transform);
        if (stageIndex >= itemSeed.Stages.Length - 1)
        {
            gameObject.AddComponent<BoxCollider>();
            gameObject.AddComponent<TakeItem>().Init(ownerNetId,itemSeed,  this);
        }
    }


    public bool TryPlant()
    {
        if (isBlockedCached) return false;
        PlantConfirm();
        return true;
    }

    public void OnTaking()
    {
        itemSeed.harvestAction.Harvest(ownerNetId,this,itemSeed);
    }
}


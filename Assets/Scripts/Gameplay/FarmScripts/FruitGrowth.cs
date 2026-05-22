using UnityEngine;

public class FruitGrowth : MonoBehaviour
{
    [SerializeField] private ItemSeed Item;
    private ItemConfig[] _harvestItems;
    private int currentStage = 0;
    private GameObject currentFruit;
    private float nextStageTime;
    private bool isInit = false;

    public void Init(ItemSeed ItemSeed)
    {
        isInit = true;
        Item = ItemSeed;
        _harvestItems = Item.HarvestItem;
        SpawnStage(0);
        SetNextStageTime();
    }

    private void Update()
    {
        if(!isInit) return;
        if (currentStage < _harvestItems.Length - 1 && Time.time >= nextStageTime)
        {
            if (currentFruit == null)
            {
                Destroy(gameObject);
                return;
            }
            NextStage();
        }
    }

    private void NextStage()
    {
        currentStage++;
        SpawnStage(currentStage);
        SetNextStageTime();
    }

    private void SpawnStage(int stageIndex)
    {
        if (_harvestItems[stageIndex].Model == null)
        {
            Debug.LogError($"Stages not assigned in {_harvestItems[stageIndex].name}");
            return;
        }
        if (currentFruit != null)
            Destroy(currentFruit);

        currentFruit = Instantiate(
            _harvestItems[stageIndex].Model,
            transform.position,
            Quaternion.identity,
            transform
        );
    }

    private void SetNextStageTime()
    {
        float delay = Random.Range(5f, 7f);
        nextStageTime = Time.time + delay;
    }
}

using UnityEngine;

public class PlantFruitingStage : MonoBehaviour
{
    [SerializeField] private Transform[] fruitPos;
    int i = 0;
    private float timeF = 5,
        currentTime;
    bool init = false;
    private ItemSeed itemSeed;
    public void Init(ItemSeed seed)
    {
        if (fruitPos == null) return;
        itemSeed = seed;
        currentTime = Time.time;
        init = true;
    }

    private void Update()
    {
        if (!init) return;
        Fruiting();
    }
    private void Fruiting()
    {
        if (i < fruitPos.Length && Time.time >= currentTime + timeF)
        {
            var o = new GameObject($"{itemSeed.HarvestItem[0].name}{i}");
            var obj = Instantiate(o, fruitPos[i].position, Quaternion.identity,transform);
            Destroy(o);
            obj.AddComponent<FruitGrowth>().Init(itemSeed);
            i++;
            currentTime = Time.time;
        }
    }
}

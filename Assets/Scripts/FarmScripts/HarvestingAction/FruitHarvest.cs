using Telepathy;
using UnityEngine;
using Mirror;
public class FruitHarvest : NetworkBehaviour, IHarvestable
{
    [SerializeField] private Transform _stem;
    [SerializeField] private Transform[] _fruitPoint;
    [SerializeField] private GameObject _fruitPrefab;
    [SerializeField] private int _fruitCount = 3;

    private uint _ownerId;
    private ItemSeed _seed;

    private void OnValidate()
    {
        if (_stem == null) return;
        _fruitPoint ??= _stem.GetComponentsInChildren<Transform>();
    }
    public void StartHarvesting(uint ownerId, ItemSeed seed)
    {
        if (!isServer) return;
        _ownerId = ownerId;
        _seed = seed;
        SpawnFruits();
    }
    [Server]
    private void SpawnFruits()
    {
        for (int i = 0; i < _fruitCount; i++)
        {
            Debug.Log("Startfdsas");
            float randomAngle = Random.Range(0f, 360f);
            int randomPoint = Random.Range(0, _fruitPoint.Length);
            GameObject fruitInstance = Instantiate(_fruitPrefab);
            fruitInstance.transform.SetParent(transform);
            fruitInstance.transform.position = _fruitPoint[randomPoint].position;
            fruitInstance.transform.localRotation = Quaternion.Euler(0, randomAngle, 0);
            NetworkServer.Spawn(fruitInstance);
            var fruitComponent = fruitInstance.GetComponent<Fruit>();
            if (fruitComponent == null)
            {
                Debug.LogError("[FruitSpawner] Fruit prefab is missing the Fruit component!", this);
                Destroy(fruitInstance);
                continue;
            }
            if (_seed.HarvestItem != null && _seed.HarvestItem.Length > 0)
            {
                fruitComponent.Init(_ownerId, _seed.HarvestItem[0]);
            }
        }
    }
}

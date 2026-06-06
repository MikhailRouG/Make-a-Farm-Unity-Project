using UnityEngine;
using Mirror;
public class FruitHarvest : NetworkBehaviour, IHarvestable
{
    [SerializeField] private Transform _stem;
    [SerializeField] private Transform[] _fruitPoint;
    [SerializeField] private GameObject _fruitPrefab;
    [SerializeField] private int _fruitCount = 3;
    private int _currentCount;
    private uint _ownerId;
    private ItemSeed _seed;

    private void Awake()
    {
        if (_stem == null) return;
        int childCount = _stem.childCount;
        if (childCount > 0)
        {
            _fruitPoint = new Transform[childCount];
            for (int i = 0; i < childCount; i++)
            {
                _fruitPoint[i] = _stem.GetChild(i);
            }
        }
        else
        {
            _fruitPoint = new Transform[] { _stem };
        }
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
        _currentCount  = _fruitCount;
        for (int i = 0; i < _fruitCount; i++)
        {
            float randomAngle = Random.Range(0f, 360f);
            int randomPoint = Random.Range(0, _fruitPoint.Length);
            Transform targetPoint = _fruitPoint[randomPoint];

            Quaternion targetRotation = Quaternion.Euler(0, randomAngle, 0);
            GameObject fruitInstance = Instantiate(_fruitPrefab, targetPoint.position, targetRotation);
            if (!fruitInstance.TryGetComponent<Fruit>(out Fruit fruitComponent))
            {
                Debug.LogError("[FruitHarvest] Fruit == null", this);
                Destroy(fruitInstance);
                continue;
            }

            if (_seed != null && _seed.HarvestItem != null && _seed.HarvestItem.Length > 0)
            {

                NetworkServer.Spawn(fruitInstance);
                fruitComponent.Init(_ownerId, _seed.HarvestItem[0]);
                fruitComponent.OnDestroyedServer += HandleFruitDestroyed;
            }
        }
    }

    [Server]
    private void HandleFruitDestroyed()
    {
        _currentCount--;

        if (_currentCount <= 0)
        {
            NetworkServer.Destroy(gameObject);
        }
    }
}

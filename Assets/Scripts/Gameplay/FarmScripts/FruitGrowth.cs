using UnityEngine;

namespace Gameplay.Farm
{
    public class FruitGrowth : MonoBehaviour
    {
        [SerializeField] private ItemSeed _item;

        private ItemConfig[] _harvestItems;
        private GameObject _currentFruit;
        private int _currentStage;
        private float _nextStageTime;
        private bool _isInit;

        public void Init(ItemSeed itemSeed)
        {
            _item = itemSeed;
            _harvestItems = _item != null ? _item.HarvestItem : null;

            if (_harvestItems == null || _harvestItems.Length == 0)
            {
                Debug.LogError($"[FruitGrowth] {name}: HarvestItem list is empty.", this);
                return;
            }

            _currentStage = 0;
            _isInit = true;

            SpawnStage(0);
            SetNextStageTime();
        }

        private void Update()
        {
            if (!_isInit) return;

            if (_currentStage < _harvestItems.Length - 1 && Time.time >= _nextStageTime)
            {
                if (_currentFruit == null)
                {
                    Destroy(gameObject);
                    return;
                }

                NextStage();
            }
        }

        private void NextStage()
        {
            _currentStage++;
            SpawnStage(_currentStage);
            SetNextStageTime();
        }

        private void SpawnStage(int stageIndex)
        {
            ItemConfig stageItem = _harvestItems[stageIndex];

            if (stageItem == null || stageItem.Model == null)
            {
                Debug.LogError($"[FruitGrowth] {name}: model for stage {stageIndex} is not assigned.", this);
                return;
            }

            if (_currentFruit != null)
                Destroy(_currentFruit);

            _currentFruit = Instantiate(
                stageItem.Model,
                transform.position,
                Quaternion.identity,
                transform
            );
        }

        private void SetNextStageTime()
        {
            _nextStageTime = Time.time + Random.Range(5f, 7f);
        }
    }
}

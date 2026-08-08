using UnityEngine;

namespace Gameplay.Farm
{
    public class PlantCheckPosition : MonoBehaviour
    {
        [Header("Check Settings")]
        [SerializeField] private Material _confirm;
        [SerializeField] private Material _reject;
        [SerializeField] private float _zonePlantsRadius = 1f;
        [SerializeField] private float _zoneFarmRadius = 0.1f;

        private LayerMask _farmLayer;
        private LayerMask _plantLayer;
        private MeshRenderer _renderer;
        private ItemSeed _itemSeed;

        public void Init(ItemSeed item)
        {
            _farmLayer = LayerMask.GetMask("Farm");
            _plantLayer = LayerMask.GetMask("Plant");
            _itemSeed = item;

            if (item.Stages == null || item.Stages.Length == 0 || item.Stages[0] == null)
            {
                Debug.LogError($"[PlantCheckPosition] {item.name}: stage 0 prefab is not assigned.", this);
                return;
            }

            GameObject ghost = Instantiate(item.Stages[0], transform.position, Quaternion.identity, transform);
            _renderer = ghost.GetComponentInChildren<MeshRenderer>();

            UpdateMaterial(false);
        }

        public bool CheckZone()
        {
            bool canPlant = IsOnFarm() && !IsCollidingWithPlants();
            UpdateMaterial(canPlant);
            return canPlant;
        }

        private bool IsOnFarm()
        {
            return Physics.OverlapSphere(transform.position, _zoneFarmRadius, _farmLayer).Length > 0;
        }

        private bool IsCollidingWithPlants()
        {
            Collider[] plantHits = Physics.OverlapSphere(transform.position, _zonePlantsRadius, _plantLayer);

            foreach (Collider hit in plantHits)
            {
                if (hit.gameObject != gameObject) return true;
            }

            return false;
        }

        private void UpdateMaterial(bool canPlant)
        {
            if (_renderer == null || _itemSeed == null) return;

            _renderer.material = canPlant ? _confirm : _reject;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, _zonePlantsRadius);
            Gizmos.color = Color.grey;
            Gizmos.DrawWireSphere(transform.position, _zoneFarmRadius);
        }
    }
}

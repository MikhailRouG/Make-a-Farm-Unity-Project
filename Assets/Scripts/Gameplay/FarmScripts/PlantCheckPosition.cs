using UnityEngine;

namespace Gameplay.Farm
{
    public class PlantCheckPosition : MonoBehaviour
    {
        [Header("Check Settings")]
        [SerializeField] private Material _confirm;
        [SerializeField] private Material _reject;
        private LayerMask farmLayer;
        private LayerMask plantLayer;
        [SerializeField]
        private float zonePlantsRadius = 1f,
            zoneFarmRadius = 0.1f;

        private MeshRenderer renderers;
        private ItemSeed itemSeed;

        public void Init(ItemSeed item)
        {
            farmLayer = LayerMask.GetMask("Farm");
            plantLayer = LayerMask.GetMask("Plant");
            itemSeed = item;
            GameObject ghost = Instantiate(itemSeed.Stages[0], transform.position, Quaternion.identity, transform);
            renderers = ghost.GetComponentInChildren<MeshRenderer>();
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
            return Physics.OverlapSphere(transform.position, zoneFarmRadius, farmLayer).Length > 0;
        }

        private bool IsCollidingWithPlants()
        {
            Collider[] plantHits = Physics.OverlapSphere(transform.position, zonePlantsRadius, plantLayer);
            foreach (var hit in plantHits)
            {
                if (hit.gameObject != gameObject) return true;
            }
            return false;
        }

        private void UpdateMaterial(bool canPlant)
        {
            if (renderers == null || itemSeed == null) return;

            Material mat = canPlant ? _confirm : _reject;
            renderers.material = mat;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, zonePlantsRadius);
            Gizmos.color = Color.grey;
            Gizmos.DrawWireSphere(transform.position, zoneFarmRadius);
        }
    }

}
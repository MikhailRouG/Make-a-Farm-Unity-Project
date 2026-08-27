using UnityEngine;

namespace Gameplay.Farm
{
    /// <summary>
    /// The planting preview. One instance lives on the player and is re-dressed for
    /// whichever seed is being placed, so it is shown and hidden rather than spawned
    /// and destroyed.
    /// </summary>
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
        private GameObject _visual;

        /// <summary>Dresses the ghost as this seed and turns it on.</summary>
        public void Show(ItemSeed seed)
        {
            // Resolved here and not in Awake: the ghost normally sits disabled in the
            // prefab, and Awake would not have run before the first Show.
            _farmLayer = LayerMask.GetMask("Farm");
            _plantLayer = LayerMask.GetMask("Plant");

            BuildVisual(seed);

            gameObject.SetActive(true);
            UpdateMaterial(false);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public bool CheckZone()
        {
            bool canPlant = IsOnFarm() && !IsCollidingWithPlants();
            UpdateMaterial(canPlant);
            return canPlant;
        }

        private void BuildVisual(ItemSeed seed)
        {
            // The previous seed's preview has to go, or ghosts pile up inside the
            // reused object.
            if (_visual != null)
                Destroy(_visual);

            _visual = null;
            _renderer = null;

            if (seed == null || seed.Stages == null || seed.Stages.Length == 0 || seed.Stages[0] == null)
            {
                Debug.LogError($"[PlantCheckPosition] {(seed != null ? seed.name : "seed")}: stage 0 prefab is not assigned.", this);
                return;
            }

            _visual = Instantiate(seed.Stages[0], transform.position, Quaternion.identity, transform);
            _renderer = _visual.GetComponentInChildren<MeshRenderer>();
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
            if (_renderer == null) return;

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

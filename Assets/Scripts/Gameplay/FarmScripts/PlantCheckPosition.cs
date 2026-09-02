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

        // Null until the first material is applied to the current visual, so a fresh
        // ghost always gets one even when its state matches the previous ghost's.
        private bool? _shownCanPlant;

        public void Show(ItemSeed seed)
        {
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
            if (_visual != null)
                Destroy(_visual);

            _visual = null;
            _renderer = null;
            _shownCanPlant = null;

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

        // CheckZone runs every frame while the ghost is up, and assigning Renderer
        // .material instantiates a fresh material each time - only a real change in
        // state is pushed through.
        private void UpdateMaterial(bool canPlant)
        {
            if (_renderer == null) return;

            if (_shownCanPlant == canPlant) return;

            _shownCanPlant = canPlant;
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

using UnityEngine;
using Mirror;
namespace Gameplay.Player
{
    public class PlayerUIBinder : NetworkBehaviour
    {
        [Header("UI")]
        [SerializeField] private PlayerUIPanel _uiPrefab;

        private Inventory _inventory;
        private PlayerInteraction _interaction;

        private PlayerUIPanel _uiInstance;
        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();

            CreateUI();
        }

        public override void OnStopLocalPlayer()
        {
            base.OnStopLocalPlayer();

            DestroyUI();
        }

        private void CreateUI()
        {
            if (_uiPrefab == null)
            {
                Debug.LogError($"{nameof(PlayerUIBinder)}: UI prefab is not assigned.", this);
                return;
            }

            if (_uiInstance != null)
                return;

            if (!TryGetComponent(out Inventory inventory))
            {
                Debug.LogError($"{nameof(PlayerUIBinder)}: Inventory not found.", this);
                return;
            }

            if (!TryGetComponent(out PlayerInventory playerInventory))
            {
                Debug.LogError($"{nameof(PlayerUIBinder)}: PlayerInventory not found.", this);
                return;
            }

            if (!TryGetComponent(out PlayerInteraction interaction))
            {
                Debug.LogError($"{nameof(PlayerUIBinder)}: PlayerInteraction not found.", this);
                return;
            }
            _uiInstance = Instantiate(_uiPrefab);
            _uiInstance.Bind(inventory, playerInventory, interaction);
            return;
        }

        private void DestroyUI()
        {
            if (_uiInstance == null)
                return;

            Destroy(_uiInstance.gameObject);
            _uiInstance = null;
        }
    }
}
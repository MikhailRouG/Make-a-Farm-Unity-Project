using TMPro;
using UnityEngine;

namespace Gameplay.Player
{
    public class PlayerUIPanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _interactionText;
        [SerializeField] private TextMeshProUGUI _moneyText;
        [SerializeField] private PlayerInventoryUI _inventoryUI;
        [SerializeField] private PauseMenuUi _pauseMenu;

        private Inventory _inventory;
        private PlayerInteraction _interaction;

        public void Bind(
            Inventory inventory,
            PlayerInventory playerInventory,
            PlayerInteraction interaction,
            Player player)
        {
            if (_inventoryUI == null)
                _inventoryUI = GetComponentInChildren<PlayerInventoryUI>();

            if (_inventoryUI != null && inventory != null)
                _inventoryUI.Init(inventory, playerInventory);

            if (_interactionText != null && interaction != null)
            {
                _interaction = interaction;
                _interaction.OnHasInteraction += SetInteractionText;
            }

            if (_moneyText != null && inventory != null)
            {
                _inventory = inventory;
                _inventory.OnMoneyChanged += MoneyUpdate;

                // The initial sync is already done by the time Bind runs, and its
                // hook may never have fired at all — Mirror skips it when the
                // incoming value equals the one baked into the prefab.
                MoneyUpdate(_inventory.Money);
            }

            if (_pauseMenu == null)
                _pauseMenu = GetComponentInChildren<PauseMenuUi>(true);

            if (_pauseMenu != null && player != null)
                _pauseMenu.Bind(player);
        }

        private void OnDestroy()
        {
            if (_interaction != null)
                _interaction.OnHasInteraction -= SetInteractionText;

            if (_inventory != null)
                _inventory.OnMoneyChanged -= MoneyUpdate;
        }

        private void SetInteractionText(string text)
        {
            bool hasText = !string.IsNullOrEmpty(text);

            _interactionText.text = hasText ? text : string.Empty;
            _interactionText.gameObject.SetActive(hasText);
        }

        private void MoneyUpdate(int count)
        {
            _moneyText.text = count.ToString();
        }
    }
}

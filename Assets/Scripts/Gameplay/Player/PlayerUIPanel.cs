using TMPro;
using UnityEngine;

namespace Gameplay.Player
{
    public class PlayerUIPanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _interactionText;
        [SerializeField] private TextMeshProUGUI _moneyText;
        private PlayerInteraction _interaction;
        [SerializeField] private PlayerInventoryUI inventoryUI;
        public void Bind(
               Inventory inventory,
               PlayerInventory playerInventory,
               PlayerInteraction interaction)
        {
            if (inventoryUI == null)
                inventoryUI = GetComponentInChildren<PlayerInventoryUI>();
            if (inventoryUI != null && inventory != null)
                inventoryUI.Init(inventory, playerInventory);
            if (_interactionText != null)
            {
                interaction.OnHasInteraction += SetInteractionText;
            }
            if (_moneyText != null) inventory.OnMoneyChanged += MoneyUpdate;
        }

        private void OnDestroy()
        {
            if (_interaction != null)
                _interaction.OnHasInteraction -= SetInteractionText;
        }
        private void SetInteractionText(string text)
        {
            if (text == string.Empty) _interactionText.gameObject.SetActive(false);
            else _interactionText.gameObject.SetActive(true);
        }

        private void MoneyUpdate(int count)
        {
            _moneyText.text = count.ToString();
        }
    }
}
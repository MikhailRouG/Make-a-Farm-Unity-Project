using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Player
{
    public class PlayerInventoryUI : MonoBehaviour
    {
        [SerializeField] private List<InventorySlotUI> _slotsUI;

        private Inventory _inventory;
        private PlayerInventory _playerInventory;
        private ItemDatabase _itemDatabase;
        private int _currentSlot = -1;

        private void Awake()
        {
            _itemDatabase = ItemDatabase.Instance;
        }

        // Unsubscribe before subscribing: Init subscribes too, so re-enabling the
        // panel would otherwise fire the handlers twice.
        private void OnEnable()
        {
            if (_inventory == null)
                return;

            _inventory.OnInventoryChanged -= RefreshUI;
            _inventory.OnInventoryChanged += RefreshUI;

            if (_playerInventory != null)
            {
                _playerInventory.OnSelectedSlotChangedEvent -= SlotClear;
                _playerInventory.OnSelectedSlotChangedEvent += SlotClear;
            }

            RefreshUI();
        }

        private void OnDisable()
        {
            if (_inventory != null)
                _inventory.OnInventoryChanged -= RefreshUI;

            if (_playerInventory != null)
                _playerInventory.OnSelectedSlotChangedEvent -= SlotClear;
        }

        private void OnDestroy()
        {
            if (_slotsUI != null)
            {
                for (int i = 0; i < _slotsUI.Count; i++)
                {
                    if (_slotsUI[i] != null)
                        _slotsUI[i].Clicked -= OnSlotClicked;
                }
            }

            if (_inventory != null)
                _inventory.OnInventoryChanged -= RefreshUI;

            if (_playerInventory != null)
                _playerInventory.OnSelectedSlotChangedEvent -= SlotClear;
        }

        public void Init(Inventory inventory, PlayerInventory player)
        {
            _inventory = inventory;
            _playerInventory = player;

            if (_inventory == null || !_inventory.isLocalPlayer)
            {
                gameObject.SetActive(false);
                return;
            }

            _slotsUI = new List<InventorySlotUI>(GetComponentsInChildren<InventorySlotUI>());

            for (int i = 0; i < _slotsUI.Count; i++)
            {
                _slotsUI[i].Init(i);
                _slotsUI[i].Clicked += OnSlotClicked;
            }

            _inventory.OnInventoryChanged -= RefreshUI;
            _inventory.OnInventoryChanged += RefreshUI;

            RefreshUI();

            _playerInventory.OnSelectedSlotChangedEvent -= SlotClear;
            _playerInventory.OnSelectedSlotChangedEvent += SlotClear;
        }

        public void RefreshUI()
        {
            if (_inventory == null)
                return;

            for (int i = 0; i < _slotsUI.Count; i++)
            {
                if (i >= _inventory.Slots.Count)
                {
                    _slotsUI[i].Clear();
                    continue;
                }

                InventorySlot slot = _inventory.Slots[i];

                if (slot.IsEmpty)
                {
                    _slotsUI[i].Clear();
                    continue;
                }

                ItemConfig item = _itemDatabase != null ? _itemDatabase.Get(slot.ItemId) : null;

                if (item == null)
                {
                    _slotsUI[i].Clear();
                    continue;
                }

                _slotsUI[i].Set(item.Icon, slot);
            }
        }

        private void OnSlotClicked(int slotIndex)
        {
            if (_inventory == null)
                return;

            if (_currentSlot >= 0 && _currentSlot < _slotsUI.Count)
                _slotsUI[_currentSlot].OnUnSelected();

            if (_currentSlot == slotIndex)
            {
                _currentSlot = -1;
            }
            else
            {
                _currentSlot = slotIndex;
                _slotsUI[_currentSlot].OnSelected();
            }

            _playerInventory.CmdSelectSlot(_currentSlot);
        }

        private void SlotClear(int slotIndex)
        {
            if (slotIndex != -1)
                return;

            if (_currentSlot >= 0 && _currentSlot < _slotsUI.Count)
                _slotsUI[_currentSlot].OnUnSelected();

            _currentSlot = -1;
        }
    }
}

    using UnityEngine;
    using System.Collections.Generic;
    using UnityEngine.UI;

    public class PlayerInventoryUI : MonoBehaviour
    {
        private Inventory _inventory;
    private PlayerInventory _playerInventory;
    [SerializeField] private List<InventorySlotUI> _slotsUI;
    [SerializeField] private ItemDatabase _itemDatabase;
    private int _currentSlot = -1;
    public void Init(Inventory inventory, PlayerInventory player)
    {
        _inventory = inventory;
        _playerInventory = player;
        if (_inventory == null || !_inventory.isLocalPlayer)
        {
            Debug.Log("Inventory isLocalPlayer: " + _inventory.isLocalPlayer);
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
    private void OnDestroy()
    {
        for (int i = 0; i < _slotsUI.Count; i++)
        {
            _slotsUI[i].Clicked -= OnSlotClicked;
        }
        _playerInventory.OnSelectedSlotChangedEvent -= SlotClear;
    }
    private void OnEnable()
    {
        if (_inventory == null)
            return;
        _inventory.OnInventoryChanged += RefreshUI;
        _playerInventory.OnSelectedSlotChangedEvent += SlotClear;
    }
    private void OnDisable()
        {
        if (_inventory == null) return;

        _inventory.OnInventoryChanged -= RefreshUI;
        _playerInventory.OnSelectedSlotChangedEvent -= SlotClear;
    }
        public void RefreshUI()
        {
        if (_inventory == null) return;

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

            ItemConfig item = _itemDatabase.Get(slot.ItemId);

            _slotsUI[i].Set(item.Icon, slot);
        }
    }

    private void OnSlotClicked(int slotIndex)
    {
        if(_inventory  == null) return;
        if (_currentSlot >= 0 && _currentSlot < _slotsUI.Count) _slotsUI[_currentSlot].OnUnSelected();

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
        Debug.Log(_currentSlot);
    }

    private void SlotClear(int i)
    {
        if (i != -1) return;
        if (_currentSlot >= 0 && _currentSlot < _slotsUI.Count) { 
                _slotsUI[_currentSlot].OnUnSelected();
            Debug.Log("esc1");
        }
        _currentSlot = -1;
        Debug.Log("esc");
    }
}


using System;
using Mirror;
using UnityEngine;

namespace Gameplay.Player
{
    [RequireComponent(typeof(Inventory))]
    public class PlayerInventory : NetworkBehaviour
    {
        [SyncVar(hook = nameof(OnSelectedSlotChanged))]
        private int _selectedSlotIndex;

        private Inventory _inventory;
        private Player _player;
        private ItemDatabase _database;

        public int SelectedSlotIndex => _selectedSlotIndex;
        public event Action<int> OnSelectedSlotChangedEvent;

        private void Awake()
        {
            _database = ItemDatabase.Instance;
            _inventory = GetComponent<Inventory>();
            _player = GetComponent<Player>();

            if (_player != null)
                _player.onUseItem += CmdUseSelectedItem;
        }

        private void OnDestroy()
        {
            if (_player != null)
                _player.onUseItem -= CmdUseSelectedItem;
        }

        private void OnSelectedSlotChanged(int oldIndex, int newIndex)
        {
            OnSelectedSlotChangedEvent?.Invoke(newIndex);
        }

        [Command]
        public void CmdSelectSlot(int slotIndex)
        {
            _selectedSlotIndex = slotIndex;
        }

        [Command]
        public void CmdUseSelectedItem()
        {
            InventorySlot slot = _inventory.GetSlotServer(_selectedSlotIndex);

            if (slot.IsEmpty)
                return;

            ItemConfig item = _database != null ? _database.Get(slot.ItemId) : null;

            if (item == null)
                return;

            if (item.UseServer(netIdentity, slot))
                _inventory.RemoveItemFromSlot(_selectedSlotIndex, 1);
        }

        [Command]
        public void CmdDropSelectedItem()
        {
            InventorySlot slot = _inventory.GetSlotServer(_selectedSlotIndex);

            if (slot.IsEmpty)
                return;

            _inventory.RemoveItemFromSlot(_selectedSlotIndex, 1);
        }

        public void OnEscape()
        {
            CmdSelectSlot(-1);
        }
    }
}

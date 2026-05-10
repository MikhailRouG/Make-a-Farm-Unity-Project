using Mirror;
using System;
using UnityEngine;

[RequireComponent(typeof(Inventory))]
public class PlayerInventory : NetworkBehaviour
{
    private Inventory _inventory;
    [SerializeField] private ItemDatabase _itemDatabase;

    [SyncVar(hook = nameof(OnSelectedSlotChanged))]
    private int _selectedSlotIndex;
    public int SelectedSlotIndex => _selectedSlotIndex;
    public event Action<int> OnSelectedSlotChangedEvent;

    private void Awake()
    {
        _inventory = GetComponent<Inventory>();
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
        ItemConfig item = _itemDatabase.Get(slot.ItemId);

        if (item == null)
            return;

        NetworkIdentity owner = netIdentity;

        bool usedSuccessfully = item.UseServer(owner, slot);

        if (!usedSuccessfully)
            return;

        if (item.ConsumeOnUse)
        {
           _inventory.RemoveItemFromSlot(_selectedSlotIndex, 1);
        }
    }
    [Command]
    public void CmdDropSelectedItem()
    {
        InventorySlot slot = _inventory.GetSlotServer(_selectedSlotIndex);

        if (slot.IsEmpty)
            return;

        ItemConfig item = _itemDatabase.Get(slot.ItemId);

        if (item == null)
            return;

        _inventory.RemoveItemFromSlot(_selectedSlotIndex, 1);
    }
    public void OnEscape()
    {

    }
}

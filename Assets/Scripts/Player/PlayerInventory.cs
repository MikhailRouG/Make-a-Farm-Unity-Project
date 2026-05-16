using Mirror;
using System;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using Zenject;

[RequireComponent(typeof(Inventory))]
public class PlayerInventory : NetworkBehaviour
{
    private Inventory _inventory;
    private Player _player;
     private ItemDatabase _itemDatabase;

    [SyncVar(hook = nameof(OnSelectedSlotChanged))]
    private int _selectedSlotIndex;
    public int SelectedSlotIndex => _selectedSlotIndex;
    public event Action<int> OnSelectedSlotChangedEvent;

    [Inject]
    private void Construct(ItemDatabase itemDatabase)
    {
        _itemDatabase = itemDatabase;
    }
    private void CheckInjection()
    {
        if (_itemDatabase == null)
        {
            var container = ProjectContext.Instance.Container;
            container.Inject(this);
        }
    }
    private void Awake()
    {
        CheckInjection();
        _inventory = GetComponent<Inventory>();
        _player = GetComponent<Player>();
        _player.onUseItem += CmdUseSelectedItem;
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
        _selectedSlotIndex = -1;
    }
}

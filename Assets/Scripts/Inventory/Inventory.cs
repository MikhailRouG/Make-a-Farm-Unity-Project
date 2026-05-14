using System;
using UnityEngine;
using Mirror;
public class Inventory : NetworkBehaviour
{
    [Header("Settings")]
    [SerializeField] private int _maxSlots = 9;
    [Header("References")]
    [SerializeField] private ItemDatabase _itemDatabase;
    public readonly SyncList<InventorySlot> Slots = new SyncList<InventorySlot>();
    public event Action OnInventoryChanged;

    public override void OnStartServer()
    {
        base.OnStartServer();

        if (Slots.Count == 0)
        {
            for (int i = 0; i < _maxSlots; i++)
            {
                Slots.Add(new InventorySlot());
            }
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        Slots.Callback += OnSlotsChanged;
        OnInventoryChanged?.Invoke();
    }
    public override void OnStopClient()
    {
        base.OnStopClient();

        Slots.Callback -= OnSlotsChanged;
    }
    private void OnSlotsChanged(
       SyncList<InventorySlot>.Operation operation,
       int index,
       InventorySlot oldItem,
       InventorySlot newItem)
    {
        OnInventoryChanged?.Invoke();
    }

    private bool IsValidSlotIndex(int slotIndex)
    {
        return slotIndex >= 0 && slotIndex < Slots.Count;
    }
    

    // SERVER API
    public bool TryAddItem(int itemId, int amount = 1)
    {
        if (amount <= 0)
            return false;

        ItemConfig item = _itemDatabase.Get(itemId);

        if (item == null)
            return false;

        if (item.IsStackable)
        {
            amount = AddToExistingStacks(item, itemId, amount);
        }

        if (amount > 0)
        {
            amount = AddToEmptySlots(item, itemId, amount);
        }

        return amount == 0;
    }

    [Server]
    public bool HasItem(int itemId, int amount = 1)
    {
        if (amount <= 0)
            return false;

        int total = 0;

        for (int i = 0; i < Slots.Count; i++)
        {
            InventorySlot slot = Slots[i];

            if (slot.ItemId != itemId)
                continue;

            total += slot.Amount;

            if (total >= amount)
                return true;
        }

        return false;
    }
    [Server]
    public bool HasItemByIndex(int itemId, int index, int amount = 1)
    {
        if (amount <= 0)
            return false;

        int total = 0;

        InventorySlot slot = Slots[index];

        if (slot.ItemId != itemId)
            return false;

        total += slot.Amount;

        if (total >= amount)
            return true;


        return false;
    }
    [Server]
    public bool TryRemoveItem(int itemId, int amount)
    {
        if (!HasItem(itemId, amount))
            return false;

        int remaining = amount;

        for (int i = 0; i < Slots.Count; i++)
        {
            InventorySlot slot = Slots[i];

            if (slot.ItemId != itemId)
                continue;

            int removeAmount = Mathf.Min(slot.Amount, remaining);

            slot.Amount -= removeAmount;
            remaining -= removeAmount;

            if (slot.Amount <= 0)
            {
                Slots[i] = new InventorySlot();
            }
            else
            {
                Slots[i] = slot;
            }

            if (remaining <= 0)
                return true;
        }

        return true;
    }
    [Server]
    public bool RemoveItemFromSlot(int slotIndex, int amount)
    {
        if (!IsValidSlotIndex(slotIndex))
            return false;

        if (amount <= 0)
            return false;

        InventorySlot slot = Slots[slotIndex];

        if (slot.IsEmpty)
            return false;

        slot.Amount -= amount;

        if (slot.Amount <= 0)
        {
            Slots[slotIndex] = new InventorySlot();
        }
        else
        {
            Slots[slotIndex] = slot;
        }

        return true;
    }

    [Server]
    public InventorySlot GetSlotServer(int slotIndex)
    {
        if (!IsValidSlotIndex(slotIndex))
            return new InventorySlot();

        return Slots[slotIndex];
    }

    // PRIVATE SERVER LOGIC


    [Server]
    private int AddToExistingStacks(ItemConfig item, int itemId, int amount)
    {
        for (int i = 0; i < Slots.Count; i++)
        {
            InventorySlot slot = Slots[i];

            if (slot.IsEmpty)
                continue;

            if (slot.ItemId != itemId)
                continue;

            int freeSpace = item.MaxStackSize - slot.Amount;

            if (freeSpace <= 0)
                continue;

            int addAmount = Mathf.Min(freeSpace, amount);

            slot.Amount += addAmount;
            amount -= addAmount;

            Slots[i] = slot;

            if (amount <= 0)
                return 0;
        }

        return amount;
    }
    [Server]
    private int AddToEmptySlots(ItemConfig item, int itemId, int amount)
    {
        for (int i = 0; i < Slots.Count; i++)
        {
            InventorySlot slot = Slots[i];

            if (!slot.IsEmpty)
                continue;

            int addAmount = item.IsStackable
                ? Mathf.Min(item.MaxStackSize, amount)
                : 1;

            Slots[i] = new InventorySlot(itemId, addAmount);

            amount -= addAmount;

            if (amount <= 0)
                return 0;
        }

        return amount;
    }
}

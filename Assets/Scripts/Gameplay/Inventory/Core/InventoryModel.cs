using System;
using System.Collections.Generic;

/// <summary>
/// SyncList implements IList
/// through its ordinary public indexer, so assigning an element here still
/// goes through the setter that records the change and marks it dirty.
/// </summary>
public sealed class InventoryModel
{
    private readonly IList<InventorySlot> _slots;
    private readonly IItemStackRules _rules;

    public InventoryModel(IList<InventorySlot> slots, IItemStackRules rules)
    {
        _slots = slots ?? throw new ArgumentNullException(nameof(slots));
        _rules = rules ?? throw new ArgumentNullException(nameof(rules));
    }

    public int SlotCount => _slots.Count;

    /// <summary>Pads the storage with empty slots up to <paramref name="count"/>.</summary>
    public void EnsureSlotCount(int count)
    {
        while (_slots.Count < count)
            _slots.Add(InventorySlot.Empty);
    }

    public bool IsValidIndex(int index) => index >= 0 && index < _slots.Count;

    public InventorySlot GetSlot(int index) =>
        IsValidIndex(index) ? _slots[index] : InventorySlot.Empty;

    public bool TryAdd(int itemId, int amount = 1, float size = 1f)
    {
        if (amount <= 0)
            return false;

        if (!_rules.TryGet(itemId, out ItemStackRule rule))
            return false;

        if (!HasRoomFor(rule, itemId, amount))
            return false;

        int remaining = amount;

        if (rule.IsStackable)
            remaining = AddToExistingStacks(rule, itemId, remaining);

        if (remaining > 0)
            remaining = AddToEmptySlots(rule, itemId, remaining, size);

        return remaining == 0;
    }

    public bool TryAdd(InventorySlot slotData) =>
        TryAdd(slotData.ItemId, slotData.Amount, slotData.Weight);

    public bool TryGetItemSize(int itemId, out float size)
    {
        for (int i = 0; i < _slots.Count; i++)
        {
            InventorySlot slot = _slots[i];

            if (slot.IsEmpty || slot.ItemId != itemId)
                continue;

            size = slot.Weight;
            return true;
        }

        size = 0f;
        return false;
    }

    public bool HasItem(int itemId, int amount = 1)
    {
        if (amount <= 0)
            return false;

        int total = 0;

        for (int i = 0; i < _slots.Count; i++)
        {
            InventorySlot slot = _slots[i];

            if (slot.IsEmpty || slot.ItemId != itemId)
                continue;

            total += slot.Amount;

            if (total >= amount)
                return true;
        }

        return false;
    }

    public bool HasAtIndex(int itemId, int index, int amount = 1)
    {
        if (amount <= 0)
            return false;

        if (!IsValidIndex(index))
            return false;

        InventorySlot slot = _slots[index];

        return !slot.IsEmpty
            && slot.ItemId == itemId
            && slot.Amount >= amount;
    }

    public bool TryRemove(int itemId, int amount = 1)
    {
        if (amount <= 0)   return false;

        if (!HasItem(itemId, amount))  return false;

        int remaining = amount;

        for (int i = 0; i < _slots.Count; i++)
        {
            InventorySlot slot = _slots[i];

            if (slot.IsEmpty || slot.ItemId != itemId)
                continue;

            int removeAmount = Math.Min(slot.Amount, remaining);
            int newAmount = slot.Amount - removeAmount;

            remaining -= removeAmount;

            _slots[i] = newAmount > 0
                ? slot.WithAmount(newAmount)
                : InventorySlot.Empty;

            if (remaining <= 0)
                return true;
        }

        return remaining <= 0;
    }

    public bool RemoveFromSlot(int index, int amount)
    {
        if (!IsValidIndex(index))
            return false;

        if (amount <= 0)
            return false;

        InventorySlot slot = _slots[index];

        if (slot.IsEmpty)
            return false;

        int newAmount = slot.Amount - amount;

        _slots[index] = newAmount > 0
            ? slot.WithAmount(newAmount)
            : InventorySlot.Empty;

        return true;
    }

    private bool HasRoomFor(ItemStackRule rule, int itemId, int amount)
    {
        int room = 0;

        for (int i = 0; i < _slots.Count; i++)
        {
            InventorySlot slot = _slots[i];

            if (slot.IsEmpty)
            {
                room += rule.SlotCapacity;
            }
            else if (rule.IsStackable && slot.ItemId == itemId)
            {
                int freeSpace = rule.MaxStackSize - slot.Amount;

                if (freeSpace > 0)
                    room += freeSpace;
            }

            if (room >= amount)
                return true;
        }

        return false;
    }

    private int AddToExistingStacks(ItemStackRule rule, int itemId, int amount)
    {
        for (int i = 0; i < _slots.Count; i++)
        {
            InventorySlot slot = _slots[i];

            if (slot.IsEmpty || slot.ItemId != itemId)
                continue;

            int freeSpace = rule.MaxStackSize - slot.Amount;

            if (freeSpace <= 0)
                continue;

            int addAmount = Math.Min(freeSpace, amount);

            _slots[i] = slot.WithAmount(slot.Amount + addAmount);

            amount -= addAmount;

            if (amount <= 0)
                return 0;
        }

        return amount;
    }

    private int AddToEmptySlots(ItemStackRule rule, int itemId, int amount, float size)
    {
        for (int i = 0; i < _slots.Count; i++)
        {
            if (!_slots[i].IsEmpty)
                continue;

            int addAmount = Math.Min(rule.SlotCapacity, amount);

            if (addAmount <= 0)
                break;

            _slots[i] = new InventorySlot(itemId, addAmount, size);

            amount -= addAmount;

            if (amount <= 0)
                return 0;
        }

        return amount;
    }
}

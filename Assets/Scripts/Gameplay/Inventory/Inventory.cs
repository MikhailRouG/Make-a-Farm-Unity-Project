using System;
using Mirror;
using UnityEngine;

public class Inventory : NetworkBehaviour
{
    [Header("Settings")]
    [SerializeField] private int _maxSlots = 9;

    [SyncVar(hook = nameof(OnMoneySyncChanged))]
    [SerializeField] private int _money;

    private ItemDatabase _database;

    public readonly SyncList<InventorySlot> Slots = new SyncList<InventorySlot>();

    public int Money => _money;

    public event Action<int> OnMoneyChanged;
    public event Action OnInventoryChanged;

    private void Awake()
    {
        _database = ItemDatabase.Instance;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        if (Slots.Count == 0)
        {
            for (int i = 0; i < _maxSlots; i++)
            {
                Slots.Add(InventorySlot.Empty);
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

    private void OnMoneySyncChanged(int oldValue, int newValue)
    {
        OnMoneyChanged?.Invoke(newValue);
    }

    private void OnSlotsChanged(
        SyncList<InventorySlot>.Operation operation,
        int index,
        InventorySlot oldItem,
        InventorySlot newItem)
    {
        OnInventoryChanged?.Invoke();
    }

    [Server]
    public bool TryAddMoney(int amount)
    {
        if (amount <= 0)
            return false;

        if (_money > int.MaxValue - amount)
            return false;

        _money += amount;
        return true;
    }

    [Server]
    public bool TrySpendMoney(int amount)
    {
        if (amount < 0)
            return false;

        if (_money < amount)
            return false;

        _money -= amount;
        return true;
    }

    // All or nothing: nothing is written until the whole amount is known to fit.
    // A partial add plus a false result would let callers assume the item was
    // never handed over and keep the source alive, duplicating it.
    [Server]
    public bool TryAddItem(int itemId, int amount = 1, float weight = 1f)
    {
        if (amount <= 0)
            return false;

        ItemConfig item = GetItem(itemId);

        if (item == null)
            return false;

        if (!HasSpaceFor(item, amount))
            return false;

        int remaining = amount;

        if (item.IsStackable)
            remaining = AddToExistingStacks(item, remaining);

        if (remaining > 0)
            remaining = AddToEmptySlots(item, remaining, weight);

        return remaining == 0;
    }

    [Server]
    public bool TryAddItem(InventorySlot slotData)
    {
        return TryAddItem(slotData.ItemId, slotData.Amount, slotData.Weight);
    }

    [Server]
    public bool TryGetItemSize(int itemId, out float size)
    {
        for (int i = 0; i < Slots.Count; i++)
        {
            InventorySlot slot = Slots[i];

            if (slot.IsEmpty || slot.ItemId != itemId)
                continue;

            size = slot.Weight;
            return true;
        }

        size = 0f;
        return false;
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

            if (slot.IsEmpty || slot.ItemId != itemId)
                continue;

            total += slot.Amount;

            if (total >= amount)
                return true;
        }

        return false;
    }

    [Server]
    public bool TryRemoveItem(int itemId, int amount = 1)
    {
        if (amount <= 0)
            return false;

        if (!HasItem(itemId, amount))
            return false;

        int remaining = amount;

        for (int i = 0; i < Slots.Count; i++)
        {
            InventorySlot slot = Slots[i];

            if (slot.IsEmpty || slot.ItemId != itemId)
                continue;

            int removeAmount = Mathf.Min(slot.Amount, remaining);

            slot.Amount -= removeAmount;
            remaining -= removeAmount;

            Slots[i] = slot.Amount <= 0 ? InventorySlot.Empty : slot;

            if (remaining <= 0)
                return true;
        }

        return remaining <= 0;
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

        Slots[slotIndex] = slot.Amount <= 0 ? InventorySlot.Empty : slot;

        return true;
    }

    [Server]
    public InventorySlot GetSlotServer(int slotIndex)
    {
        if (!IsValidSlotIndex(slotIndex))
            return InventorySlot.Empty;

        return Slots[slotIndex];
    }

    private bool IsValidSlotIndex(int slotIndex)
    {
        return slotIndex >= 0 && slotIndex < Slots.Count;
    }

    [Server]
    private ItemConfig GetItem(int itemId)
    {
        if (_database == null)
            _database = ItemDatabase.Instance;

        return _database != null ? _database.Get(itemId) : null;
    }

    // Clamped to at least 1: a zero MaxStackSize in the config would turn the
    // free-space count into an infinite loop.
    private static int MaxStack(ItemConfig item)
    {
        return item.IsStackable ? Mathf.Max(1, item.MaxStackSize) : 1;
    }

    [Server]
    private bool HasSpaceFor(ItemConfig item, int amount)
    {
        int maxStack = MaxStack(item);
        int free = 0;

        for (int i = 0; i < Slots.Count; i++)
        {
            InventorySlot slot = Slots[i];

            if (slot.IsEmpty)
                free += maxStack;
            else if (item.IsStackable && slot.ItemId == item.Id)
                free += Mathf.Max(0, maxStack - slot.Amount);

            if (free >= amount)
                return true;
        }

        return false;
    }

    [Server]
    private int AddToExistingStacks(ItemConfig item, int amount)
    {
        int maxStack = MaxStack(item);

        for (int i = 0; i < Slots.Count; i++)
        {
            InventorySlot slot = Slots[i];

            if (slot.IsEmpty || slot.ItemId != item.Id)
                continue;

            int freeSpace = maxStack - slot.Amount;

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
    private int AddToEmptySlots(ItemConfig item, int amount, float weight)
    {
        int maxStack = MaxStack(item);

        for (int i = 0; i < Slots.Count; i++)
        {
            if (!Slots[i].IsEmpty)
                continue;

            int addAmount = Mathf.Min(maxStack, amount);

            Slots[i] = new InventorySlot(item.Id, addAmount, weight);

            amount -= addAmount;

            if (amount <= 0)
                return 0;
        }

        return amount;
    }
}

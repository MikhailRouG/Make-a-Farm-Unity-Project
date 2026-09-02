using System;
using Mirror;
using UnityEngine;

/// <summary>
/// Network shell around InventoryModel: syncing, money and the [Server] guards live
/// here, every slot rule lives in the model.
/// </summary>
public class Inventory : NetworkBehaviour
{
    [Header("Settings")]
    [SerializeField] private int _maxSlots = 9;

    [SyncVar(hook = nameof(OnMoneySyncChanged))]
    [SerializeField] private int _money;

    public readonly SyncList<InventorySlot> Slots = new SyncList<InventorySlot>();

    private InventoryModel _model;

    public int Money => _money;

    public event Action<int> OnMoneyChanged;
    public event Action OnInventoryChanged;

    private void Awake()
    {
        _model = new InventoryModel(Slots, new ItemDatabaseStackRules(ItemDatabase.Instance));
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        _model.EnsureSlotCount(_maxSlots);
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

    private void OnSlotsChanged( SyncList<InventorySlot>.Operation operation,  int index,
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

    [Server]
    public bool TryAddItem(int itemId, int amount = 1, float weight = 1f) =>
        _model.TryAdd(itemId, amount, weight);

    [Server]
    public bool TryAddItem(InventorySlot slotData) => _model.TryAdd(slotData);

    [Server]
    public bool TryGetItemSize(int itemId, out float size) => _model.TryGetItemSize(itemId, out size);

    [Server]
    public bool HasItem(int itemId, int amount = 1) => _model.HasItem(itemId, amount);

    [Server]
    public bool TryRemoveItem(int itemId, int amount = 1) => _model.TryRemove(itemId, amount);

    [Server]
    public bool RemoveItemFromSlot(int slotIndex, int amount) => _model.RemoveFromSlot(slotIndex, amount);

    [Server]
    public bool TryReplaceInSlot(int slotIndex, int newItemId) =>
        _model.TryReplaceInSlot(slotIndex, newItemId);

    [Server]
    public InventorySlot GetSlotServer(int slotIndex) => _model.GetSlot(slotIndex);
}

using Gameplay.Player;
using Mirror;
using UnityEngine;

/// <summary>
/// A world source of one resource - a well, a water barrel. It knows nothing about
/// the containers that come to it: the item in hand declares what it fills from and
/// what it becomes, so a new container is one asset and no code here.
/// </summary>
public class RefillStation : NetworkBehaviour, IInteractable
{
    [SerializeField] private FillResource _provides = FillResource.Water;
    [SerializeField] private string _prompt = "Fill the bucket";

    private ItemDatabase _database;

    // Asked for once, when the ray first lands on this object, so it cannot react to
    // the player changing slots while already aiming here - hence a fixed line
    // rather than one built from what is in hand.
    public string InteractionPrompt => _prompt;

    private void Awake()
    {
        _database = ItemDatabase.Instance;
    }

    [Server]
    public void Interact(GameObject interactor)
    {
        if (_provides == FillResource.None)
        {
            Debug.LogError($"[{nameof(RefillStation)}] {name}: resource is not set.", this);
            return;
        }

        if (interactor == null || _database == null)
            return;

        if (!interactor.TryGetComponent(out Inventory inventory)) return;
        if (!interactor.TryGetComponent(out PlayerInventory playerInventory)) return;

        int slotIndex = playerInventory.SelectedSlotIndex;
        InventorySlot slot = inventory.GetSlotServer(slotIndex);

        if (slot.IsEmpty)
            return;

        if (_database.Get(slot.ItemId) is not ItemContainer container)
            return;

        if (!container.TryGetFilled(_provides, out ItemConfig filled))
            return;

        inventory.TryReplaceInSlot(slotIndex, filled.Id);
    }
}

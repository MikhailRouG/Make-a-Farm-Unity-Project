using Mirror;
using UnityEngine;

public class ItemSource : NetworkBehaviour, IInteractable
{
    [Header("What it gives")]
    [SerializeField] private ItemConfig _item;
    [SerializeField, Min(1)] private int _amount = 1;

    [SerializeField, Min(0f)] private float _cooldown;

    [SerializeField] private bool _destroyWhenTaken;

    [SerializeField] private string _prompt;

    [SyncVar] private double _readyAt;

    public string InteractionPrompt
    {
        get
        {
            if (_item == null)
                return string.Empty;

            double remaining = _readyAt - NetworkTime.time;

            if (remaining > 0d)
                return $"Ready in {remaining:F1}";

            return string.IsNullOrEmpty(_prompt) ? $"Take {_item.Name}" : _prompt;
        }
    }

    [Server]
    public void Interact(GameObject interactor)
    {
        if (_item == null)
        {
            Debug.LogError($"[ItemSource] {name}: item is not assigned.", this);
            return;
        }

        if (NetworkTime.time < _readyAt)
            return;

        if (!interactor.TryGetComponent(out Inventory inventory))
            return;

        if (!inventory.TryAddItem(_item.Id, _amount))
            return;

        _readyAt = NetworkTime.time + _cooldown;

        if (_destroyWhenTaken)
            NetworkServer.Destroy(gameObject);
    }
}

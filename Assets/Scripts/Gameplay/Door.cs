using Mirror;
using UnityEngine;

public class Door : NetworkBehaviour, IInteractable
{
    [SerializeField] private Transform _door;

    [SyncVar(hook = nameof(OnOpenChanged))]
    private bool _isOpen;

    private BoxCollider _boxCollider;

    public string InteractionPrompt => _isOpen ? "close the door" : "open the door";

    private void Awake()
    {
        _boxCollider = GetComponent<BoxCollider>();
    }

    [Server]
    public void Interact(GameObject interactor)
    {
        _isOpen = !_isOpen;
    }

    private void OnOpenChanged(bool oldValue, bool newValue)
    {
        if (_door != null)
            _door.localRotation = Quaternion.Euler(0f, newValue ? -130f : 0f, 0f);

        if (_boxCollider != null)
            _boxCollider.isTrigger = newValue;
    }
}

using UnityEngine;
using Mirror;
public class Door : NetworkBehaviour, IInteractable
{
    
    [SyncVar(hook = nameof(OnOpenChanged))]
    private bool isOpen;
    [SerializeField] private Transform door;
    private BoxCollider boxCollider;
    public string InteractionPrompt => isOpen ? "close the door" : "open the door";

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();
    }
    [Server]
    public void Interact(NetworkIdentity interactor)
    {
        isOpen = !isOpen;
    }

    private void OnOpenChanged(bool oldValue, bool newValue)
    {
        door.localRotation = Quaternion.Euler(0, newValue ? -130 : 0, 0);
        boxCollider.isTrigger = newValue;
    }
}
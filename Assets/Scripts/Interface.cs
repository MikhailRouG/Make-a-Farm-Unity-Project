using Mirror;

public interface IInteractable
{
    string InteractionPrompt { get; }
     void Interact(NetworkIdentity interactor);
}
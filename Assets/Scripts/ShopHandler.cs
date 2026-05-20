using UnityEngine;
using Zenject;

public class ShopHandler : MonoBehaviour, IInteractable
{
    private ShopBuilderUi _shopUi;

    public string InteractionPrompt => "Open shop";
    [Inject]
    public void Construct(ShopBuilderUi shopUi)
    {
        _shopUi = shopUi;
    }
    public void Interact(GameObject interactor)
    {
        var shopServer = interactor.GetComponent<PlayerShopServer>();

        if (shopServer != null)
        {
            _shopUi.OpenShop(shopServer);
        }
    }
}

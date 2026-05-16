using UnityEngine;
using Zenject;

public class ShopInstaller : MonoInstaller
{
    [SerializeField] private ShopBuilderUi _shopUiPrefab;
    [SerializeField] private ShopCardUi _cardPrefab;

    public override void InstallBindings()
    {
        Container.BindFactory<PlayerShopServer, ShopBuilderUi, ShopUiFactory>()
                    .FromComponentInNewPrefab(_shopUiPrefab);

        Container.BindFactory<ItemConfig, ShopCardUi, ShopCardUi.Factory>()
            .FromComponentInNewPrefab(_cardPrefab);
    }
    public class ShopUiFactory : PlaceholderFactory<PlayerShopServer, ShopBuilderUi> { }
}
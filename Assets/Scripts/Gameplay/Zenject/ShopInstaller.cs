using UnityEngine;
using Zenject;

namespace Gameplay.Player.UI.Shop
{
    public class ShopInstaller : MonoInstaller
    {
        [SerializeField] private ShopBuilderUi _shopUiInstance;
        [SerializeField] private ShopCardUi _cardPrefab;
        public override void InstallBindings()
        {
            Container.Bind<ShopBuilderUi>().FromInstance(_shopUiInstance).AsSingle().NonLazy();
            Container.Bind<ShopHandler>()
                .FromComponentsInHierarchy()
                .AsCached()
                .NonLazy();
            Container.BindFactory<ShopCardUi, ShopCardUi.Factory>()
                .FromComponentInNewPrefab(_cardPrefab);
        }
    }
}
using UnityEngine;
using Zenject;

public class GameplaySceneInstaller : MonoInstaller
{
    [SerializeField] private ItemDatabase _itemDatabase;

    public override void InstallBindings()
    {
        //   Container.BindInstance(_itemDatabase).AsSingle().NonLazy(); ;
        Container.Bind<ItemDatabase>().FromInstance(_itemDatabase).AsSingle();
    }
}

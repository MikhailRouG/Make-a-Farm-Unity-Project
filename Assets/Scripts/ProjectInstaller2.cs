using UnityEngine;
using Zenject;

public class ProjectInstaller2 : MonoInstaller
{
    [SerializeField] private ItemDatabase _itemDatabase;

    public override void InstallBindings()
    {
        Container.Bind<ItemDatabase>()
            .FromInstance(_itemDatabase)
            .AsSingle();
    }
}

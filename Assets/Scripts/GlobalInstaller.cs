using UnityEngine;
using Zenject;
public class GlobalInstaller : MonoInstaller
{
    [SerializeField] private ItemDatabase _itemDatabase;

    public override void InstallBindings()
    {
        Container.BindInstance(_itemDatabase).AsSingle().NonLazy();
    }
}

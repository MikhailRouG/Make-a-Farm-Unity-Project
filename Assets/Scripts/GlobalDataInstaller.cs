using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "GlobalDataInstaller", menuName = "Installers/GlobalDataInstaller")]
public class GlobalDataInstaller : ScriptableObjectInstaller<GlobalDataInstaller>
{
    [SerializeField] private ItemDatabase _itemDatabase;

    public override void InstallBindings()
    {
        Container.BindInstance(_itemDatabase).AsSingle().NonLazy();
    }
}

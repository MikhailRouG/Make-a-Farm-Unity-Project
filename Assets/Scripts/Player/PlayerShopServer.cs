using Mirror;
using Zenject;

public class PlayerShopServer : NetworkBehaviour
{
     private ItemDatabase _database;
    private Inventory _inventory;
    [Inject]
    public void Construct(ItemDatabase database)
    {
        _database = database;
    }
    private void Awake()
    {
        if (_database == null)
        {
            var container = ProjectContext.Instance.Container;
            container.Inject(this);
        }
        _inventory = GetComponent<Inventory>();
    }
    [Command]
    public void CmdBuyItem(int itemId)
    {
        TryBuyItem(itemId);
    }

    [Server]
    private void TryBuyItem(int itemId)
    {
        if (_database == null)
        {
            UnityEngine.Debug.LogError("[SERVER] Ошибка покупки: _database равен null на сервере!");
            return;
        }

        if (_inventory == null)
        {
            UnityEngine.Debug.LogError("[SERVER] Ошибка покупки: Компонент Inventory не найден!");
            return;
        }
        _inventory.TryAddItem(itemId);
    }
}

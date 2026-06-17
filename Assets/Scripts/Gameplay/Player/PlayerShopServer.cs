using Mirror;
using Zenject;

namespace Gameplay.Player
{ 
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
            UnityEngine.Debug.LogError("[SERVER] Error Buy: _database null!");
            return;
        }

        if (_inventory == null)
        {
            UnityEngine.Debug.LogError("[SERVER] Error Buy:  Inventory didnt found!");
            return;
        }
        _inventory.TryAddItem(itemId);
    }
}
}
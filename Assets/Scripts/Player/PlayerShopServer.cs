using Mirror;
using Zenject;
using static ShopInstaller;

public class PlayerShopServer : NetworkBehaviour
{
     private ItemDatabase _database;
    private Inventory _inventory;
    private ShopUiFactory _shopUiFactory;
    [Inject]
    public void Construct(ItemDatabase database, ShopUiFactory shopUiFactory)
    {
        _database = database;
        _shopUiFactory = shopUiFactory;
    }
    private void Awake()
    {
            _inventory = GetComponent<Inventory>();
    }
    public override void OnStartLocalPlayer()
    {
        var sceneContext = FindFirstObjectByType<SceneContext>();
        if (sceneContext != null && sceneContext.Container != null)
        {
            sceneContext.Container.Inject(this);
        }
        _shopUiFactory.Create(this);
    }

    [Command]
    public void CmdBuyItem(int itemId)
    {
        TryBuyItem(itemId);
    }

    [Server]
    private void TryBuyItem(int itemId)
    {
        if (_database == null || _inventory == null) return;
        _inventory.TryAddItem(itemId);
    }
}

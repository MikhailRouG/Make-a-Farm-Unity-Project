using Mirror;
using UnityEngine;

public class PlayerShopServer : NetworkBehaviour
{
    [SerializeField] private ItemDatabase _database;
    private Inventory _inventory;

    [SerializeField] private ShopBuilderUi _shopPrefab;
    private void Awake()
    {
        if (_inventory == null)
            _inventory = GetComponent<Inventory>();
    }
    public override void OnStartLocalPlayer()
    {
        CreateShopUi();
    }
    private void CreateShopUi()
    {
        if (!isLocalPlayer)
            return;

        if (_shopPrefab == null)
        {
            Debug.LogError("Shop prefab is missing", this);
            return;
        }
        ShopBuilderUi _shopView = Instantiate(_shopPrefab);
        _shopView.Initialize(this);
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
            return;
        }

        if (_inventory == null)
        {
            return;
        }
        _inventory.TryAddItem(itemId);
    }
}

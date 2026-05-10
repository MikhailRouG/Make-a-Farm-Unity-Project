using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class ShopBuilderUi : MonoBehaviour
{
    private PlayerShopServer _playerShopServer;
    private Inventory _playerInventory;
    [SerializeField] private ItemDatabase _database;
    [SerializeField] private ShopCardUi _itemCard;
    private List<ShopCardUi> _cards = new() ;
    [SerializeField] private Transform _cardParent;
    private void OnEnable()
    {
        BuildShop();
    }
    private void OnDisable()
    {
        ClearShop();
    }
    public void Initialize(PlayerShopServer shopServer)
    {
        _playerShopServer = shopServer;
    }
    private void BuildShop()
    {
        var items = _database.GetAllItem();
        foreach (var item in items)
        {
            ShopCardUi card = Instantiate(_itemCard, _cardParent);
            card.Build(item);
            card.OnClick += OnSelectedItem;
            _cards.Add(card);
        }
    }
    private void ClearShop()
    {
        foreach (var card in _cards)
        {
            if (card == null)
                continue;

           card.OnClick -= OnSelectedItem;
            Destroy(card.gameObject);
        }

        _cards.Clear();
    }
    public void OnSelectedItem(int id)
    {
        _playerShopServer.CmdBuyItem(id);
    }
}

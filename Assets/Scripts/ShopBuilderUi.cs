using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Zenject;
public class ShopBuilderUi : MonoBehaviour
{
    private PlayerShopServer _playerShopServer;
   private ItemDatabase _database;
    private ShopCardUi.Factory _cardFactory;
    private List<ShopCardUi> _cards = new() ;
    [SerializeField] private Transform _cardParent;

    [Inject]
    public void Construct(PlayerShopServer playerShopServer, ItemDatabase database, ShopCardUi.Factory cardFactory)
    {
        _playerShopServer = playerShopServer;
        _database = database;
        _cardFactory = cardFactory;
    }
    private void OnEnable()
    {
        if (_database != null)
            BuildShop();
    }
    private void OnDisable()
    {
        ClearShop();
    }
    private void BuildShop()
    {
        var items = _database.GetAllItem();
        foreach (var item in items)
        {
            ShopCardUi card = _cardFactory.Create(item);
            card.transform.SetParent(_cardParent, false);
            card.Build(item);
            card.OnClick += OnSelectedItem;
            _cards.Add(card);
        }
    }
    private void ClearShop()
    {
        foreach (var card in _cards)
        {
            if (card == null)continue;
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

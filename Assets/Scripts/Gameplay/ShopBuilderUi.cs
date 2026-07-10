using System.Collections.Generic;
using UnityEngine;
using Zenject;
namespace Gameplay.Player.UI.Shop
{ 
    public class ShopBuilderUi : MonoBehaviour
    {
        private PlayerShopServer _playerShopServer;
        private ItemDatabase _database;
        private ShopCardUi.Factory _cardFactory;
        private List<ShopCardUi> _cards = new();
        [SerializeField] private Transform _cardParent;


        [Inject]
        public void Construct(ItemDatabase database, ShopCardUi.Factory cardFactory)
        {
            _database = database;
            if (_database == null)
            {
                Debug.LogError("[ShopBuilderUi] _database = null!");
            }
            _cardFactory = cardFactory;
        }
        public void OpenShop(PlayerShopServer buyer)
        {
            _playerShopServer = buyer;
            gameObject.SetActive(true);
            BuildShop();
        }
        public void CloseShop()
        {
            ClearShop();
            _playerShopServer = null;
            gameObject.SetActive(false);
        }
        private void BuildShop()
        {
            ClearShop();
            if (_database == null)
            {
                Debug.LogError("[ShopBuilderUi] _database равен null!");
                return;
            }
            var items = _database.GetAllItem();
            foreach (var item in items)
            {
                ShopCardUi card = _cardFactory.Create();
                card.transform.SetParent(_cardParent, false);
                card.Build(item);
                card.OnBuyClick += OnBuyItem;
                card.OnSellClick += OnSellItem;
                _cards.Add(card);
            }
        }
        private void ClearShop()
        {
            foreach (var card in _cards)
            {
                if (card == null) continue;
                card.OnBuyClick -= OnBuyItem;
                card.OnSellClick -= OnSellItem;
                Destroy(card.gameObject);
            }
            _cards.Clear();
        }
        private void OnBuyItem(int id)
        {
            if (_playerShopServer == null) return;
            _playerShopServer.CmdBuyItem(id);
        }

        private void OnSellItem(int id)
        {
            if (_playerShopServer == null) return;
            _playerShopServer.CmdSellItem(id);
        }
    }
}
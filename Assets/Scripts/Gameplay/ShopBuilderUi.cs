using System.Collections.Generic;
using UnityEngine;
using Zenject;
namespace Gameplay.Player.UI.Shop
{ 
    public class ShopBuilderUi : MonoBehaviour, ICloseableUi
    {
        private PlayerShopServer _playerShopServer;
        private ItemDatabase _database;
        private ShopCardUi.Factory _cardFactory;
        private List<ShopCardUi> _cards = new();
        [SerializeField] private Transform _cardParent;

        [SerializeField] private ShopTabUi[] _tabs;
        [SerializeField] private ShopCategory _defaultCategory = ShopCategory.Seed;

        [Inject]
        public void Construct(ShopCardUi.Factory cardFactory)
        {
            _cardFactory = cardFactory;
        }
        private void OnValidate()
        {
            _tabs ??= GetComponentsInChildren<ShopTabUi>();
        }
        private void Awake()
        {
            _database = ItemDatabase.Instance;
            foreach (var tab in _tabs)
                tab.OnSelected += ShowCategory;
        }
        public void OpenShop(PlayerShopServer buyer)
        {
            Cursor.visible = true;
            _playerShopServer = buyer;
            gameObject.SetActive(true);
            ShowCategory(_defaultCategory);
            UiManager.Instance?.Register(this);
        }
        public void CloseShop()
        {
            Cursor.visible = false;
            ClearShop();
            _playerShopServer = null;
            gameObject.SetActive(false);
            SelectTab(_defaultCategory);
            UiManager.Instance?.Unregister(this);
        }
        public void Close() => CloseShop();
        private void SelectTab(ShopCategory category)
        {
            foreach (var tab in _tabs)
                tab.SetSelectedSilently(tab.Category == category);

            ShowCategory(category);
        }
        private void ShowCategory(ShopCategory category)
        {
            BuildShop(_database.GetByCategory(category));
        }

        private void BuildShop(IReadOnlyList<ItemConfig> items)
        {
            ClearShop();
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
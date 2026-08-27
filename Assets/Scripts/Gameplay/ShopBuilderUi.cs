using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Gameplay.Player.UI.Shop
{
    public class ShopBuilderUi : MonoBehaviour, ICloseableUi
    {
        [SerializeField] private Transform _cardParent;
        [SerializeField] private ShopTabUi[] _tabs;
        [SerializeField] private ItemType _defaultCategory = ItemType.Seed;

        private PlayerShopServer _playerShopServer;
        private ItemDatabase _database;
        private ShopCardUi.Factory _cardFactory;
        private readonly List<ShopCardUi> _cards = new();

        [Inject]
        public void Construct(ShopCardUi.Factory cardFactory)
        {
            _cardFactory = cardFactory;
        }

        private void OnValidate()
        {
            if (_tabs == null || _tabs.Length == 0)
                _tabs = GetComponentsInChildren<ShopTabUi>(true);
        }

        private void Awake()
        {
            _database = ItemDatabase.Instance;

            if (_tabs == null)
                return;

            foreach (ShopTabUi tab in _tabs)
            {
                if (tab != null)
                    tab.OnSelected += ShowCategory;
            }
        }

        private void OnDestroy()
        {
            if (_tabs == null)
                return;

            foreach (ShopTabUi tab in _tabs)
            {
                if (tab != null)
                    tab.OnSelected -= ShowCategory;
            }
        }

        public void OpenShop(PlayerShopServer buyer)
        {
            _playerShopServer = buyer;
            gameObject.SetActive(true);
            SelectTab(_defaultCategory);
            UiManager.Instance?.Register(this);
        }

        public void CloseShop()
        {
            ClearShop();
            _playerShopServer = null;
            gameObject.SetActive(false);
            UiManager.Instance?.Unregister(this);
        }

        public void Close() => CloseShop();

        private void SelectTab(ItemType category)
        {
            if (_tabs != null)
            {
                foreach (ShopTabUi tab in _tabs)
                {
                    if (tab != null)
                        tab.SetSelectedSilently(tab.Category == category);
                }
            }

            ShowCategory(category);
        }

        private void ShowCategory(ItemType category)
        {
            if (_database == null)
                return;

            BuildShop(_database.GetByCategory(category));
        }

        private void BuildShop(IReadOnlyList<ItemConfig> items)
        {
            ClearShop();

            foreach (ItemConfig item in items)
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
            foreach (ShopCardUi card in _cards)
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
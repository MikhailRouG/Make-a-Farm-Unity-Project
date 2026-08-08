using Mirror;
using UnityEngine;

namespace Gameplay.Player
{
    public class PlayerShopServer : NetworkBehaviour
    {
        private ItemDatabase _database;
        private Inventory _inventory;

        private void Awake()
        {
            _database = ItemDatabase.Instance;
            _inventory = GetComponent<Inventory>();
        }

        [Command]
        public void CmdBuyItem(int itemId)
        {
            TryBuyItem(itemId);
        }

        [Command]
        public void CmdSellItem(int itemId)
        {
            TrySellItem(itemId);
        }

        [Server]
        private void TryBuyItem(int itemId)
        {
            if (!IsReady("Buy"))
                return;

            ItemConfig item = _database.Get(itemId);

            if (item == null)
                return;

            if (_inventory.TrySpendMoney(item.Price))
            {
                // Refund when the inventory turned out to be full, so the price
                // is never charged without handing the item over.
                if (!_inventory.TryAddItem(itemId))
                    _inventory.TryAddMoney(item.Price);
            }
        }

        [Server]
        private void TrySellItem(int itemId)
        {
            if (!IsReady("Sell"))
                return;

            ItemConfig item = _database.Get(itemId);

            if (item == null)
                return;

            if (!_inventory.TryGetItemSize(itemId, out float size))
                return;

            if (!_inventory.TryRemoveItem(itemId))
                return;

            int payout = Mathf.RoundToInt(item.Price * size);
            _inventory.TryAddMoney(payout);
        }

        [Server]
        private bool IsReady(string operation)
        {
            if (_database == null)
            {
                Debug.LogError($"[SERVER] {operation} failed: ItemDatabase is null.", this);
                return false;
            }

            if (_inventory == null)
            {
                Debug.LogError($"[SERVER] {operation} failed: Inventory not found.", this);
                return false;
            }

            return true;
        }
    }
}

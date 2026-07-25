using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu]
public class ItemDatabase : ScriptableObject
{
    private static ItemDatabase _instance;
    public static ItemDatabase Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<ItemDatabase>("ItemDatabase");

                if (_instance == null)
                    Debug.LogError("ItemDatabase.asset not found in Resources!");
            }

            return _instance;
        }
    }

    public List<ItemConfig> items;

    private Dictionary<int, ItemConfig> _map;
    private Dictionary<ShopCategory, List<ItemConfig>> _byCategory;

    public void Init()
    {
        _map = new Dictionary<int, ItemConfig>();
        _byCategory = new Dictionary<ShopCategory, List<ItemConfig>>();

        foreach (var item in items)
        {
            _map[item.Id] = item;
            if (!_byCategory.TryGetValue(item.Category, out var list))
            {
                list = new List<ItemConfig>();
                _byCategory[item.Category] = list;
            }
            list.Add(item);
        }
    }

    public ItemConfig Get(int id)
    {
        if (_map == null) Init();

        if (_map != null && _map.TryGetValue(id, out var item))
        {
            return item;
        }

        Debug.LogError($"ItemDatabase: item with ID {id} didn't found!");
        return null;
    }
    public IReadOnlyList<ItemConfig> GetAllItem() { return items; }
    public IReadOnlyList<ItemConfig> GetByCategory(ShopCategory category)
    {
        if (_byCategory == null) Init();
        return _byCategory.TryGetValue(category, out var list)
            ? list
            : System.Array.Empty<ItemConfig>();
    }
}

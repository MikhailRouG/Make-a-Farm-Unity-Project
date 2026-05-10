using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu]
public class ItemDatabase : ScriptableObject
{
    public static ItemDatabase Instance;
    public List<ItemConfig> items;

    private Dictionary<int, ItemConfig> _map;

    public void Init()
    {
        Instance = this;

        _map = new Dictionary<int, ItemConfig>();

        foreach (var item in items)
        {
            _map[item.Id] = item;
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
}

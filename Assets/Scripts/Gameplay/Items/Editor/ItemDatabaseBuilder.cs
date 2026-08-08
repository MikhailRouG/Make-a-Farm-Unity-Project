#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Linq;

public static class ItemDatabaseBuilder
{
    [MenuItem("Tools/Items/Rebuild Database")]
    public static void Rebuild()
    {
        ItemDatabase db = FindDatabase();
        if (db == null) return;

        db.items = AssetDatabase
            .FindAssets("t:ItemConfig")
            .Select(g =>
                AssetDatabase.LoadAssetAtPath<ItemConfig>(
                    AssetDatabase.GUIDToAssetPath(g)))
            .ToList();

        // The lookup dictionaries live on the ScriptableObject and survive the
        // rebuild, so drop them or Get() keeps serving the old set.
        db.Invalidate();

        EditorUtility.SetDirty(db);
        AssetDatabase.SaveAssets();

        Debug.Log($"ItemDatabase rebuilt: {db.items.Count} items");
    }

    static ItemDatabase FindDatabase()
    {
        string guid = AssetDatabase.FindAssets("t:ItemDatabase").FirstOrDefault();
        if (guid == null)
        {
            Debug.LogError("ItemDatabase not found");
            return null;
        }

        return AssetDatabase.LoadAssetAtPath<ItemDatabase>(
            AssetDatabase.GUIDToAssetPath(guid));
    }
}
#endif

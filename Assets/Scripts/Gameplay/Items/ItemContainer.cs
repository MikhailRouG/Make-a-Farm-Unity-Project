using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Item/Care Container")]
public class ItemContainer : ItemTool
{
    [Header("Container")]
    [field: SerializeField] public RefillRecipe[] Fills { get; private set; }

    public bool TryGetFilled(FillResource resource, out ItemConfig filled)
    {
        filled = null;

        if (resource == FillResource.None || Fills == null)
            return false;

        for (int i = 0; i < Fills.Length; i++)
        {
            if (Fills[i].Resource != resource || Fills[i].Result == null)
                continue;

            filled = Fills[i].Result;
            return true;
        }

        return false;
    }
}

[Serializable]
public struct RefillRecipe
{
    public FillResource Resource;
    public ItemConfig Result;
}

public enum FillResource
{
    None = 0,
    Water = 1,
}

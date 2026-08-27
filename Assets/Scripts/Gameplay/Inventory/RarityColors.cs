using UnityEngine;

[CreateAssetMenu(fileName = "RarityColors", menuName = "Game/UI/Rarity Colors")]
public class RarityColors : ScriptableObject
{
    private static RarityColors _instance;

    [Tooltip("One colour per ItemRarity, in the order the enum declares them.")]
    [SerializeField] private Color[] _colors;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => _instance = null;

    public static Color Of(ItemRarity rarity)
    {
        if (_instance == null)
            _instance = Resources.Load<RarityColors>(nameof(RarityColors));

        return _instance != null ? _instance.Get(rarity) : Color.white;
    }

    public Color Get(ItemRarity rarity)
    {
        int index = (int)rarity;

        return _colors != null && index >= 0 && index < _colors.Length
            ? _colors[index]
            : Color.white;
    }
}

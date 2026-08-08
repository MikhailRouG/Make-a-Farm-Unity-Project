using Gameplay.Farm;
using UnityEngine;

[CreateAssetMenu(
    fileName = "Effect Config",
    menuName = "Game/Effects/Effect Config"
)]
public class EffectConfig : ScriptableObject
{
    [SerializeField] private EffectEntry[] _effects;

    public bool TryGetEffect(EffectState state, out EffectEntry effect)
    {
        effect = null;

        if (_effects == null)
            return false;

        foreach (EffectEntry entry in _effects)
        {
            if (entry != null && entry.State == state)
            {
                effect = entry;
                return true;
            }
        }

        return false;
    }
}

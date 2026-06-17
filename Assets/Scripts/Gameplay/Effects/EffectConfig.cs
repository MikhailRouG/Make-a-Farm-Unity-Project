using UnityEngine;
using Gameplay.Farm;
[CreateAssetMenu(
    fileName = "Effect Config",
    menuName = "Game/Effects/Effect Config"
)]
public class EffectConfig : ScriptableObject
{
    [SerializeField] private EffectEntry[] _effects;

    public bool TryGetEffect(EffectState state, out EffectEntry effect)
    {
        foreach (EffectEntry entry in _effects)
        {
            if (entry.State == state)
            {
                effect = entry;
                return true;
            }
        }

        effect = null;
        return false;
    }
}

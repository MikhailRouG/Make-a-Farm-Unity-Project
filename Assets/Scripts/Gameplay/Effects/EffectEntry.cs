using System;
using UnityEngine;

namespace Gameplay.Farm
{
    public enum EffectState
    {
        Start,
        Upgrade,
        Destroy,
        Collect,
        Hit,
        Grow,
        Harvest
    }

    [Serializable]
    public class EffectEntry
    {
        [field: SerializeField] public EffectState State { get; private set; }
        [field: SerializeField] public GameObject Prefab { get; private set; }
        [field: SerializeField] public float Lifetime { get; private set; } = 2f;
    }
}

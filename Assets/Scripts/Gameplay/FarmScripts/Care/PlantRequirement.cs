using UnityEngine;

namespace Gameplay.Farm
{

    [CreateAssetMenu(fileName = "Plant Requirement", menuName = "Game/Farm/Plant Requirement")]
    public class PlantRequirement : ScriptableObject
    {
        [field: SerializeField] public string Prompt { get; private set; } = "Tend the plant";
        [field: SerializeField] public ItemConfig RequiredItem { get; private set; }
        [field: SerializeField] public float FirstDueAfter { get; private set; } = 30f;
        [field: SerializeField] public float RepeatEvery { get; private set; } = 60f;
        [field: SerializeField] public float DamagePerTick { get; private set; } = 5f;
        [field: SerializeField] public float HealthRestored { get; private set; } = 25f;
        [field: SerializeField] public bool ConsumeItem { get; private set; }

        public bool Matches(int itemId) => RequiredItem != null && RequiredItem.Id == itemId;
    }
}

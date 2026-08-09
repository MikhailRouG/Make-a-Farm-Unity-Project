using UnityEngine;

namespace Gameplay.Farm
{
    public enum CareType : byte
    {
        Water = 0,
        Fertilize = 1,
        Weed = 2,
        Prune = 3,
    }
    [CreateAssetMenu(fileName = "Care Requirement", menuName = "Game/Farm/Care Requirement")]
    public class CareRequirement : ScriptableObject
    {
        [field: SerializeField] public CareType Type { get; private set; }

        [field: SerializeField] public string Prompt { get; private set; } = "Tend the plant";

        [field: Tooltip("Item the player must hold in the selected slot to meet this need.")]
        [field: SerializeField] public ItemCare RequiredItem { get; private set; }

        [field: Tooltip("Seconds after planting before the need comes up for the first time.")]
        [field: SerializeField] public float FirstDueAfter { get; private set; } = 30f;

        [field: Tooltip("Seconds until the need comes up again after it has been met.")]
        [field: SerializeField] public float RepeatEvery { get; private set; } = 60f;

        public bool Matches(int itemId) => RequiredItem != null && RequiredItem.Id == itemId;
    }
}

using System.Collections;
using Gameplay.Player;
using Mirror;
using UnityEngine;

namespace Gameplay.Farm
{
    [RequireComponent(typeof(Plant))]
    public class PlantCare : NetworkBehaviour, IInteractable
    {
        private const float TickSeconds = 0.5f;

        // One mask bit per requirement, so anything past 32 could not be tracked.
        private const int MaxRequirements = 32;

        [SyncVar] private float _health;

        // Bit i is set while Seed.Requirements[i] is overdue. Indexed by position and
        // not by CareType, so one plant can demand two needs of the same kind.
        [SyncVar] private uint _overdueMask;

        private Plant _plant;
        private Coroutine _routine;

        // Server only, indexed like Seed.Requirements.
        private double[] _nextDueTime;

        public bool NeedsCare => _overdueMask != 0u;

        public float HealthFraction
        {
            get
            {
                ItemSeed seed = Seed;

                if (seed == null || seed.MaxHealth <= 0f)
                    return 1f;

                return Mathf.Clamp01(_health / seed.MaxHealth);
            }
        }

        public string InteractionPrompt
        {
            get
            {
                CareRequirement overdue = FirstOverdueRequirement();
                return overdue != null ? overdue.Prompt : string.Empty;
            }
        }

        private ItemSeed Seed => _plant != null ? _plant.Seed : null;

        private void Awake()
        {
            _plant = GetComponent<Plant>();
        }

        private void OnDestroy()
        {
            if (_routine != null) StopCoroutine(_routine);
        }

        public override void OnStartServer()
        {
            base.OnStartServer();

            ItemSeed seed = Seed;

            if (seed != null && seed.HasRequirements)
                _health = seed.MaxHealth;

            _routine = StartCoroutine(CareRoutine());
        }

        public bool IsOverdue(int index) =>
            index >= 0 && index < MaxRequirements && (_overdueMask & (1u << index)) != 0u;

        public bool IsOverdue(CareType type)
        {
            ItemSeed seed = Seed;

            if (seed == null || !seed.HasRequirements)
                return false;

            int count = Mathf.Min(seed.Requirements.Length, MaxRequirements);

            for (int i = 0; i < count; i++)
            {
                CareRequirement requirement = seed.Requirements[i];

                if (requirement != null && requirement.Type == type && IsOverdue(i))
                    return true;
            }

            return false;
        }

        [Server]
        public void Interact(GameObject interactor)
        {
            ItemSeed seed = Seed;

            if (seed == null || _nextDueTime == null)
                return;

            if (!interactor.TryGetComponent(out Inventory inventory)) return;
            if (!interactor.TryGetComponent(out PlayerInventory playerInventory)) return;

            int slotIndex = playerInventory.SelectedSlotIndex;
            InventorySlot slot = inventory.GetSlotServer(slotIndex);

            if (slot.IsEmpty)
                return;

            // Only an overdue need can be closed, otherwise the tool could be spammed
            // to keep health topped up and the timers permanently pushed away.
            int index = FindOverdueFor(seed, slot.ItemId);

            if (index < 0)
                return;

            CareRequirement requirement = seed.Requirements[index];
            ItemCare tool = requirement.RequiredItem;

            _nextDueTime[index] = NetworkTime.time + requirement.RepeatEvery;
            _health = Mathf.Min(seed.MaxHealth, _health + tool.HealthRestored);

            RefreshOverdueMask(seed);

            if (tool.ConsumedOnUse)
                inventory.RemoveItemFromSlot(slotIndex, 1);
        }

        [Server]
        private int FindOverdueFor(ItemSeed seed, int itemId)
        {
            int count = Mathf.Min(seed.Requirements.Length, MaxRequirements);

            for (int i = 0; i < count; i++)
            {
                CareRequirement requirement = seed.Requirements[i];

                if (requirement != null && requirement.Matches(itemId) && IsOverdue(i))
                    return i;
            }

            return -1;
        }

        [Server]
        private IEnumerator CareRoutine()
        {
            if (Seed == null)
                yield return new WaitUntil(() => _plant == null || _plant.Seed != null);

            ItemSeed seed = Seed;

            if (seed == null || !seed.HasRequirements)
                yield break;

            if (seed.Requirements.Length > MaxRequirements)
            {
                Debug.LogError(
                    $"[PlantCare] {seed.name}: {seed.Requirements.Length} requirements, " +
                    $"only the first {MaxRequirements} can be tracked.", this);
            }

            int count = Mathf.Min(seed.Requirements.Length, MaxRequirements);

            _health = seed.MaxHealth;
            _nextDueTime = new double[seed.Requirements.Length];

            double now = NetworkTime.time;
            for (int i = 0; i < count; i++)
            {
                CareRequirement requirement = seed.Requirements[i];
                _nextDueTime[i] = now + (requirement != null ? requirement.FirstDueAfter : float.MaxValue);
            }

            WaitForSeconds tick = new WaitForSeconds(TickSeconds);

            while (!_plant.IsFullyGrown)
            {
                yield return tick;

                Tick(seed, TickSeconds);

                if (_health <= 0f)
                {
                    _plant.Kill();
                    yield break;
                }
            }

            _overdueMask = 0u;
            _routine = null;
        }

        [Server]
        private void Tick(ItemSeed seed, float deltaTime)
        {
            int overdueCount = RefreshOverdueMask(seed);

            float change = overdueCount > 0
                ? -seed.DrainPerSecond * overdueCount * deltaTime
                : seed.RegenPerSecond * deltaTime;

            _health = Mathf.Clamp(_health + change, 0f, seed.MaxHealth);
        }

        [Server]
        private int RefreshOverdueMask(ItemSeed seed)
        {
            double now = NetworkTime.time;
            uint mask = 0u;
            int overdueCount = 0;
            int count = Mathf.Min(seed.Requirements.Length, MaxRequirements);

            for (int i = 0; i < count; i++)
            {
                CareRequirement requirement = seed.Requirements[i];

                if (requirement == null || now < _nextDueTime[i])
                    continue;

                mask |= 1u << i;
                overdueCount++;
            }

            _overdueMask = mask;
            return overdueCount;
        }

        private CareRequirement FirstOverdueRequirement()
        {
            ItemSeed seed = Seed;

            if (seed == null || _overdueMask == 0u || !seed.HasRequirements)
                return null;

            int count = Mathf.Min(seed.Requirements.Length, MaxRequirements);

            for (int i = 0; i < count; i++)
            {
                if (seed.Requirements[i] != null && IsOverdue(i))
                    return seed.Requirements[i];
            }

            return null;
        }
    }
}

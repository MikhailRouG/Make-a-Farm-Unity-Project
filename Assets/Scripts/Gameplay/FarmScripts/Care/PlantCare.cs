using System;
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

        [SyncVar] private float _health;

        [SyncVar(hook = nameof(OnOverdueMaskChanged))]
        private uint _overdueMask;

        private Plant _plant;
        private Coroutine _routine;
        private double[] _nextDueTime;
        public bool NeedsCare => _overdueMask != 0u;

        public event Action<PlantRequirement, bool> OnChangedReqirement;
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
                PlantRequirement overdue = FirstOverdueRequirement();
                return overdue != null ? overdue.Prompt : string.Empty;
            }
        }

        public PlantRequirement CurrentNeed => FirstOverdueRequirement();

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

            // Full health even when the seed asks for no care: leaving it at zero
            // synced a dying plant to every client and showed "HP 0%" on the label.
            if (seed != null)
                _health = seed.MaxHealth;

            _routine = StartCoroutine(CareRoutine());
        }

        // Mirror calls this only when the value really changed, so subscribers wake
        // up when a need appears or is met, not once per frame.
        private void OnOverdueMaskChanged(uint oldMask, uint newMask)
        {
            OnChangedReqirement?.Invoke(CurrentNeed, NeedsCare);
        }

        /// <summary>
        /// Check by byte
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool IsOverdue(int index) =>
            index >= 0  && (_overdueMask & (1u << index)) != 0u;

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

            int index = CheckItem(seed, slot.ItemId);

            if (index < 0)
                return;

            PlantRequirement requirement = seed.Requirements[index];

            _nextDueTime[index] = NetworkTime.time + requirement.RepeatEvery;
            _health = Mathf.Min(seed.MaxHealth, _health + requirement.HealthRestored);

            RefreshOverdueMask(seed);

            if (requirement.ConsumeItem)
                inventory.RemoveItemFromSlot(slotIndex, 1);
        }

        [Server]
        private int CheckItem(ItemSeed seed, int itemId)
        {
            int count = seed.Requirements.Length;

            for (int i = 0; i < count; i++)
            {
                PlantRequirement requirement = seed.Requirements[i];

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

            int count = seed.Requirements.Length;

            _health = seed.MaxHealth;
            _nextDueTime = new double[seed.Requirements.Length];

            double now = NetworkTime.time;
            for (int i = 0; i < count; i++)
            {
                PlantRequirement requirement = seed.Requirements[i];
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
                ? seed.DrainPerSecond * overdueCount * deltaTime * -1 //Damage
                : seed.RegenPerSecond * deltaTime; // Heal

            _health = Mathf.Clamp(_health + change, 0f, seed.MaxHealth);
        }

        [Server]
        private int RefreshOverdueMask(ItemSeed seed)
        {
            double now = NetworkTime.time;
            uint mask = 0u;
            int overdueCount = 0;
            int count = seed.Requirements.Length;

            for (int i = 0; i < count; i++)
            {
                PlantRequirement requirement = seed.Requirements[i];

                if (requirement == null || now < _nextDueTime[i])
                    continue;

                mask |= 1u << i;
                overdueCount++;
            }

            _overdueMask = mask;
            return overdueCount;
        }

        private PlantRequirement FirstOverdueRequirement()
        {
            ItemSeed seed = Seed;

            if (seed == null || _overdueMask == 0u || !seed.HasRequirements)
                return null;

            int count = seed.Requirements.Length;

            for (int i = 0; i < count; i++)
            {
                if (seed.Requirements[i] != null && IsOverdue(i))
                    return seed.Requirements[i];
            }

            return null;
        }
    }
}

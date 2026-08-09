using System.Collections.Generic;
using NUnit.Framework;

public class InventoryModelTests
{
    private const int Stackable = 1;
    private const int Unique = 2;

    private sealed class FakeRules : IItemStackRules
    {
        private readonly Dictionary<int, ItemStackRule> _rules = new()
        {
            { Stackable, new ItemStackRule(true, 32) },
            { Unique,    new ItemStackRule(false, 1) },
        };

        public bool TryGet(int itemId, out ItemStackRule rule) => _rules.TryGetValue(itemId, out rule);
    }

    private static List<InventorySlot> EmptySlots(int count)
    {
        var slots = new List<InventorySlot>();
        for (int i = 0; i < count; i++) slots.Add(InventorySlot.Empty);
        return slots;
    }

    [Test]
    public void TryAdd_StacksOntoAnExistingStack()
    {
        var slots = EmptySlots(2);
        var model = new InventoryModel(slots, new FakeRules());

        model.TryAdd(Stackable, 10);
        model.TryAdd(Stackable, 5);

        Assert.AreEqual(15, slots[0].Amount);
        Assert.IsTrue(slots[1].IsEmpty);
    }

    [Test]
    public void TryAdd_SpillsIntoTheNextSlotWhenAStackIsFull()
    {
        var slots = EmptySlots(2);
        var model = new InventoryModel(slots, new FakeRules());

        Assert.IsTrue(model.TryAdd(Stackable, 40));

        Assert.AreEqual(32, slots[0].Amount);
        Assert.AreEqual(8, slots[1].Amount);
    }

    [Test]
    public void TryAdd_WhenSpaceRunsOut_LeavesTheInventoryUntouched()
    {
        var slots = EmptySlots(1);
        var model = new InventoryModel(slots, new FakeRules());

        Assert.IsFalse(model.TryAdd(Stackable, 40));
        Assert.IsTrue(slots[0].IsEmpty, "a failed add must not put anything in");
    }

    [Test]
    public void TryAdd_NonStackableTakesOneSlotEach()
    {
        var slots = EmptySlots(3);
        var model = new InventoryModel(slots, new FakeRules());

        Assert.IsTrue(model.TryAdd(Unique, 3));

        Assert.AreEqual(1, slots[0].Amount);
        Assert.AreEqual(1, slots[1].Amount);
        Assert.AreEqual(1, slots[2].Amount);
    }

    [Test]
    public void TryRemove_WhenThereIsNotEnough_RemovesNothing()
    {
        var slots = EmptySlots(1);
        var model = new InventoryModel(slots, new FakeRules());
        model.TryAdd(Stackable, 5);

        Assert.IsFalse(model.TryRemove(Stackable, 10));
        Assert.AreEqual(5, slots[0].Amount);
    }

    [Test]
    public void TryRemove_EmptiesTheSlotCompletely()
    {
        var slots = EmptySlots(1);
        var model = new InventoryModel(slots, new FakeRules());
        model.TryAdd(Stackable, 5);

        Assert.IsTrue(model.TryRemove(Stackable, 5));
        Assert.IsTrue(slots[0].IsEmpty);
    }

    [Test]
    public void WithAmount_KeepsIdAndWeight()
    {
        var slot = new InventorySlot(Stackable, 5, 1.7f);
        var changed = slot.WithAmount(2);

        Assert.AreEqual(Stackable, changed.ItemId);
        Assert.AreEqual(2, changed.Amount);
        Assert.AreEqual(1.7f, changed.Weight);
    }
}
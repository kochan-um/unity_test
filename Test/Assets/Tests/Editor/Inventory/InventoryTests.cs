using InventorySystem.Core;
using InventorySystem.Data;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;

namespace InventorySystem.Tests
{
    public class InventoryTests
    {
        private ItemDefinition CreateItem(string id, int maxStack)
        {
            var item = ScriptableObject.CreateInstance<ItemDefinition>();
            var so = new SerializedObject(item);
            so.FindProperty("id").stringValue = id;
            so.FindProperty("displayName").stringValue = id;
            so.FindProperty("maxStackSize").intValue = maxStack;
            so.ApplyModifiedPropertiesWithoutUndo();
            return item;
        }

        [Test]
        public void AddItem_StacksIntoExistingSlot()
        {
            var inv = new Inventory(5);
            var item = CreateItem("potion", 5);

            inv.AddItem(item, 3);
            inv.AddItem(item, 1);

            Assert.AreEqual(4, inv.GetSlot(0).Quantity);
        }

        [Test]
        public void AddItem_SplitsIntoMultipleSlots()
        {
            var inv = new Inventory(5);
            var item = CreateItem("coin", 2);

            inv.AddItem(item, 3);

            Assert.AreEqual(2, inv.GetSlot(0).Quantity);
            Assert.AreEqual(1, inv.GetSlot(1).Quantity);
        }

        [Test]
        public void Move_SwapsWhenDifferentItems()
        {
            var inv = new Inventory(5);
            var a = CreateItem("a", 1);
            var b = CreateItem("b", 1);

            inv.AddItem(a, 1);
            inv.AddItem(b, 1);

            inv.Move(0, 1);

            Assert.AreEqual("b", inv.GetSlot(0).Item.Id);
            Assert.AreEqual("a", inv.GetSlot(1).Item.Id);
        }
    }
}

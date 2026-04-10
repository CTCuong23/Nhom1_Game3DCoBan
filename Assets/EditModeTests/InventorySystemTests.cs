using NUnit.Framework;
using System.Collections.Generic;

namespace EditModeTests
{
    public class InventorySystemTests
    {
        [Test]
        public void TestInventory_AddItemIncreasesCount()
        {
            List<string> inventory = new List<string>();
            string item = "KeyCard";

            inventory.Add(item);

            Assert.AreEqual(1, inventory.Count, "Hòm đồ phải có 1 vật phẩm sau khi thêm");
            Assert.Contains("KeyCard", inventory);
        }

        [Test]
        public void TestInventory_FullCapacityCheck()
        {
            List<string> inventory = new List<string>() { "Item1", "Item2", "Item3" };
            int maxSlots = 3;
            string newItem = "Item4";
            bool addedSuccessfully = false;

            // Logic kiểm tra sức chứa
            if (inventory.Count < maxSlots)
            {
                inventory.Add(newItem);
                addedSuccessfully = true;
            }
            else
            {
                addedSuccessfully = false;
            }

            Assert.IsFalse(addedSuccessfully, "Không được thêm vật phẩm khi hòm đồ đã đầy");
            Assert.AreEqual(3, inventory.Count);
        }
    }
}
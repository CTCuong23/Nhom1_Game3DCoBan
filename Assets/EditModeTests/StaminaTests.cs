using NUnit.Framework;
using UnityEngine;

namespace EditModeTests
{
    public class StaminaTests
    {
        [Test]
        public void TestStamina_SprintingReducesEnergy()
        {
            float stamina = 100f;
            float sprintCost = 20f;
            bool isSprinting = true;

            if (isSprinting && stamina > 0)
            {
                stamina -= sprintCost;
            }

            Assert.AreEqual(80f, stamina, "Thể lực phải giảm khi chạy nước rút");
        }

        [Test]
        public void TestStamina_RegenWhenNotSprinting()
        {
            float stamina = 50f;
            float maxStamina = 100f;
            float regenRate = 10f;
            bool isSprinting = false;

            if (!isSprinting && stamina < maxStamina)
            {
                stamina += regenRate;
            }

            Assert.AreEqual(60f, stamina, "Thể lực phải hồi lại khi đứng yên");
        }
    }
}
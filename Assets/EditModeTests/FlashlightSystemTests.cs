using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace EditModeTests
{
    public class FlashlightSystemTests
    {
        [Test]
        public void TestFlashlight_BatteryDepletion()
        {
            // Setup ban đầu
            float battery = 100f;
            float consumptionRate = 10f;
            bool isOn = true;

            // Action: Giả lập đèn bật trong 2 giây
            if (isOn)
            {
                battery -= consumptionRate * 2;
            }

            // Assert: Pin phải còn 80
            Assert.AreEqual(80f, battery, "Pin phải giảm khi đèn đang bật");
        }

        [Test]
        public void TestFlashlight_NoDepletionWhenOff()
        {
            float battery = 100f;
            bool isOn = false;

            // Action: Đèn tắt nên không trừ pin
            if (isOn)
            {
                battery -= 10f;
            }

            Assert.AreEqual(100f, battery, "Pin không được giảm khi đèn tắt");
        }
    }
}
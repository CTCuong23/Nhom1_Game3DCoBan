using NUnit.Framework;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

namespace EditModeTests
{
    public class BossHealthEditModeTests
    {
        private GameObject bossObject;
        private MonoBehaviour bossHealth;
        // Khai báo List để gom rác. Bất cứ GameObject nào tạo ra khi test sẽ nhét vào đây để dọn dẹp 1 lượt.
        private List<Object> m_TestObjects = new List<Object>();

        [SetUp]
        public void SetUp()
        {
            // Dùng Reflection để lấy Type từ Assembly-CSharp, tránh lỗi asmdef
            var bossHealthType = System.Type.GetType("BossHealth, Assembly-CSharp");
            Assert.IsNotNull(bossHealthType, "Không tìm thấy class BossHealth trong Assembly-CSharp.");

            // Tạo GameObject vật lý và gắn script BossHealth thật vào
            bossObject = new GameObject("Boss");
            bossHealth = (MonoBehaviour)bossObject.AddComponent(bossHealthType);
            m_TestObjects.Add(bossObject);
            
            // Đặt máu tối đa cho BossHealth để chuẩn bị test
            SetPrivateField(bossHealth, "maxHealth", 1000f);
        }

        [TearDown]
        public void TearDown()
        {
            // Dọn rác GameObject vật lý: Diệt tận gốc để không bị kẹt rác màn hình
            foreach (Object obj in m_TestObjects)
            {
                if (obj != null)
                {
                    Object.DestroyImmediate(obj);
                }
            }
            m_TestObjects.Clear();
        }

        // --- Hàm hỗ trợ dùng Reflection để can thiệp vào biến private và gọi hàm ---
        private void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(target, value);
            }
        }

        private T GetPrivateField<T>(object target, string fieldName)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                return (T)field.GetValue(target);
            }
            return default(T);
        }

        private void CallMethod(object target, string methodName, params object[] args)
        {
            MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (method != null)
            {
                method.Invoke(target, args);
            }
        }

        [Test]
        public void TakeDamage_DecreasesHealth_WhenNotInvulnerable()
        {
            // Arrange
            SetPrivateField(bossHealth, "currentHealth", 1000f);
            SetPrivateField(bossHealth, "isInvulnerable", false); // Boss hết bất tử (hoàn thành Intro)
            
            // Act
            CallMethod(bossHealth, "TakeDamage", 150f);

            // Assert
            float currentHealth = GetPrivateField<float>(bossHealth, "currentHealth");
            bool isDead = GetPrivateField<bool>(bossHealth, "isDead");

            Assert.AreEqual(850f, currentHealth, "Máu phải trừ chính xác khi không còn sát thương bất tử.");
            Assert.IsFalse(isDead);
        }

        [Test]
        public void TakeDamage_DoesNotDecreaseHealth_WhenShieldActive()
        {
            // Arrange
            SetPrivateField(bossHealth, "currentHealth", 1000f);
            SetPrivateField(bossHealth, "isInvulnerable", false);

            // Setup BossShieldSkill thật bằng Reflection và bật trạng thái khiên lên
            var shieldType = System.Type.GetType("BossShieldSkill, Assembly-CSharp");
            MonoBehaviour shieldSkill = (MonoBehaviour)bossObject.AddComponent(shieldType);
            SetPrivateField(shieldSkill, "isShieldActive", true); // Bật khiên
            SetPrivateField(bossHealth, "shieldSkill", shieldSkill); // Gắn script khiên vào BossHealth

            // Act
            CallMethod(bossHealth, "TakeDamage", 30f);

            // Assert
            float currentHealth = GetPrivateField<float>(bossHealth, "currentHealth");
            Assert.AreEqual(1000f, currentHealth, "Máu không được giảm khi boss đang bật khiên.");
        }

        [Test]
        public void TakeDamage_KillBoss_WhenDamageExceedsHealth()
        {
            // Arrange
            SetPrivateField(bossHealth, "currentHealth", 1000f);
            SetPrivateField(bossHealth, "isInvulnerable", false);

            // Act
            CallMethod(bossHealth, "TakeDamage", 1500f); // Sát thương vượt mức 1000

            // Assert
            float currentHealth = GetPrivateField<float>(bossHealth, "currentHealth");
            bool isDead = GetPrivateField<bool>(bossHealth, "isDead");

            Assert.LessOrEqual(currentHealth, 0f, "Máu phải <= 0 khi nhận sát thương lớn hơn máu hiện tại.");
            Assert.IsTrue(isDead, "Boss phải chết khi bị lượng sát thương vượt mức máu.");
        }

        [Test]
        public void TakeDamage_DoesNotDecreaseHealth_WhenInvulnerable()
        {
            // Arrange
            SetPrivateField(bossHealth, "currentHealth", 1000f);
            SetPrivateField(bossHealth, "isInvulnerable", true); // Boss đang bất tử (đang chạy Intro)

            // Act
            CallMethod(bossHealth, "TakeDamage", 100f);

            // Assert
            float currentHealth = GetPrivateField<float>(bossHealth, "currentHealth");
            bool isDead = GetPrivateField<bool>(bossHealth, "isDead");

            Assert.AreEqual(1000f, currentHealth, "Máu không được giảm khi boss đang trong trạng thái bất tử (Intro).");
            Assert.IsFalse(isDead);
        }

        [Test]
        public void TakeDamage_DoesNotTakeEffect_WhenAlreadyDead()
        {
            // Arrange
            SetPrivateField(bossHealth, "currentHealth", 1000f);
            SetPrivateField(bossHealth, "isInvulnerable", false);
            
            // Giết boss trước
            CallMethod(bossHealth, "TakeDamage", 1000f); 

            // Act
            CallMethod(bossHealth, "TakeDamage", 500f); // Gây thêm sát thương khi boss đã chết

            // Assert
            float currentHealth = GetPrivateField<float>(bossHealth, "currentHealth");
            bool isDead = GetPrivateField<bool>(bossHealth, "isDead");

            Assert.AreEqual(0f, currentHealth, "Lượng sát thương thứ hai sẽ bị bỏ qua vì Boss đã chết.");
            Assert.IsTrue(isDead, "Boss vẫn phải duy trì trạng thái chết.");
        }
    }
}

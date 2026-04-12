using NUnit.Framework;
using UnityEngine;

namespace EditModeTests
{
    // Tạo một Mock class đại diện cho logic của BossHealth giống như cách thầy viết ở FPSMicrogamesTests
    public class MockBossHealth
    {
        public float MaxHealth;
        public float CurrentHealth;
        public bool IsDead;
        public bool IsInvulnerable;
        public bool IsShieldActive;

        public MockBossHealth(float maxHealth)
        {
            MaxHealth = maxHealth;
            CurrentHealth = maxHealth;
            IsDead = false;
            // Mặc định ban đầu boss đang bất tử (vd: trong lúc chiếu Intro)
            IsInvulnerable = true; 
        }

        // Giả lập hàm kết thúc Intro, boss bắt đầu vào phase đánh
        public void StartFighting()
        {
            IsInvulnerable = false;
            CurrentHealth = MaxHealth;
        }

        // Logic tính toán sát thương tương tự hàm TakeDamage trong BossHealth.cs
        public void TakeDamage(float damage)
        {
            if (IsDead || IsInvulnerable) return;

            // Nếu đang bật khiên thì miễn sát thương
            if (IsShieldActive) return;

            CurrentHealth -= damage;
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, MaxHealth);

            if (CurrentHealth <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            IsDead = true;
        }
    }

    public class BossHealthEditModeTests
    {
        private MockBossHealth bossHealth;
        // Học theo thầy: Khai báo List để gom rác. Bất cứ GameObject nào tạo ra khi test sẽ nhét vào đây để dọn dẹp 1 lượt.
        private System.Collections.Generic.List<Object> m_TestObjects = new System.Collections.Generic.List<Object>();

        [SetUp]
        public void SetUp()
        {
            bossHealth = new MockBossHealth(1000f);
        }

        [TearDown]
        public void TearDown()
        {
            // Dọn dẹp Mock class C# thuần
            bossHealth = null; 

            // Chiêu dọn rác GameObject vật lý của thầy: Diệt tận gốc để không bị kẹt rác màn hình
            foreach (Object obj in m_TestObjects)
            {
                if (obj != null)
                {
                    Object.DestroyImmediate(obj);
                }
            }
            m_TestObjects.Clear();
        }

        [Test]
        public void TakeDamage_DecreasesHealth_WhenNotInvulnerable()
        {
            // Arrange
            bossHealth.StartFighting(); // Hết bất tử (hoàn thành Intro)
            
            // Act
            bossHealth.TakeDamage(150f);

            // Assert
            Assert.AreEqual(850f, bossHealth.CurrentHealth, "Máu phải trừ chính xác khi không còn sát thương bất tử.");
            Assert.IsFalse(bossHealth.IsDead);
        }

        [Test]
        public void TakeDamage_DoesNotDecreaseHealth_WhenShieldActive()
        {
            // Arrange
            bossHealth.StartFighting();
            bossHealth.IsShieldActive = true; // Bật khiên

            // Act
            bossHealth.TakeDamage(30f);

            // Assert
            Assert.AreEqual(1000f, bossHealth.CurrentHealth, "Máu không được giảm khi boss đang bật khiên.");
        }

        [Test]
        public void TakeDamage_KillBoss_WhenDamageExceedsHealth()
        {
            // Arrange
            bossHealth.StartFighting();

            // Act
            bossHealth.TakeDamage(1500f); // Sát thương vượt mức 1000

            // Assert
            Assert.AreEqual(0f, bossHealth.CurrentHealth);
            Assert.IsTrue(bossHealth.IsDead, "Boss phải chết khi bị lượng sát thương vượt mức máu.");
        }

        [Test]
        public void TakeDamage_DoesNotDecreaseHealth_WhenInvulnerable()
        {
            // Arrange
            // Không gọi StartFighting(), boss mặc định đang bất tử (IsInvulnerable = true)

            // Act
            bossHealth.TakeDamage(100f);

            // Assert
            Assert.AreEqual(1000f, bossHealth.CurrentHealth, "Máu không được giảm khi boss đang trong trạng thái bất tử (Intro).");
            Assert.IsFalse(bossHealth.IsDead);
        }

        [Test]
        public void TakeDamage_DoesNotTakeEffect_WhenAlreadyDead()
        {
            // Arrange
            bossHealth.StartFighting();
            bossHealth.TakeDamage(1000f); // Giết boss

            // Act
            bossHealth.TakeDamage(500f); // Gây thêm sát thương khi đã chết

            // Assert
            Assert.AreEqual(0f, bossHealth.CurrentHealth, "Máu rớt xuống dưới 0 vẫn sẽ bị giới hạn ở 0.");
            Assert.IsTrue(bossHealth.IsDead, "Boss vẫn phải duy trì trạng thái chết.");
        }
    }
}

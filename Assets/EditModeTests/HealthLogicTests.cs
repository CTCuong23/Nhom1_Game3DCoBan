using NUnit.Framework;
using UnityEngine;

public class HealthLogicTests
{
    [Test]
    public void HealthSystem_Heal_DoesNotExceedMaxHealth()
    {
        // Arrange: Cài đặt các chỉ số ban đầu
        int maxHealth = 100;
        int currentHealth = 80;
        int healAmount = 50;

        // Act: Thực hiện logic hồi máu (giả lập hàm Heal() trong script của bạn)
        currentHealth += healAmount;

        // Logic chặn giới hạn máu (Clamp)
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        // Hoặc nếu bạn dùng hàm của Unity: 
        // currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);

        // Assert: Máu cuối cùng chỉ được phép là 100, không phải 130
        Assert.AreEqual(100, currentHealth, "Lượng máu sau khi hồi phục không được phép vượt qua giới hạn Max Health.");
    }
}
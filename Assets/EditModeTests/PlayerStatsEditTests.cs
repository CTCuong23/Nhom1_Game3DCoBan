using NUnit.Framework;
using System.Reflection;
using UnityEngine;

public class PlayerStatsEditTests
{
    private GameObject playerObject;
    private MonoBehaviour playerStats;
    private System.Type playerStatsType;

    [SetUp]
    public void Setup()
    {
        playerObject = new GameObject("PlayerTest_EditMode");

        // Dùng Reflection giống PlayMode để tránh lỗi mất tham chiếu Assembly
        playerStatsType = System.Type.GetType("PlayerStats, Assembly-CSharp");

        if (playerStatsType == null)
        {
            Assert.Fail("Không tìm thấy component PlayerStats.");
            return;
        }

        playerStats = playerObject.AddComponent(playerStatsType) as MonoBehaviour;

        // Đặt max health và max stamina
        SetField("maxHealth", 100f);
        SetField("maxStamina", 100f);

        // Trạng thái dàn dựng (Arrange): Nhân vật đang gần chết và kiệt sức
        SetField("currentHealth", 10f);
        SetField("currentStamina", 5f);
        SetField("isInvincible", true);
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(playerObject);
    }

    // --- Các Hàm Hỗ Trợ Đóng Gói (Reflection Wrappers) ---
    private void SetField(string fieldName, object value)
    {
        var field = playerStatsType.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null) field.SetValue(playerStats, value);
    }

    private float GetFloatField(string fieldName)
    {
        var field = playerStatsType.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        return (float)field.GetValue(playerStats);
    }

    private bool GetBoolField(string fieldName)
    {
        var field = playerStatsType.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        return (bool)field.GetValue(playerStats);
    }

    private void CallMethod(string methodName)
    {
        var method = playerStatsType.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (method != null) method.Invoke(playerStats, null);
    }

    private void CallMethodWithArgs(string methodName, object[] parameters)
    {
        var method = playerStatsType.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (method != null) method.Invoke(playerStats, parameters);
    }
    // --------------------------------------------------------

    [Test]
    public void ResetStats_RestoresHealthAndStaminaToMax()
    {
        // Act: Gọi hàm ResetStats() - giả lập việc người chơi hồi sinh
        CallMethod("ResetStats");

        // Assert: Kiểm tra cả 3 chỉ số phải được Reset về chuẩn
        Assert.AreEqual(100f, GetFloatField("currentHealth"), "Máu phải được hồi đầy về maxHealth.");
        Assert.AreEqual(100f, GetFloatField("currentStamina"), "Thể lực phải được hồi đầy về maxStamina.");
        Assert.AreEqual(false, GetBoolField("isInvincible"), "Trạng thái bất tử (cờ isInvincible) bắt buộc phải tắt.");
    }

    [Test]
    public void TakeDamage_ReducesHealth_WhenNotInvincible()
    {
        // Arrange: Cài đặt máu 100 và tắt trạng thái bất tử
        SetField("currentHealth", 100f);
        SetField("isInvincible", false);

        // Act: Gọi hàm TakeDamage nhận 20 sát thương
        CallMethodWithArgs("TakeDamage", new object[] { 20f });

        // Assert: Kiểm tra máu giảm đi 20 (còn 80)
        Assert.AreEqual(80f, GetFloatField("currentHealth"), "Máu phải bị trừ đi 20 sau khi nhận sát thương.");
    }

    [Test]
    public void Heal_IncreasesHealth_ButNotAboveMax()
    {
        // Arrange: Cài đặt máu tối đa là 100 và máu hiện tại là 90
        SetField("maxHealth", 100f);
        SetField("currentHealth", 90f);

        // Act: Gọi hàm Heal để hồi 20 máu
        CallMethodWithArgs("Heal", new object[] { 20f });

        // Assert: Kiểm tra máu không vượt giới hạn maxHealth (tối đa là 100)
        Assert.AreEqual(100f, GetFloatField("currentHealth"), "Máu phải được hồi nhưng không được vượt quá giới hạn maxHealth.");
    }

    [Test]
    public void TakeDamage_WhenInvincible_ShouldNotReduceHealth()
    {
        // Arrange: Nhân vật có 80 máu, bật trạng thái bất tử
        SetField("currentHealth", 80f);
        SetField("isInvincible", true);

        // Act: Đóng gói lại gọi hàm TakeDamage nhận 15 sát thương
        CallMethodWithArgs("TakeDamage", new object[] { 15f });

        // Assert: Máu phải còn giữ nguyên không suy suyển
        Assert.AreEqual(80f, GetFloatField("currentHealth"), "Máu không bị trừ do nhân vật đang trong trạng thái bất tử (isInvincible=true).");
    }

    [Test]
    public void TakeDamage_WhenAlreadyDead_ShouldNotGoBelowZero()
    {
        // Arrange: Người chơi đã chết (máu bằng 0), đã tắt bất tử
        SetField("currentHealth", 0f);
        SetField("isInvincible", false);

        // Act: Tiếp tục gọi hàm nhận thêm sát thương (VD: thi thể bị đánh)
        CallMethodWithArgs("TakeDamage", new object[] { 30f });

        // Assert: Máu không được rớt xuống mức âm, đứng im ở 0
        Assert.AreEqual(0f, GetFloatField("currentHealth"), "Máu không được rớt xuống mức âm, hàm TakeDamage phải tự chặn khi máu đã là 0.");
    }
}

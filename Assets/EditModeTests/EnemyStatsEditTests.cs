using NUnit.Framework;
using System.Reflection;
using UnityEngine;

public class EnemyStatsEditTests
{
    private GameObject enemyObject;
    private MonoBehaviour enemyStats;
    private System.Type enemyStatsType;

    [SetUp]
    public void Setup()
    {
        enemyObject = new GameObject("EnemyTest_EditMode");
        
        // Cần add Collider do trong code EnemyStats.cs hàm Die() gọi GetComponent<Collider>().enabled = false; mà không check null
        enemyObject.AddComponent<BoxCollider>();

        // Dùng Reflection giống PlayMode để tránh lỗi mất tham chiếu
        enemyStatsType = System.Type.GetType("EnemyStats, Assembly-CSharp");

        if (enemyStatsType == null)
        {
            Assert.Fail("Không tìm thấy component EnemyStats.");
            return;
        }

        enemyStats = enemyObject.AddComponent(enemyStatsType) as MonoBehaviour;

        // Khởi tạo các giá trị đầu vào cho test setup
        SetField("currentHealth", 100f);
        SetField("isDead", false);
        SetField("maxStunTimes", 2);
        SetField("currentStunCount", 0);
        SetField("lastHitTime", 0f);
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(enemyObject);
    }

    // --- Các Hàm Hỗ Trợ Đóng Gói (Reflection Wrappers) ---
    private void SetField(string fieldName, object value)
    {
        var field = enemyStatsType.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null) field.SetValue(enemyStats, value);
    }

    private float GetFloatField(string fieldName)
    {
        var field = enemyStatsType.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        return (float)field.GetValue(enemyStats);
    }

    private int GetIntField(string fieldName)
    {
        var field = enemyStatsType.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        return (int)field.GetValue(enemyStats);
    }

    private bool GetBoolField(string fieldName)
    {
        var field = enemyStatsType.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        return (bool)field.GetValue(enemyStats);
    }

    private void CallMethodWithArgs(string methodName, object[] parameters)
    {
        var method = enemyStatsType.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (method != null) method.Invoke(enemyStats, parameters);
    }
    // --------------------------------------------------------

    [Test]
    public void TakeDamage_ReducesHealth_WhenHit()
    {
        // Act: Gây 30 sát thương cho quái
        // Giả lập Time.time bằng cách set lastHitTime rất nhỏ để không dính reset logic
        SetField("lastHitTime", 9999f);
        CallMethodWithArgs("TakeDamage", new object[] { 30f });

        // Assert: Quái bắt đầu từ 100 máu, nhận 30 sát thương thì còn 70
        Assert.AreEqual(70f, GetFloatField("currentHealth"), "Máu của quái vật phải giảm đúng lượng sát thương nhận vào.");
        Assert.IsFalse(GetBoolField("isDead"), "Quái vật không được chết nếu máu trưa tụt về 0.");
    }

    [Test]
    public void TakeDamage_IncreasesStunCount_RespectsMaxLimit()
    {
        // Arrange
        SetField("maxStunTimes", 2);
        SetField("currentStunCount", 0);
        
        // Cần fake Time.time nếu không sẽ bị reset do (Time.time > lastHitTime + stunResetTime)
        // Vì Time.time trong EditMode thường là 0, ta cấu hình lastHitTime lớn hơn 0 để không bị ăn reset
        SetField("lastHitTime", 999f); 

        // Act: Gọi hàm TakeDamage liên tiếp 3 phát
        CallMethodWithArgs("TakeDamage", new object[] { 10f }); // Lần 1 (Count = 1)
        CallMethodWithArgs("TakeDamage", new object[] { 10f }); // Lần 2 (Count = 2)
        CallMethodWithArgs("TakeDamage", new object[] { 10f }); // Lần 3 (Vượt quá maxStunTimes => Không tăng nữa)

        // Assert
        Assert.AreEqual(2, GetIntField("currentStunCount"), "Stun count không được vượt quá giới hạn maxStunTimes (2).");
    }

    [Test]
    public void TakeDamage_KillsEnemy_WhenHealthDepleted()
    {
        // Arrange: Chỉnh máu quái hiện tại xuống một mức nhỏ
        SetField("currentHealth", 40f);
        SetField("isDead", false);
        SetField("lastHitTime", 9999f);

        // Act: Gây số sát thương lớn hơn tổng số máu còn lại
        CallMethodWithArgs("TakeDamage", new object[] { 50f });

        // Assert: Quái vật phải chuyển sang trạng thái isDead
        Assert.IsTrue(GetBoolField("isDead"), "Quái vật phải chết (isDead = true) khi chịu một đòn kết liễu.");
    }

    [Test]
    public void TakeDamage_IgnoresDamage_WhenAlreadyDead()
    {
        // Arrange: Giả lập quái vật ĐÃ CHẾT và máu đã tụt về 0
        SetField("isDead", true);
        SetField("currentHealth", 0f);

        // Act: Người chơi vẫn cố tình chém thêm vào xác quái
        CallMethodWithArgs("TakeDamage", new object[] { 999f });

        // Assert: Lệnh TakeDamage chứa dòng "if (isDead) return;", máu không bị tụt xuống âm
        Assert.AreEqual(0f, GetFloatField("currentHealth"), "Máu không được phép giảm thêm (tránh bị lỗi số âm) nếu quái vật đã chết.");
    }

    [Test]
    public void TakeDamage_DisablesEnemyCollider_UponDeath()
    {
        // Arrange: Bật Collider của quái vật lên và set máu yếu
        BoxCollider col = enemyObject.GetComponent<BoxCollider>();
        col.enabled = true;
        SetField("currentHealth", 10f);
        SetField("isDead", false);
        SetField("lastHitTime", 9999f);

        // Act: Chém bạo kích hạ gục quái vật (sẽ tự động kích hoạt hàm Die() bên trong mã gốc)
        CallMethodWithArgs("TakeDamage", new object[] { 100f });

        // Assert: Lệnh Die() bắt buộc phải tắt Collider đi (GetComponent<Collider>().enabled = false)
        Assert.IsFalse(col.enabled, "Collider của quái vật phải tự động bị tắt đi khi tử vong để đạn/người chơi đi xuyên qua cái xác.");
    }
}

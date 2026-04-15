using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class CuongPlayModeTests
{
    // Helpers dùng Reflection (bắt buộc do asmdef isolation)
    private static void SetField(object target, string fieldName, object value)
    {
        var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        target.GetType().GetField(fieldName, flags)?.SetValue(target, value);
    }

    private static T GetField<T>(object target, string fieldName)
    {
        var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        return (T)target.GetType().GetField(fieldName, flags).GetValue(target);
    }

    private static void CallMethod(object target, string methodName, params object[] args)
    {
        var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        target.GetType().GetMethod(methodName, flags)?.Invoke(target, args);
    }

    // TEST 1 – PlayerStats: Đòn thứ 2 bị chặn khi đang bất tử
    [UnityTest]
    public IEnumerator PlayerStats_SecondHitBlocked_WhileInvincible()
    {
        // Arrange
        var type = System.Type.GetType("PlayerStats, Assembly-CSharp");
        Assert.IsNotNull(type, "Không tìm thấy class PlayerStats trong Assembly-CSharp.");

        var go = new GameObject("Player_Test1");
        var stats = (MonoBehaviour)go.AddComponent(type);

        yield return null; // Chờ Start() chạy xong trước khi set field

        SetField(stats, "maxHealth", 100f);
        SetField(stats, "currentHealth", 100f);
        SetField(stats, "invincibilityDuration", 2f);
        SetField(stats, "isInvincible", false);

        // Act
        CallMethod(stats, "TakeDamage", 30f); // Đòn 1: kích hoạt invincibility
        CallMethod(stats, "TakeDamage", 50f); // Đòn 2: bị chặn vì isInvincible = true
        yield return null;

        // Assert: chỉ mất 30 từ đòn đầu
        float hp = GetField<float>(stats, "currentHealth");
        Assert.AreEqual(70f, hp, "Máu phải là 70 (chỉ bị trừ đòn đầu 30), đòn 2 bị chặn bởi invincibility.");

        Object.DestroyImmediate(go);
    }

    // TEST 2 – PlayerStats: Có thể nhận dame lại sau khi hết thời gian bất tử
    [UnityTest]
    public IEnumerator PlayerStats_CanTakeDamageAgain_AfterInvincibilityExpires()
    {
        // Arrange
        var type = System.Type.GetType("PlayerStats, Assembly-CSharp");
        Assert.IsNotNull(type, "Không tìm thấy class PlayerStats.");

        var go = new GameObject("Player_Test2");
        var stats = (MonoBehaviour)go.AddComponent(type);

        yield return null; // Chờ Start() chạy xong

        const float invDuration = 0.3f;
        SetField(stats, "maxHealth", 100f);
        SetField(stats, "currentHealth", 100f);
        SetField(stats, "invincibilityDuration", invDuration);
        SetField(stats, "isInvincible", false);

        // Act
        CallMethod(stats, "TakeDamage", 20f); // currentHealth = 80, kích hoạt invincibility

        yield return new WaitForSeconds(invDuration + 0.1f); // Chờ coroutine kết thúc

        CallMethod(stats, "TakeDamage", 30f); // currentHealth = 50
        yield return null;

        // Assert: 100 - 20 - 30 = 50
        float hp = GetField<float>(stats, "currentHealth");
        Assert.AreEqual(50f, hp, "Sau khi hết bất tử, đòn 2 phải được tính. Máu còn lại phải là 50.");

        Object.DestroyImmediate(go);
    }

    // TEST 3 – bulletProjectTile: Viên đạn bay theo hướng forward sau Start()
    [UnityTest]
    public IEnumerator Bullet_HasCorrectForwardVelocity_AfterStart()
    {
        // Arrange
        var bulletType = System.Type.GetType("bulletProjectTile, Assembly-CSharp");
        Assert.IsNotNull(bulletType, "Không tìm thấy class bulletProjectTile trong Assembly-CSharp.");

        var go = new GameObject("Bullet_Test3");
        go.AddComponent<Rigidbody>();
        var bullet = (MonoBehaviour)go.AddComponent(bulletType);

        const float testSpeed = 15f;
        SetField(bullet, "speed", testSpeed);
        go.transform.forward = Vector3.forward;

        // Act – yield 1 frame để Awake + Start thực thi
        yield return null;

        // Assert
        var rb = go.GetComponent<Rigidbody>();
        float actualSpeed = rb.linearVelocity.magnitude;
        Assert.AreEqual(testSpeed, actualSpeed, 0.01f, $"Vận tốc viên đạn phải bằng speed={testSpeed}. Thực tế: {actualSpeed}");

        Vector3 expectedVelocity = Vector3.forward * testSpeed;
        Assert.AreEqual(expectedVelocity.x, rb.linearVelocity.x, 0.01f, "Hướng X sai.");
        Assert.AreEqual(expectedVelocity.z, rb.linearVelocity.z, 0.01f, "Hướng Z sai.");

        Object.DestroyImmediate(go);
    }

    // TEST 4 – PlayerCombatLayerController: UnlockWeapon() → isArmed = true
    [UnityTest]
    public IEnumerator PlayerCombatLayerController_IsArmed_AfterUnlockWeapon()
    {
        // Arrange
        var controllerType = System.Type.GetType("PlayerCombatLayerController, Assembly-CSharp");
        Assert.IsNotNull(controllerType, "Không tìm thấy class PlayerCombatLayerController.");

        var go = new GameObject("Player_Combat_Test4");
        go.AddComponent<Animator>(); // Bắt buộc để Start() không crash
        var controller = (MonoBehaviour)go.AddComponent(controllerType);

        yield return null; // Chờ Start() chạy xong

        SetField(controller, "combatLayerIndex", 0); // Override về Base Layer (luôn tồn tại)
        SetField(controller, "hasWeapon", false);
        SetField(controller, "isArmed", false);

        bool isArmedBefore = GetField<bool>(controller, "isArmed");
        Assert.IsFalse(isArmedBefore, "Trước khi unlock, isArmed phải là false.");

        // Act
        CallMethod(controller, "UnlockWeapon");
        yield return null;

        // Assert
        bool isArmedAfter = GetField<bool>(controller, "isArmed");
        Assert.IsTrue(isArmedAfter, "Sau khi gọi UnlockWeapon(), isArmed phải là true.");

        Object.DestroyImmediate(go);
    }

    // TEST 5 – EnemyStats: TakeDamage() trừ máu đúng; quái chết khi hết máu
    [UnityTest]
    public IEnumerator EnemyStats_TakeDamage_ReducesHealth_AndDiesWhenZero()
    {
        // Arrange
        var go = new GameObject("Enemy_Test5");
        go.AddComponent<BoxCollider>(); // Die() cần Collider để tắt

        var type = System.Type.GetType("EnemyStats, Assembly-CSharp");
        Assert.IsNotNull(type, "Không tìm thấy class EnemyStats.");

        var enemy = (MonoBehaviour)go.AddComponent(type);

        SetField(enemy, "currentHealth", 50f);
        SetField(enemy, "isDead", false);
        SetField(enemy, "maxStunTimes", 10);

        yield return null; // Chờ Start()

        // Act – đòn 1: 20 dame
        CallMethod(enemy, "TakeDamage", 20f);
        yield return null;

        float hpAfterFirstHit = GetField<float>(enemy, "currentHealth");
        Assert.AreEqual(30f, hpAfterFirstHit, "Sau đòn 20 dame, máu quái phải còn 30.");

        // Act – đòn 2: 30 dame → máu = 0 → Die() được gọi
        CallMethod(enemy, "TakeDamage", 30f);
        yield return null;

        // Assert
        bool isDead = GetField<bool>(enemy, "isDead");
        Assert.IsTrue(isDead, "Khi máu = 0, hàm Die() phải bật cờ isDead = true.");

        Object.DestroyImmediate(go);
    }
}

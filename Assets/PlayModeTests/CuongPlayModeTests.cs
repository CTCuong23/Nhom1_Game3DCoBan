using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// 5 Play Mode tests cho phần của Cường:
///   - Player (PlayerStats): invincibility coroutine chạy thật theo thời gian
///   - Súng (bulletProjectTile): viên đạn bay đúng hướng sau Start()
///   - Vũ khí (PlayerCombatLayerController): UnlockWeapon cho phép chiến đấu
///   - Quái (EnemyStats): TakeDamage trừ máu thật
/// Tất cả gọi hàm thật – KHÔNG mock, KHÔNG fake.
/// </summary>
public class CuongPlayModeTests
{
    // ─────────────────────────────────────────────
    // Helpers dùng Reflection (bắt buộc do asmdef)
    // ─────────────────────────────────────────────
    private static void SetField(object target, string fieldName, object value)
    {
        target.GetType()
              .GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
              ?.SetValue(target, value);
    }

    private static T GetField<T>(object target, string fieldName)
    {
        var field = target.GetType()
                          .GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        return (T)field.GetValue(target);
    }

    private static void CallMethod(object target, string methodName, params object[] args)
    {
        target.GetType()
              .GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
              ?.Invoke(target, args);
    }

    // ════════════════════════════════════════════════════════════════
    // TEST 1 – PlayerStats: TakeDamage lần 2 bị chặn vì đang bất tử
    // Kiểm tra cơ chế invincibility bật ngay sau lần nhận sát thương đầu tiên.
    // ════════════════════════════════════════════════════════════════
    [UnityTest]
    public IEnumerator PlayerStats_SecondHitBlocked_WhileInvincible()
    {
        // Arrange
        var type = System.Type.GetType("PlayerStats, Assembly-CSharp");
        Assert.IsNotNull(type, "Không tìm thấy class PlayerStats trong Assembly-CSharp.");

        var go = new GameObject("Player_Test1");
        var stats = (MonoBehaviour)go.AddComponent(type);

        // ⚠️ Yield TRƯỚC để Start() chạy xong (Start() gọi ResetStats() reset toàn bộ field)
        // Sau đó mới set lại giá trị – nếu set trước thì Start() sẽ override hết
        yield return null;

        SetField(stats, "maxHealth",            100f);
        SetField(stats, "currentHealth",         100f);
        SetField(stats, "invincibilityDuration",  2f);
        SetField(stats, "isInvincible",          false);

        // Act – đòn 1: TakeDamage thật → StartCoroutine(InvincibilityRoutine)
        // Coroutine chạy ngay dòng đầu (isInvincible = true) TRƯỚC yield đầu tiên của nó
        // nên isInvincible = true ngay sau lời gọi này (đồng bộ)
        CallMethod(stats, "TakeDamage", 30f);

        // Act – đòn 2 ngay lập tức (cùng frame), isInvincible đã true → bị chặn
        CallMethod(stats, "TakeDamage", 50f);
        yield return null;

        // Assert: chỉ mất 30 dame từ đòn đầu, đòn 2 bị chặn
        float hp = GetField<float>(stats, "currentHealth");
        Assert.AreEqual(70f, hp, "Máu phải là 70 (chỉ bị trừ đòn đầu 30), đòn 2 bị chặn bởi invincibility.");

        Object.DestroyImmediate(go);
    }

    // ════════════════════════════════════════════════════════════════
    // TEST 2 – PlayerStats: Sau khi hết thời gian bất tử, có thể nhận dame lại
    // Đây là lý do phải dùng Play Mode – coroutine yield WaitForSeconds chạy thật.
    // ════════════════════════════════════════════════════════════════
    [UnityTest]
    public IEnumerator PlayerStats_CanTakeDamageAgain_AfterInvincibilityExpires()
    {
        // Arrange
        var type = System.Type.GetType("PlayerStats, Assembly-CSharp");
        Assert.IsNotNull(type, "Không tìm thấy class PlayerStats.");

        var go = new GameObject("Player_Test2");
        var stats = (MonoBehaviour)go.AddComponent(type);

        // ⚠️ Yield TRƯỚC để Start() chạy xong (Start() gọi ResetStats() reset toàn bộ field)
        yield return null;

        const float invDuration = 0.3f;   // đặt ngắn để test chạy nhanh
        SetField(stats, "maxHealth",            100f);
        SetField(stats, "currentHealth",         100f);
        SetField(stats, "invincibilityDuration", invDuration);
        SetField(stats, "isInvincible",          false);

        // Act – đòn 1: kích hoạt InvincibilityRoutine thật → isInvincible = true
        CallMethod(stats, "TakeDamage", 20f); // currentHealth = 80

        // Chờ cho hết thời gian bất tử (coroutine WaitForSeconds chạy thật trong Play Mode)
        yield return new WaitForSeconds(invDuration + 0.1f);
        // Sau khi chờ: coroutine đã chạy xong, isInvincible = false

        // Act – đòn 2: isInvincible = false → dame được tính
        CallMethod(stats, "TakeDamage", 30f); // currentHealth = 80 - 30 = 50
        yield return null;

        // Assert: 100 - 20 - 30 = 50
        float hp = GetField<float>(stats, "currentHealth");
        Assert.AreEqual(50f, hp, "Sau khi hết bất tử, đòn 2 phải được tính. Máu còn lại phải là 50.");

        Object.DestroyImmediate(go);
    }

    // ════════════════════════════════════════════════════════════════
    // TEST 3 – bulletProjectTile: Viên đạn bay theo hướng forward sau Start()
    // Dùng Reflection để tránh lỗi CS0246 do asmdef isolation.
    // ════════════════════════════════════════════════════════════════
    [UnityTest]
    public IEnumerator Bullet_HasCorrectForwardVelocity_AfterStart()
    {
        // Arrange
        var bulletType = System.Type.GetType("bulletProjectTile, Assembly-CSharp");
        Assert.IsNotNull(bulletType, "Không tìm thấy class bulletProjectTile trong Assembly-CSharp.");

        var go = new GameObject("Bullet_Test3");
        go.AddComponent<Rigidbody>();   // bulletProjectTile.Awake() lấy component này
        var bullet = (MonoBehaviour)go.AddComponent(bulletType);

        const float testSpeed = 15f;
        SetField(bullet, "speed", testSpeed);

        // Đặt hướng bay rõ ràng
        go.transform.forward = Vector3.forward;

        // Act – yield 1 frame để Awake + Start của MonoBehaviour thực thi
        yield return null;

        // Assert
        var rb = go.GetComponent<Rigidbody>();
        float actualSpeed = rb.linearVelocity.magnitude;
        Assert.AreEqual(testSpeed, actualSpeed, 0.01f,
            $"Vận tốc viên đạn phải bằng speed={testSpeed}. Thực tế: {actualSpeed}");

        // Kiểm tra chiều bay đúng hướng forward
        Vector3 expectedVelocity = Vector3.forward * testSpeed;
        Assert.AreEqual(expectedVelocity.x, rb.linearVelocity.x, 0.01f, "Hướng X sai.");
        Assert.AreEqual(expectedVelocity.z, rb.linearVelocity.z, 0.01f, "Hướng Z sai.");

        Object.DestroyImmediate(go);
    }

    // ════════════════════════════════════════════════════════════════
    // TEST 4 – PlayerCombatLayerController: UnlockWeapon() → GetIsArmed() = true
    // Dùng Reflection để tránh lỗi CS0246 do asmdef isolation.
    // ════════════════════════════════════════════════════════════════
    [UnityTest]
    public IEnumerator PlayerCombatLayerController_IsArmed_AfterUnlockWeapon()
    {
        // Arrange – Dùng Reflection để lấy type
        var controllerType = System.Type.GetType("PlayerCombatLayerController, Assembly-CSharp");
        Assert.IsNotNull(controllerType, "Không tìm thấy class PlayerCombatLayerController.");

        var go = new GameObject("Player_Combat_Test4");
        // ⚠️ Fix: Start() gọi animator.GetLayerIndex() → crash nếu không có Animator
        // Thêm Animator TRƯỚC khi AddComponent để Start() không throw MissingComponentException
        go.AddComponent<Animator>();
        var controller = (MonoBehaviour)go.AddComponent(controllerType);

        // Chờ Start() chạy xong:
        // - animator = GetComponent<Animator>() → OK (đã add)
        // - combatLayerIndex = animator.GetLayerIndex("Aim Layer") → -1 (layer không tồn tại)
        // - animator.SetLayerWeight(-1, 0f) → warning nhưng không crash
        yield return null;

        // ⚠️ Override combatLayerIndex về 0 (Base Layer luôn tồn tại)
        // để khi UnlockWeapon() gọi SetLayerWeight(combatLayerIndex, 1f) không bị lỗi index
        SetField(controller, "combatLayerIndex", 0);
        SetField(controller, "hasWeapon", false);
        SetField(controller, "isArmed",   false);

        bool isArmedBefore = GetField<bool>(controller, "isArmed");
        Assert.IsFalse(isArmedBefore, "Trước khi unlock, isArmed phải là false.");

        // Act – gọi hàm thật UnlockWeapon() qua Reflection
        // Hàm này: hasWeapon=true, isArmed=true, gọi UpdateVisuals()
        // UpdateVisuals(): weaponInHand null→ skip, animator.SetLayerWeight(0, 1f) → OK
        CallMethod(controller, "UnlockWeapon");
        yield return null;

        // Assert
        bool isArmedAfter = GetField<bool>(controller, "isArmed");
        Assert.IsTrue(isArmedAfter,
            "Sau khi gọi UnlockWeapon(), isArmed phải là true.");

        Object.DestroyImmediate(go);
    }

    // ════════════════════════════════════════════════════════════════
    // TEST 5 – EnemyStats: TakeDamage() trừ máu thật; quái chết khi hết máu
    // Gọi hàm TakeDamage thật; kiểm tra isDead flag sau khi máu = 0.
    // ════════════════════════════════════════════════════════════════
    [UnityTest]
    public IEnumerator EnemyStats_TakeDamage_ReducesHealth_AndDiesWhenZero()
    {
        // Arrange – EnemyStats implements IDamageable, không cần NavMesh hay BehaviorAgent
        var go = new GameObject("Enemy_Test5");
        go.AddComponent<BoxCollider>();   // Die() gọi GetComponent<Collider>().enabled = false

        var type = System.Type.GetType("EnemyStats, Assembly-CSharp");
        Assert.IsNotNull(type, "Không tìm thấy class EnemyStats.");

        var enemy = (MonoBehaviour)go.AddComponent(type);

        // Đặt máu trực tiếp (data ScriptableObject null nên Start không set được)
        SetField(enemy, "currentHealth", 50f);
        SetField(enemy, "isDead",        false);
        SetField(enemy, "maxStunTimes",  10);   // set cao để tránh exception trong HitStunRoutine

        yield return null;  // chờ Start

        // Act – gây 20 dame (hàm TakeDamage thật)
        CallMethod(enemy, "TakeDamage", 20f);
        yield return null;

        float hpAfterFirstHit = GetField<float>(enemy, "currentHealth");
        Assert.AreEqual(30f, hpAfterFirstHit, "Sau đòn 20 dame, máu quái phải còn 30.");

        // Act – gây thêm 30 dame → máu = 0 → quái chết → Die() được gọi thật
        CallMethod(enemy, "TakeDamage", 30f);
        yield return null;

        bool isDead = GetField<bool>(enemy, "isDead");
        Assert.IsTrue(isDead, "Khi máu = 0, hàm Die() phải bật cờ isDead = true.");

        Object.DestroyImmediate(go);
    }
}

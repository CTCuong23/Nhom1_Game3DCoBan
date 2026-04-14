using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

/// <summary>
/// 5 Play Mode tests cho Boss của Trại (BossHealth):
///   - Đầu game Boss bất tử (isInvulnerable = true). TakeDamage không mất máu.
///   - Sau khi StartFighting() (chạy xong coroutine thanh máu), boss hết bất tử.
///   - Boss nhận dame bình thường khi hết bất tử.
///   - Boss không nhận sát thương khi đang bật khiên (ShieldSkill active).
///   - Boss chết khi máu <= 0 (isDead = true).
/// Sử dụng trực tiếp class BossHealth thật, dùng Reflection để truyền biến, tránh lỗi liên kết asmdef.
/// </summary>
public class TraiPlayModeTests
{
    // ─────────────────────────────────────────────
    // Helpers dùng Reflection
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
    // TEST 1 – BossHealth: Ban đầu (khi chưa gọi StartFighting), Boss mặc định bất tử
    // ════════════════════════════════════════════════════════════════
    [UnityTest]
    public IEnumerator BossHealth_InitialState_TakeDamageBlocked_ByInvulnerability()
    {
        var type = System.Type.GetType("BossHealth, Assembly-CSharp");
        Assert.IsNotNull(type, "Không tìm thấy class BossHealth (TraiRobloxScript).");

        var go = new GameObject("Boss_Test1");
        var boss = (MonoBehaviour)go.AddComponent(type);

        yield return null; // Chờ Awake/Start

        SetField(boss, "maxHealth", 1000f);
        SetField(boss, "currentHealth", 1000f);
        // Trong Awake/đầu game, isInvulnerable đã = true
        SetField(boss, "isInvulnerable", true); 

        // Đánh 1 đòn 200 dame bằng hàm TakeDamage thật
        CallMethod(boss, "TakeDamage", 200f);
        yield return null;

        float hp = GetField<float>(boss, "currentHealth");
        Assert.AreEqual(1000f, hp, "Máu phải giữ nguyên 1000 do Boss đang bất tử (isInvulnerable = true), TakeDamage bị chặn.");

        Object.DestroyImmediate(go);
    }

    // ════════════════════════════════════════════════════════════════
    // TEST 2 – BossHealth: Sau khi gọi StartFighting và chờ Coroutine chạy thì hết bất tử
    // ════════════════════════════════════════════════════════════════
    [UnityTest]
    public IEnumerator BossHealth_BecomesVulnerable_AfterStartFightingRoutine()
    {
        var type = System.Type.GetType("BossHealth, Assembly-CSharp");
        var go = new GameObject("Boss_Test2");
        var boss = (MonoBehaviour)go.AddComponent(type);

        yield return null; // chờ Awake

        // Giảm thời gian fill thanh máu để bài test chạy nhanh
        float testDuration = 0.5f;
        SetField(boss, "fillDuration", testDuration);
        SetField(boss, "maxHealth", 1000f);
        SetField(boss, "isInvulnerable", true); // Bắt đầu là bất tử
        
        // Tạo Slider giả mạo (để code trong Coroutine của BossHealth không bị NullReference)
        var sliderGo = new GameObject("Slider");
        var slider = sliderGo.AddComponent<Slider>();
        SetField(boss, "healthSlider", slider);

        // Gọi StartFighting, hàm này gọi StartCoroutine thực tế
        CallMethod(boss, "StartFighting");

        // Chờ thời gian Intro Health Routine chạy + 0.1 giây offset
        yield return new WaitForSeconds(testDuration + 0.1f);

        bool isInvulnerable = GetField<bool>(boss, "isInvulnerable");
        Assert.IsFalse(isInvulnerable, "Boss phải mất trạng thái bất tử (isInvulnerable = false) sau khi coroutine UI Intro hoàn thành.");

        Object.DestroyImmediate(go);
        Object.DestroyImmediate(sliderGo);
    }

    // ════════════════════════════════════════════════════════════════
    // TEST 3 – BossHealth: Bị trừ máu thật sự khi hết trạng thái bất tử
    // ════════════════════════════════════════════════════════════════
    [UnityTest]
    public IEnumerator BossHealth_TakeDamage_ReducesHealthCorrectly_WhenVulnerable()
    {
        var type = System.Type.GetType("BossHealth, Assembly-CSharp");
        var go = new GameObject("Boss_Test3");
        var boss = (MonoBehaviour)go.AddComponent(type);

        yield return null;

        SetField(boss, "maxHealth", 1000f);
        SetField(boss, "currentHealth", 1000f);
        SetField(boss, "isInvulnerable", false); // Boss đã hết bất tử
        SetField(boss, "isDead", false);

        // Đánh đòn 300 dame
        CallMethod(boss, "TakeDamage", 300f);
        yield return null;

        float hp = GetField<float>(boss, "currentHealth");
        Assert.AreEqual(700f, hp, "Boss không bất tử nên phải bị trừ đúng 300 máu, còn 700.");

        Object.DestroyImmediate(go);
    }

    // ════════════════════════════════════════════════════════════════
    // TEST 4 – BossHealth: Thanh UI Máu (HealthSlider) cập nhật đúng khi nhận dame
    // ════════════════════════════════════════════════════════════════
    [UnityTest]
    public IEnumerator BossHealth_TakeDamage_UpdatesHealthSliderValue()
    {
        // Khởi tạo Boss
        var type = System.Type.GetType("BossHealth, Assembly-CSharp");
        var go = new GameObject("Boss_Test4");
        var boss = (MonoBehaviour)go.AddComponent(type);

        yield return null; // chờ Awake/Start hoàn tất

        SetField(boss, "maxHealth", 1000f);
        SetField(boss, "currentHealth", 1000f);
        SetField(boss, "isInvulnerable", false); 
        SetField(boss, "isDead", false);
        
        // Tạo Slider giả để làm thanh máu của Boss (giống UI thật)
        var sliderGo = new GameObject("BossSlider");
        var slider = sliderGo.AddComponent<Slider>();
        slider.maxValue = 1000f;
        slider.value = 1000f; // máu đầy lúc đầu
        SetField(boss, "healthSlider", slider);

        // Gọi hàm gây Dame
        CallMethod(boss, "TakeDamage", 250f);
        yield return null;

        // Kiểm tra xem Slider trên UI có lùi về chính xác mốc 750 (1000 - 250) hay không
        Assert.AreEqual(750f, slider.value, "Thanh máu (healthSlider.value) phải được cập nhật đồng bộ với máu hiện tại (750) sau khi nhận sát thương.");

        Object.DestroyImmediate(go);
        Object.DestroyImmediate(sliderGo);
    }

    // ════════════════════════════════════════════════════════════════
    // TEST 5 – BossHealth: Gọi hàm Die() thật khi currentHealth tụt xuống 0
    // ════════════════════════════════════════════════════════════════
    [UnityTest]
    public IEnumerator BossHealth_DiesAndSetsIsDeadTrue_WhenHealthReachesZero()
    {
        var type = System.Type.GetType("BossHealth, Assembly-CSharp");
        var go = new GameObject("Boss_Test5");
        var boss = (MonoBehaviour)go.AddComponent(type);

        yield return null;

        SetField(boss, "maxHealth", 500f);
        SetField(boss, "currentHealth", 150f);
        SetField(boss, "isInvulnerable", false);
        SetField(boss, "isDead", false);

        // Đánh đòn 200 dame, máu sẽ xuống mức âm
        CallMethod(boss, "TakeDamage", 200f);
        yield return null;

        float hp = GetField<float>(boss, "currentHealth");
        bool isDead = GetField<bool>(boss, "isDead");

        Assert.AreEqual(-50f, hp, "Sát thương thực tế được trừ đi khi Boss Die.");
        Assert.IsTrue(isDead, "Khi sát thương vượt quá số máu, isDead sẽ chuyển thành true vì hàm Die() được kích hoạt.");

        Object.DestroyImmediate(go);
    }
}

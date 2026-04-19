using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;


public class AutomationPlayModeTests
{

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
        if (field != null) field.SetValue(target, value);
    }

    private static object GetPrivateField(object target, string fieldName)
    {
        FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
        return field?.GetValue(target);
    }

    private static void CallMethod(object target, string methodName, params object[] args)
    {
        target.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?.Invoke(target, args);
    }
    // ====================================================

    [UnityTest]
    public IEnumerator PowerManager_CutPower_DisablesLights_And_SetsPowerOffFlag()
    {
        System.Type powerManagerType = System.Type.GetType("PowerManager, Assembly-CSharp");
        Assert.IsNotNull(powerManagerType, "Không tìm thấy class PowerManager trong Assembly-CSharp.");

        GameObject pmObj = new GameObject();
        MonoBehaviour pm = (MonoBehaviour)pmObj.AddComponent(powerManagerType);

        GameObject lightObj = new GameObject();
        Light lightComp = lightObj.AddComponent<Light>();
        
        List<Light> mockLights = new List<Light> { lightComp };
        SetPrivateField(pm, "mapLights", mockLights);

        CallMethod(pm, "CutPower");

        bool isPowerOff = (bool)GetPrivateField(pm, "isPowerOff");
        Assert.IsTrue(isPowerOff, "Flag isPowerOff phải được chuyển thành true khi sập điện.");
        Assert.IsFalse(lightComp.enabled, "Đèn trong bản đồ phải bị tắt (enabled = false) sau khi gọi CutPower.");

        GameObject.DestroyImmediate(pmObj);
        GameObject.DestroyImmediate(lightObj);
        yield return null;
    }

    [UnityTest]
    public IEnumerator PowerManager_RestorePower_EnablesLights_And_ResetsTimer()
    {
        System.Type powerManagerType = System.Type.GetType("PowerManager, Assembly-CSharp");
        Assert.IsNotNull(powerManagerType, "Không tìm thấy class PowerManager trong Assembly-CSharp.");

        GameObject pmObj = new GameObject();
        MonoBehaviour pm = (MonoBehaviour)pmObj.AddComponent(powerManagerType);
        
        SetPrivateField(pm, "isPowerOff", true); // Cố tình thiết bị đang sập nguồn

        GameObject lightObj = new GameObject();
        Light lightComp = lightObj.AddComponent<Light>();
        lightComp.enabled = false;

        List<Light> mockLights = new List<Light> { lightComp };
        SetPrivateField(pm, "mapLights", mockLights);

        CallMethod(pm, "RestorePower");

        bool isPowerOff = (bool)GetPrivateField(pm, "isPowerOff");
        Assert.IsFalse(isPowerOff, "Biến isPowerOff phải reset về false sau khi khôi phục nguồn điện.");
        Assert.IsTrue(lightComp.enabled, "Hệ thống đèn phải được bật sáng trở lại.");
        
        float currentTime = (float)GetPrivateField(pm, "currentTime");
        float normalCycleDuration = (float)GetPrivateField(pm, "normalCycleDuration");
        Assert.AreEqual(normalCycleDuration, currentTime, "Thời gian chu kỳ nguồn điện không khớp, timer chưa được reset đúng!");

        GameObject.DestroyImmediate(pmObj);
        GameObject.DestroyImmediate(lightObj);
        yield return null;
    }

    [UnityTest]
    public IEnumerator FlashlightController_PickupFlashlight_EnablesLightSource_And_SetsFlag()
    {
        System.Type fcType = System.Type.GetType("FlashlightController, Assembly-CSharp");
        Assert.IsNotNull(fcType, "Không tìm thấy class FlashlightController.");

        GameObject fcObj = new GameObject();
        MonoBehaviour fc = (MonoBehaviour)fcObj.AddComponent(fcType);

        GameObject lightObj = new GameObject();
        lightObj.SetActive(false); 

        SetPrivateField(fc, "lightSource", lightObj);

        CallMethod(fc, "PickupFlashlight");

        bool hasFlashlight = (bool)GetPrivateField(fc, "hasFlashlight");
        Assert.IsTrue(hasFlashlight, "Flag hasFlashlight phải được thiết lập là true sau thao tác nhặt.");
        Assert.IsTrue(lightObj.activeSelf, "GameObject thành phần của Nguồn sáng đèn pin phải được bật lên.");

        GameObject.DestroyImmediate(fcObj);
        GameObject.DestroyImmediate(lightObj);
        yield return null;
    }

    [UnityTest]
    public IEnumerator GameManager_ToggleKeypadMode_SetsCursorVisible_And_DisablesMovement()
    {
        System.Type gmType = System.Type.GetType("GameManager, Assembly-CSharp");
        Assert.IsNotNull(gmType, "Không tìm thấy class GameManager.");

        GameObject gmObj = new GameObject();
        MonoBehaviour gm = (MonoBehaviour)gmObj.AddComponent(gmType);

        GameObject playerObj = new GameObject();
        
        // Đóng giả MonoBehaviour PlayerMovement bằng một script khác nằm trong hệ thống sẵn (Light ko được vì nó ko kế thừa MonoBehaviour, ta sẽ tạm dùng GameManager luôn)
        MonoBehaviour dummyMovement = (MonoBehaviour)playerObj.AddComponent(gmType); 
        
        SetPrivateField(gm, "playerMovementScript", dummyMovement);
        SetPrivateField(gm, "playerMesh", playerObj);

        CallMethod(gm, "ToggleKeypadMode", true);

        bool isUsingKeypad = (bool)GetPrivateField(gm, "isUsingKeypad");
        Assert.IsTrue(isUsingKeypad, "Thuộc tính isUsingKeypad nội bộ của GameManager phải là true.");
        Assert.IsFalse(dummyMovement.enabled, "Script Player Movement phải bị khóa để tránh nhân vật di chuyển khi nhập pass.");
        Assert.IsFalse(playerObj.activeSelf, "Player mesh bị giấu sai (chưa disable Mesh/FPS component).");
        Assert.AreEqual(CursorLockMode.None, Cursor.lockState, "Chuột (Cursor) không được Unlock đúng cách.");
        Assert.IsTrue(Cursor.visible, "Chuột bị ẩn trong keypad mode, người chơi sẽ không thể bấm được số!");

        GameObject.DestroyImmediate(gmObj);
        GameObject.DestroyImmediate(playerObj);
        yield return null;
    }

    [UnityTest]
    public IEnumerator Login_Logout_ClearsPlayerPrefs_And_ResetsSkipLogin()
    {
        System.Type loginType = System.Type.GetType("Login, Assembly-CSharp");
        Assert.IsNotNull(loginType, "Không tìm thấy class Login.");

        PlayerPrefs.SetString("JWT_TOKEN", "MockEnvironmentTokenValue");
        PlayerPrefs.SetString("USERNAME", "TestPlayerAccount");
        PlayerPrefs.SetInt("USER_ID", 1);
        
        FieldInfo skipLoginField = loginType.GetField("SkipLogin", BindingFlags.Public | BindingFlags.Static);
        skipLoginField.SetValue(null, true);

        // Gọi hàm tĩnh Logout
        loginType.GetMethod("Logout", BindingFlags.Public | BindingFlags.Static).Invoke(null, null);

        Assert.IsFalse(PlayerPrefs.HasKey("JWT_TOKEN"), "Token an ninh JWT_TOKEN chưa bị xóa hoàn toàn khỏi hệ thống khi log out.");
        Assert.IsFalse(PlayerPrefs.HasKey("USERNAME"), "Thông tin user name chưa bị xóa.");
        Assert.IsFalse(PlayerPrefs.HasKey("USER_ID"), "Thông tin metadata về user info chưa bị xóa.");
        
        bool isSkipLogin = (bool)skipLoginField.GetValue(null);
        Assert.IsFalse(isSkipLogin, "Biến kiểm thử SkipLogin không được hoàn nguyên, sẽ sinh lỗi phiên sau.");
        yield return null;
    }
}

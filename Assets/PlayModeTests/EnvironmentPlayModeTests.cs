using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class EnvironmentPlayModeTests
{
    // --- BIẾN TOÀN CỤC ---
    private MonoBehaviour gm;
    private GameObject itemGo;
    private GameObject keypadGo;
    private GameObject doorGo;
    private GameObject uiTextGo;

    // --- REFLECTION HELPERS ---
    private static void SetField(object target, string fieldName, object value)
    {
        target.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(target, value);
    }

    private static T GetField<T>(object target, string fieldName)
    {
        var field = target.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        return (T)field.GetValue(target);
    }

    private static void CallMethod(object target, string methodName, params object[] args)
    {
        target.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?.Invoke(target, args);
    }
    
    // Khởi tạo TMPro Text ẩn danh để ngừa lỗi CS0246 & ngừa NullReferenceException trong Code KeypadController.cs
    private Component CreateTMProMock()
    {
        var tmproType = System.Type.GetType("TMPro.TextMeshProUGUI, Unity.TextMeshPro");
        uiTextGo = new GameObject("ScreenText");
        return uiTextGo.AddComponent(tmproType);
    }

    // ════════════════════════════════════════════════════════════════
    // QUY TRÌNH CHUẨN: SETUP VÀ TEARDOWN
    // ════════════════════════════════════════════════════════════════
    [SetUp]
    public void Setup()
    {
        var gmType = System.Type.GetType("GameManager, Assembly-CSharp");
        var gmGo = new GameObject("GameManagerInstance");
        gm = (MonoBehaviour)gmGo.AddComponent(gmType);
        
        UnityEngine.UI.Image[] dummyIcons = new UnityEngine.UI.Image[5];
        GameObject[] dummyBorders = new GameObject[5];
        for (int i = 0; i < 5; i++)
        {
            var uiSlot = new GameObject("MockUI_Slot_" + i);
            uiSlot.transform.SetParent(gmGo.transform); 
            dummyIcons[i] = uiSlot.AddComponent<UnityEngine.UI.Image>();
            dummyBorders[i] = uiSlot;
        }
        SetField(gm, "slotIcons", dummyIcons);
        SetField(gm, "slotBorders", dummyBorders);
        SetField(gm, "keyCardSlots", new UnityEngine.UI.Image[10]);

        var itemTypeEnum = System.Type.GetType("InteractableObject+ItemType, Assembly-CSharp");
        System.Array inventorySlots = System.Array.CreateInstance(itemTypeEnum, 5);
        SetField(gm, "inventorySlots", inventorySlots);
    }

    [TearDown]
    public void TearDown()
    {
        if (gm != null) Object.DestroyImmediate(gm.gameObject);
        if (itemGo != null) Object.DestroyImmediate(itemGo);
        if (keypadGo != null) Object.DestroyImmediate(keypadGo);
        if (doorGo != null) Object.DestroyImmediate(doorGo);
        if (uiTextGo != null) Object.DestroyImmediate(uiTextGo);
    }

    // ════════════════════════════════════════════════════════════════
    // TẬP 1: CÁC TEST VỀ NHẶT ĐỒ (INTERACTABLE ITEM)
    // ════════════════════════════════════════════════════════════════

    [UnityTest]
    public IEnumerator Interactable_Item_IsPickedUpAndDestroyed_WhenInventoryHasSpace()
    {
        var interactableType = System.Type.GetType("InteractableObject, Assembly-CSharp");
        itemGo = new GameObject("TestBatteryItem");
        var interactable = (MonoBehaviour)itemGo.AddComponent(interactableType);

        var objTypeEnum = System.Type.GetType("InteractableObject+ObjectType, Assembly-CSharp");
        SetField(interactable, "type", System.Enum.Parse(objTypeEnum, "Item"));
        var itemTypeEnum = System.Type.GetType("InteractableObject+ItemType, Assembly-CSharp");
        SetField(interactable, "specificItemType", System.Enum.Parse(itemTypeEnum, "Battery"));

        yield return null; 

        CallMethod(interactable, "PerformAction");
        yield return null; 

        Assert.IsTrue(itemGo == null || itemGo.Equals(null), "Model 3D của vật phẩm bắt buộc phải bị Destroy() khỏi Scene sau khi đã vào túi.");
    }

    [UnityTest]
    public IEnumerator Interactable_Item_RefusesPickup_WhenInventoryIsFull()
    {
        var interactableType = System.Type.GetType("InteractableObject, Assembly-CSharp");
        itemGo = new GameObject("TestBatteryItem");
        var interactable = (MonoBehaviour)itemGo.AddComponent(interactableType);
        
        var objTypeEnum = System.Type.GetType("InteractableObject+ObjectType, Assembly-CSharp");
        SetField(interactable, "type", System.Enum.Parse(objTypeEnum, "Item"));
        var itemTypeEnum = System.Type.GetType("InteractableObject+ItemType, Assembly-CSharp");
        SetField(interactable, "specificItemType", System.Enum.Parse(itemTypeEnum, "Battery"));

        yield return null;

        System.Array inventorySlots = GetField<System.Array>(gm, "inventorySlots");
        for (int i = 0; i < 5; i++)
        {
            inventorySlots.SetValue(System.Enum.Parse(itemTypeEnum, "Meat"), i);
        }
        SetField(gm, "inventorySlots", inventorySlots);

        CallMethod(interactable, "PerformAction");
        yield return null; 

        Assert.IsFalse(itemGo == null || itemGo.Equals(null), "Vật phẩm không được phép biến mất do túi đồ đã quá tải 5 cục thịt.");
    }

    // ════════════════════════════════════════════════════════════════
    // TẬP 2: CÁC TEST VỀ MÁY QUẸT THẺ (KEYPAD CONTROLLER)
    // ════════════════════════════════════════════════════════════════

    [UnityTest]
    public IEnumerator KeypadController_RegistersInput_Internally()
    {
        var keypadType = System.Type.GetType("KeypadController, Assembly-CSharp");
        keypadGo = new GameObject("KeypadTerminal");
        var keypad = (MonoBehaviour)keypadGo.AddComponent(keypadType);

        SetField(keypad, "screenText", CreateTMProMock());
        yield return null; 

        CallMethod(keypad, "InputNumber", "1");
        CallMethod(keypad, "InputNumber", "9");
        CallMethod(keypad, "InputNumber", "9");

        string currentInput = GetField<string>(keypad, "currentInput");
        Assert.AreEqual("199", currentInput, "Bộ vi xử lý nhúng trong chip phải lưu đúng số '199' theo bộ nhớ nội bộ thay vì phụ thuộc màn hình hiển thị.");
    }

    [UnityTest]
    public IEnumerator KeypadController_ClearsInput_WhenPasswordIsIncorrect()
    {
        var keypadType = System.Type.GetType("KeypadController, Assembly-CSharp");
        keypadGo = new GameObject("KeypadTerminal");
        var keypad = (MonoBehaviour)keypadGo.AddComponent(keypadType);

        SetField(keypad, "screenText", CreateTMProMock());
        SetField(keypad, "correctPassword", "1997"); 
        yield return null; 

        CallMethod(keypad, "InputNumber", "0");
        CallMethod(keypad, "InputNumber", "0");
        CallMethod(keypad, "InputNumber", "0");
        CallMethod(keypad, "InputNumber", "0");

        bool isLocked = GetField<bool>(keypad, "isLocked");
        Assert.IsTrue(isLocked, "Hệ thống phải khoá bàn phím lại ngay lúc đang check mật khẩu.");

        // Đợi 1.6s để Coroutine hoàn thiện cả bước 1 (0.5s check pass) và bước 2 (1.0s báo Error)
        yield return new WaitForSeconds(1.6f);

        string currentInput = GetField<string>(keypad, "currentInput");
        bool isSolved = GetField<bool>(keypad, "isSolved");

        Assert.AreEqual("", currentInput, "Báo sai lỗi, hệ thống phải xoá trắng chuỗi số ngầm.");
        Assert.IsFalse(isSolved, "Cờ isSolved không được phép bật true rò rỉ bảo mật cmnl.");
    }

    [UnityTest]
    public IEnumerator KeypadController_UnlocksDoor_WhenPasswordIsCorrect()
    {
        var keypadType = System.Type.GetType("KeypadController, Assembly-CSharp");
        keypadGo = new GameObject("KeypadTerminal");
        var keypad = (MonoBehaviour)keypadGo.AddComponent(keypadType);

        SetField(keypad, "screenText", CreateTMProMock());
        SetField(keypad, "correctPassword", "1997");

        var interactableType = System.Type.GetType("InteractableObject, Assembly-CSharp");
        doorGo = new GameObject("PasswordDoor");
        var door = (MonoBehaviour)doorGo.AddComponent(interactableType);
        
        var doorCollider = doorGo.AddComponent<BoxCollider>();
        SetField(door, "doorBlockCollider", doorCollider);
        SetField(keypad, "myInteractObject", door);

        yield return null;

        CallMethod(keypad, "InputNumber", "1");
        CallMethod(keypad, "InputNumber", "9");
        CallMethod(keypad, "InputNumber", "9");
        CallMethod(keypad, "InputNumber", "7");

        // Đợi 1.6s để Coroutine hoàn thiện cả bước 1 (0.5s check pass) và bước 2 (1.0s báo chữ OPEN trước khi OpenDoor)
        yield return new WaitForSeconds(1.6f);

        bool isSolved = GetField<bool>(keypad, "isSolved");
        Assert.IsTrue(isSolved, "Mật mã 1997 được giải, bảng mạch điện tử phát tín hiệu rờ-le Mở Khoá.");

        Assert.IsFalse(doorCollider.enabled, "Xác nhận Collider tường rào của Cửa Mật Khẩu đã bị tắt điện, mở đường cho người chơi!");
    }
}

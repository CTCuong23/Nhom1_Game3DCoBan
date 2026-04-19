using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class GameManagerEditTests
{
    private GameObject gmObject;
    private MonoBehaviour gameManager;
    private Type gmType;
    private Type itemTypeEnum;

    [SetUp]
    public void Setup()
    {
        gmObject = new GameObject("GameManagerTest_EditMode");
        
        gmType = Type.GetType("GameManager, Assembly-CSharp");
        if (gmType == null)
        {
            Assert.Fail("Không tìm thấy lớp GameManager. Xin hãy kiểm tra lại Assembly Reference.");
            return;
        }

        itemTypeEnum = Type.GetType("InteractableObject+ItemType, Assembly-CSharp");
        if (itemTypeEnum == null)
        {
            Assert.Fail("Không tìm thấy enum InteractableObject.ItemType.");
            return;
        }

        gameManager = gmObject.AddComponent(gmType) as MonoBehaviour;

        // Gán GameManager.instance = gameManager thông qua Reflection
        var instanceField = gmType.GetField("instance", BindingFlags.Public | BindingFlags.Static);
        if (instanceField != null)
        {
            instanceField.SetValue(null, gameManager);
        }

        // Khởi tạo Slot Icons và Borders để tránh lỗi NullReferenceException
        var slotIcons = new Image[5];
        var slotBorders = new GameObject[5];
        for (int i = 0; i < 5; i++)
        {
            GameObject borderObj = new GameObject("Border");
            GameObject iconObj = new GameObject("Icon");
            borderObj.transform.SetParent(gmObject.transform);
            iconObj.transform.SetParent(gmObject.transform);
            slotBorders[i] = borderObj;
            slotIcons[i] = iconObj.AddComponent<Image>();
        }

        var iconsField = gmType.GetField("slotIcons", BindingFlags.Public | BindingFlags.Instance);
        if (iconsField != null) iconsField.SetValue(gameManager, slotIcons);

        var bordersField = gmType.GetField("slotBorders", BindingFlags.Public | BindingFlags.Instance);
        if (bordersField != null) bordersField.SetValue(gameManager, slotBorders);
    }

    [TearDown]
    public void Teardown()
    {
        UnityEngine.Object.DestroyImmediate(gmObject);
        if (gmType != null)
        {
            var instanceField = gmType.GetField("instance", BindingFlags.Public | BindingFlags.Static);
            if (instanceField != null) instanceField.SetValue(null, null);
        }
    }

    // --- CÁC HÀM HỖ TRỢ ĐÓNG GÓI CHIẾT XUẤT BẰNG REFLECTION ---
    private object GetItemEnum(string enumName)
    {
        return Enum.Parse(itemTypeEnum, enumName);
    }

    private Array GetInventorySlots()
    {
        var field = gmType.GetField("inventorySlots", BindingFlags.NonPublic | BindingFlags.Instance);
        return (Array)field.GetValue(gameManager);
    }

    private int GetCurrentSlotIndex()
    {
        var field = gmType.GetField("currentSlotIndex", BindingFlags.NonPublic | BindingFlags.Instance);
        return (int)field.GetValue(gameManager);
    }

    private void CallSelectSlot(int index)
    {
        var method = gmType.GetMethod("SelectSlot", BindingFlags.NonPublic | BindingFlags.Instance);
        if (method != null) method.Invoke(gameManager, new object[] { index });
    }

    private bool CallAddItemToHotbar(object itemType, Sprite icon)
    {
        var method = gmType.GetMethod("AddItemToHotbar", BindingFlags.Public | BindingFlags.Instance);
        return (bool)method.Invoke(gameManager, new object[] { itemType, icon });
    }

    // -------------------------------------------------------------------------
    // TEST 1: Kiểm tra hàm ToggleKeypadMode đổi trạng thái UI và Cursor hợp lệ
    // -------------------------------------------------------------------------
    [Test]
    public void ToggleKeypadMode_ActivatesKeypadState_AndFreesCursor()
    {
        var method = gmType.GetMethod("ToggleKeypadMode", BindingFlags.Public | BindingFlags.Instance);
        method.Invoke(gameManager, new object[] { true });

        var isUsingKeypadField = gmType.GetField("isUsingKeypad", BindingFlags.Public | BindingFlags.Instance);
        bool isUsingKeypad = (bool)isUsingKeypadField.GetValue(gameManager);

        Assert.IsTrue(isUsingKeypad, "Biến GameManager.isUsingKeypad phải được set thành true.");
        Assert.AreEqual(CursorLockMode.None, Cursor.lockState, "LockState của con trỏ chuột phải được giải phóng.");
        Assert.IsTrue(Cursor.visible, "Con trỏ chuột phải ở trạng thái có thể nhìn thấy.");
    }

    // -------------------------------------------------------------------------
    // TEST 2: Kiểm tra việc thêm item cơ bản thành công vào Slot rỗng đầu tiên
    // -------------------------------------------------------------------------
    [Test]
    public void AddItemToHotbar_AddsItemToFirstEmptySlot_AndSelectsIt()
    {
        bool result = CallAddItemToHotbar(GetItemEnum("Battery"), null);
        Array inventory = GetInventorySlots();

        Assert.IsTrue(result, "Hàm thực thi trả về true nếu thêm thành công vào Hotbar.");
        Assert.AreEqual(GetItemEnum("Battery"), inventory.GetValue(0), "Vật phẩm index 0 phải là cục Pin (Battery).");
        Assert.AreEqual(0, GetCurrentSlotIndex(), "Hệ thống phải tự động Select vào Slot item vừa nhặt.");
    }

    // -------------------------------------------------------------------------
    // TEST 3: Đảm bảo Hotbar không phá vỡ logic khi cố gắng nhặt đồ lúc túi đã đầy đồ
    // -------------------------------------------------------------------------
    [Test]
    public void AddItemToHotbar_WhenInventoryIsFull_ReturnsFalse_AndPreservesData()
    {
        for (int i = 0; i < 5; i++)
        {
            CallAddItemToHotbar(GetItemEnum("Battery"), null);
        }

        bool result = CallAddItemToHotbar(GetItemEnum("Meat"), null);

        Assert.IsFalse(result, "Hàm trả về false nếu chứa vật phẩm vượt ngưỡng max 5 slot.");
        
        Array inventory = GetInventorySlots();
        for (int i = 0; i < 5; i++)
        {
            Assert.AreEqual(GetItemEnum("Battery"), inventory.GetValue(i), "Các vật phẩm cũ ko thể phép bị đè bằng Meat.");
        }
    }

    // -------------------------------------------------------------------------
    // TEST 4: Hành vi RemoveCurrentItem dọn dẹp data và đưa icon về trạng thái ẩn
    // -------------------------------------------------------------------------
    [Test]
    public void RemoveCurrentItem_ClearsData_AndSetsAlphaToZero_AtSelectedSlot()
    {
        CallAddItemToHotbar(GetItemEnum("Battery"), null); 
        CallAddItemToHotbar(GetItemEnum("Meat"), null);    
        
        CallSelectSlot(1);

        var method = gmType.GetMethod("RemoveCurrentItem", BindingFlags.Public | BindingFlags.Instance);
        method.Invoke(gameManager, null);
        
        Array inventory = GetInventorySlots();
        Assert.AreEqual(GetItemEnum("Battery"), inventory.GetValue(0), "Slot số 0 (Pin) không bị ảnh hưởng.");
        Assert.AreEqual(GetItemEnum("None"), inventory.GetValue(1), "Slot số 1 hiện tại vừa vứt phải rỗng (None).");
        
        var iconsField = gmType.GetField("slotIcons", BindingFlags.Public | BindingFlags.Instance);
        var iconsArray = (Image[])iconsField.GetValue(gameManager);

        Color hiddenColor = new Color(1f, 1f, 1f, 0f);
        Assert.AreEqual(hiddenColor, iconsArray[1].color, "Biểu tượng ở Slot 1 trên Canvas buộc phải tàng hình (Alpha=0).");
    }

    // -------------------------------------------------------------------------
    // TEST 5: Phản ứng check vật phẩm trả về kết quả chuẩn xác theo Slot hiện tại
    // -------------------------------------------------------------------------
    [Test]
    public void IsHoldingItem_EvaluatesCorrectly_BasedOnCurrentSelectedSlot()
    {
        CallAddItemToHotbar(GetItemEnum("Meat"), null); 
        CallSelectSlot(0);

        var method = gmType.GetMethod("IsHoldingItem", BindingFlags.Public | BindingFlags.Instance);
        bool holdsMeat = (bool)method.Invoke(gameManager, new object[] { GetItemEnum("Meat") });
        bool holdsBattery = (bool)method.Invoke(gameManager, new object[] { GetItemEnum("Battery") });

        Assert.IsTrue(holdsMeat, "Người chơi rõ ràng đang cầm Meat, hàm phải trả về True.");
        Assert.IsFalse(holdsBattery, "Người chơi không cầm Battery, hàm phải trả về False.");
    }
}

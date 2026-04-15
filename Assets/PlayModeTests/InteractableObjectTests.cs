using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class InteractableObjectTests
{
    private GameObject testObj;
    private InteractableObject interactable;

    [SetUp]
    public void Setup()
    {
        // Tạo một GameObject trống và gắn script InteractableObject vào
        testObj = new GameObject("TestInteractable");
        interactable = testObj.AddComponent<InteractableObject>();
    }

    [TearDown]
    public void Teardown()
    {
        // Dọn dẹp sau khi test xong
        Object.DestroyImmediate(testObj);
    }

    // TEST 1: Kiểm tra hàm Start() có tự động tìm và bật QuestMarker không
    [UnityTest]
    public IEnumerator Start_FindsChildMarker_And_SetsItActive()
    {
        // Tạo một object con có tên chứa chữ "Marker"
        GameObject childMarker = new GameObject("QuestMarker_01");
        childMarker.transform.SetParent(testObj.transform);
        childMarker.SetActive(false); // Cố tình tắt đi

        // Chờ 1 frame để Unity chạy hàm Start() của InteractableObject
        yield return null;

        // Script phải tự tìm được child này và gán vào biến questMarker
        Assert.AreEqual(childMarker, interactable.questMarker, "Hàm Start() không tìm thấy object con có tên 'Marker'.");

        // Script phải tự bật (SetActive true) cho marker
        Assert.IsTrue(childMarker.activeSelf, "Hàm Start() không bật questMarker.");
    }

    // TEST 2: Kiểm tra nội dung Hint Text của loại Locker (Tủ trốn)
    [Test]
    public void GetHintText_WhenTypeIsLocker_ReturnsCorrectString()
    {
        // Gán Enum type
        interactable.type = InteractableObject.ObjectType.Locker;

        // Gọi hàm gốc
        string hint = interactable.GetHintText();

        Assert.AreEqual("Nhấn F để trốn", hint);
    }

    // TEST 3: Kiểm tra nội dung Hint Text của loại Item (Nhặt đồ)
    [Test]
    public void GetHintText_WhenTypeIsItem_ReturnsCorrectString()
    {
        interactable.type = InteractableObject.ObjectType.Item;
        interactable.specificItemType = InteractableObject.ItemType.Battery;

        string hint = interactable.GetHintText();

        Assert.AreEqual("Giữ E để nhặt Battery", hint);
    }

    // TEST 4: Kiểm tra nội dung Hint Text của loại Keypad
    [Test]
    public void GetHintText_WhenTypeIsKeypad_ReturnsCorrectString()
    {
        interactable.type = InteractableObject.ObjectType.Keypad;

        string hint = interactable.GetHintText();

        Assert.AreEqual("Nhấn F để nhập mật khẩu", hint);
    }

    // TEST 5: Kiểm tra nội dung Hint Text của Computer (Khi máy tính ĐÃ BẬT)
    [Test]
    public void GetHintText_WhenComputerIsOn_ReturnsEmptyString()
    {
        // Nếu isComputerOn = true, code gốc sẽ return "" ngay lập tức 
        // (Không gọi tới GameManager, rất an toàn để test)
        interactable.type = InteractableObject.ObjectType.Computer;
        interactable.isComputerOn = true;

        string hint = interactable.GetHintText();

        Assert.AreEqual("", hint, "Computer đã bật nhưng HintText không trả về chuỗi rỗng.");
    }

    // TEST 6: Kiểm tra hàm OpenDoorByKeypad() có tắt Collider chặn cửa không
    [UnityTest]
    public IEnumerator OpenDoorByKeypad_DisablesDoorBlockCollider()
    {
        // Giả lập một Collider dùng để chặn cửa
        BoxCollider blockCollider = testObj.AddComponent<BoxCollider>();
        interactable.doorBlockCollider = blockCollider;

        // Đảm bảo ban đầu collider đang bật
        Assert.IsTrue(blockCollider.enabled);

        // Gọi hàm gốc
        interactable.OpenDoorByKeypad();
        yield return null;

        // Collider phải bị vô hiệu hóa
        Assert.IsFalse(blockCollider.enabled, "Hàm OpenDoorByKeypad() không tắt doorBlockCollider.");
    }
}
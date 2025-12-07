using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Cài đặt")]
    public float interactionRange = 3f;
    public LayerMask interactableLayer;

    private float currentHoldTime = 0f;

    // Biến lưu trữ vật phẩm đang đứng gần (Xử lý trong Update cho mượt)
    private InteractableObject currentTriggerObj;

    // --- PHẦN 1: XÁC ĐỊNH ĐANG ĐỨNG GẦN CÁI GÌ ---
    private void OnTriggerEnter(Collider other)
    {
        InteractableObject obj = other.GetComponent<InteractableObject>();
        if (obj != null)
        {
            currentTriggerObj = obj; // Lưu lại đối tượng
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // Vẫn giữ để chắc chắn cập nhật nếu có thay đổi
        InteractableObject obj = other.GetComponent<InteractableObject>();
        if (obj != null && currentTriggerObj != obj)
        {
            currentTriggerObj = obj;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        InteractableObject obj = other.GetComponent<InteractableObject>();
        if (obj != null && obj == currentTriggerObj)
        {
            ResetInteraction();
            currentTriggerObj = null; // Xóa mục tiêu khi đi ra xa
        }
    }

    // --- PHẦN 2: XỬ LÝ PHÍM BẤM (Trong Update để nhạy 100%) ---
    void Update()
    {
        // Nếu không đứng gần cái gì hoặc đang dùng Keypad thì thôi
        if (currentTriggerObj == null || GameManager.instance.isUsingKeypad) return;

        InteractableObject obj = currentTriggerObj;

        // 1. LOGIC KEYPAD (Phím F) --> ƯU TIÊN SỐ 1
        if (obj.type == InteractableObject.ObjectType.Keypad)
        {
            GameManager.instance.ShowHint(obj.GetHintText());

            if (Input.GetKeyDown(KeyCode.F))
            {
                obj.PerformAction(); // Gọi Keypad Zoom ngay lập tức
            }
            return; // Xong việc Keypad, không chạy phần dưới
        }

        // 2. LOGIC CỬA & MÁY TÍNH (Phím E)

        // Kiểm tra điều kiện
        if (obj.type == InteractableObject.ObjectType.Computer && obj.isComputerOn)
        {
            GameManager.instance.HideHint(); // Đảm bảo tắt bảng gợi ý đi
            return;
        }
        if (obj.type == InteractableObject.ObjectType.Door && GameManager.instance.currentItems < 3)
        {
            GameManager.instance.ShowHint(obj.GetHintText());
            return;
        }
        if (obj.type == InteractableObject.ObjectType.Computer && !GameManager.instance.hasBattery)
        {
            GameManager.instance.ShowHint(obj.GetHintText());
            return;
        }

        // Hiện gợi ý chung
        GameManager.instance.ShowHint(obj.GetHintText());

        // Xử lý giữ phím E
        if (Input.GetKey(KeyCode.E))
        {
            currentHoldTime += Time.deltaTime;
            GameManager.instance.UpdateLoading(currentHoldTime, obj.holdTime);

            if (currentHoldTime >= obj.holdTime)
            {
                obj.PerformAction();
                ResetInteraction();
            }
        }
        else
        {
            if (currentHoldTime > 0)
            {
                currentHoldTime = 0f;
                GameManager.instance.StopLoading();
            }
        }
    }

    void ResetInteraction()
    {
        GameManager.instance.HideHint();
        GameManager.instance.StopLoading();
        currentHoldTime = 0f;
    }
}
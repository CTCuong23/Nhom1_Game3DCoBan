using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Cài đặt")]
    public float interactionRange = 3f;
    public LayerMask interactableLayer;

    private float currentHoldTime = 0f;
    private InteractableObject currentTriggerObj;

    private void OnTriggerEnter(Collider other)
    {
        InteractableObject obj = other.GetComponent<InteractableObject>();
        if (obj != null) currentTriggerObj = obj;
    }

    private void OnTriggerStay(Collider other)
    {
        InteractableObject obj = other.GetComponent<InteractableObject>();
        if (obj != null && currentTriggerObj != obj) currentTriggerObj = obj;
    }

    private void OnTriggerExit(Collider other)
    {
        InteractableObject obj = other.GetComponent<InteractableObject>();
        if (obj != null && obj == currentTriggerObj)
        {
            ResetInteraction();
            currentTriggerObj = null;
        }
    }

    void Update()
    {
        // Kiểm tra an toàn
        if (currentTriggerObj == null || GameManager.instance.isUsingKeypad) return;

        InteractableObject obj = currentTriggerObj;

        // 1. LOGIC KEYPAD (Phím F)
        if (obj.type == InteractableObject.ObjectType.Keypad)
        {
            GameManager.instance.ShowHint(obj.GetHintText());
            if (Input.GetKeyDown(KeyCode.F))
            {
                obj.PerformAction();
            }
            return;
        }

        // 2. LOGIC CỬA & MÁY TÍNH (Phím E)

        // Kiểm tra Máy tính đã bật chưa
        if (obj.type == InteractableObject.ObjectType.Computer && obj.isComputerOn)
        {
            GameManager.instance.HideHint();
            return;
        }

        // --- CẬP NHẬT LOGIC MỚI TẠI ĐÂY ---

        // Kiểm tra Cửa: Dùng biến collectedKeyCards mới (10 thẻ)
        if (obj.type == InteractableObject.ObjectType.Door && GameManager.instance.collectedKeyCards < 10)
        {
            GameManager.instance.ShowHint(obj.GetHintText());
            return; // Chưa đủ thẻ thì không cho giữ E
        }

        // Kiểm tra Máy tính: Check xem tay có đang cầm Pin không?
        if (obj.type == InteractableObject.ObjectType.Computer)
        {
            // Nếu tay KHÔNG cầm Pin -> Chỉ hiện gợi ý, không cho bấm
            if (!GameManager.instance.IsHoldingItem(InteractableObject.ItemType.Battery))
            {
                GameManager.instance.ShowHint(obj.GetHintText());
                return;
            }
        }
        // -----------------------------------

        // Hiện gợi ý (Ví dụ: "Giữ E để mở")
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
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    private float currentHoldTime = 0f;
    private InteractableObject currentZoneTarget;

    // Khi Player đi vào vùng Trigger
    private void OnTriggerStay(Collider other)
    {
        InteractableObject obj = other.GetComponent<InteractableObject>();

        // Chấp nhận cả Cửa VÀ Máy Tính
        if (obj != null && (obj.type == InteractableObject.ObjectType.Door || obj.type == InteractableObject.ObjectType.Computer))
        {
            currentZoneTarget = obj;

            // 1. LOGIC CỬA
            if (obj.type == InteractableObject.ObjectType.Door && GameManager.instance.currentItems < 3)
            {
                GameManager.instance.ShowHint(obj.GetHintText());
                return; // Thiếu đồ -> Không cho bấm
            }

            // 2. LOGIC MÁY TÍNH (MỚI)
            if (obj.type == InteractableObject.ObjectType.Computer)
            {
                // Chưa có pin -> Hiện thông báo thiếu, không cho bấm
                if (!GameManager.instance.hasBattery)
                {
                    GameManager.instance.ShowHint(obj.GetHintText());
                    return;
                }
            }

            // 3. ĐỦ ĐIỀU KIỆN -> HIỆN GỢI Ý & CHO BẤM
            GameManager.instance.ShowHint(obj.GetHintText());

            if (Input.GetKey(KeyCode.E))
            {
                currentHoldTime += Time.deltaTime;
                GameManager.instance.UpdateLoading(currentHoldTime, obj.holdTime);

                if (currentHoldTime >= obj.holdTime)
                {
                    obj.PerformAction(); // Thực hiện hành động
                    ResetDoorInteraction(); // Reset sau khi xong
                }
            }
            else
            {
                // Thả tay ra -> Reset loading
                if (currentHoldTime > 0)
                {
                    currentHoldTime = 0f;
                    GameManager.instance.StopLoading();
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        InteractableObject obj = other.GetComponent<InteractableObject>();
        if (obj != null && obj == currentZoneTarget)
        {
            ResetDoorInteraction();
        }
    }

    void ResetDoorInteraction()
    {
        GameManager.instance.HideHint();
        GameManager.instance.StopLoading();
        currentHoldTime = 0f;
        currentZoneTarget = null;
    }
}
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    // Script này chỉ chuyên xử lý việc đi vào vùng Trigger (Cửa)

    private float currentHoldTime = 0f;
    private InteractableObject currentZoneTarget; // Cửa đang đứng gần

    // Khi Player đi vào vùng Trigger (InteractionZone)
    private void OnTriggerStay(Collider other)
    {
        // 1. Kiểm tra xem cái mình đụng trúng có phải là Cửa không
        InteractableObject obj = other.GetComponent<InteractableObject>();

        // Chỉ xử lý nếu là DOOR
        if (obj != null && obj.type == InteractableObject.ObjectType.Door)
        {
            currentZoneTarget = obj;

            // 2. Logic điều kiện thắng
            if (GameManager.instance.currentItems < 3)
            {
                // Chưa đủ đồ
                GameManager.instance.ShowHint(obj.GetHintText());
                return; // Không cho giữ E
            }

            // 3. Đủ đồ -> Hiện gợi ý
            GameManager.instance.ShowHint(obj.GetHintText());

            // 4. Kiểm tra giữ E
            if (Input.GetKey(KeyCode.E))
            {
                currentHoldTime += Time.deltaTime;
                GameManager.instance.UpdateLoading(currentHoldTime, obj.holdTime);

                if (currentHoldTime >= obj.holdTime)
                {
                    obj.PerformAction(); // Mở cửa & Cutscene
                    ResetDoorInteraction(); // Reset ngay để tránh gọi nhiều lần
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

    // Khi Player đi ra khỏi vùng Trigger
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
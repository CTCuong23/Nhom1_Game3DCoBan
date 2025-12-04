using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Cài đặt")]
    public float interactionRange = 3f; // Khoảng cách tương tác (ví dụ 3 mét)
    public LayerMask interactableLayer; // Layer của vật phẩm (để tránh bắn tia vào sàn nhà/tường)

    private float currentHoldTime = 0f;
    private InteractableObject currentTarget; // Vật phẩm đang được nhìn thấy
    public float interactionRadius = 0.07f; // Bán kính của SphereCast

    void Update()
    {
        // 1. Nếu chưa bật tâm ngắm -> Reset hết và không làm gì cả
        if (!GameManager.instance.isAiming)
        {
            ResetInteraction();
            return;
        }

        // 2. Bắn tia Raycast từ giữa màn hình (Camera)
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        // Bắn tia ra xa interactionRange mét
        if (Physics.SphereCast(ray, interactionRadius, out hit, interactionRange, interactableLayer))
        {
            // Kiểm tra xem vật bị bắn trúng có script InteractableObject không
            InteractableObject obj = hit.collider.GetComponent<InteractableObject>();

            if (obj != null)
            {
                // Nếu nhìn thấy vật phẩm mới hoặc vẫn là vật phẩm cũ
                HandleInteraction(obj);
            }
            else
            {
                // Nhìn vào tường hoặc vật không tương tác được
                ResetInteraction();
            }
        }
        else
        {
            // Không nhìn thấy gì
            ResetInteraction();
        }
    }

    void HandleInteraction(InteractableObject obj)
    {
        currentTarget = obj;

        // Logic đặc biệt cho Cửa: Nếu chưa đủ đồ thì không cho tương tác
        if (obj.type == InteractableObject.ObjectType.Door && GameManager.instance.currentItems < 3)
        {
            GameManager.instance.ShowHint(obj.GetHintText()); // Chỉ hiện chữ, không cho giữ E
            return;
        }

        // Hiện gợi ý
        GameManager.instance.ShowHint(obj.GetHintText());

        // Kiểm tra giữ phím E
        if (Input.GetKey(KeyCode.E))
        {
            currentHoldTime += Time.deltaTime;
            GameManager.instance.UpdateLoading(currentHoldTime, obj.holdTime);

            if (currentHoldTime >= obj.holdTime)
            {
                obj.PerformAction(); // Thực hiện hành động nhặt/mở
                ResetInteraction();  // Reset sau khi nhặt xong
            }
        }
        else
        {
            // Nếu thả tay ra thì reset thanh loading
            if (currentHoldTime > 0)
            {
                currentHoldTime = 0f;
                GameManager.instance.StopLoading();
            }
        }
    }

    void ResetInteraction()
    {
        if (currentTarget != null || currentHoldTime > 0)
        {
            GameManager.instance.HideHint();
            GameManager.instance.StopLoading();
            currentHoldTime = 0f;
            currentTarget = null;
        }
    }
}
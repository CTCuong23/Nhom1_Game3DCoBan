using UnityEngine;

public class test : MonoBehaviour
{
    [Header("Cài đặt")]
    public float interactionRange = 2f; // Phạm vi quét (2 mét là đẹp)
    public LayerMask interactableLayer; // BẮT BUỘC PHẢI CHỌN ĐÚNG LAYER

    private float currentHoldTime = 0f;
    private InteractableObject currentTarget; // Vật phẩm đang đứng gần nhất

    void Update()
    {
        // 1. Quét xung quanh Player xem có vật phẩm nào không
        // (Tạo ra một hình cầu ảo tại vị trí Player, bán kính interactionRange)
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, interactionRange, interactableLayer);

        InteractableObject closestObj = null;
        float minDistance = Mathf.Infinity;

        // 2. Tìm vật phẩm GẦN NHẤT trong số những cái quét được
        foreach (var hit in hitColliders)
        {
            InteractableObject obj = hit.GetComponent<InteractableObject>();
            if (obj != null)
            {
                // Tính khoảng cách để xem cái nào gần hơn
                float dist = Vector3.Distance(transform.position, hit.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closestObj = obj;
                }
            }
        }

        // 3. Xử lý tương tác
        if (closestObj != null)
        {
            // Nếu tìm thấy vật (và nó gần nhất)
            HandleInteraction(closestObj);
        }
        else
        {
            // Không có vật nào xung quanh
            ResetInteraction();
        }
    }

    void HandleInteraction(InteractableObject obj)
    {
        // Nếu chuyển sang vật mới thì reset thời gian của vật cũ
        if (currentTarget != obj)
        {
            currentHoldTime = 0f;
            currentTarget = obj;
        }

        // --- LOGIC CỬA: Chưa đủ đồ thì chỉ hiện chữ, không cho bấm ---
        if (obj.type == InteractableObject.ObjectType.Door && GameManager.instance.currentItems < 3)
        {
            GameManager.instance.ShowHint(obj.GetHintText());
            return;
        }

        // Hiện gợi ý ngay lập tức
        GameManager.instance.ShowHint(obj.GetHintText());

        // Kiểm tra giữ phím E
        if (Input.GetKey(KeyCode.E))
        {
            currentHoldTime += Time.deltaTime;
            GameManager.instance.UpdateLoading(currentHoldTime, obj.holdTime);

            if (currentHoldTime >= obj.holdTime)
            {
                obj.PerformAction();
                ResetInteraction(); // Xong việc thì reset
            }
        }
        else
        {
            // Thả tay ra thì reset loading
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

    // Vẽ cái vòng tròn đỏ trong Scene để bro dễ căn chỉnh phạm vi
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
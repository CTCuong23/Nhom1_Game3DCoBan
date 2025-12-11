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

        // --- ƯU TIÊN 1: PHÍM F (Keypad & Tủ) ---
        if (obj.type == InteractableObject.ObjectType.Keypad || obj.type == InteractableObject.ObjectType.Locker)
        {
            GameManager.instance.ShowHint(obj.GetHintText());

            if (Input.GetKeyDown(KeyCode.F))
            {
                obj.PerformAction();
            }
            return; // Xong việc, không chạy phần dưới
        }

        // --- ƯU TIÊN 2: PHÍM E (Nhặt đồ, Cửa, Máy tính) ---

        // Kiểm tra điều kiện Máy tính
        if (obj.type == InteractableObject.ObjectType.Computer)
        {
            if (obj.isComputerOn)
            {
                GameManager.instance.HideHint();
                return;
            }
            if (!GameManager.instance.IsHoldingItem(InteractableObject.ItemType.Battery))
            {
                GameManager.instance.ShowHint(obj.GetHintText());
                return;
            }
        }

        // Kiểm tra điều kiện Cửa
        if (obj.type == InteractableObject.ObjectType.Door && GameManager.instance.collectedKeyCards < 10)
        {
            GameManager.instance.ShowHint(obj.GetHintText());
            return;
        }

        // Hiện gợi ý chung
        GameManager.instance.ShowHint(obj.GetHintText());

        // Xử lý Giữ Phím E (ĐÃ KHÔI PHỤC)
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
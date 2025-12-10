using UnityEngine;
using UnityEngine.Playables;
using TMPro;

public class InteractableObject : MonoBehaviour
{
    public enum ObjectType { Item, Door, Computer, Keypad }
    public ObjectType type;
    public enum ItemType { None, Battery, HealthPotion, KeyCard, Chip }

    [Header("--- QUEST MARKER ---")]
    [Tooltip("Kéo dấu chấm than vào đây. Nếu để trống, code sẽ tự tìm object tên 'QuestMarker' trong đám con.")]
    public GameObject questMarker;

    [Header("Loại vật phẩm")]
    public ItemType specificItemType;
    public Sprite itemIcon;
    public float holdTime = 2f;

    [Header("Cài đặt khác")]
    public Animator doorAnimator;
    public Collider doorBlockCollider;
    public PlayableDirector timelineDirector;
    public GameObject screenCanvas;
    public TextMeshProUGUI passwordText;
    public string passwordContent = "1997";
    public bool isComputerOn = false;
    public KeypadController keypadController;

    void Start()
    {
        // --- TỰ ĐỘNG TÌM & BẬT MARKER ---
        if (questMarker == null)
        {
            // Nếu bro quên kéo, code sẽ thử tìm đứa con nào có tên chứa chữ "Marker"
            foreach (Transform child in transform)
            {
                if (child.name.Contains("Marker") || child.name.Contains("marker"))
                {
                    questMarker = child.gameObject;
                    break;
                }
            }
        }

        // Nếu tìm thấy marker -> BẮT BUỘC BẬT NÓ LÊN
        if (questMarker != null)
        {
            questMarker.SetActive(true);

            // Fix lỗi Scale tí hon: Nếu scale quá bé (< 0.1), tự động phóng to lên
            if (questMarker.transform.lossyScale.x < 0.1f)
            {
                // Mẹo: Unparent ra để scale to, rồi parent lại (hoặc bro tự chỉnh tay cho đẹp)
                // Debug.LogWarning("⚠️ Dấu chấm than quá bé! Hãy chỉnh Scale to lên trong Inspector.");
            }
        }
    }

    public string GetHintText()
    {
        if (type == ObjectType.Item) return "Giữ E để nhặt " + specificItemType.ToString();

        if (type == ObjectType.Door)
        {
            if (GameManager.instance.collectedKeyCards >= 10) return "Đã đủ 10 thẻ\nGiữ E để thoát";
            else return $"Cần tìm thẻ ({GameManager.instance.collectedKeyCards}/10)";
        }

        if (type == ObjectType.Computer)
        {
            if (isComputerOn) return "";
            if (GameManager.instance.IsHoldingItem(ItemType.Battery)) return "Giữ E để lắp Pin";
            else return "Cần trang bị Pin (Phím 1-5)";
        }

        if (type == ObjectType.Keypad) return "Nhấn F để nhập mật khẩu";

        return "";
    }

    public void PerformAction()
    {
        GameManager.instance.StopLoading();
        GameManager.instance.HideHint();

        // --- XỬ LÝ ITEM ---
        if (type == ObjectType.Item)
        {
            bool success = false;

            if (specificItemType == ItemType.KeyCard || specificItemType == ItemType.Chip)
            {
                if (specificItemType == ItemType.KeyCard) GameManager.instance.CollectKeyCard(transform.position);
                success = true;
            }
            else
            {
                if (GameManager.instance.AddItemToHotbar(specificItemType, itemIcon)) success = true;
                else GameManager.instance.ShowHint("Túi đồ đã đầy!");
            }

            if (success) Destroy(gameObject); // Marker con tự hủy theo
        }
        // --- XỬ LÝ MÁY TÍNH ---
        else if (type == ObjectType.Computer)
        {
            if (GameManager.instance.IsHoldingItem(ItemType.Battery))
            {
                isComputerOn = true;
                if (screenCanvas != null) screenCanvas.SetActive(true);
                if (passwordText != null) passwordText.text = "PASSWORD\n" + passwordContent;
                GameManager.instance.RemoveCurrentItem();

                if (questMarker != null) Destroy(questMarker); // Xóa marker thủ công
            }
        }
        // --- XỬ LÝ CỬA ---
        else if (type == ObjectType.Door)
        {
            if (GameManager.instance.collectedKeyCards >= 10)
            {
                if (doorAnimator != null) doorAnimator.SetTrigger("Open");
                if (doorBlockCollider != null) doorBlockCollider.enabled = false;
                if (timelineDirector != null) GameManager.instance.StartEndingSequence(timelineDirector);
                else GameManager.instance.WinGame();
            }
        }
        // --- XỬ LÝ KEYPAD ---
        else if (type == ObjectType.Keypad)
        {
            if (keypadController != null) keypadController.ActivateKeypad();
        }
    }

    public void OpenDoorByKeypad()
    {
        if (doorAnimator != null) doorAnimator.SetTrigger("Open");
        if (doorBlockCollider != null) doorBlockCollider.enabled = false;
    }
}
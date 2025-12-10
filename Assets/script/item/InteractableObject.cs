using UnityEngine;
using UnityEngine.Playables;
using TMPro;

public class InteractableObject : MonoBehaviour
{
    public enum ObjectType { Item, Door, Computer, Keypad }
    public ObjectType type;

    // Phân loại Item kỹ hơn
    public enum ItemType
    {
        None,
        Battery,    // Dùng được -> Vào Hotbar
        HealthPotion, // Dùng được -> Vào Hotbar
        KeyCard,    // Cốt truyện -> Bay lên góc
        Chip        // Cốt truyện -> Bay lên góc
    }

    [Header("Loại vật phẩm")]
    public ItemType specificItemType;

    [Header("Hình ảnh (Cho Hotbar)")]
    public Sprite itemIcon; // <--- KÉO ẢNH ICON CỤC PIN VÀO ĐÂY

    [Header("Cài đặt chung")]
    public float holdTime = 2f;

    [Header("Cài đặt cho Cửa")]
    public Animator doorAnimator;
    public Collider doorBlockCollider;
    public PlayableDirector timelineDirector;

    [Header("Cài đặt cho Máy Tính")]
    public GameObject screenCanvas;
    public TextMeshProUGUI passwordText;
    public string passwordContent = "1997";
    public bool isComputerOn = false;

    [Header("Cài đặt cho Keypad")]
    public KeypadController keypadController;

    public string GetHintText()
    {
        if (type == ObjectType.Item) return "Giữ E để nhặt " + specificItemType.ToString();

        if (type == ObjectType.Door)
        {
            // Yêu cầu 10 thẻ
            if (GameManager.instance.collectedKeyCards >= 10) return "Đã đủ 10 thẻ\nGiữ E để thoát";
            else return $"Cần tìm thẻ ({GameManager.instance.collectedKeyCards}/10)";
        }

        if (type == ObjectType.Computer)
        {
            if (isComputerOn) return "";

            // Logic mới: Kiểm tra xem tay có đang cầm Pin không?
            if (GameManager.instance.IsHoldingItem(ItemType.Battery))
                return "Giữ E để lắp Pin";
            else
                return "Cần trang bị Pin (Phím 1-5)";
        }

        if (type == ObjectType.Keypad) return "Nhấn F để nhập mật khẩu";

        return "";
    }

    public void PerformAction()
    {
        GameManager.instance.StopLoading();
        GameManager.instance.HideHint();

        if (type == ObjectType.Item)
        {
            if (specificItemType == ItemType.KeyCard)
            {
                // GỌI HÀM MỚI: Truyền thêm vị trí của cái thẻ (transform.position)
                GameManager.instance.CollectKeyCard(transform.position);

                Destroy(gameObject); // Xóa thẻ sau khi nhặt
            }
            else if (specificItemType == ItemType.Chip)
            {
                // (Logic cho Chip nếu có sau này)
            }
            else
            {
                // Item dùng được (Pin, Máu) -> Vào Hotbar
                bool added = GameManager.instance.AddItemToHotbar(specificItemType, itemIcon);
                if (added) Destroy(gameObject);
                else GameManager.instance.ShowHint("Túi đồ đã đầy!");
            }
        }
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
        else if (type == ObjectType.Computer)
        {
            // Kiểm tra lần nữa cho chắc
            if (GameManager.instance.IsHoldingItem(ItemType.Battery))
            {
                isComputerOn = true;
                if (screenCanvas != null) screenCanvas.SetActive(true);
                if (passwordText != null) passwordText.text = "PASSWORD\n" + passwordContent;

                // Dùng xong thì xóa Pin khỏi tay
                GameManager.instance.RemoveCurrentItem();
            }
        }
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
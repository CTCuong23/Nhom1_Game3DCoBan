using UnityEngine;
using UnityEngine.Playables;
using TMPro;

public class InteractableObject : MonoBehaviour
{
    // Thêm Keypad vào đây
    public enum ObjectType { Item, Door, Computer, Keypad }
    public ObjectType type;

    public enum ItemType { None, Battery, KeyCard, Chip }
    [Header("Loại vật phẩm")]
    public ItemType specificItemType;

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
    public KeypadController keypadController; // Kéo script KeypadController vào đây

    public string GetHintText()
    {
        if (type == ObjectType.Item) return "Giữ E để nhặt";

        if (type == ObjectType.Door)
        {
            if (GameManager.instance.currentItems >= 3) return "Đủ vật phẩm\nGiữ E để mở";
            else return $"Chưa đủ đồ ({GameManager.instance.currentItems}/3)";
        }

        if (type == ObjectType.Computer)
        {
            if (isComputerOn) return "";
            return GameManager.instance.hasBattery ? "Giữ E để khởi động" : "Cần Pin";
        }

        // --- LOGIC KEYPAD MỚI ---
        if (type == ObjectType.Keypad)
        {
            return "Nhấn F để nhập mật khẩu";
        }

        return "";
    }

    public void PerformAction()
    {
        GameManager.instance.StopLoading();
        GameManager.instance.HideHint();

        if (type == ObjectType.Item)
        {
            GameManager.instance.CollectItemAnimated(specificItemType);
            Destroy(gameObject);
        }
        else if (type == ObjectType.Door)
        {
            if (doorAnimator != null) doorAnimator.SetTrigger("Open");
            if (doorBlockCollider != null) doorBlockCollider.enabled = false;
            if (timelineDirector != null) GameManager.instance.StartEndingSequence(timelineDirector);
            else GameManager.instance.WinGame();
        }
        else if (type == ObjectType.Computer)
        {
            isComputerOn = true;
            if (screenCanvas != null) screenCanvas.SetActive(true);
            if (passwordText != null) passwordText.text = "PASSWORD\n" + passwordContent;
        }
        // --- KÍCH HOẠT KEYPAD ---
        else if (type == ObjectType.Keypad)
        {
            if (keypadController != null)
            {
                keypadController.ActivateKeypad();
            }
        }
    }

    // Hàm mở cửa do Keypad gọi khi nhập đúng pass
    public void OpenDoorByKeypad()
    {
        if (doorAnimator != null) doorAnimator.SetTrigger("Open");
        if (doorBlockCollider != null) doorBlockCollider.enabled = false;

        // Phát âm thanh mở cửa ở đây nếu muốn
        Debug.Log("Cửa đã mở do đúng mật khẩu!");
    }
}
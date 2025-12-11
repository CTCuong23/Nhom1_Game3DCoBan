using UnityEngine;
using UnityEngine.Playables;
using TMPro;

public class InteractableObject : MonoBehaviour
{
    public enum ObjectType { Item, Door, Computer, Keypad, Locker }
    public ObjectType type;

    public enum ItemType { None, Battery, HealthPotion, KeyCard, Chip }

    [Header("--- QUEST MARKER ---")]
    public GameObject questMarker;

    [Header("Cài đặt Tủ Trốn")]
    public HidingLocker lockerScript;

    [Header("Loại vật phẩm")]
    public ItemType specificItemType;
    public Sprite itemIcon;
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

    void Start()
    {
        // Tự động tìm Marker nếu chưa kéo
        if (questMarker == null)
        {
            foreach (Transform child in transform)
            {
                if (child.name.Contains("Marker") || child.name.Contains("marker"))
                {
                    questMarker = child.gameObject;
                    break;
                }
            }
        }
        if (questMarker != null) questMarker.SetActive(true);
    }

    public virtual void Start()
    {
        // Tự động tìm Marker nếu chưa kéo
        if (questMarker == null)
        {
            foreach (Transform child in transform)
            {
                if (child.name.Contains("Marker") || child.name.Contains("marker"))
                {
                    questMarker = child.gameObject;
                    break;
                }
            }
        }
        if (questMarker != null) questMarker.SetActive(true);
    }

    public virtual string GetHintText()
    {
        if (type == ObjectType.Item) return "Giữ E để nhặt " + specificItemType.ToString();

        if (type == ObjectType.Locker) return "Nhấn F để trốn";

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

    public virtual void PerformAction()
    {
        GameManager.instance.StopLoading();
        GameManager.instance.HideHint();

        // 1. XỬ LÝ TỦ (MỚI)
        if (type == ObjectType.Locker)
        {
            if (lockerScript != null) lockerScript.EnterLocker();
        }

        // 2. XỬ LÝ ITEM (ĐÃ KHÔI PHỤC)
        else if (type == ObjectType.Item)
        {
            bool success = false;

            if (specificItemType == ItemType.KeyCard || specificItemType == ItemType.Chip)
            {
                // Item cốt truyện (Thẻ) -> Nhặt luôn
                if (specificItemType == ItemType.KeyCard)
                {
                    GameManager.instance.CollectKeyCard(transform.position);
                }
                success = true;
            }
            else
            {
                // Item dùng được (Pin, Máu) -> Vào Hotbar
                if (GameManager.instance.AddItemToHotbar(specificItemType, itemIcon))
                {
                    success = true;
                }
                else
                {
                    GameManager.instance.ShowHint("Túi đồ đã đầy!");
                }
            }

            if (success)
            {
                Destroy(gameObject); // Xóa vật phẩm (Marker con sẽ tự mất theo)
            }
        }

        // 3. XỬ LÝ MÁY TÍNH
        else if (type == ObjectType.Computer)
        {
            if (GameManager.instance.IsHoldingItem(ItemType.Battery))
            {
                isComputerOn = true;
                if (screenCanvas != null) screenCanvas.SetActive(true);
                if (passwordText != null) passwordText.text = "PASSWORD\n" + passwordContent;

                GameManager.instance.RemoveCurrentItem();

                if (questMarker != null) Destroy(questMarker);
            }
        }

        // 4. XỬ LÝ CỬA
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

        // 5. XỬ LÝ KEYPAD
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
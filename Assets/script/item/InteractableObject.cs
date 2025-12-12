using UnityEngine;
using UnityEngine.Playables;
using TMPro;

public class InteractableObject : MonoBehaviour
{
    public enum ObjectType { Item, Door, Computer, Keypad, Locker, Pet, None }
    public ObjectType type;

    // --- ĐỔI TÊN Ở ĐÂY: EnergyOrb -> BlueKey ---
    public enum ItemType { None, Battery, HealthPotion, KeyCard, Chip, BlueKey, GateCard }

    [Header("--- QUEST MARKER ---")]
    public GameObject questMarker;

    [Header("Cài đặt Tủ Trốn")]
    public HidingLocker lockerScript;

    [Header("Cài đặt Pet")]
    public PetController petScript;

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

    public virtual void Start()
    {
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

        // --- GỢI Ý CHO PET (Dùng BlueKey) ---
        if (type == ObjectType.Pet)
        {
            if (petScript != null && petScript.isTamed) return "";

            // Kiểm tra BlueKey thay vì EnergyOrb
            if (GameManager.instance.IsHoldingItem(ItemType.BlueKey))
                return "Giữ E để Thuần Phục";
            else
                return "Cần Chìa Khóa Xanh (Blue Key)";
        }
        // ------------------------------------

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
        // --- FIX LỖI LOADING KHÔNG MẤT: Tắt ngay lập tức ---
        if (GameManager.instance != null)
        {
            GameManager.instance.StopLoading();
            GameManager.instance.HideHint();
        }
        // --------------------------------------------------

        // 1. XỬ LÝ PET
        if (type == ObjectType.Pet)
        {
            if (GameManager.instance.IsHoldingItem(ItemType.BlueKey)) // Check BlueKey
            {
                if (petScript != null)
                {
                    petScript.TamePet();
                    GameManager.instance.RemoveCurrentItem();

                    Collider col = GetComponent<Collider>();
                    if (col != null) col.enabled = false;

                    if (questMarker != null) Destroy(questMarker);

                    GameManager.instance.ShowHint("Đã thu phục Pet!");
                    this.enabled = false;
                }
            }
            else
            {
                GameManager.instance.ShowHint("Cần trang bị Chìa Khóa Xanh!");
            }
        }

        // 2. XỬ LÝ TỦ
        else if (type == ObjectType.Locker)
        {
            if (lockerScript != null) lockerScript.EnterLocker();
        }

        // 3. XỬ LÝ ITEM (Nhặt đồ)
        else if (type == ObjectType.Item)
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

            if (success) Destroy(gameObject);
        }

        // 4. XỬ LÝ MÁY TÍNH
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

        // 5. XỬ LÝ CỬA
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

        // 6. XỬ LÝ KEYPAD
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
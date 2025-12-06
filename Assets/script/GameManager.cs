using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Playables;
using System.Collections; // Để dùng Coroutine

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Cài đặt UI")]
    public GameObject gameplayUI;
    public TextMeshProUGUI itemCountText; // Có thể giữ lại hoặc ẩn đi tùy bro
    public TextMeshProUGUI hintText;
    public TextMeshProUGUI winText;

    [Header("UI Inventory Mới")] // <--- PHẦN MỚI
    public Image slotBattery; // Kéo Slot_Battery vào đây
    public Image slotCard;    // Kéo Slot_Card vào đây
    public Image slotChip;    // Kéo Slot_Chip vào đây

    [Header("Animation Bay")] // <--- PHẦN MỚI
    public Image flyingIcon;  // Kéo FlyingIcon vào đây
    public float flyDuration = 1.0f; // Thời gian bay

    [Header("UI Loading Mới")]
    public GameObject interactionPanel;
    public Image loadingBar;
    public TextMeshProUGUI timerText;

    [HideInInspector] public int currentItems = 0;
    private int totalItems = 3;

    public MonoBehaviour playerMovementScript;
    public Animator playerAnimator;

    public bool isAiming = false;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Setup mặc định
        UpdateUI();
        hintText.text = "";
        winText.gameObject.SetActive(false);
        interactionPanel.SetActive(false);

        if (gameplayUI != null) gameplayUI.SetActive(true);

        // --- MỚI: Reset màu các ô về đen mờ lúc đầu ---
        ResetSlotColor(slotBattery);
        ResetSlotColor(slotCard);
        ResetSlotColor(slotChip);
    }

    void ResetSlotColor(Image img)
    {
        if (img != null) img.color = new Color(0, 0, 0, 0.5f); // Màu đen mờ
    }

    // Hàm cũ (Giữ lại để logic không bị gãy, nhưng không dùng để hiện số nữa)
    public void CollectItem()
    {
        currentItems++;
        UpdateUI();
    }

    // === HÀM MỚI: XỬ LÝ NHẶT ĐỒ CÓ HIỆU ỨNG ===
    public void CollectItemAnimated(InteractableObject.ItemType type)
    {
        Image targetSlot = null;

        // Xác định món đồ vừa nhặt thuộc ô nào
        switch (type)
        {
            case InteractableObject.ItemType.Battery: targetSlot = slotBattery; break;
            case InteractableObject.ItemType.KeyCard: targetSlot = slotCard; break;
            case InteractableObject.ItemType.Chip: targetSlot = slotChip; break;
        }

        if (targetSlot != null)
        {
            StartCoroutine(AnimateItemFly(targetSlot));
        }

        // Vẫn tăng số lượng để tính logic mở cửa
        currentItems++;
        UpdateUI();
    }

    // Coroutine xử lý bay
    IEnumerator AnimateItemFly(Image targetSlot)
    {
        if (flyingIcon != null)
        {
            // 1. Setup Icon bay
            flyingIcon.gameObject.SetActive(true);
            flyingIcon.sprite = targetSlot.sprite; // Lấy hình giống ô đích

            // Bay từ giữa màn hình
            flyingIcon.rectTransform.position = new Vector3(Screen.width / 2, Screen.height / 2, 0);
            flyingIcon.transform.localScale = Vector3.one * 1.5f; // Phóng to xíu

            Vector3 startPos = flyingIcon.rectTransform.position;
            Vector3 endPos = targetSlot.rectTransform.position;

            float time = 0;

            // 2. Bay từ từ đến đích
            while (time < flyDuration)
            {
                time += Time.deltaTime;
                float t = time / flyDuration;

                // Công thức làm mượt chuyển động (Smooth Step)
                t = t * t * (3f - 2f * t);

                flyingIcon.rectTransform.position = Vector3.Lerp(startPos, endPos, t);
                flyingIcon.transform.localScale = Vector3.Lerp(Vector3.one * 1.5f, Vector3.one, t);

                yield return null;
            }

            flyingIcon.gameObject.SetActive(false);
        }

        // 3. Đổi màu ô đích thành sáng trưng (Đã nhặt)
        if (targetSlot != null)
        {
            targetSlot.color = Color.white;
        }
    }

    void UpdateUI()
    {
        // Vẫn cập nhật text số lượng (nếu bro muốn ẩn thì setactive false trong unity)
        if (itemCountText != null)
        {
            itemCountText.text = $"Số vật phẩm: {currentItems}/{totalItems}";
            itemCountText.color = currentItems >= totalItems ? Color.green : Color.red;
        }
    }

    // ... (Giữ nguyên các hàm ShowHint, HideHint, UpdateLoading, StopLoading...) ...
    public void ShowHint(string message) { hintText.text = message; hintText.gameObject.SetActive(true); }
    public void HideHint() { hintText.text = ""; hintText.gameObject.SetActive(false); }
    public void UpdateLoading(float current, float max) { interactionPanel.SetActive(true); hintText.gameObject.SetActive(false); loadingBar.fillAmount = current / max; timerText.text = (max - current).ToString("F2") + "s"; }
    public void StopLoading() { interactionPanel.SetActive(false); loadingBar.fillAmount = 0; if (hintText.text != "") hintText.gameObject.SetActive(true); }

    // Hàm của nhóm bro (Giữ nguyên logic tắt gameplayUI)
    public void StartEndingSequence(PlayableDirector director)
    {
        if (playerMovementScript != null) playerMovementScript.enabled = false;

        // Tắt luôn cả bộ UI Inventory mới tạo
        if (gameplayUI != null)
        {
            gameplayUI.SetActive(false);
        }

        // Ẩn thủ công nếu bro để mấy cái slot ra ngoài gameplayUI
        if (slotBattery != null) slotBattery.gameObject.SetActive(false);
        if (slotCard != null) slotCard.gameObject.SetActive(false);
        if (slotChip != null) slotChip.gameObject.SetActive(false);

        if (director != null) director.Play();
    }

    public void WinGame()
    {
        winText.gameObject.SetActive(true);
        if (playerMovementScript != null) playerMovementScript.enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
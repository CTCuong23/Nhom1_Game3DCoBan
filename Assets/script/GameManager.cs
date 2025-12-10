using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Playables;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Cài đặt UI Chung")]
    public GameObject gameplayUI;
    public TextMeshProUGUI hintText;
    public TextMeshProUGUI winText;

    [Header("Hệ thống Thẻ Bài (10 Slots)")]
    public Image[] keyCardSlots;
    public Image flyingIcon;

    // --- SETTING ANIMATION MỚI ---
    [Range(0.5f, 3f)] public float flyDuration = 1.2f; // Thời gian bay
    [Range(0f, 500f)] public float curveStrength = 200f; // Độ cong (lùi lại)

    public TextMeshProUGUI keyCardCountText; // Kéo cái Text "0/3" cũ vào đây

    [HideInInspector] public int collectedKeyCards = 0;
    private int totalKeyCards = 10;

    [Header("Hệ thống Hotbar (Inventory)")]
    public GameObject[] slotBorders;
    public Image[] slotIcons;

    private InteractableObject.ItemType[] inventorySlots = new InteractableObject.ItemType[5];
    private int currentSlotIndex = 0;

    [Header("UI Loading")]
    public GameObject interactionPanel;
    public Image loadingBar;
    public TextMeshProUGUI timerText;

    // --- References ---
    public MonoBehaviour playerMovementScript;
    public bool isAiming = false;
    public bool isUsingKeypad = false;
    public GameObject playerMesh;

    void Awake() { if (instance == null) instance = this; else Destroy(gameObject); }

    void Start()
    {
        // 1. FIX LỖI TEXT 0/3: Cập nhật ngay lập tức khi vào game
        UpdateKeyCardUI();

        // 2. Reset Hotbar
        for (int i = 0; i < 5; i++)
        {
            inventorySlots[i] = InteractableObject.ItemType.None;
            if (slotIcons[i] != null) slotIcons[i].color = new Color(1, 1, 1, 0);
            if (slotBorders[i] != null) slotBorders[i].SetActive(false);
        }

        // 3. Reset Card Slots
        if (keyCardSlots != null)
        {
            foreach (var slot in keyCardSlots)
                if (slot != null) slot.color = Color.black;
        }

        SelectSlot(0);
        if (hintText != null) hintText.text = "";
        if (winText != null) winText.gameObject.SetActive(false);
        if (interactionPanel != null) interactionPanel.SetActive(false);
        if (flyingIcon != null) flyingIcon.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!isUsingKeypad && Time.timeScale != 0)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) SelectSlot(0);
            if (Input.GetKeyDown(KeyCode.Alpha2)) SelectSlot(1);
            if (Input.GetKeyDown(KeyCode.Alpha3)) SelectSlot(2);
            if (Input.GetKeyDown(KeyCode.Alpha4)) SelectSlot(3);
            if (Input.GetKeyDown(KeyCode.Alpha5)) SelectSlot(4);
        }
    }

    // --- HỆ THỐNG THẺ BÀI & ANIMATION CONG ---

    public void CollectKeyCard(Vector3 worldPosOfCard)
    {
        if (collectedKeyCards >= keyCardSlots.Length) return;

        Image targetSlot = keyCardSlots[collectedKeyCards];
        collectedKeyCards++;

        // Cập nhật số ngay khi nhặt
        UpdateKeyCardUI();

        // Bắt đầu bay
        StartCoroutine(AnimateCardFlyBezier(worldPosOfCard, targetSlot));
    }

    void UpdateKeyCardUI()
    {
        if (keyCardCountText != null)
        {
            // Sửa lại nội dung text cho đúng ý đồ mới
            keyCardCountText.text = $"THẺ TỪ: {collectedKeyCards}/10";
            keyCardCountText.color = collectedKeyCards >= 10 ? Color.green : Color.white;
        }
    }

    // --- LOGIC BAY ĐƯỜNG CONG (BEZIER CURVE) ---
    IEnumerator AnimateCardFlyBezier(Vector3 startWorldPos, Image targetSlot)
    {
        if (flyingIcon == null || targetSlot == null) yield break;

        // 1. Điểm Bắt Đầu (P0)
        Vector3 startScreenPos = Camera.main.WorldToScreenPoint(startWorldPos);

        // 2. Điểm Kết Thúc (P2)
        Vector3 endScreenPos = targetSlot.transform.position;

        // 3. Điểm Điều Khiển (P1) - Để tạo độ cong
        // Logic: Lấy trung điểm, rồi kéo lùi ra sau một chút so với hướng bay
        Vector3 direction = (endScreenPos - startScreenPos).normalized;
        Vector3 perpendicular = new Vector3(-direction.y, direction.x, 0) * curveStrength; // Vuông góc

        // Tạo điểm P1: Nằm giữa nhưng bị đẩy lùi xuống dưới/trái
        Vector3 controlPoint = startScreenPos + (endScreenPos - startScreenPos) * 0.2f - (direction * curveStrength * 1.5f);

        flyingIcon.gameObject.SetActive(true);
        flyingIcon.sprite = targetSlot.sprite;
        flyingIcon.rectTransform.position = startScreenPos;

        float time = 0;

        while (time < flyDuration)
        {
            time += Time.deltaTime;
            float t = time / flyDuration;

            // Công thức Bezier bậc 2: (1-t)^2 * P0 + 2(1-t)t * P1 + t^2 * P2
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;

            Vector3 p = (uu * startScreenPos) + (2 * u * t * controlPoint) + (tt * endScreenPos);

            flyingIcon.rectTransform.position = p;

            // Hiệu ứng Scale:
            // 0% -> 50%: Phóng to lên 2 lần (cảm giác bay lại gần màn hình)
            // 50% -> 100%: Thu nhỏ về 1 (chui vào ô)
            if (t < 0.5f)
                flyingIcon.transform.localScale = Vector3.Lerp(Vector3.one * 0.5f, Vector3.one * 2.5f, t * 2);
            else
                flyingIcon.transform.localScale = Vector3.Lerp(Vector3.one * 2.5f, Vector3.one, (t - 0.5f) * 2);

            yield return null;
        }

        flyingIcon.gameObject.SetActive(false);
        targetSlot.color = Color.white; // Bật sáng slot
    }

    // --- CÁC HÀM CŨ GIỮ NGUYÊN ---
    void SelectSlot(int index)
    {
        if (slotBorders[currentSlotIndex] != null) slotBorders[currentSlotIndex].SetActive(false);
        currentSlotIndex = index;
        if (slotBorders[currentSlotIndex] != null) slotBorders[currentSlotIndex].SetActive(true);
    }

    public bool AddItemToHotbar(InteractableObject.ItemType itemType, Sprite icon)
    {
        for (int i = 0; i < 5; i++)
        {
            if (inventorySlots[i] == InteractableObject.ItemType.None)
            {
                inventorySlots[i] = itemType;
                slotIcons[i].sprite = icon;
                slotIcons[i].color = new Color(1, 1, 1, 1);
                SelectSlot(i);
                return true;
            }
        }
        return false;
    }

    public bool IsHoldingItem(InteractableObject.ItemType typeNeeded)
    {
        return inventorySlots[currentSlotIndex] == typeNeeded;
    }

    public void RemoveCurrentItem()
    {
        inventorySlots[currentSlotIndex] = InteractableObject.ItemType.None;
        slotIcons[currentSlotIndex].color = new Color(1, 1, 1, 0);
        slotIcons[currentSlotIndex].sprite = null;
    }

    public void CollectStoryItem(InteractableObject.ItemType type) { /* Đã chuyển logic vào InteractableObject, hàm này giữ để tương thích code cũ nếu cần */ }
    public void ShowHint(string message) { if (hintText) { hintText.text = message; hintText.gameObject.SetActive(true); } }
    public void HideHint() { if (hintText) { hintText.text = ""; hintText.gameObject.SetActive(false); } }
    public void UpdateLoading(float current, float max) { if (interactionPanel) { interactionPanel.SetActive(true); hintText.gameObject.SetActive(false); loadingBar.fillAmount = current / max; timerText.text = (max - current).ToString("F2") + "s"; } }
    public void StopLoading() { if (interactionPanel) { interactionPanel.SetActive(false); loadingBar.fillAmount = 0; if (hintText.text != "") hintText.gameObject.SetActive(true); } }
    public void StartEndingSequence(PlayableDirector director) { if (playerMovementScript != null) playerMovementScript.enabled = false; if (gameplayUI != null) gameplayUI.SetActive(false); if (director != null) director.Play(); }
    public void WinGame() { if (winText) winText.gameObject.SetActive(true); if (playerMovementScript != null) playerMovementScript.enabled = false; Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
    public void ToggleKeypadMode(bool active) { isUsingKeypad = active; if (playerMovementScript != null) playerMovementScript.enabled = !active; if (playerMesh != null) playerMesh.SetActive(!active); if (active) { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; HideHint(); } else { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; } }
}
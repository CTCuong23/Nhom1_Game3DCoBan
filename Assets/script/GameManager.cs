using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Playables;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Cài đặt UI")]
    public GameObject gameplayUI;
    public TextMeshProUGUI itemCountText;
    public TextMeshProUGUI hintText;
    public TextMeshProUGUI winText;

    [Header("UI Inventory Mới")]
    public Image slotBattery;
    public Image slotCard;
    public Image slotChip;

    [Header("Animation Bay (Giữ đường cong đẹp)")]
    public Image flyingIcon;
    public float flyDuration = 1.2f; // Thời gian bay

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
        UpdateUI();
        hintText.text = "";
        winText.gameObject.SetActive(false);
        interactionPanel.SetActive(false);

        if (gameplayUI != null) gameplayUI.SetActive(true);

        ResetSlotColor(slotBattery);
        ResetSlotColor(slotCard);
        ResetSlotColor(slotChip);
    }

    void ResetSlotColor(Image img)
    {
        if (img != null) img.color = new Color(0, 0, 0, 0.5f);
    }

    public void CollectItem()
    {
        currentItems++;
        UpdateUI();
    }

    public void CollectItemAnimated(InteractableObject.ItemType type)
    {
        Image targetSlot = null;
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

        currentItems++;
        UpdateUI();
    }

    // === COROUTINE BAY ĐƯỜNG CONG (Đã bỏ VFX) ===
    IEnumerator AnimateItemFly(Image targetSlot)
    {
        if (flyingIcon != null && targetSlot != null)
        {
            // 1. SETUP BAN ĐẦU
            flyingIcon.gameObject.SetActive(true);
            flyingIcon.sprite = targetSlot.sprite;
            flyingIcon.transform.localScale = Vector3.one * 1.5f;

            // --- TÍNH TOÁN ĐƯỜNG CONG BEZIER ---
            // Điểm bắt đầu (P0): Giữa màn hình
            Vector3 p0 = new Vector3(Screen.width / 2, Screen.height / 2, 0);
            // Điểm kết thúc (P2): Vị trí ô chứa đồ
            Vector3 p2 = targetSlot.rectTransform.position;

            // Điểm điều khiển (P1) - Tạo độ cong xuống góc trái
            // Bro vẫn có thể chỉnh số 0.3f và 0.4f này nếu muốn cong nhiều/ít hơn
            Vector3 p1 = p0 + new Vector3(-Screen.width * 0.3f, -Screen.height * 0.4f, 0);

            flyingIcon.rectTransform.position = p0;

            float time = 0;

            // 2. VÒNG LẶP DI CHUYỂN
            while (time < flyDuration)
            {
                time += Time.deltaTime;
                float t = time / flyDuration;

                // Công thức Bezier bậc 2 (Giữ nguyên vì nó đẹp)
                float u = 1 - t;
                float tt = t * t;
                float uu = u * u;
                Vector3 p = (uu * p0) + (2 * u * t * p1) + (tt * p2);

                flyingIcon.rectTransform.position = p;

                // Thu nhỏ dần khi bay
                flyingIcon.transform.localScale = Vector3.Lerp(Vector3.one * 1.5f, Vector3.one, t);

                yield return null;
            }

            // 3. KẾT THÚC
            flyingIcon.gameObject.SetActive(false);

            // Làm sáng ô đích
            targetSlot.color = Color.white;
        }
    }

    // ... (Giữ nguyên các hàm còn lại) ...
    void UpdateUI() { if (itemCountText != null) { itemCountText.text = $"Số vật phẩm: {currentItems}/{totalItems}"; itemCountText.color = currentItems >= totalItems ? Color.green : Color.red; } }
    public void ShowHint(string message) { hintText.text = message; hintText.gameObject.SetActive(true); }
    public void HideHint() { hintText.text = ""; hintText.gameObject.SetActive(false); }
    public void UpdateLoading(float current, float max) { interactionPanel.SetActive(true); hintText.gameObject.SetActive(false); loadingBar.fillAmount = current / max; timerText.text = (max - current).ToString("F2") + "s"; }
    public void StopLoading() { interactionPanel.SetActive(false); loadingBar.fillAmount = 0; if (hintText.text != "") hintText.gameObject.SetActive(true); }
    public void StartEndingSequence(PlayableDirector director) { if (playerMovementScript != null) playerMovementScript.enabled = false; if (gameplayUI != null) gameplayUI.SetActive(false); if (slotBattery != null) slotBattery.gameObject.SetActive(false); if (slotCard != null) slotCard.gameObject.SetActive(false); if (slotChip != null) slotChip.gameObject.SetActive(false); if (director != null) director.Play(); }
    public void WinGame() { winText.gameObject.SetActive(true); if (playerMovementScript != null) playerMovementScript.enabled = false; Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
}
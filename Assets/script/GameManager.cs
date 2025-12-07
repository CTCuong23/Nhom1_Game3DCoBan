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

    [Header("Animation Bay")]
    public Image flyingIcon;
    public float flyDuration = 1.2f;

    [Header("UI Loading Mới")]
    public GameObject interactionPanel;
    public Image loadingBar;
    public TextMeshProUGUI timerText;

    [HideInInspector] public int currentItems = 0;
    private int totalItems = 3;

    [HideInInspector] public bool hasBattery = false;

    public MonoBehaviour playerMovementScript;
    public Animator playerAnimator;
    public bool isAiming = false;

    // --- BIẾN CHO KEYPAD ---
    public bool isUsingKeypad = false;

    [Header("Fix Lỗi Che Camera")]
    public GameObject playerMesh; // Kéo model nhân vật vào đây để ẩn đi

    void Awake() { if (instance == null) instance = this; else Destroy(gameObject); }

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

    void ResetSlotColor(Image img) { if (img != null) img.color = new Color(0, 0, 0, 0.5f); }
    public void CollectItem() { currentItems++; UpdateUI(); }
    public void CollectItemAnimated(InteractableObject.ItemType type) { Image targetSlot = null; switch (type) { case InteractableObject.ItemType.Battery: targetSlot = slotBattery; hasBattery = true; break; case InteractableObject.ItemType.KeyCard: targetSlot = slotCard; break; case InteractableObject.ItemType.Chip: targetSlot = slotChip; break; } if (targetSlot != null) StartCoroutine(AnimateItemFly(targetSlot)); currentItems++; UpdateUI(); }
    IEnumerator AnimateItemFly(Image targetSlot) { if (flyingIcon != null && targetSlot != null) { flyingIcon.gameObject.SetActive(true); flyingIcon.sprite = targetSlot.sprite; flyingIcon.transform.localScale = Vector3.one * 1.5f; Vector3 p0 = new Vector3(Screen.width / 2, Screen.height / 2, 0); Vector3 p2 = targetSlot.rectTransform.position; Vector3 p1 = p0 + new Vector3(-Screen.width * 0.3f, -Screen.height * 0.4f, 0); flyingIcon.rectTransform.position = p0; float time = 0; while (time < flyDuration) { time += Time.deltaTime; float t = time / flyDuration; float u = 1 - t; float tt = t * t; float uu = u * u; Vector3 p = (uu * p0) + (2 * u * t * p1) + (tt * p2); flyingIcon.rectTransform.position = p; flyingIcon.transform.localScale = Vector3.Lerp(Vector3.one * 1.5f, Vector3.one, t); yield return null; } flyingIcon.gameObject.SetActive(false); targetSlot.color = Color.white; } }
    void UpdateUI() { if (itemCountText != null) { itemCountText.text = $"Số vật phẩm: {currentItems}/{totalItems}"; itemCountText.color = currentItems >= totalItems ? Color.green : Color.red; } }
    public void ShowHint(string message) { hintText.text = message; hintText.gameObject.SetActive(true); }
    public void HideHint() { hintText.text = ""; hintText.gameObject.SetActive(false); }
    public void UpdateLoading(float current, float max) { interactionPanel.SetActive(true); hintText.gameObject.SetActive(false); loadingBar.fillAmount = current / max; timerText.text = (max - current).ToString("F2") + "s"; }
    public void StopLoading() { interactionPanel.SetActive(false); loadingBar.fillAmount = 0; if (hintText.text != "") hintText.gameObject.SetActive(true); }
    public void StartEndingSequence(PlayableDirector director) { if (playerMovementScript != null) playerMovementScript.enabled = false; if (gameplayUI != null) gameplayUI.SetActive(false); if (slotBattery != null) slotBattery.gameObject.SetActive(false); if (slotCard != null) slotCard.gameObject.SetActive(false); if (slotChip != null) slotChip.gameObject.SetActive(false); if (director != null) director.Play(); }
    public void WinGame() { winText.gameObject.SetActive(true); if (playerMovementScript != null) playerMovementScript.enabled = false; Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }

    // === HÀM QUẢN LÝ KEYPAD ===
    public void ToggleKeypadMode(bool active)
    {
        isUsingKeypad = active;

        // Khóa di chuyển
        if (playerMovementScript != null)
            playerMovementScript.enabled = !active;

        // Ẩn/Hiện nhân vật (Fix lỗi che bảng)
        if (playerMesh != null)
            playerMesh.SetActive(!active);

        // Chuột
        if (active)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            HideHint();
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
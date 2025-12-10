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

    [Header("UI Groups (Kéo 2 cái cha vào đây để ẩn hiện)")]
    public GameObject hotbarPanelGroup;   // Kéo cái "HotbarPanel" vào đây
    public GameObject keyCardPanelGroup;  // Kéo cái "KeyCard_Panel" vào đây

    [Header("Hệ thống Thẻ Bài (10 Slots)")]
    public Image[] keyCardSlots;
    public Image flyingIcon;

    [Range(0.5f, 3f)] public float flyDuration = 1.2f;
    [Range(0f, 500f)] public float curveStrength = 200f;

    public TextMeshProUGUI keyCardCountText;

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
        UpdateKeyCardUI();

        // Reset Hotbar
        for (int i = 0; i < 5; i++)
        {
            inventorySlots[i] = InteractableObject.ItemType.None;
            if (slotIcons[i] != null) slotIcons[i].color = new Color(1, 1, 1, 0);
            if (slotBorders[i] != null) slotBorders[i].SetActive(false);
        }

        // Reset Card Slots
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

    // --- LOGIC KEYPAD (SỬA ĐỂ ẨN UI) ---
    public void ToggleKeypadMode(bool active)
    {
        isUsingKeypad = active;

        // 1. Xử lý Nhân vật
        if (playerMovementScript != null) playerMovementScript.enabled = !active;
        if (playerMesh != null) playerMesh.SetActive(!active);

        // 2. Xử lý Chuột & Gợi ý
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

        // 3. --- ẨN/HIỆN GIAO DIỆN TÚI ĐỒ ---
        if (hotbarPanelGroup != null) hotbarPanelGroup.SetActive(!active);
        if (keyCardPanelGroup != null) keyCardPanelGroup.SetActive(!active);
        if (keyCardCountText != null) keyCardCountText.gameObject.SetActive(!active);
    }

    // --- CÁC HÀM KHÁC GIỮ NGUYÊN ---

    public void CollectKeyCard(Vector3 worldPosOfCard)
    {
        if (collectedKeyCards >= keyCardSlots.Length) return;

        Image targetSlot = keyCardSlots[collectedKeyCards];
        collectedKeyCards++;

        UpdateKeyCardUI();
        StartCoroutine(AnimateCardFlyBezier(worldPosOfCard, targetSlot));
    }

    void UpdateKeyCardUI()
    {
        if (keyCardCountText != null)
        {
            keyCardCountText.text = $"THẺ TỪ: {collectedKeyCards}/10";
            keyCardCountText.color = collectedKeyCards >= 10 ? Color.green : Color.white;
        }
    }

    IEnumerator AnimateCardFlyBezier(Vector3 startWorldPos, Image targetSlot)
    {
        if (flyingIcon == null || targetSlot == null) yield break;

        Vector3 startScreenPos = Camera.main.WorldToScreenPoint(startWorldPos);
        Vector3 endScreenPos = targetSlot.transform.position;
        Vector3 direction = (endScreenPos - startScreenPos).normalized;
        Vector3 controlPoint = startScreenPos + (endScreenPos - startScreenPos) * 0.2f - (direction * curveStrength * 1.5f);

        flyingIcon.gameObject.SetActive(true);
        flyingIcon.sprite = targetSlot.sprite;
        flyingIcon.rectTransform.position = startScreenPos;

        float time = 0;

        while (time < flyDuration)
        {
            time += Time.deltaTime;
            float t = time / flyDuration;

            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;

            Vector3 p = (uu * startScreenPos) + (2 * u * t * controlPoint) + (tt * endScreenPos);
            flyingIcon.rectTransform.position = p;

            if (t < 0.5f) flyingIcon.transform.localScale = Vector3.Lerp(Vector3.one * 0.5f, Vector3.one * 2.5f, t * 2);
            else flyingIcon.transform.localScale = Vector3.Lerp(Vector3.one * 2.5f, Vector3.one, (t - 0.5f) * 2);

            yield return null;
        }

        flyingIcon.gameObject.SetActive(false);
        targetSlot.color = Color.white;
    }

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

    public void ShowHint(string message) { if (hintText) { hintText.text = message; hintText.gameObject.SetActive(true); } }
    public void HideHint() { if (hintText) { hintText.text = ""; hintText.gameObject.SetActive(false); } }
    public void UpdateLoading(float current, float max) { if (interactionPanel) { interactionPanel.SetActive(true); hintText.gameObject.SetActive(false); loadingBar.fillAmount = current / max; timerText.text = (max - current).ToString("F2") + "s"; } }
    public void StopLoading() { if (interactionPanel) { interactionPanel.SetActive(false); loadingBar.fillAmount = 0; if (hintText.text != "") hintText.gameObject.SetActive(true); } }
    public void StartEndingSequence(PlayableDirector director) { if (playerMovementScript != null) playerMovementScript.enabled = false; if (gameplayUI != null) gameplayUI.SetActive(false); if (director != null) director.Play(); }
    public void WinGame() { if (winText) winText.gameObject.SetActive(true); if (playerMovementScript != null) playerMovementScript.enabled = false; Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
}
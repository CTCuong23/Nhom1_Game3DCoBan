using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Playables;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Cài đặt UI")]
    public GameObject gameplayUI; // <--- THÊM CÁI NÀY (Kéo cái GameplayUI vừa tạo vào đây)

    public TextMeshProUGUI itemCountText;
    public TextMeshProUGUI hintText;
    public TextMeshProUGUI winText;

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

        // Đảm bảo lúc đầu game thì UI phải hiện lên
        if (gameplayUI != null) gameplayUI.SetActive(true);
    }

    // ... (Giữ nguyên các hàm CollectItem, UpdateUI, ShowHint, UpdateLoading...) ...
    public void CollectItem()
    {
        currentItems++;
        UpdateUI();
    }

    void UpdateUI()
    {
        itemCountText.text = $"Số vật phẩm đã nhặt: {currentItems}/{totalItems}";
        if (currentItems >= totalItems) itemCountText.color = Color.green;
        else itemCountText.color = Color.red;
    }

    public void ShowHint(string message)
    {
        hintText.text = message;
        hintText.gameObject.SetActive(true);
    }

    public void HideHint()
    {
        hintText.text = "";
        hintText.gameObject.SetActive(false);
    }

    public void UpdateLoading(float currentHoldTime, float maxHoldTime)
    {
        interactionPanel.SetActive(true);
        hintText.gameObject.SetActive(false);
        float progress = currentHoldTime / maxHoldTime;
        loadingBar.fillAmount = progress;
        float timeRemaining = maxHoldTime - currentHoldTime;
        if (timeRemaining < 0) timeRemaining = 0;
        timerText.text = timeRemaining.ToString("F2") + "s";
    }

    public void StopLoading()
    {
        interactionPanel.SetActive(false);
        loadingBar.fillAmount = 0;
        if (hintText.text != "") hintText.gameObject.SetActive(true);
    }

    // === HÀM QUAN TRỌNG CẦN SỬA ===
    public void StartEndingSequence(PlayableDirector director)
    {
        // 1. Tắt di chuyển (Như cũ)
        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = false;
        }

        // 2. TẮT TOÀN BỘ UI GAMEPLAY (Mới)
        if (gameplayUI != null)
        {
            gameplayUI.SetActive(false); // Bùm! Sạch trơn màn hình
        }
        else
        {
            // Dự phòng nếu bro quên kéo object vào thì tắt lẻ tẻ
            HideHint();
            interactionPanel.SetActive(false);
            if (itemCountText != null) itemCountText.gameObject.SetActive(false);
        }

        // 3. Chạy phim
        if (director != null)
        {
            director.Play();
        }
    }

    public void WinGame()
    {
        winText.gameObject.SetActive(true); // Cái này nằm ngoài GameplayUI nên vẫn hiện ngon lành

        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = false;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
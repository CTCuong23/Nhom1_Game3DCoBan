using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pauseMenuUI;      // Panel to (chứa tất cả)
    public GameObject mainButtonsPanel; // Panel chứa các nút Resume, Settings, Quit
    public GameObject settingsPanel;    // Panel Settings (Options)

    [Header("Settings")]
    public string menuSceneName = "MainMenu";

    public static bool GameIsPaused = false;

    void Start()
    {
        // Đảm bảo khi game bắt đầu thì mọi menu đều tắt
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        GameIsPaused = false;
    }

    void Update()
    {
        // Bắt sự kiện nhấn phím ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // TRƯỜNG HỢP 1: Game đang chạy bình thường (Chưa Pause)
            if (!GameIsPaused)
            {
                Pause(); // -> Gọi hàm Pause để hiện menu
            }
            // TRƯỜNG HỢP 2: Game ĐANG PAUSE
            else
            {
                // Nếu đang mở bảng Settings thì cho phép ấn ESC để quay lại menu nút bấm chính
                if (settingsPanel != null && settingsPanel.activeSelf)
                {
                    CloseSettings();
                }
                // Nếu đang ở menu chính (có nút Resume) thì ấn ESC KHÔNG LÀM GÌ CẢ (đúng ý bạn)
            }
        }
    }

    public void Resume()
    {
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        Time.timeScale = 1f;
        GameIsPaused = false;

        // Khóa chuột và ẩn chuột khi quay lại game
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Pause()
    {
        if (pauseMenuUI != null) pauseMenuUI.SetActive(true);

        // QUAN TRỌNG: Khi mới Pause, BẮT BUỘC hiện nút bấm chính và ẨN settings đi
        if (mainButtonsPanel != null) mainButtonsPanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        Time.timeScale = 0f;
        GameIsPaused = true;

        // Mở khóa chuột để bấm menu
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void OpenSettings()
    {
        if (mainButtonsPanel != null) mainButtonsPanel.SetActive(false); // Ẩn nút chính
        if (settingsPanel != null) settingsPanel.SetActive(true);     // Hiện Settings
    }

    public void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);    // Tắt Settings
        if (mainButtonsPanel != null) mainButtonsPanel.SetActive(true);  // Hiện lại nút chính
    }

    public void LoadMenu()
    {
        Time.timeScale = 1f;
        GameIsPaused = false;
        SceneManager.LoadScene(menuSceneName);
    }
}
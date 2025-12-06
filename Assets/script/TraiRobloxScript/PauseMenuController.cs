using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pauseMenuUI;      // Panel chứa toàn bộ menu pause
    public GameObject mainButtonsPanel; // Panel chứa các nút: Resume, Settings, Quit (Tạo thêm cái này để gom nút lại cho dễ ẩn)
    public GameObject settingsPanel;    // Kéo cái Prefab UI từ Menu vào đây

    [Header("Settings")]
    public string menuSceneName = "MainMenu";

    public static bool GameIsPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameIsPaused)
            {
                // Nếu đang mở bảng setting thì ấn Esc sẽ quay lại menu pause chính
                if (settingsPanel.activeSelf)
                {
                    CloseSettings();
                }
                else
                {
                    Resume();
                }
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        settingsPanel.SetActive(false); // Đảm bảo tắt setting
        Time.timeScale = 1f;
        GameIsPaused = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);
        mainButtonsPanel.SetActive(true); // Hiện lại các nút chính
        settingsPanel.SetActive(false);   // Ẩn bảng setting đi

        Time.timeScale = 0f;
        GameIsPaused = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Hàm này gắn vào nút "Cài đặt" (hoặc nút Menu mà bạn muốn)
    public void OpenSettings()
    {
        mainButtonsPanel.SetActive(false); // Ẩn các nút Resume/Quit đi
        settingsPanel.SetActive(true);     // Hiện bảng Setting (Prefab cũ) lên
    }

    // Hàm này gắn vào nút "Back" hoặc "X" ở trong bảng SettingsPanel
    public void CloseSettings()
    {
        settingsPanel.SetActive(false);    // Tắt bảng Setting
        mainButtonsPanel.SetActive(true);  // Hiện lại menu chính
    }

    public void LoadMenu()
    {
        Time.timeScale = 1f;
        GameIsPaused = false;
        SceneManager.LoadScene(menuSceneName);
    }
}
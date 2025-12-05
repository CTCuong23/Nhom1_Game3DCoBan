using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI; // Để dùng Image

public class MainMenuController : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject optionsPanel;

    [Header("Settings")]
    public TextMeshProUGUI volumeText; // Kéo cái chữ hiển thị % vào đây
    private float currentVolume = 1.0f; // Mặc định là 1.0 (100%)

    [Header("Speaker Settings")]
    public Image speakerImage;   // Kéo cái Image Loa vào đây
    public Sprite soundOnIcon;   // Kéo hình loa BẬT vào đây
    public Sprite soundOffIcon;  // Kéo hình loa TẮT vào đây
    
    private bool isMuted = false;

    void Start()
    {
        // Lấy âm lượng hiện tại của game để hiển thị đúng ngay từ đầu
        currentVolume = AudioListener.volume;
        UpdateVolumeUI();
    }

    // --- CÁC HÀM CŨ (GIỮ NGUYÊN) ---
    public void PlayGame()
    {
        SceneManager.LoadScene("MainMap");
    }

    public void QuitGame()
    {
        Debug.Log("Đã thoát game!");
        Application.Quit();
    }

    public void OpenOptions()
    {
        optionsPanel.SetActive(true);
    }

    public void CloseOptions()
    {
        optionsPanel.SetActive(false);
    }

    // --- CÁC HÀM MỚI ĐỂ CHỈNH ÂM THANH ---

    // Hàm cho nút Tăng (+)
    public void IncreaseVolume()
    {
        currentVolume += 0.1f; // Mỗi lần bấm tăng 10%

        // Không cho tăng quá 1.0 (100%)
        if (currentVolume > 1.0f) currentVolume = 1.0f;

        ApplyVolume();
    }

    // Hàm cho nút Giảm (-)
    public void DecreaseVolume()
    {
        currentVolume -= 0.1f; // Mỗi lần bấm giảm 10%

        // Không cho giảm dưới 0
        if (currentVolume < 0.0f) currentVolume = 0.0f;

        ApplyVolume();
    }

    // Hàm phụ để áp dụng âm thanh và cập nhật chữ
    private void ApplyVolume()
    {
        AudioListener.volume = currentVolume; // Chỉnh âm thanh thật
        UpdateVolumeUI(); // Cập nhật chữ trên màn hình
    }

    private void UpdateVolumeUI()
    {
        // Đổi số float (0.8) thành số tròn (80) và thêm dấu %
        volumeText.text = Mathf.RoundToInt(currentVolume * 100) + "%";
    }

    // Hàm này gắn vào nút Loa
    public void ToggleMute()
    {
        isMuted = !isMuted; // Đảo ngược trạng thái (Đang bật -> tắt, đang tắt -> bật)

        if (isMuted)
        {
            AudioListener.volume = 0; // Tắt tiếng
            speakerImage.sprite = soundOffIcon; // Đổi sang hình loa tắt
        }
        else
        {
            AudioListener.volume = 1; // Bật tiếng (hoặc trả về biến currentVolume nếu bạn muốn xịn hơn)
            speakerImage.sprite = soundOnIcon; // Đổi về hình loa bật
        }
    }
}
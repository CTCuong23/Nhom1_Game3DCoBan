using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject optionsPanel;

    [Header("Settings")]
    public TextMeshProUGUI volumeText;
    private float currentVolume = 1.0f;

    [Header("Speaker Settings")]
    public Image speakerImage;
    public Sprite soundOnIcon;
    public Sprite soundOffIcon;

    private bool isMuted = false;
    private bool isGameplayScene = false; // Biến kiểm tra xem có đang ở trong màn chơi không

    void Start()
    {
        // Tự động kiểm tra: Nếu tìm thấy script PauseMenuController trong cảnh
        // thì nghĩa là đang ở trong Game Play (không phải Menu chính)
        if (FindObjectOfType<PauseMenuController>() != null)
        {
            isGameplayScene = true;
        }

        currentVolume = AudioListener.volume;
        UpdateVolumeUI();
    }

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
        if (optionsPanel != null) optionsPanel.SetActive(true);
    }

    public void CloseOptions()
    {
        if (optionsPanel != null) optionsPanel.SetActive(false);
    }

    public void IncreaseVolume()
    {
        currentVolume += 0.1f;
        if (currentVolume > 1.0f) currentVolume = 1.0f;
        ApplyVolume();
    }

    public void DecreaseVolume()
    {
        currentVolume -= 0.1f;
        if (currentVolume < 0.0f) currentVolume = 0.0f;
        ApplyVolume();
    }

    private void ApplyVolume()
    {
        AudioListener.volume = currentVolume;
        UpdateVolumeUI();
    }

    private void UpdateVolumeUI()
    {
        if (volumeText != null)
            volumeText.text = Mathf.RoundToInt(currentVolume * 100) + "%";
    }

    public void ToggleMute()
    {
        isMuted = !isMuted;

        if (isMuted)
        {
            AudioListener.volume = 0;
            if (speakerImage != null && soundOffIcon != null) speakerImage.sprite = soundOffIcon;
        }
        else
        {
            AudioListener.volume = 1;
            if (speakerImage != null && soundOnIcon != null) speakerImage.sprite = soundOnIcon;
        }
    }
}
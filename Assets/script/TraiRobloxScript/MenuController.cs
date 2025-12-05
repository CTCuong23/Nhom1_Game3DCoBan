using UnityEngine;
using UnityEngine.SceneManagement; // Bắt buộc phải có dòng này để chuyển cảnh

public class MainMenuController : MonoBehaviour
{
    // Hàm này sẽ chạy khi bấm nút Play
    public void PlayGame()
    {
        // Load scene có tên là "MainMap" như bạn yêu cầu
        SceneManager.LoadScene("MainMap");
    }

    // Hàm này sẽ chạy khi bấm nút Exit
    public void QuitGame()
    {
        Debug.Log("Đã thoát game!"); // Dòng này để test trong Editor cho biết là code đã chạy
        Application.Quit();
    }
}
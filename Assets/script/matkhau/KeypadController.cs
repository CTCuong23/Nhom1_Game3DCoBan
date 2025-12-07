using UnityEngine;
using TMPro;
using System.Collections;
using Cinemachine;

public class KeypadController : MonoBehaviour
{
    [Header("Cấu hình")]
    public TextMeshProUGUI screenText;
    public CinemachineVirtualCamera keypadCamera;
    public string correctPassword = "1997";

    [Header("Kết nối")]
    public InteractableObject myInteractObject;

    private string currentInput = "";
    private bool isLocked = false;
    private bool isSolved = false; // Biến mới: Đánh dấu đã giải xong chưa

    void Start()
    {
        if (keypadCamera != null) keypadCamera.Priority = 0;
        UpdateScreen();
    }

    void Update()
    {
        // Chỉ xử lý input khi đang dùng Keypad
        if (GameManager.instance.isUsingKeypad)
        {
            // Chỉ dùng phím F để thoát
            if (Input.GetKeyDown(KeyCode.F))
            {
                ExitKeypad();
            }
        }
    }

    public void ActivateKeypad()
    {
        if (keypadCamera != null) keypadCamera.Priority = 20;
        GameManager.instance.ToggleKeypadMode(true);

        // Nếu đã giải xong rồi thì hiện chữ OPEN luôn, không reset nữa
        if (isSolved)
        {
            screenText.text = "OPEN";
            screenText.color = Color.green;
        }
        else
        {
            currentInput = "";
            screenText.color = Color.white;
            UpdateScreen();
        }
    }

    public void ExitKeypad()
    {
        // FIX LỖI 1: Gọi Coroutine để tắt từ từ, tránh xung đột với PlayerInteraction
        StartCoroutine(ExitRoutine());
    }

    IEnumerator ExitRoutine()
    {
        // 1. Hạ Priority Camera xuống trước
        if (keypadCamera != null) keypadCamera.Priority = 0;

        // 2. Chờ hết frame hiện tại (Để đảm bảo PlayerInteraction không bắt dính phím F lần nữa)
        yield return new WaitForEndOfFrame();

        // 3. Mới báo cho GameManager biết là đã tắt
        GameManager.instance.ToggleKeypadMode(false);
    }

    public void InputNumber(string num)
    {
        // Nếu đang khóa hoặc đã giải xong rồi thì không cho nhập số nữa
        if (isLocked || isSolved) return;
        if (currentInput.Length >= 4) return;

        currentInput += num;
        UpdateScreen();

        if (currentInput.Length == 4)
        {
            StartCoroutine(CheckPassword());
        }
    }

    void UpdateScreen()
    {
        if (screenText != null && !isSolved) screenText.text = currentInput;
    }

    IEnumerator CheckPassword()
    {
        isLocked = true;
        yield return new WaitForSeconds(0.5f);

        if (currentInput == correctPassword)
        {
            screenText.color = Color.green;
            screenText.text = "OPEN";
            isSolved = true; // FIX LỖI 2: Đánh dấu là xong, thay vì tắt script

            yield return new WaitForSeconds(1f);

            ExitKeypad();

            if (myInteractObject != null)
            {
                myInteractObject.OpenDoorByKeypad();
            }

            // BỎ DÒNG NÀY: this.enabled = false; 
            // Để hàm Update vẫn chạy -> Vẫn nhấn F thoát được nếu lỡ vào lại
            isLocked = false;
        }
        else
        {
            screenText.color = Color.red;
            screenText.text = "ERROR";

            yield return new WaitForSeconds(1f);

            currentInput = "";
            screenText.color = Color.white;
            UpdateScreen();
            isLocked = false;
        }
    }
}
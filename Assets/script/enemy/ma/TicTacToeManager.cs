using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine.SceneManagement; // Để load lại game nếu cần

public class TicTacToeManager : MonoBehaviour
{
    [Header("Setup UI")]
    public Button[] buttons;
    public TextMeshProUGUI[] buttonTexts;
    public GameObject gameUI; // UI súng đạn

    [Header("Setup Cinematic")]
    public CinemachineVirtualCamera caroCamera;
    public GameObject playerModel; // Kéo cái PlayerArmature hoặc PlayerMesh vào đây

    [Header("Setup Con Ma")]
    public Animator ghostAnimator; // Kéo Animator con ma vào
    public AudioSource ghostAudioSource; // Kéo AudioSource của con ma vào
    public AudioClip laughSound;   // Kéo tiếng cười vào
    public AudioClip jumpscareSound; // Kéo tiếng hù (nếu có) vào

    [Header("Game Over & Win")]
    public GameObject keyPrefab;
    public Transform dropPoint;
    public GameObject deathPanel; // Kéo cái Panel YOU DIED vào đây
    public TextMeshProUGUI deathText; // (Tùy chọn) Để đổi chữ

    // Logic game
    private string[] board = new string[9];
    private bool isPlayerTurn = true;
    private bool gameActive = false;
    private int movesCount = 0;

    void Start()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i;
            buttons[i].onClick.AddListener(() => OnPlayerClick(index));
            buttonTexts[i].text = "";
        }
        if (caroCamera != null) caroCamera.Priority = 0;
        if (deathPanel != null) deathPanel.SetActive(false);
    }

    public void StartGame()
    {
        if (gameActive) return;
        gameActive = true;

        // 1. ZOOM CAM VÀO
        caroCamera.Priority = 20;

        // 2. ẨN PLAYER ĐI (FIX LỖI CHE CAM)
        if (playerModel != null) playerModel.SetActive(false);

        // 3. ẨN UI GAME
        if (gameUI != null) gameUI.SetActive(false);

        // 4. HIỆN CHUỘT
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        ResetBoard();
    }

    void OnPlayerClick(int index)
    {
        if (!isPlayerTurn || !gameActive || board[index] != "") return;

        MakeMove(index, "X");

        if (CheckWin("X"))
        {
            EndGame(true); // Player Thắng
        }
        else if (movesCount >= 9)
        {
            StartCoroutine(ResetWithDelay()); // Hòa
        }
        else
        {
            isPlayerTurn = false;
            StartCoroutine(GhostMove());
        }
    }

    IEnumerator GhostMove()
    {
        yield return new WaitForSeconds(1f); // Ma suy nghĩ

        List<int> emptySlots = new List<int>();
        for (int i = 0; i < 9; i++)
        {
            if (board[i] == "") emptySlots.Add(i);
        }

        if (emptySlots.Count > 0 && gameActive)
        {
            // MA ĐÁNH
            int randomIndex = emptySlots[Random.Range(0, emptySlots.Count)];
            MakeMove(randomIndex, "O");

            // --- MA CƯỜI KHI ĐÁNH ---
            if (ghostAudioSource != null && laughSound != null)
            {
                ghostAudioSource.PlayOneShot(laughSound);
            }

            if (CheckWin("O"))
            {
                // --- MA THẮNG -> CHẾT ---
                StartCoroutine(TriggerJumpscare());
            }
            else if (movesCount >= 9)
            {
                StartCoroutine(ResetWithDelay());
            }
            else
            {
                isPlayerTurn = true;
            }
        }
    }

    // --- XỬ LÝ MA GIẾT NGƯỜI ---
    IEnumerator TriggerJumpscare()
    {
        gameActive = false; // Khóa game lại
        Debug.Log("MA THẮNG! CHUẨN BỊ CHẾT!");

        // 1. Chơi Animation Jumpscare
        if (ghostAnimator != null)
        {
            ghostAnimator.SetTrigger("Kill");
        }

        // 2. Chơi tiếng Hù (nếu có)
        if (ghostAudioSource != null && jumpscareSound != null)
        {
            ghostAudioSource.Stop(); // Tắt tiếng cười
            ghostAudioSource.PlayOneShot(jumpscareSound);
        }

        // 3. Đợi Animation chạy xong (ví dụ animation dài 2 giây)
        yield return new WaitForSeconds(2f);

        // 4. Hiện bảng Game Over
        ShowDeathPanel();
    }

    void ShowDeathPanel()
    {
        // Hiện chuột để bấm nút Restart
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (deathPanel != null)
        {
            deathPanel.SetActive(true);
            // Dùng lại hàm Pause của GameController để đóng băng mọi thứ khác
            Script.UI.GameController.PauseGame(deathPanel);
        }

        if (deathText != null) deathText.text = "BẠN ĐÃ THUA TRÍ TUỆ CỦA MA!";
    }

    // --- LOGIC CŨ GIỮ NGUYÊN ---
    void MakeMove(int index, string mark)
    {
        board[index] = mark;
        buttonTexts[index].text = mark;
        buttonTexts[index].color = (mark == "X") ? Color.green : Color.red;
        movesCount++;
    }

    bool CheckWin(string mark)
    {
        if (CheckLine(0, 1, 2, mark)) return true;
        if (CheckLine(3, 4, 5, mark)) return true;
        if (CheckLine(6, 7, 8, mark)) return true;
        if (CheckLine(0, 3, 6, mark)) return true;
        if (CheckLine(1, 4, 7, mark)) return true;
        if (CheckLine(2, 5, 8, mark)) return true;
        if (CheckLine(0, 4, 8, mark)) return true;
        if (CheckLine(2, 4, 6, mark)) return true;
        return false;
    }

    bool CheckLine(int a, int b, int c, string mark)
    {
        return board[a] == mark && board[b] == mark && board[c] == mark;
    }

    IEnumerator ResetWithDelay()
    {
        yield return new WaitForSeconds(1.5f);
        ResetBoard();
    }

    void ResetBoard()
    {
        for (int i = 0; i < 9; i++)
        {
            board[i] = "";
            buttonTexts[i].text = "";
        }
        isPlayerTurn = true;
        movesCount = 0;
    }

    void EndGame(bool playerWin)
    {
        if (playerWin)
        {
            // Logic thắng giữ nguyên
            if (keyPrefab != null) Instantiate(keyPrefab, dropPoint.position, Quaternion.identity);
            CloseGame();
        }
    }

    void CloseGame()
    {
        caroCamera.Priority = 0;

        // HIỆN LẠI PLAYER
        if (playerModel != null) playerModel.SetActive(true);
        if (gameUI != null) gameUI.SetActive(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        this.enabled = false;
        gameObject.SetActive(false);
    }
}
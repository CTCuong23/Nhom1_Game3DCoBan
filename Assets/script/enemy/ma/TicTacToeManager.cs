using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;

public class TicTacToeManager : MonoBehaviour
{
    [Header("Setup UI")]
    public Button[] buttons;
    public TextMeshProUGUI[] buttonTexts;
    public GameObject gameUI;

    [Header("Setup Cinematic")]
    public CinemachineVirtualCamera caroCamera; // Cam bàn cờ
    public CinemachineVirtualCamera faceCamera; // <--- KÉO CAM ZOOM MẶT VÀO ĐÂY
    public GameObject playerModel;

    [Header("Setup Con Ma")]
    public Animator ghostAnimator;
    public AudioSource ghostAudioSource;
    public AudioClip laughSound;
    public AudioClip jumpscareSound;

    [Header("Game Over & Win")]
    public GameObject keyPrefab;
    public Transform dropPoint;
    public GameObject deathPanel;
    public TextMeshProUGUI deathText;

    // Biến nội bộ
    private string[] board = new string[9];
    private bool isPlayerTurn = true;
    public bool gameActive = false;
    private int movesCount = 0;
    [HideInInspector] public float lastCloseTime = 0f;
    private PlayerStats playerStats;

    void Start()
    {
        // Tự động tìm Text
        if (buttonTexts == null || buttonTexts.Length != buttons.Length)
        {
            buttonTexts = new TextMeshProUGUI[buttons.Length];
            for (int i = 0; i < buttons.Length; i++)
                if (buttons[i] != null) buttonTexts[i] = buttons[i].GetComponentInChildren<TextMeshProUGUI>();
        }

        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i;
            buttons[i].onClick.AddListener(() => OnPlayerClick(index));
            if (buttonTexts[i] != null) buttonTexts[i].text = "";
        }

        // Reset Priority Camera
        if (caroCamera != null) caroCamera.Priority = 0;
        if (faceCamera != null) faceCamera.Priority = 0; // Đảm bảo cam mặt tắt lúc đầu

        if (deathPanel != null) deathPanel.SetActive(false);

        // Tìm script máu
        if (playerModel != null) playerStats = playerModel.GetComponentInParent<PlayerStats>();
        if (playerStats == null) playerStats = FindObjectOfType<PlayerStats>();
    }

    void Update()
    {
        // F để thoát (Chỉ khi game đang chạy và không bị Pause)
        if (gameActive && Input.GetKeyDown(KeyCode.F) && Time.timeScale != 0)
        {
            CloseGame();
        }

        if (gameActive && Cursor.lockState != CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void StartGame()
    {
        if (gameActive) return;
        gameActive = true;

        caroCamera.Priority = 20; // Bật cam bàn cờ
        if (faceCamera != null) faceCamera.Priority = 0; // Tắt cam mặt

        if (playerModel != null)
        {
            Renderer[] renderers = playerModel.GetComponentsInChildren<Renderer>();
            foreach (var r in renderers) r.enabled = false;
        }

        if (gameUI != null) gameUI.SetActive(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        ResetBoard();
    }

    void OnPlayerClick(int index)
    {
        if (!isPlayerTurn || !gameActive || board[index] != "" || Time.timeScale == 0) return;

        MakeMove(index, "X");

        if (CheckWin("X")) EndGame(true);
        else if (movesCount >= 9) StartCoroutine(ResetWithDelay());
        else { isPlayerTurn = false; StartCoroutine(GhostMove()); }
    }

    IEnumerator GhostMove()
    {
        yield return new WaitForSeconds(1f);

        List<int> emptySlots = new List<int>();
        for (int i = 0; i < 9; i++) if (board[i] == "") emptySlots.Add(i);

        if (emptySlots.Count > 0 && gameActive)
        {
            int randomIndex = emptySlots[Random.Range(0, emptySlots.Count)];
            MakeMove(randomIndex, "O");

            if (ghostAudioSource != null && laughSound != null) ghostAudioSource.PlayOneShot(laughSound);

            if (CheckWin("O"))
            {
                StartCoroutine(TriggerJumpscare());
            }
            else if (movesCount >= 9) StartCoroutine(ResetWithDelay());
            else isPlayerTurn = true;
        }
    }

    // --- LOGIC JUMPSCARE & LAST HIT MỚI ---
    IEnumerator TriggerJumpscare()
    {
        // 1. Kiểm tra xem cú này có chết không?
        bool isLastHit = false;
        if (playerStats != null)
        {
            // Nếu máu hiện tại <= 50 thì trừ 50 nữa là đi đời
            if (playerStats.currentHealth <= 50f) isLastHit = true;
        }

        // 2. Nếu là Last Hit -> Zoom vào mặt
        if (isLastHit && faceCamera != null)
        {
            Debug.Log("LAST HIT! ZOOM MẶT!");
            faceCamera.Priority = 30; // Cao hơn cả CaroCamera (20)
            caroCamera.Priority = 0;
        }

        Debug.Log("MA THẮNG! Animation Jumpscare...");

        // 3. Chơi Animation & Âm thanh
        if (ghostAnimator != null) ghostAnimator.SetTrigger("Kill");

        if (ghostAudioSource != null && jumpscareSound != null)
        {
            ghostAudioSource.Stop();
            ghostAudioSource.PlayOneShot(jumpscareSound);
        }

        // 4. Đợi diễn xong (2 giây)
        yield return new WaitForSecondsRealtime(2f);

        // 5. Xử lý Hậu quả
        if (playerStats != null)
        {
            // Trừ máu (Nếu last hit -> Máu về 0 -> PlayerStats tự gọi Die -> Tự hiện DeathPanel)
            playerStats.TakeDamage(50f);

            // Nếu KHÔNG PHẢI last hit (vẫn sống) -> Thoát game cờ để chạy
            if (!isLastHit)
            {
                Debug.Log("Vẫn sống! Chạy đi!");
                CloseGame();
            }
            else
            {
                // NẾU CHẾT RỒI:
                // Không gọi CloseGame() nữa (để giữ camera ở mặt con ma)
                // PlayerStats.Die() sẽ lo việc hiện bảng và Pause game
                Debug.Log("Đã chết! Game Over!");
            }
        }
        else
        {
            // Fallback nếu lỗi script
            ShowDeathPanel();
        }
    }

    // ... (Giữ nguyên các hàm ShowDeathPanel, MakeMove, CheckWin, CheckLine, ResetWithDelay, ResetBoard, EndGame...) 
    // COPY LẠI HÀM CŨ NẾU CẦN, HOẶC GIỮ NGUYÊN FILE CŨ CHỈ THAY ĐOẠN TRÊN

    // ... (Phần dưới giữ nguyên như cũ) ...
    void ShowDeathPanel() { /* Code cũ */ if (gameUI != null) gameUI.SetActive(true); Cursor.lockState = CursorLockMode.None; Cursor.visible = true; if (deathPanel != null) { if (gameUI != null && deathPanel.transform.parent == gameUI.transform) deathPanel.transform.SetParent(gameUI.transform.parent); deathPanel.SetActive(true); deathPanel.transform.SetAsLastSibling(); Script.UI.GameController.PauseGame(deathPanel); } if (deathText != null) deathText.text = "BẠN ĐÃ THUA TRÍ TUỆ CỦA MA!"; }
    void MakeMove(int index, string mark) { board[index] = mark; buttonTexts[index].text = mark; buttonTexts[index].color = (mark == "X") ? Color.green : Color.red; movesCount++; }
    bool CheckWin(string mark) { if (CheckLine(0, 1, 2, mark) || CheckLine(3, 4, 5, mark) || CheckLine(6, 7, 8, mark) || CheckLine(0, 3, 6, mark) || CheckLine(1, 4, 7, mark) || CheckLine(2, 5, 8, mark) || CheckLine(0, 4, 8, mark) || CheckLine(2, 4, 6, mark)) return true; return false; }
    bool CheckLine(int a, int b, int c, string mark) { return board[a] == mark && board[b] == mark && board[c] == mark; }
    IEnumerator ResetWithDelay() { yield return new WaitForSeconds(1.5f); ResetBoard(); }
    void ResetBoard() { for (int i = 0; i < 9; i++) { board[i] = ""; buttonTexts[i].text = ""; } isPlayerTurn = true; movesCount = 0; }
    void EndGame(bool playerWin) { if (playerWin) { if (keyPrefab != null) Instantiate(keyPrefab, dropPoint.position, Quaternion.identity); CloseGame(); } }

    void CloseGame()
    {
        lastCloseTime = Time.time;
        gameActive = false;

        caroCamera.Priority = 0;
        if (faceCamera != null) faceCamera.Priority = 0; // Tắt cam mặt luôn

        if (ghostAnimator != null)
        {
            ghostAnimator.ResetTrigger("Kill");
            ghostAnimator.Play("Idle");
        }

        if (playerModel != null)
        {
            Renderer[] renderers = playerModel.GetComponentsInChildren<Renderer>();
            foreach (var r in renderers) r.enabled = true;
        }

        if (gameUI != null) gameUI.SetActive(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        ResetBoard();
    }
}
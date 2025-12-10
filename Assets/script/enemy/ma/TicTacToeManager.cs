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
    public CinemachineVirtualCamera caroCamera;
    public CinemachineVirtualCamera faceCamera;
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

    private string[] board = new string[9];
    private bool isPlayerTurn = true;
    public bool gameActive = false;
    private int movesCount = 0;

    [HideInInspector] public float lastCloseTime = 0f;

    // --- BIẾN MỚI ĐỂ FIX LỖI ---
    private float gameStartTime = 0f; // Thời điểm bắt đầu game
    // ---------------------------

    private PlayerStats playerStats;

    void Start()
    {
        // Auto tìm Text
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

        if (caroCamera != null) caroCamera.Priority = 0;
        if (faceCamera != null) faceCamera.Priority = 0;
        if (deathPanel != null) deathPanel.SetActive(false);

        if (playerModel != null) playerStats = playerModel.GetComponentInParent<PlayerStats>();
        if (playerStats == null) playerStats = FindObjectOfType<PlayerStats>();
    }

    void Update()
    {
        // --- ĐOẠN FIX QUAN TRỌNG ---
        // Chỉ cho phép thoát nếu game đã chạy được hơn 0.5 giây
        // (Để tránh xung đột với nút F lúc bắt đầu)
        if (gameActive && Input.GetKeyDown(KeyCode.F) && Time.timeScale != 0)
        {
            if (Time.time > gameStartTime + 0.5f)
            {
                CloseGame();
            }
        }
        // ---------------------------

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
        gameStartTime = Time.time; // --- GHI LẠI GIỜ BẮT ĐẦU ---

        caroCamera.Priority = 20;
        if (faceCamera != null) faceCamera.Priority = 0;

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

    // ... (Giữ nguyên phần còn lại không thay đổi) ...

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

            if (CheckWin("O")) StartCoroutine(TriggerJumpscare());
            else if (movesCount >= 9) StartCoroutine(ResetWithDelay());
            else isPlayerTurn = true;
        }
    }

    IEnumerator TriggerJumpscare()
    {
        bool isLastHit = false;
        if (playerStats != null)
        {
            if (playerStats.currentHealth <= 50f) isLastHit = true;
        }

        if (isLastHit && faceCamera != null)
        {
            faceCamera.Priority = 30;
            caroCamera.Priority = 0;
        }

        if (ghostAnimator != null) ghostAnimator.SetTrigger("Kill");
        if (ghostAudioSource != null && jumpscareSound != null)
        {
            ghostAudioSource.Stop();
            ghostAudioSource.PlayOneShot(jumpscareSound);
        }

        yield return new WaitForSecondsRealtime(2f);

        if (playerStats != null)
        {
            playerStats.TakeDamage(50f);
            if (!isLastHit) CloseGame();
        }
        else ShowDeathPanel();
    }

    void ShowDeathPanel()
    {
        if (gameUI != null) gameUI.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (deathPanel != null)
        {
            if (gameUI != null && deathPanel.transform.parent == gameUI.transform)
                deathPanel.transform.SetParent(gameUI.transform.parent);
            deathPanel.SetActive(true);
            deathPanel.transform.SetAsLastSibling();
            Script.UI.GameController.PauseGame(deathPanel);
        }
        if (deathText != null) deathText.text = "BẠN ĐÃ THUA TRÍ TUỆ CỦA MA!";
    }

    void MakeMove(int index, string mark)
    {
        board[index] = mark;
        buttonTexts[index].text = mark;
        buttonTexts[index].color = (mark == "X") ? Color.green : Color.red;
        movesCount++;
    }

    bool CheckWin(string mark)
    {
        if (CheckLine(0, 1, 2, mark) || CheckLine(3, 4, 5, mark) || CheckLine(6, 7, 8, mark) ||
            CheckLine(0, 3, 6, mark) || CheckLine(1, 4, 7, mark) || CheckLine(2, 5, 8, mark) ||
            CheckLine(0, 4, 8, mark) || CheckLine(2, 4, 6, mark)) return true;
        return false;
    }

    bool CheckLine(int a, int b, int c, string mark) { return board[a] == mark && board[b] == mark && board[c] == mark; }
    IEnumerator ResetWithDelay() { yield return new WaitForSeconds(1.5f); ResetBoard(); }
    void ResetBoard() { for (int i = 0; i < 9; i++) { board[i] = ""; buttonTexts[i].text = ""; } isPlayerTurn = true; movesCount = 0; }
    void EndGame(bool playerWin) { if (playerWin) { if (keyPrefab != null) Instantiate(keyPrefab, dropPoint.position, Quaternion.identity); CloseGame(); } }

    void CloseGame()
    {
        lastCloseTime = Time.time;
        gameActive = false;

        caroCamera.Priority = 0;
        if (faceCamera != null) faceCamera.Priority = 0;

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
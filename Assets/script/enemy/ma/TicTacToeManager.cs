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

    // Biến trạng thái
    private string[] board = new string[9];
    private bool isPlayerTurn = true;
    public bool gameActive = false; // Public để GhostInteract đọc được
    private int movesCount = 0;
    [HideInInspector] public float lastCloseTime = 0f;
    private PlayerStats playerStats;

    void Start()
    {
        // Setup nút bấm
        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i;
            buttons[i].onClick.AddListener(() => OnPlayerClick(index));
            buttonTexts[i].text = "";
        }
        
        if (caroCamera != null) caroCamera.Priority = 0;
        // Đảm bảo bảng chết tắt lúc đầu
        if (deathPanel != null) deathPanel.SetActive(false);
        if (playerModel != null) 
             playerStats = playerModel.GetComponent<PlayerStats>();
        else // Fallback nếu playerModel là object con, tìm ở cha
             playerStats = FindObjectOfType<PlayerStats>();
    }

    void Update()
    {
        // --- CHỨC NĂNG MỚI: NHẤN F ĐỂ THOÁT ---
        // Chỉ hoạt động khi game đang bật VÀ không bị Pause
        if (gameActive && Input.GetKeyDown(KeyCode.F) && Time.timeScale != 0)
        {
            CloseGame();
        }
        
        // --- FIX LỖI CON TRỎ CHUỘT ---
        // Đảm bảo khi chơi cờ thì luôn hiện chuột (tránh bị script khác khóa lại)
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
        
        caroCamera.Priority = 20;
        if (playerModel != null) playerModel.SetActive(false);
        if (gameUI != null) gameUI.SetActive(false);

        // Mở chuột ngay lập tức
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        ResetBoard();
    }

    // ... (Giữ nguyên logic đánh cờ OnPlayerClick, MakeMove, CheckWin, CheckLine...)
    void OnPlayerClick(int index)
    {
        if (!isPlayerTurn || !gameActive || board[index] != "" || Time.timeScale == 0) return;

        MakeMove(index, "X");

        if (CheckWin("X")) EndGame(true);
        else if (movesCount >= 9) StartCoroutine(ResetWithDelay());
        else { isPlayerTurn = false; StartCoroutine(GhostMove()); }
    }

    // Logic cũ của ông, tui rút gọn cho dễ nhìn
    void MakeMove(int index, string mark)
    {
        board[index] = mark;
        buttonTexts[index].text = mark;
        buttonTexts[index].color = (mark == "X") ? Color.green : Color.red;
        movesCount++;
    }

    bool CheckWin(string mark)
    {
        if (CheckLine(0,1,2,mark) || CheckLine(3,4,5,mark) || CheckLine(6,7,8,mark) ||
            CheckLine(0,3,6,mark) || CheckLine(1,4,7,mark) || CheckLine(2,5,8,mark) ||
            CheckLine(0,4,8,mark) || CheckLine(2,4,6,mark)) return true;
        return false;
    }

    bool CheckLine(int a, int b, int c, string mark) { return board[a] == mark && board[b] == mark && board[c] == mark; }

    IEnumerator ResetWithDelay() { yield return new WaitForSeconds(1.5f); ResetBoard(); }

    void ResetBoard()
    {
        for (int i = 0; i < 9; i++) { board[i] = ""; buttonTexts[i].text = ""; }
        isPlayerTurn = true;
        movesCount = 0;
    }

    // ... (Hết phần logic cũ)

    IEnumerator GhostMove()
    {
        yield return new WaitForSeconds(1f);

        List<int> emptySlots = new List<int>();
        for (int i = 0; i < 9; i++) if (board[i] == "") emptySlots.Add(i);

        // Chỉ đánh khi game vẫn đang Active (đề phòng người chơi bấm F thoát lúc ma đang nghĩ)
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

    IEnumerator TriggerJumpscare()
    {
        gameActive = false;
        Debug.Log("💀 MA THẮNG! Jumpscare...");

        if (ghostAnimator != null) ghostAnimator.SetTrigger("Kill");
        if (ghostAudioSource != null && jumpscareSound != null)
        {
            ghostAudioSource.Stop();
            ghostAudioSource.PlayOneShot(jumpscareSound);
        }

        // Chờ animation hù dọa xong
        yield return new WaitForSecondsRealtime(2f);

        // --- LOGIC MỚI: TRỪ MÁU ---
        if (playerStats != null)
        {
            // Trừ 50 máu (50%)
            playerStats.TakeDamage(50f);

            // Kiểm tra xem còn sống không?
            // (Truy cập biến currentHealth hơi khó vì nó private, nên ta check fillAmount hoặc check trạng thái game)
            // Cách đơn giản: Nếu GameController chưa Pause nghĩa là chưa chết
            if (!Script.UI.GameController.IsGamePaused())
            {
                // CÒN SỐNG -> Thoát game cờ để chạy trốn
                Debug.Log("Vẫn còn sống! Chạy ngay đi!");
                CloseGame();
            }
            else
            {
                // ĐÃ CHẾT (PlayerStats tự gọi PauseGame rồi)
                // Ta chỉ cần đảm bảo UI DeathPanel hiện đúng (PlayerStats lo vụ này hoặc code cũ lo)
                // Vì PlayerStats đã gọi PauseGame(deathPanel), ta không cần gọi ShowDeathPanel ở đây nữa
            }
        }
        else
        {
            // Nếu không tìm thấy script máu thì cứ giết luôn cho chắc
            ShowDeathPanel();
        }
    }

    void ShowDeathPanel()
    {
        // --- FIX LỖI UI BỊ ẨN ---
        // Bật lại GameUI tổng trước (đề phòng DeathPanel nằm trong đó)
        if (gameUI != null) gameUI.SetActive(true);

        // Hiện chuột để bấm nút
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (deathPanel != null)
        {
            deathPanel.SetActive(true);
            // Đưa panel lên trên cùng để không bị cái gì che mất
            deathPanel.transform.SetAsLastSibling();

            // Gọi Pause Game
            Script.UI.GameController.PauseGame(deathPanel);
        }
        else
        {
            Debug.LogError("LỖI: Chưa kéo DeathPanel vào script TicTacToeManager kìa bro!");
        }

        if (deathText != null) deathText.text = "BẠN ĐÃ THUA TRÍ TUỆ CỦA MA!";
    }

    void EndGame(bool playerWin)
    {
        if (playerWin)
        {
            if (keyPrefab != null) Instantiate(keyPrefab, dropPoint.position, Quaternion.identity);
            CloseGame(); // Thắng thì tự thoát
        }
    }

    // Hàm thoát game chung (dùng cho cả nút F và khi thắng)
    void CloseGame()
    {
        // --- THÊM DÒNG NÀY ---
        lastCloseTime = Time.time; // Ghi lại giờ đóng cửa

        gameActive = false;
        caroCamera.Priority = 0;

        if (playerModel != null) playerModel.SetActive(true);
        if (gameUI != null) gameUI.SetActive(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        ResetBoard();
    }
}
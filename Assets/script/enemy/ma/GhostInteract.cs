using UnityEngine;

public class GhostInteract : MonoBehaviour
{
    [Header("Kéo thả")]
    public TicTacToeManager gameManager; // Kéo CaroBoard vào đây

    [Header("Cài đặt")]
    public string hintMessage = "Nhấn F để thách đấu";
    public float interactRange = 4f; // Khoảng cách 4 mét là chơi được

    private Transform playerTransform;

    void Start()
    {
        // Tự tìm thằng Player, khỏi lo bị mất kết nối
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogError("❌ LỖI: Không tìm thấy Player! Xem lại Tag 'Player' trên nhân vật chưa?");
        }
    }

    void Update()
    {
        // 1. Kiểm tra an toàn
        if (playerTransform == null || gameManager == null) return;

        // 2. Nếu game đang Pause (TimeScale = 0) -> Nghỉ
        if (Time.timeScale == 0) return;

        // --- CÔNG NGHỆ RADAR: Tự đo khoảng cách ---
        // Không cần Trigger, không cần va chạm, cứ gần là tính
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        bool isNearby = distance <= interactRange;

        if (isNearby)
        {
            // Chỉ xử lý khi game cờ CHƯA bắt đầu
            if (!gameManager.gameActive)
            {
                // A. XỬ LÝ NHẤN NÚT F
                if (Input.GetKeyDown(KeyCode.F))
                {
                    // Chặn spam nút (nếu vừa đóng game xong)
                    if (Time.time < gameManager.lastCloseTime + 0.5f) return;

                    Debug.Log("✅ Đã nhấn F! Bắt đầu StartGame()...");

                    // Tắt gợi ý trước rồi mới start game
                    if (GameManager.instance != null) GameManager.instance.HideHint();

                    gameManager.StartGame(); // <--- GỌI HÀM NÀY CAM MỚI ZOOM
                    return;
                }

                // B. HIỆN CHỮ GỢI Ý (Nếu chưa bấm gì)
                if (GameManager.instance != null) GameManager.instance.ShowHint(hintMessage);
            }
            else
            {
                // Nếu đang chơi cờ thì tắt gợi ý
                if (GameManager.instance != null) GameManager.instance.HideHint();
            }
        }
        else
        {
            // Nếu đi ra xa (ngoài 4 mét) -> Tắt gợi ý
            if (GameManager.instance != null && GameManager.instance.hintText.text == hintMessage)
            {
                GameManager.instance.HideHint();
            }
        }
    }

    // Vẽ vòng tròn đỏ trong Scene để ông dễ căn chỉnh
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}
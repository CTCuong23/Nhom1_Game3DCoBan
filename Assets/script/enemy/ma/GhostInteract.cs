using UnityEngine;

public class GhostInteract : MonoBehaviour
{
    public TicTacToeManager gameManager;
    public string hintMessage = "Nhấn F để thách đấu";

    private bool isPlayerNearby = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) isPlayerNearby = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            GameManager.instance.HideHint();
        }
    }

    void Update()
    {
        // 1. Nếu game đang Pause (TimeScale = 0) -> KHÔNG LÀM GÌ CẢ (Fix lỗi nhấn F khi chết)
        if (Time.timeScale == 0) return;

        if (isPlayerNearby)
        {
            if (!gameManager.gameActive)
            {
                GameManager.instance.ShowHint(hintMessage);

                if (Input.GetKeyDown(KeyCode.F))
                {
                    // Check thời gian đóng cũ (để tránh spam)
                    if (Time.time < gameManager.lastCloseTime + 0.5f) return;

                    gameManager.StartGame();
                    GameManager.instance.HideHint();
                }
            }
            else
            {
                // Khi đang chơi thì tắt gợi ý
                GameManager.instance.HideHint();
            }
        }
    }
}
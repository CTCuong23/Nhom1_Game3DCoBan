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
        if (isPlayerNearby)
        {
            if (!gameManager.gameActive)
            {
                GameManager.instance.ShowHint(hintMessage);

                if (Input.GetKeyDown(KeyCode.F))
                {
                    // --- THÊM DÒNG CHECK NÀY ---
                    // Nếu thời gian hiện tại chưa qua khỏi 0.5 giây kể từ lúc đóng -> Bỏ qua
                    if (Time.time < gameManager.lastCloseTime + 0.5f) return;

                    gameManager.StartGame();
                    GameManager.instance.HideHint();
                }
            }
            else
            {
                GameManager.instance.HideHint();
            }
        }
    }
}
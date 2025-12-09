using UnityEngine;

public class GhostInteract : MonoBehaviour
{
    public TicTacToeManager gameManager; // Kéo cái script CaroBoard vào đây

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            gameManager.StartGame();
            // Tắt Trigger này đi để không bị gọi lại lần nữa
            GetComponent<Collider>().enabled = false;
        }
    }
}
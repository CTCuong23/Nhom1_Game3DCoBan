using UnityEngine;

public class EnemyGizmos : MonoBehaviour
{
    [Header("Cài đặt thông số để nhìn thử (Không ảnh hưởng logic thật)")]
    [SerializeField] float wanderRadius = 10f; // Nhập số giống hệt trong Blackboard
    [SerializeField] float detectRange = 5f;   // Nhập số giống DetectRange

    [SerializeField] Color wanderColor = Color.yellow;
    [SerializeField] Color detectColor = Color.red;

    // OnDrawGizmosSelected chỉ vẽ khi bạn Click chọn vào con Enemy (đỡ rối mắt)
    private void OnDrawGizmosSelected()
    {
        // 1. Vẽ vùng đi tuần (Wander Radius)
        Gizmos.color = wanderColor;
        Gizmos.DrawWireSphere(transform.position, wanderRadius);

        // 2. Vẽ vùng phát hiện (Detect Range)
        Gizmos.color = detectColor;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}
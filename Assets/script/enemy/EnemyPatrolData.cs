using System.Collections.Generic;
using UnityEngine;

public class EnemyPatrolData : MonoBehaviour
{
    [Header("Kéo thả các điểm Waypoint vào đây")]
    [SerializeField] List<Transform> patrolPoints; // Danh sách chứa các điểm

    private int _lastIndex = -1; // Biến lưu lại điểm vừa đi qua

    // Hàm này để AI gọi, lấy ngẫu nhiên 1 điểm
    public Vector3 GetRandomWaypoint()
    {
        if (patrolPoints.Count == 0) return transform.position;

        // Nếu chỉ có 1 điểm thì đành chịu, trả về điểm đó luôn
        if (patrolPoints.Count == 1) return patrolPoints[0].position;

        int newIndex = Random.Range(0, patrolPoints.Count);

        // Vòng lặp: Nếu random trúng điểm cũ thì random lại cho đến khi ra số khác
        while (newIndex == _lastIndex)
        {
            newIndex = Random.Range(0, patrolPoints.Count);
        }

        // Lưu lại điểm mới này để lần sau so sánh
        _lastIndex = newIndex;

        return patrolPoints[newIndex].position;
    }

    private void OnDrawGizmos()
    {
        // 1. Kiểm tra nếu danh sách rỗng thì không vẽ gì cả
        if (patrolPoints == null || patrolPoints.Count == 0) return;

        // 2. Chọn màu cho Gizmos (ví dụ màu Xanh Cyan)
        Gizmos.color = Color.cyan;

        for (int i = 0; i < patrolPoints.Count; i++)
        {
            if (patrolPoints[i] != null)
            {
                // Vẽ một quả cầu nhỏ tại vị trí waypoint
                Gizmos.DrawSphere(patrolPoints[i].position, 0.5f);

                // (Tùy chọn) Vẽ đường nối giữa các điểm để dễ nhìn thứ tự
                // Nối điểm hiện tại với điểm kế tiếp
                Transform nextPoint = patrolPoints[(i + 1) % patrolPoints.Count];
                if (nextPoint != null)
                {
                    Gizmos.DrawLine(patrolPoints[i].position, nextPoint.position);
                }
            }
        }
    }
 }
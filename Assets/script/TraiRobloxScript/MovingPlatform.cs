using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform3D : MonoBehaviour
{
    [Header("Cài đặt Di Chuyển")]
    [SerializeField] private Transform[] points; // Mảng chứa các điểm đến (kéo thả bao nhiêu cũng được)
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float waitTime = 0.5f; // Thời gian nghỉ tại mỗi điểm

    private int currentTargetIndex = 0;
    private bool isWaiting = false;

    void Start()
    {
        // Nếu có điểm trong danh sách, đặt vị trí bắt đầu tại điểm đầu tiên
        if (points.Length > 0)
        {
            transform.position = points[0].position;
        }
    }

    void FixedUpdate()
    {
        // Nếu không có điểm nào hoặc đang nghỉ thì không chạy
        if (points.Length == 0 || isWaiting) return;

        // Xác định mục tiêu hiện tại
        Transform targetPoint = points[currentTargetIndex];

        // Di chuyển Box tới mục tiêu
        transform.position = Vector3.MoveTowards(transform.position, targetPoint.position, moveSpeed * Time.fixedDeltaTime);

        // Kiểm tra xem đã tới nơi chưa
        if (Vector3.Distance(transform.position, targetPoint.position) < 0.1f)
        {
            // Đã tới nơi, bắt đầu quy trình chuyển sang điểm tiếp theo
            StartCoroutine(WaitAndNextPoint());
        }
    }

    // Coroutine để xử lý việc nghỉ ngơi rồi mới đi tiếp
    IEnumerator WaitAndNextPoint()
    {
        isWaiting = true;
        yield return new WaitForSeconds(waitTime);

        // Tăng chỉ số để chuyển sang điểm tiếp theo trong mảng
        currentTargetIndex++;

        // Nếu đi hết danh sách (vượt quá số lượng điểm) thì quay về điểm đầu (0)
        if (currentTargetIndex >= points.Length)
        {
            currentTargetIndex = 0;
        }

        isWaiting = false;
    }
}
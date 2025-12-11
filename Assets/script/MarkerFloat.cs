using UnityEngine;

public class MarkerFloat : MonoBehaviour
{
    public float speed = 2f;    // Tốc độ nhấp nhô
    public float height = 0.2f; // Biên độ nhấp nhô

    private Vector3 worldOffset; // Khoảng cách lệch theo hệ tọa độ thế giới (Không xoay theo cha)

    void Start()
    {
        // 1. Tính toán khoảng cách giữa Marker và Cha theo không gian thế giới (World Space)
        // Lúc này ông đặt Marker ở đâu, nó sẽ nhớ vị trí đó so với tâm của Cha.
        // Ví dụ: Lệch phải 1m, cao 2m -> Nó nhớ Vector (1, 2, 0)
        if (transform.parent != null)
        {
            worldOffset = transform.position - transform.parent.position;
        }
    }

    void LateUpdate()
    {
        // Nếu cha bị hủy hoặc mất dấu -> Tự hủy để tránh lỗi
        if (transform.parent == null)
        {
            Destroy(gameObject);
            return;
        }

        // 2. Tính vị trí mục tiêu:
        // Lấy vị trí hiện tại của Cha + Cộng thêm khoảng cách gốc (worldOffset)
        // QUAN TRỌNG: Vì worldOffset là Vector thế giới, nên dù Cha có xoay mòng mòng,
        // thì Marker vẫn nằm đúng ở vị trí lệch ban đầu (ví dụ vẫn là lệch phải 1m, cao 2m so với tâm Cha).
        Vector3 targetPos = transform.parent.position + worldOffset;

        // 3. Cộng thêm nhấp nhô vào trục Y (Thẳng đứng của thế giới)
        targetPos.y += Mathf.Sin(Time.time * speed) * height;

        // 4. Áp dụng vị trí
        transform.position = targetPos;

        // 5. Billboard: Luôn xoay mặt về Camera để dễ nhìn
        if (Camera.main != null)
        {
            transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward,
                             Camera.main.transform.rotation * Vector3.up);
        }
    }
}
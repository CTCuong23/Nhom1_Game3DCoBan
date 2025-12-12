using UnityEngine;

public class PetController : MonoBehaviour
{
    [Header("Trạng thái")]
    public bool isTamed = false; // Biến này sẽ bật lên khi nhấn E

    [Header("Di chuyển")]
    public Transform player; // Kéo nhân vật vào, hoặc để trống nó tự tìm
    // Vị trí bay: X (Lệch phải), Y (Cao), Z (Sau lưng)
    public Vector3 followOffset = new Vector3(0.8f, 1.5f, -1f);
    public float smoothSpeed = 3f;

    [Header("Chiến đấu")]
    public float detectRange = 10f;   // Tầm phát hiện quái
    public float fireCooldown = 20f;  // Hồi chiêu 20 giây
    public Transform firePoint;       // Vị trí viên đạn bay ra (tạo 1 empty object ở miệng pet)
    public GameObject bulletPrefab;   // Kéo Prefab viên đạn đỏ vào đây
    public LayerMask enemyLayer;      // Chọn Layer của Enemy (Rất quan trọng)

    private float cooldownTimer = 0f;

    void Start()
    {
        // Tự động tìm Player nếu quên kéo
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    // Hàm này được gọi từ InteractableObject khi nhấn E
    public void TamePet()
    {
        isTamed = true;
        Debug.Log("Pet đã được thuần phục! Sẵn sàng chiến đấu.");
    }

    void Update()
    {
        if (!isTamed) return; // Chưa thuần phục thì đứng yên

        FollowPlayer();
        HandleCombat();
    }

    void FollowPlayer()
    {
        if (player == null) return;

        // Tính toán vị trí mục tiêu dựa trên hướng nhìn của Player
        // TransformPoint giúp chuyển đổi tọa độ local (sau lưng) thành tọa độ thế giới
        Vector3 targetPos = player.TransformPoint(followOffset);

        // Bay mượt mà tới đó
        transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed * Time.deltaTime);

        // Xoay mặt theo hướng Player đang nhìn
        transform.rotation = Quaternion.Slerp(transform.rotation, player.rotation, smoothSpeed * Time.deltaTime);
    }

    void HandleCombat()
    {
        // Đếm ngược hồi chiêu
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
            return;
        }

        // Quét quái xung quanh
        Collider[] enemies = Physics.OverlapSphere(transform.position, detectRange, enemyLayer);

        if (enemies.Length > 0)
        {
            // Tìm con quái đầu tiên trong danh sách (hoặc có thể nâng cấp tìm con gần nhất)
            Transform targetEnemy = enemies[0].transform;

            Shoot(targetEnemy);
            cooldownTimer = fireCooldown; // Reset hồi chiêu
        }
    }

    void Shoot(Transform target)
    {
        if (bulletPrefab == null || firePoint == null) return;

        // Sinh ra viên đạn
        GameObject bulletGO = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        PetBullet bulletScript = bulletGO.GetComponent<PetBullet>();

        // Gán mục tiêu cho đạn
        if (bulletScript != null)
        {
            bulletScript.Seek(target);
        }
    }

    // Vẽ vòng tròn tầm bắn trong Scene để dễ căn chỉnh
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}
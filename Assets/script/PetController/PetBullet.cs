using UnityEngine;

public class PetBullet : MonoBehaviour
{
    public float speed = 15f;
    public float stunDuration = 5f; // Thời gian choáng

    private Transform target;

    // Hàm nhận mục tiêu từ PetController
    public void Seek(Transform _target)
    {
        target = _target;
    }

    void Update()
    {
        // Nếu quái chết hoặc biến mất thì hủy đạn
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // Tính hướng bay
        Vector3 dir = target.position - transform.position;
        float distanceThisFrame = speed * Time.deltaTime;

        // Nếu khoảng cách nhỏ hơn bước di chuyển -> Trúng đích
        if (dir.magnitude <= distanceThisFrame)
        {
            HitTarget();
            return;
        }

        // Di chuyển đạn
        transform.Translate(dir.normalized * distanceThisFrame, Space.World);
        transform.LookAt(target); // Đầu đạn hướng về mục tiêu
    }

    void HitTarget()
    {
        // Tìm script Stun trên người quái
        EnemyStunHandler stunHandler = target.GetComponent<EnemyStunHandler>();
        if (stunHandler != null)
        {
            stunHandler.ApplyStun(stunDuration);
        }

        // Tạo hiệu ứng nổ bùm chíu ở đây nếu thích (Instantiate Particle)

        Destroy(gameObject); // Hủy viên đạn
    }
}
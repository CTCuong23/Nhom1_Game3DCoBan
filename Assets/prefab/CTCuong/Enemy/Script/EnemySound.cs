using UnityEngine;
using UnityEngine.AI;

public class EnemySound : MonoBehaviour
{
    [Header("Cài đặt chung")]
    public AudioSource audioSource;
    public NavMeshAgent agent; // Kéo NavMeshAgent vào đây (hoặc để code tự tìm)

    [Header("Cài đặt Bước chân")]
    public AudioClip footstepSound;
    [Range(0.1f, 1.0f)]
    public float stepRate = 0.5f; // Thời gian giữa 2 bước chân (số càng nhỏ chân bước càng nhanh)

    [Range(0.8f, 1.2f)] public float minPitch = 0.9f;
    [Range(0.8f, 1.2f)] public float maxPitch = 1.1f;

    [Header("Âm thanh Phát hiện")]
    public AudioClip alertSound;

    private float _nextStepTime = 0f; // Biến đếm ngược

    private void Start()
    {
        // Tự động tìm component nếu quên kéo
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (agent == null) agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        // Kiểm tra xem có đang di chuyển không
        // agent.velocity.sqrMagnitude > 0.1f nghĩa là vận tốc > 0 (đang chạy)
        if (agent != null && agent.velocity.sqrMagnitude > 0.1f)
        {
            // Kiểm tra đã đến lúc phát tiếng chưa
            if (Time.time >= _nextStepTime)
            {
                PlayFootstep();
                // Hẹn giờ cho bước tiếp theo
                _nextStepTime = Time.time + stepRate;
            }
        }
    }

    // Hàm phát tiếng bước chân (Tự động gọi trong Update)
    void PlayFootstep()
    {
        if (footstepSound == null || audioSource == null) return;

        // Random pitch cho đỡ nhàm
        audioSource.pitch = Random.Range(minPitch, maxPitch);
        audioSource.PlayOneShot(footstepSound, 1f); // Volume mặc định là 1
    }

    public void PlayAlertSound()
    {
        if (alertSound == null || audioSource == null) return;

        // Tiếng báo động thường không cần random pitch, cần nghe rõ ràng
        audioSource.pitch = 1f;

        // PlayOneShot để nó đè lên tiếng bước chân (không bị ngắt tiếng bước chân)
        // Số 1f là volume (tối đa) để tiếng báo động nghe rõ nhất
        audioSource.PlayOneShot(alertSound, 1f);
    }

    // Hàm này để hứng sự kiện OnLand (nếu có)
    public void OnLand(AnimationEvent animationEvent)
    {
    }

    // --- THÊM ĐOẠN NÀY ĐỂ CHẶN LỖI ---
    // Hàm này để hứng sự kiện từ Animation (dù mình không dùng nữa)
    // Để nó không báo lỗi đỏ lòm trên màn hình
    public void OnFootstep()
    {
        // Để trống, không làm gì cả vì mình đã có code tự động chạy bước chân rồi
    }
}
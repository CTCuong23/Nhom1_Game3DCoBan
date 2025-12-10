using UnityEngine;

public class EnemySound : MonoBehaviour
{
    [Header("Cài đặt chung")]
    [SerializeField] AudioSource audioSource;

    [Header("Âm thanh Phát hiện")]
    [SerializeField] AudioClip alertSound;

    [Header("Âm thanh Tấn công")]
    [SerializeField] AudioClip attackSound; // Kéo tiếng vung tay/gầm gừ vào đây

    // --- BIẾN MỚI: Dùng để đếm giờ ---
    private float lastAlertTime = -100f; // Mốc thời gian lần cuối kêu
    public float alertCooldown = 5f;     // Cứ 5 giây mới được kêu 1 lần

    private void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    public void OnFootstep(AnimationEvent animationEvent)
    {
        // Không làm gì cả -> Im lặng
    }

    public void PlayAlertSound()
    {
        if (alertSound == null || audioSource == null) return;

        // --- KIỂM TRA COOLDOWN ---
        // Nếu thời gian hiện tại chưa vượt qua (lần cuối + 5s) thì thôi, nín
        if (Time.time < lastAlertTime + alertCooldown) return;

        // Cập nhật lại thời gian vừa kêu
        lastAlertTime = Time.time;

        // Reset pitch và phát âm thanh
        audioSource.pitch = 1f;
        audioSource.PlayOneShot(alertSound, 1f);
    }

    // Hàm này sẽ gọi từ Animation (giống hàm bước chân)
    public void PlayAttackSound()
    {
        if (attackSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(attackSound);
        }
    }
}
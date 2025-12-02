using UnityEngine;

public class EnemySound : MonoBehaviour
{
    [Header("Cài đặt chung")]
    public AudioSource audioSource;

    [Header("Âm thanh Phát hiện")]
    public AudioClip alertSound; 

    private void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    // --- QUAN TRỌNG: GIỮ HÀM NÀY NHƯNG ĐỂ RỖNG ---
    // Mục đích: Để Animation không báo lỗi đỏ khi chạy qua sự kiện bước chân.
    public void OnFootstep(AnimationEvent animationEvent)
    {
        // Không làm gì cả -> Im lặng
    }

    // --- GIỮ LẠI HÀM BÁO ĐỘNG ---
    public void PlayAlertSound()
    {
        if (alertSound == null || audioSource == null) return;

        // Reset pitch về bình thường
        audioSource.pitch = 1f;

        // Phát tiếng báo động ở mức to nhất (1f)
        audioSource.PlayOneShot(alertSound, 1f);
    }
}
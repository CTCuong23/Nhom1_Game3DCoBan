using UnityEngine;
using System.Collections;

public class MusicManager : MonoBehaviour
{
    public static MusicManager instance;

    [Header("Cài đặt Nhạc")]
    [SerializeField] AudioSource bgmSource;      // Kéo cái AudioSource phát nhạc nền vào đây
    [SerializeField] AudioClip normalMusic;      // Để Trống (None) cũng được
    [SerializeField] AudioClip chaseMusic;       // Nhạc bị đuổi

    [Header("Thông số (Chỉ để xem)")]
    public int enemyChasingCount = 0;  // Đếm xem có bao nhiêu con đang đuổi

    void Awake()
    {
        // Tạo Singleton để gọi từ bất kỳ đâu
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Mới vào game: Chạy nhạc Normal (Nếu có), không có thì im lặng
        SwitchMusic(normalMusic);
    }

    // Hàm này gọi khi Quái bắt đầu đuổi
    public void StartChase()
    {
        enemyChasingCount++;

        // Nếu đây là con quái ĐẦU TIÊN phát hiện -> Đổi nhạc Chase ngay
        if (enemyChasingCount == 1)
        {
            SwitchMusic(chaseMusic);
        }
    }

    // Hàm này gọi khi Quái bỏ cuộc hoặc chết
    public void StopChase()
    {
        enemyChasingCount--;

        // Giữ số không bao giờ âm
        if (enemyChasingCount < 0) enemyChasingCount = 0;

        // Nếu KHÔNG CÒN con nào đuổi nữa -> Về nhạc Normal
        if (enemyChasingCount == 0)
        {
            SwitchMusic(normalMusic);
        }
    }

    // --- HÀM ĐỔI NHẠC (QUAN TRỌNG: Đã sửa lỗi xử lý Null) ---
    void SwitchMusic(AudioClip newClip)
    {
        if (bgmSource == null) return;

        // TRƯỜNG HỢP 1: Nếu không có nhạc (null) -> Tắt loa luôn
        // (Đây là chỗ giúp game im lặng khi thoát khỏi quái)
        if (newClip == null)
        {
            bgmSource.Stop();
            bgmSource.clip = null;
            return;
        }

        // TRƯỜNG HỢP 2: Nếu nhạc mới trùng với nhạc đang phát -> Kệ nó phát tiếp
        if (bgmSource.clip == newClip && bgmSource.isPlaying) return;

        // TRƯỜNG HỢP 3: Có nhạc mới -> Bật lên
        bgmSource.clip = newClip;
        bgmSource.Play();
    }
}
using UnityEngine;
using System.Collections.Generic;
using TMPro; // Bắt buộc có để chỉnh chữ UI

public class PowerManager : MonoBehaviour
{
    public static PowerManager instance;

    [Header("UI Hiển thị")]
    [SerializeField] TextMeshProUGUI powerTimerText; // Kéo cái Text trên màn hình vào đây

    [Header("Cài đặt Thời gian (Giây)")]
    [SerializeField] float firstTimeDuration = 60f;   // Lần đầu: 1 phút (60s)
    [SerializeField] float normalCycleDuration = 180f; // Các lần sau: 3 phút (180s)
    [SerializeField] float warningThreshold = 60f;     // Cảnh báo đỏ khi còn 1 phút

    [Header("Danh sách đèn (Tự tìm)")]
    [SerializeField] List<Light> mapLights = new List<Light>();

    [Header("Âm thanh")]
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip powerDownSFX;
    [SerializeField] AudioClip powerUpSFX;

    [Header("Liên kết Quest (Kéo Cầu dao vào đây)")]
    public Breaker mainBreaker; // <--- BIẾN MỚI QUAN TRỌNG

    public bool isPowerOff = false;
    private float currentTime; // Biến đếm thời gian thực tế
    

    void Awake() { if (instance == null) instance = this; }

    void Start()
    {
        // 1. TÌM ĐÈN (Code cũ - Giữ nguyên)
        Light[] allLights = FindObjectsByType<Light>(FindObjectsSortMode.None);
        foreach (var l in allLights)
        {
            if (l.GetComponentInParent<PlayerStats>() != null) continue;
            if (l.GetComponentInParent<EnemySound>() != null) continue;
            if (l.GetComponentInParent<EnemyPatrolData>() != null) continue;
            if (l.bakingOutput.lightmapBakeType == LightmapBakeType.Baked) continue;
            mapLights.Add(l);
        }

        // 2. THIẾT LẬP THỜI GIAN BAN ĐẦU
        currentTime = firstTimeDuration; // Gán 60s cho lần đầu
        
    }

    void Update()
    {
        // Nếu điện đang tắt thì không đếm giờ nữa (hoặc hiện thông báo khác)
        if (isPowerOff)
        {
            if (powerTimerText != null)
            {
                // SỬA DÒNG NÀY: Để về số 00:00 cho gọn
                powerTimerText.text = "NGUỒN ĐIỆN: 00:00";
                powerTimerText.color = Color.red;
            }
            return;
        }

        // --- LOGIC ĐẾM NGƯỢC ---
        if (currentTime > 0)
        {
            currentTime -= Time.deltaTime; // Trừ thời gian theo từng khung hình

            // Cập nhật lên màn hình
            UpdateTimerUI();
        }
        else
        {
            // Hết giờ -> Cúp điện
            currentTime = 0;
            CutPower();
        }
    }

    void UpdateTimerUI()
    {
        if (powerTimerText == null) return;

        // 1. Đổi phút : giây (Ví dụ 125s -> 02:05)
        int minutes = Mathf.FloorToInt(currentTime / 60F);
        int seconds = Mathf.FloorToInt(currentTime % 60F);
        powerTimerText.text = string.Format("NGUỒN ĐIỆN: {0:00}:{1:00}", minutes, seconds);

        // 2. Đổi màu chữ
        if (currentTime <= warningThreshold)
        {
            powerTimerText.color = Color.red; // Dưới 1 phút -> Đỏ
        }
        else
        {
            powerTimerText.color = Color.green; // Còn nhiều -> Xanh
        }
    }

    public void CutPower()
    {
        if (isPowerOff) return;
        isPowerOff = true;

        if (audioSource && powerDownSFX) audioSource.PlayOneShot(powerDownSFX);
        foreach (var l in mapLights) if (l != null) l.enabled = false;

        if (GameManager.instance) GameManager.instance.ShowHint("HỆ THỐNG SẬP! KHỞI ĐỘNG LẠI CẦU DAO.");

        // ---> KÍCH HOẠT DẤU CHẤM THAN Ở ĐÂY <---
        if (mainBreaker != null)
        {
            mainBreaker.ToggleMarker(true);
        }
    }

    public void RestorePower()
    {
        if (!isPowerOff) return;
        isPowerOff = false;

        if (audioSource && powerUpSFX) audioSource.PlayOneShot(powerUpSFX);
        foreach (var l in mapLights) if (l != null) l.enabled = true;

        if (mainBreaker != null)
        {
            mainBreaker.ToggleMarker(false);
        }

        // --- RESET LẠI THỜI GIAN ---
        // Từ lần thứ 2 trở đi, set thời gian là 3 phút (180s)
        currentTime = normalCycleDuration;

        // Cập nhật ngay lập tức để người chơi thấy màu xanh lại liền
        UpdateTimerUI();
    }
}
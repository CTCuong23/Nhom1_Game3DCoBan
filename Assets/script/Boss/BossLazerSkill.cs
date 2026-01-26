using System.Collections;
using UnityEngine;

public class BossLaserSkill : MonoBehaviour
{
    [Header("--- Cài đặt Prefab ---")]
    public GameObject warningZonePrefab;
    public GameObject laserPrefab;

    [Header("--- Cài đặt Thời Gian ---")]
    public float skillActiveDuration = 10.0f; // Tổng thời gian skill hoạt động (Boss gồng trong 10s)
    public float waveInterval = 2.0f;         // Cứ 2s bắn 1 đợt
    public float skillCooldown = 20.0f;       // Hồi chiêu sau khi xong hết

    [Header("--- Cài đặt Chi tiết đợt bắn ---")]
    public int lasersPerWave = 10;            // Số lượng tia (và số vùng cảnh báo) mỗi đợt
    public float warningDuration = 1.5f;      // Thời gian vòng đỏ cảnh báo to dần trước khi bắn
    public float laserLifeTime = 1.0f;        // Laze tồn tại bao lâu rồi tắt

    [Header("--- Cài đặt Khu Vực ---")]
    public float warningSize = 1.5f;          // Kích thước của MỖI vòng cảnh báo nhỏ
    public float laserSpawnHeight = 10.0f;    // Độ cao laze
    public float warningHeight = 0.5f;

    [Header("--- Phạm Vi Ngẫu Nhiên (Map) ---")]
    public float minX = -10f;
    public float maxX = 10f;
    public float minZ = -10f;
    public float maxZ = 10f;

    private Vector3 initialWarningScale;

    void Start()
    {
        if (warningZonePrefab != null)
        {
            initialWarningScale = warningZonePrefab.transform.localScale;
        }

        Debug.Log("[BossSkill] Start: Hệ thống Boss đa điểm sẵn sàng.");
        StartCoroutine(AutoSkillLoop());
    }

    // Vòng lặp chính quản lý trạng thái Boss (Bắn -> Nghỉ -> Bắn)
    IEnumerator AutoSkillLoop()
    {
        while (true)
        {
            Debug.Log("[BossSkill] --- BẮT ĐẦU TRẠNG THÁI CUỒNG NỘ (10s) ---");

            float activeTimer = 0f;

            // Trong 10 giây này, cứ mỗi waveInterval (2s) sẽ gọi lệnh bắn 1 lần
            while (activeTimer < skillActiveDuration)
            {
                Debug.Log($"[BossSkill] >>> Kích hoạt đợt tấn công tại giây thứ {activeTimer}: Rải {lasersPerWave} điểm nổ.");

                // Gọi hàm bắn 1 đợt (Hàm này sẽ tự xử lý việc sinh ra 10 điểm riêng biệt)
                SpawnMultiShotWave();

                // Chờ đến đợt tiếp theo
                yield return new WaitForSeconds(waveInterval);
                activeTimer += waveInterval;
            }

            Debug.Log($"[BossSkill] Hết thời gian cuồng nộ. Đang hồi chiêu... ({skillCooldown}s)");
            yield return new WaitForSeconds(skillCooldown);
        }
    }

    // Hàm sinh ra 1 đợt gồm nhiều tia riêng biệt
    void SpawnMultiShotWave()
    {
        for (int i = 0; i < lasersPerWave; i++)
        {
            // 1. Chọn vị trí ngẫu nhiên cho tia này
            float rX = Random.Range(minX, maxX);
            float rZ = Random.Range(minZ, maxZ);
            Vector3 targetPos = new Vector3(rX, warningHeight, rZ);

            // 2. Chạy quy trình (Cảnh báo -> Bắn) cho riêng vị trí này
            // Dùng StartCoroutine ở đây để 10 tia chạy song song nhau cùng lúc
            StartCoroutine(ProcessSingleStrike(targetPos));
        }
    }

    // Quy trình xử lý cho MỘT tia laze duy nhất (Warning -> Laser)
    IEnumerator ProcessSingleStrike(Vector3 position)
    {
        // --- GIAI ĐOẠN 1: CẢNH BÁO TẠI ĐIỂM NÀY ---
        GameObject warnObj = Instantiate(warningZonePrefab, position, Quaternion.identity);

        // Reset scale về 0 để hiệu ứng phình to
        warnObj.transform.localScale = new Vector3(0, initialWarningScale.y, 0);

        float timer = 0f;
        while (timer < warningDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / warningDuration;
            // Lerp từ 0 lên kích thước warningSize (nhỏ)
            float currentSize = Mathf.Lerp(0, warningSize, progress);
            warnObj.transform.localScale = new Vector3(currentSize, initialWarningScale.y, currentSize);
            yield return null;
        }

        // Đảm bảo kích thước cuối cùng chuẩn xác
        warnObj.transform.localScale = new Vector3(warningSize, initialWarningScale.y, warningSize);

        // Chờ xíu xiu sau khi vòng đỏ đầy rồi mới bắn (tùy chọn, để 0 cũng được)
        yield return new WaitForSeconds(0.1f);

        // Xóa cảnh báo
        Destroy(warnObj);

        // --- GIAI ĐOẠN 2: BẮN LAZE TẠI ĐIỂM NÀY ---
        Vector3 spawnPos = position + Vector3.up * laserSpawnHeight;
        GameObject laserObj = Instantiate(laserPrefab, spawnPos, Quaternion.identity);

        // Laze tồn tại một chút rồi mất
        yield return new WaitForSeconds(laserLifeTime);

        Destroy(laserObj);
    }
}
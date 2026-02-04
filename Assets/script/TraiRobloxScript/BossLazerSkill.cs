using System.Collections;
using UnityEngine;

// Đổi tên class nếu muốn, không thì giữ nguyên cũng được nhưng logic đã thay đổi
public class BossLaserSkill : MonoBehaviour
{
    [Header("--- KẾT NỐI CÁC SKILL ---")]
    public BossShieldSkill shieldSkillScript; // Kéo Script BossShieldSkill vào đây

    [Header("--- Cài đặt Laze Cũ ---")]
    public GameObject warningZonePrefab;
    public GameObject laserPrefab;
    public AudioSource audioSource;
    public AudioClip warningSound;
    public AudioClip laserSound;
    [Range(0f, 1f)] public float soundVolume = 0.3f;

    [Header("--- Cài đặt Số Đợt Bắn (MỚI) ---")]
    public int wavesPerSkill = 15; // <-- Thay đổi số này trong Inspector

    [Header("--- Cài đặt Chung ---")]
    public float timeBetweenSkills = 5.0f; // Thời gian nghỉ giữa các skill

    // Các biến Laze cũ giữ nguyên...
    public float waveInterval = 2.0f;
    public int lasersPerWave = 10;
    public float warningDuration = 1.5f;
    public float laserLifeTime = 1.0f;
    public float warningSize = 1.5f;
    public float laserSpawnHeight = 10.0f;
    public float warningHeight = 0.5f;
    public float minX = -10f; public float maxX = 10f;
    public float minZ = -10f; public float maxZ = 10f;
    private Vector3 initialWarningScale;

    void Start()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (warningZonePrefab != null) initialWarningScale = warningZonePrefab.transform.localScale;
    }

    public void StartFighting()
    {
        Debug.Log("[Boss] Đã nhận lệnh chiến đấu!");
        StartCoroutine(BossBattleLoop());
    }

    IEnumerator BossBattleLoop()
    {
        // Chờ một chút trước khi bắt đầu
        yield return new WaitForSeconds(20f);

        while (true)
        {
            // --- RANDOM SKILL ---
            // 0: Laze, 1: Tạo Giáp
            int randomSkill = Random.Range(0, 2);

            if (randomSkill == 0)
            {
                Debug.Log("[Boss] Skill: MƯA LAZE!");
                yield return StartCoroutine(DoLaserSkill());
            }
            else
            {
                Debug.Log("[Boss] Skill: TẠO GIÁP!");
                if (shieldSkillScript != null)
                {
                    // Gọi skill giáp
                    shieldSkillScript.ActivateShieldSkill();

                    // Chờ cho đến khi skill giáp xong (người chơi phá hết neo)
                    yield return new WaitUntil(() => shieldSkillScript.IsSkillFinished());
                }
                else
                {
                    Debug.LogWarning("Chưa gắn Script BossShieldSkill!");
                }
            }

            Debug.Log($"[Boss] Nghỉ mệt {timeBetweenSkills}s...");
            yield return new WaitForSeconds(timeBetweenSkills);
        }
    }

    // Tách logic Laze cũ ra thành hàm riêng để gọn
    IEnumerator DoLaserSkill()
    {
        for (int i = 0; i < wavesPerSkill; i++)
        {
            SpawnMultiShotWave();
            yield return new WaitForSeconds(waveInterval);
        }
    }

    // Giữ nguyên các hàm SpawnMultiShotWave và ProcessSingleStrike cũ ở dưới...
    void SpawnMultiShotWave()
    {
        for (int i = 0; i < lasersPerWave; i++)
        {
            float rX = Random.Range(minX, maxX);
            float rZ = Random.Range(minZ, maxZ);
            Vector3 targetPos = new Vector3(rX, warningHeight, rZ);
            StartCoroutine(ProcessSingleStrike(targetPos));
        }
    }

    IEnumerator ProcessSingleStrike(Vector3 position)
    {
        // ... (Code cũ của bạn giữ nguyên phần này)
        // COPY LẠI PHẦN LOGIC CẢNH BÁO VÀ BẮN LAZE TỪ FILE CŨ VÀO ĐÂY
        // ...

        // Đoạn này để code chạy được mình viết tắt, bạn copy paste ruột hàm ProcessSingleStrike cũ vào nhé
        if (audioSource != null && warningSound != null) audioSource.PlayOneShot(warningSound, soundVolume);
        GameObject warnObj = Instantiate(warningZonePrefab, position, Quaternion.identity);
        warnObj.transform.localScale = new Vector3(0, initialWarningScale.y, 0);

        float timer = 0f;
        while (timer < warningDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / warningDuration;
            float currentSize = Mathf.Lerp(0, warningSize, progress);
            warnObj.transform.localScale = new Vector3(currentSize, initialWarningScale.y, currentSize);
            yield return null;
        }
        Destroy(warnObj);

        if (audioSource != null && laserSound != null) audioSource.PlayOneShot(laserSound, soundVolume);
        Vector3 spawnPos = position + Vector3.up * laserSpawnHeight;
        GameObject laserObj = Instantiate(laserPrefab, spawnPos, Quaternion.identity);
        yield return new WaitForSeconds(laserLifeTime);
        Destroy(laserObj);
    }
}
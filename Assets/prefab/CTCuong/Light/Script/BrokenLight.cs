using UnityEngine;
using System.Collections;

public class BrokenLight : MonoBehaviour
{
    public Light myLight; // Kéo cái Light vào đây

    [Header("Tốc độ nháy (Giây)")]
    public float minTime = 0.05f; // Thời gian ngắn nhất (nháy nhanh)
    public float maxTime = 0.5f;  // Thời gian dài nhất (chờ lâu)

    [Header("Âm thanh chập điện (Tùy chọn)")]
    public AudioSource audioSource;
    public AudioClip sparkSound; // Tiếng "tạch tạch" của điện

    void Start()
    {
        if (myLight == null) myLight = GetComponent<Light>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        // Bắt đầu vòng lặp nháy
        StartCoroutine(FlickerRoutine());
    }

    IEnumerator FlickerRoutine()
    {
        while (true)
        {
            // 1. Random thời gian chờ
            float waitTime = Random.Range(minTime, maxTime);
            yield return new WaitForSeconds(waitTime);

            // 2. Đảo trạng thái đèn (Sáng -> Tắt, Tắt -> Sáng)
            myLight.enabled = !myLight.enabled;

            // 3. Nếu đèn vừa BẬT sáng lại -> Phát tiếng "tạch" (nếu có)
            if (myLight.enabled && audioSource != null && sparkSound != null)
            {
                // Random pitch tí cho tự nhiên
                audioSource.pitch = Random.Range(0.8f, 1.2f);
                audioSource.PlayOneShot(sparkSound, 0.5f); // 0.5f là âm lượng
            }
        }
    }
}
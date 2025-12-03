using UnityEngine;

public class LedBreathing : MonoBehaviour
{
    public Light targetLight;       // Biến cũ: denCuaToi
    public Material targetMaterial; // Biến cũ: vatLieuDen

    [Header("Settings")]
    [ColorUsage(true, true)] // Cho phép chọn màu HDR sáng rực trong Inspector
    public Color baseColor = Color.cyan; // Biến cũ: LightColor

    public float pulseSpeed = 3.0f;          // Biến cũ: tocDoNhip
    public float maxLightIntensity = 5.0f;   // Biến cũ: doSangDenLight (Tăng độ sáng đèn chiếu ra)
    public float maxEmissionIntensity = 10.0f; // Biến cũ: doSangVatLieu (Tăng độ rực của thanh LED để thấy Bloom)

    private void Start()
    {
        // Tự động tìm đèn nếu chưa kéo vào
        if (targetLight == null) targetLight = GetComponentInChildren<Light>();

        // Lấy bản sao material (Instance) để chỉnh sửa
        if (targetMaterial == null) targetMaterial = GetComponent<Renderer>().material;

        // --- DÒNG QUAN TRỌNG: Ép Unity bật Emission ---
        targetMaterial.EnableKeyword("_EMISSION");
    }

    void Update()
    {
        // Tính toán nhịp thở (pulseFactor) từ 0.2 đến 1
        // (không cho tắt hẳn đen thui, để luôn thấy mờ mờ)
        float pulseFactor = (Mathf.Sin(Time.time * pulseSpeed) + 1.0f) / 2.0f;
        pulseFactor = Mathf.Lerp(0.2f, 1.0f, pulseFactor);

        // 1. Chỉnh đèn chiếu sáng (Point Light)
        if (targetLight != null)
        {
            targetLight.intensity = pulseFactor * maxLightIntensity;
        }

        // 2. Chỉnh độ rực (Emission)
        if (targetMaterial != null)
        {
            // Nhân màu gốc với cường độ và nhịp thở để ra màu HDR cuối cùng
            Color finalColor = baseColor * maxEmissionIntensity * pulseFactor;
            targetMaterial.SetColor("_EmissionColor", finalColor);
        }
    }
}
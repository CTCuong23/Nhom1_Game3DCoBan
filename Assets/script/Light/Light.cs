using UnityEngine;
using System.Collections;

public class FlickeringLight : MonoBehaviour
{
    // Biến để gán component Light
    private Light lightComponent;

    // Cường độ cơ bản của đèn
    public float baseIntensity = 1f;

    // Khoảng thời gian nhấp nháy tối thiểu và tối đa (giây)
    public float minFlickerTime = 0.05f;
    public float maxFlickerTime = 0.2f;

    // Độ dao động tối đa của cường độ so với cường độ cơ bản
    public float maxIntensityVariation = 0.5f;

    void Start()
    {
        // Lấy component Light trên đối tượng này
        lightComponent = GetComponent<Light>();

        // Bắt đầu coroutine nhấp nháy
        StartCoroutine(DoFlicker());
    }

    IEnumerator DoFlicker()
    {
        while (true)
        {
            // 1. Tính cường độ mới: 
            // Cường độ cơ bản + một giá trị ngẫu nhiên trong khoảng (-maxVariation, +maxVariation)
            float newIntensity = baseIntensity + Random.Range(-maxIntensityVariation, maxIntensityVariation);

            // 2. Gán cường độ mới cho đèn
            lightComponent.intensity = newIntensity;

            // 3. Đợi một khoảng thời gian ngẫu nhiên trước khi nhấp nháy lần nữa
            float flickerDelay = Random.Range(minFlickerTime, maxFlickerTime);
            yield return new WaitForSeconds(flickerDelay);
        }
    }
}
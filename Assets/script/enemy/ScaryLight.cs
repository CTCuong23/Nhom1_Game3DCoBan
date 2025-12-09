using UnityEngine;

public class ScaryLight : MonoBehaviour
{
    Light myLight;
    [SerializeField] float minIntensity = 2f; // Độ sáng thấp nhất
    [SerializeField] float maxIntensity = 5f; // Độ sáng cao nhất
    [SerializeField] float speed = 2f;        // Tốc độ nhấp nháy

    void Start()
    {
        myLight = GetComponent<Light>();
    }

    void Update()
    {
        // Hàm PingPong giúp giá trị chạy qua lại giữa 0 và 1 liên tục
        myLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, Mathf.PingPong(Time.time * speed, 1));
    }
}
using UnityEngine;

public class FlashlightController : MonoBehaviour
{
    [SerializeField] GameObject lightSource; // Kéo cái Spotlight trên ngực Player vào đây
    [SerializeField] bool hasFlashlight = false; // Mới vào chưa có đèn
    [SerializeField] AudioClip clickSound;
    [SerializeField] AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (lightSource != null) lightSource.SetActive(false);
    }

    void Update()
    {
        // Bấm L để bật tắt (Nếu đã nhặt đèn)
        if (hasFlashlight && Input.GetKeyDown(KeyCode.L))
        {
            ToggleLight();
        }
    }

    void ToggleLight()
    {
        bool newState = !lightSource.activeSelf;
        lightSource.SetActive(newState);
        if (audioSource && clickSound) audioSource.PlayOneShot(clickSound);
    }

    // Hàm gọi khi nhặt được đèn
    public void PickupFlashlight()
    {
        hasFlashlight = true;
        if (lightSource) lightSource.SetActive(true); // Tự bật luôn cho ngầu
    }
}
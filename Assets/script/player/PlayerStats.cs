using UnityEngine;
using UnityEngine.UI;
using StarterAssets; // Để gọi script input của nhân vật
using Script.UI; // Để gọi GameController

public class PlayerStats : MonoBehaviour
{
    [Header("UI References")]
    public Image healthFill;   // Kéo ảnh Health_Fill vào đây
    public Image staminaFill;  // Kéo ảnh Stamina_Fill vào đây

    [Header("Health Settings")]
    public float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    [Header("Stamina Settings")]
    public float maxStamina = 100f;
    public float staminaDrainRate = 20f; // Tốc độ tụt khi chạy
    public float staminaRegenRate = 10f; // Tốc độ hồi phục
    [SerializeField] private float currentStamina;

    // References
    private StarterAssetsInputs _input;
    private ThirdPersonController _controller;

    void Start()
    {
        currentHealth = maxHealth;
        currentStamina = maxStamina;

        _input = GetComponent<StarterAssetsInputs>();
        _controller = GetComponent<ThirdPersonController>();

        UpdateUI();
    }

    void Update()
    {
        HandleStamina();
        UpdateUI();
    }

    // --- XỬ LÝ NĂNG LƯỢNG (STAMINA) ---
    void HandleStamina()
    {
        if (_input == null) return;

        // Nếu đang giữ Shift (sprint) VÀ đang di chuyển
        if (_input.sprint && _input.move != Vector2.zero)
        {
            if (currentStamina > 0)
            {
                currentStamina -= staminaDrainRate * Time.deltaTime;
            }
            else
            {
                // Hết hơi -> Ép tắt chạy nhanh
                currentStamina = 0;
                _input.sprint = false;
            }
        }
        else
        {
            // Hồi phục khi không chạy
            if (currentStamina < maxStamina)
            {
                currentStamina += staminaRegenRate * Time.deltaTime;
            }
        }
    }

    // --- XỬ LÝ MÁU (HEALTH) ---
    public void TakeDamage(float amount)
    {
        if (currentHealth <= 0) return; // Đã chết rồi thì thôi

        currentHealth -= amount;
        Debug.Log($"Bị đánh! Máu còn: {currentHealth}");

        UpdateUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        currentHealth = 0;
        Debug.Log("Player Dead!");

        // Gọi bảng chết từ GameController (sử dụng lại logic cũ của bro)
        // Tìm UI DeathPanel trong scene (hoặc gán cứng nếu muốn)
        GameObject deathPanel = GameObject.Find("YoureDeadPanel");
        if (deathPanel != null)
        {
            GameController.PauseGame(deathPanel);
        }
        else
        {
            Debug.LogError("Không tìm thấy YoureDeadPanel để hiện!");
        }
    }

    void UpdateUI()
    {
        if (healthFill != null) healthFill.fillAmount = currentHealth / maxHealth;
        if (staminaFill != null) staminaFill.fillAmount = currentStamina / maxStamina;
    }

    // Hàm hồi máu (dùng cho vật phẩm nếu cần)
    public void Heal(float amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        UpdateUI();
    }
}
using UnityEngine;
using UnityEngine.UI;
using StarterAssets;
using Script.UI; // Để gọi GameController

public class PlayerStats : MonoBehaviour
{
    [Header("UI References")]
    public Image healthFill;
    public Image staminaFill;
    public GameObject deathPanel; // <--- THÊM BIẾN NÀY ĐỂ KÉO THẢ

    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Stamina Settings")]
    public float maxStamina = 100f;
    public float staminaDrainRate = 20f;
    public float staminaRegenRate = 10f;
    [SerializeField] private float currentStamina;

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

    void HandleStamina()
    {
        if (_input == null) return;

        // Logic hồi năng lượng vẫn chạy bình thường kể cả khi đang chơi cờ
        // (Vì nhân vật chỉ bị tàng hình chứ không bị tắt)
        if (_input.sprint && _input.move != Vector2.zero)
        {
            if (currentStamina > 0)
            {
                currentStamina -= staminaDrainRate * Time.deltaTime;
            }
            else
            {
                currentStamina = 0;
                _input.sprint = false;
            }
        }
        else
        {
            if (currentStamina < maxStamina)
            {
                currentStamina += staminaRegenRate * Time.deltaTime;
            }
        }
    }

    public void TakeDamage(float amount)
    {
        if (currentHealth <= 0) return;

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

        if (deathPanel != null)
        {
            // Bật UI cha (nếu có)
            if (deathPanel.transform.parent != null)
                deathPanel.transform.parent.gameObject.SetActive(true);

            // Gọi Pause Game
            Script.UI.GameController.PauseGame(deathPanel);

            // --- FIX LỖI MẤT CHUỘT ---
            // Ép mở khóa chuột lần nữa cho chắc chắn
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Debug.LogError("QUÊN KÉO CÁI DEATH PANEL VÀO SCRIPT PLAYER STATS RỒI ÔNG ƠI!");
        }
    }

    void UpdateUI()
    {
        if (healthFill != null) healthFill.fillAmount = currentHealth / maxHealth;
        if (staminaFill != null) staminaFill.fillAmount = currentStamina / maxStamina;
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        UpdateUI();
    }
}
using UnityEngine;

public class PitOfDeath : MonoBehaviour
{
    // Đảm bảo vật thể này (cái hố) có Collider và Is Trigger được BẬT (checked)
    private void OnTriggerEnter(Collider other)
    {
        // Tìm component PlayerStats trên vật thể va chạm
        PlayerStats playerStats = other.GetComponent<PlayerStats>();

        // Nếu tìm thấy PlayerStats, tức là người chơi đã rơi vào hố
        if (playerStats != null)
        {
            // Gọi hàm gây sát thương tối đa, khiến người chơi chết ngay lập tức
            // Nếu bạn đã có hàm TakeDamage trong PlayerStats.cs

            // Lấy lượng máu tối đa của người chơi
            float damageToKill = playerStats.maxHealth;

            // Giả định bạn có hàm TakeDamage(float) hoặc KillPlayer()
            // Tốt nhất là sử dụng một hàm chuyên dụng:
            playerStats.Die();

            // Tùy chọn: Để tránh nhân vật bị kẹt/lún sâu vào hố sau khi chết, 
            // bạn có thể vô hiệu hóa vật thể hoặc di chuyển nó.
            // other.gameObject.SetActive(false); 
        }
    }
}
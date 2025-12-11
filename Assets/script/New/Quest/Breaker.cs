using UnityEngine;

// Kế thừa từ InteractableObject để dùng chung hệ thống E, Loading Bar
public class Breaker : InteractableObject
{
    public override string GetHintText()
    {
        // Nếu có điện rồi thì báo khác
        if (PowerManager.instance != null && !PowerManager.instance.isPowerOff)
            return "Hệ thống điện ổn định.";

        return "Giữ E để BẬT CẦU DAO";
    }

    public override void PerformAction()
    {
        // 1. KHÔNG GỌI base.PerformAction() NỮA (Để tránh bị Destroy)

        // 2. Tự tắt UI bằng tay (Copy từ bố sang)
        if (GameManager.instance != null)
        {
            GameManager.instance.StopLoading();
            GameManager.instance.HideHint();
        }

        // Logic riêng của bạn
        if (PowerManager.instance != null && PowerManager.instance.isPowerOff)
        {
            PowerManager.instance.RestorePower();
            Debug.Log("Đã bật cầu dao!");
        }
    }
}
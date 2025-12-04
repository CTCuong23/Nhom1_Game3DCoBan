using UnityEngine;
using UnityEngine.Playables;

public class InteractableObject : MonoBehaviour
{
    public enum ObjectType { Item, Door }
    public ObjectType type;

    [Header("Cài đặt")]
    public float holdTime = 2f;

    // --- KHÔI PHỤC LẠI PHẦN CỬA CINEMATIC ---
    [Header("Cài đặt cho Cửa (Chỉ dùng khi type là Door)")]
    public Animator doorAnimator;      // Kéo Animator cửa vào đây
    public Collider doorBlockCollider; // Kéo Collider chặn cửa vào đây
    public PlayableDirector timelineDirector;

    // Hàm này được script PlayerInteraction gọi để lấy nội dung hiển thị
    public string GetHintText()
    {
        if (type == ObjectType.Item)
            return "Giữ E để nhặt";

        if (type == ObjectType.Door)
        {
            if (GameManager.instance.currentItems >= 3)
                return "Vật phẩm: 3/3\nGiữ E để mở";
            else
                return $"Chưa đủ đồ ({GameManager.instance.currentItems}/3)";
        }
        return "";
    }

    // Hàm này được script PlayerInteraction gọi khi đã giữ đủ thời gian
    public void PerformAction()
    {
        GameManager.instance.StopLoading();
        GameManager.instance.HideHint();

        if (type == ObjectType.Item)
        {
            GameManager.instance.CollectItem();
            Destroy(gameObject);
        }
        else if (type == ObjectType.Door)
        {
            // 1. Mở cửa (Animation Cửa)
            if (doorAnimator != null) doorAnimator.SetTrigger("Open");
            if (doorBlockCollider != null) doorBlockCollider.enabled = false;

            // 2. BẮT ĐẦU CUTSCENE (Giao lại cho GameManager lo)
            if (timelineDirector != null)
            {
                GameManager.instance.StartEndingSequence(timelineDirector);
            }
            else
            {
                GameManager.instance.WinGame();
            }
        }
    }
}
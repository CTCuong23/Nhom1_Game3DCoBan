using UnityEngine;
using UnityEngine.Playables;

public class InteractableObject : MonoBehaviour
{
    public enum ObjectType { Item, Door }
    public ObjectType type;

    // --- MỚI: THÊM LOẠI VẬT PHẨM ---
    public enum ItemType { None, Battery, KeyCard, Chip }
    [Header("Loại vật phẩm (Chỉ chỉnh nếu Type là Item)")]
    public ItemType specificItemType;

    [Header("Cài đặt")]
    public float holdTime = 2f;

    [Header("Cài đặt cho Cửa")]
    public Animator doorAnimator;
    public Collider doorBlockCollider;
    public PlayableDirector timelineDirector;

    public string GetHintText()
    {
        if (type == ObjectType.Item)
            return "Giữ E để nhặt";

        if (type == ObjectType.Door)
        {
            if (GameManager.instance.currentItems >= 3)
                return "Đủ vật phẩm\nGiữ E để mở";
            else
                return $"Chưa đủ đồ ({GameManager.instance.currentItems}/3)";
        }
        return "";
    }

    public void PerformAction()
    {
        GameManager.instance.StopLoading();
        GameManager.instance.HideHint();

        if (type == ObjectType.Item)
        {
            // --- THAY ĐỔI: GỌI HÀM ANIMATION MỚI ---
            GameManager.instance.CollectItemAnimated(specificItemType);
            Destroy(gameObject);
        }
        else if (type == ObjectType.Door)
        {
            if (doorAnimator != null) doorAnimator.SetTrigger("Open");
            if (doorBlockCollider != null) doorBlockCollider.enabled = false;

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
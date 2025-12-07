using UnityEngine;
using UnityEngine.Playables;
using TMPro;

public class InteractableObject : MonoBehaviour
{
    // Thêm Computer vào đây
    public enum ObjectType { Item, Door, Computer }
    public ObjectType type;

    public enum ItemType { None, Battery, KeyCard, Chip }
    [Header("Loại vật phẩm (Chỉ chỉnh nếu Type là Item)")]
    public ItemType specificItemType;

    [Header("Cài đặt chung")]
    public float holdTime = 2f;

    [Header("Cài đặt cho Cửa")]
    public Animator doorAnimator;
    public Collider doorBlockCollider;
    public PlayableDirector timelineDirector;

    [Header("Cài đặt cho Máy Tính")]
    public GameObject screenCanvas; // Kéo cái Canvas màn hình vào đây
    public TextMeshProUGUI passwordText; // Kéo Text hiển thị pass vào đây
    public string passwordContent = "1997"; // Nội dung mật khẩu
    private bool isComputerOn = false;

    public string GetHintText()
    {
        if (type == ObjectType.Item) return "Giữ E để nhặt";

        if (type == ObjectType.Door)
        {
            if (GameManager.instance.currentItems >= 3) return "Đủ vật phẩm\nGiữ E để mở";
            else return $"Chưa đủ đồ ({GameManager.instance.currentItems}/3)";
        }

        // --- LOGIC MÁY TÍNH ---
        if (type == ObjectType.Computer)
        {
            if (isComputerOn) return ""; // Đã bật rồi thì thôi

            if (GameManager.instance.hasBattery)
            {
                return "Giữ E để khởi động máy tính";
            }
            else
            {
                return "Cần Pin để khởi động"; // Báo thiếu đồ
            }
        }

        return "";
    }

    public void PerformAction()
    {
        GameManager.instance.StopLoading();
        GameManager.instance.HideHint();

        if (type == ObjectType.Item)
        {
            GameManager.instance.CollectItemAnimated(specificItemType);
            Destroy(gameObject);
        }
        else if (type == ObjectType.Door)
        {
            if (doorAnimator != null) doorAnimator.SetTrigger("Open");
            if (doorBlockCollider != null) doorBlockCollider.enabled = false;
            if (timelineDirector != null) GameManager.instance.StartEndingSequence(timelineDirector);
            else GameManager.instance.WinGame();
        }
        // --- XỬ LÝ MÁY TÍNH ---
        else if (type == ObjectType.Computer)
        {
            isComputerOn = true;

            // Bật màn hình lên
            if (screenCanvas != null)
            {
                screenCanvas.SetActive(true);
            }

            // Gán mật khẩu
            if (passwordText != null)
            {
                passwordText.text = "PASSWORD\n" + passwordContent;
            }
        }
    }
}
using UnityEngine;
using System.Collections; // Cần cái này để chạy Coroutine (hành động theo thời gian)

public class LockedDoor : InteractableObject
{
    [Header("Cài đặt Cửa Khóa")]
    public ItemType keyRequired = ItemType.GateCard;

    [Header("Cấu hình Mở Cửa (Điền số vào đây)")]
    public Vector3 openPosition;   // Vị trí khi cửa MỞ (Lấy từ Hình 2)
    public Vector3 openRotation;   // Góc xoay khi cửa MỞ (Lấy từ Hình 2)
    public float openSpeed = 2f;   // Tốc độ mở cửa (Càng lớn càng nhanh)

    private Vector3 closedPosition;
    private Quaternion closedRotation;
    private bool isOpened = false; // Biến kiểm tra xem đã mở chưa

    public override void Start()
    {
        base.Start();

        // 1. Lưu lại vị trí/góc xoay hiện tại làm trạng thái ĐÓNG (Hình 1)
        closedPosition = transform.localPosition;
        closedRotation = transform.localRotation;

        // 2. Đánh lừa hệ thống để không tính là cửa 10 thẻ
        type = ObjectType.None;
        holdTime = 3f;
    }

    public override string GetHintText()
    {
        if (isOpened) return ""; // Mở rồi thì không hiện chữ nữa

        if (GameManager.instance.IsHoldingItem(keyRequired))
            return "Đã có Keycard, Giữ E để mở cửa";
        else
            return "Cần có Keycard để mở cửa";
    }

    public override void PerformAction()
    {
        if (isOpened) return;

        if (GameManager.instance.IsHoldingItem(keyRequired))
        {
            // --- THAY ĐỔI Ở ĐÂY: KHÔNG DÙNG ANIMATOR NỮA ---
            StartCoroutine(OpenDoorSmoothly());
            // -----------------------------------------------

            GameManager.instance.RemoveCurrentItem();

            // Tắt UI Loading
            if (GameManager.instance != null)
            {
                GameManager.instance.StopLoading();
                GameManager.instance.HideHint();
            }

            if (questMarker != null) Destroy(questMarker);

            // Không tắt script (this.enabled = false) ngay, 
            // phải để nó chạy xong hiệu ứng mở cửa đã.
            isOpened = true;
        }
        else
        {
            GameManager.instance.ShowHint("Bạn chưa có chìa khóa!");
        }
    }

    // Hàm di chuyển cửa từ từ
    IEnumerator OpenDoorSmoothly()
    {
        float time = 0;

        // Tính toán góc xoay đích (đổi từ số Vector3 sang Quaternion)
        Quaternion targetRot = Quaternion.Euler(openRotation);

        while (time < 1)
        {
            time += Time.deltaTime * openSpeed;

            // Lerp là hàm di chuyển mượt từ điểm A đến điểm B
            // Di chuyển vị trí
            transform.localPosition = Vector3.Lerp(closedPosition, openPosition, time);

            // Di chuyển góc xoay
            transform.localRotation = Quaternion.Slerp(closedRotation, targetRot, time);

            yield return null; // Chờ 1 khung hình rồi chạy tiếp
        }

        // Đảm bảo chính xác tuyệt đối khi kết thúc
        transform.localPosition = openPosition;
        transform.localRotation = targetRot;
    }
}
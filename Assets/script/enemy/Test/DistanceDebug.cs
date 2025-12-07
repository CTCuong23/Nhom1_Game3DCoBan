using UnityEngine;

public class DistanceDebug : MonoBehaviour
{
    public Transform targetPlayer;
    public float detectRange = 15f;

    void Start()
    {
        // Kiểm tra xem script có chạy không
        Debug.Log("Script Debug ĐÃ KHỞI ĐỘNG trên: " + gameObject.name);

        if (targetPlayer == null)
        {
            Debug.LogError("LỖI TO: Chưa kéo Player vào script DistanceDebug kìa bạn ơi!");
        }
    }

    void Update()
    {
        // In ra Log dù có Player hay không để test
        if (targetPlayer == null) return;

        float distance = Vector3.Distance(transform.position, targetPlayer.position);

        // In ra mỗi 100 frame để đỡ spam, nhưng chắc chắn phải thấy
        if (Time.frameCount % 60 == 0)
        {
            Debug.Log($"Khoảng cách hiện tại: {distance}");
        }
    }
}
using UnityEngine;

public class EnemySound : MonoBehaviour
{
    // Hàm này để hứng sự kiện OnFootstep từ Animation chạy
    public void OnFootstep(AnimationEvent animationEvent)
    {
        // Hiện tại mình để trống để nó không báo lỗi đỏ nữa.
        // Sau này muốn AI chạy có tiếng động thì viết code phát âm thanh vào đây.
    }

    // Hàm này để hứng sự kiện OnLand (nếu có)
    public void OnLand(AnimationEvent animationEvent)
    {
    }
}
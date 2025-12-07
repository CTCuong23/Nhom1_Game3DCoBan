using UnityEngine;
using UnityEngine.EventSystems; // Cần thư viện này để bắt sự kiện chuột

public class ButtonSound : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [Header("Cài đặt âm thanh")]
    [SerializeField] AudioSource source; // Nơi phát ra âm thanh (Cái loa)
    [SerializeField] AudioClip hoverSound; // File âm thanh khi rê chuột
    [SerializeField] AudioClip clickSound; // File âm thanh khi nhấn (nếu muốn)

    // Hàm này tự động chạy khi chuột đi vào vùng nút (Hover)
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (source != null && hoverSound != null)
        {
            source.PlayOneShot(hoverSound); // PlayOneShot giúp âm thanh không bị cắt ngang nhạc nền
        }
    }

    // Hàm này tự động chạy khi nhấn nút (Click) - Tặng thêm cho bạn
    public void OnPointerClick(PointerEventData eventData)
    {
        if (source != null && clickSound != null)
        {
            source.PlayOneShot(clickSound);
        }
    }
}
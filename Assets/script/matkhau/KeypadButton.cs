using UnityEngine;

public class KeypadButton : MonoBehaviour
{
    [Header("Giá trị nút bấm")]
    public string number; // Điền số 1, 2, ... 9 vào đây

    private KeypadController controller;
    private Renderer rend;
    private Color originalColor;

    void Start()
    {
        controller = GetComponentInParent<KeypadController>();
        rend = GetComponent<Renderer>();
        if (rend != null) originalColor = rend.material.color;
    }

    // Khi di chuột vào -> Đổi màu chút cho biết đang chọn
    void OnMouseEnter()
    {
        if (GameManager.instance.isUsingKeypad && rend != null)
            rend.material.color = Color.yellow; // Hoặc màu gì bro thích
    }

    void OnMouseExit()
    {
        if (rend != null) rend.material.color = originalColor;
    }

    // Khi bấm chuột -> Gửi số về Controller
    void OnMouseDown()
    {
        if (GameManager.instance.isUsingKeypad && controller != null)
        {
            controller.InputNumber(number);

            // Hiệu ứng bấm (nháy màu)
            if (rend != null)
            {
                rend.material.color = Color.green;
                Invoke("ResetColor", 0.1f);
            }
        }
    }

    void ResetColor()
    {
        if (rend != null) rend.material.color = Color.yellow;
    }
}
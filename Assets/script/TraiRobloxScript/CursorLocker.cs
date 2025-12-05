using UnityEngine;

public class CursorLocker : MonoBehaviour
{
    void Start()
    {
        // Dòng này để khóa con chuột vào giữa màn hình
        Cursor.lockState = CursorLockMode.Locked;
        // Dòng này để ẩn con chuột đi
        Cursor.visible = false;
    }
}
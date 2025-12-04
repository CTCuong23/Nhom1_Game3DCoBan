using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DeathEffect : MonoBehaviour
{
    public Image tamHinhMau;
    public float tocDoChay = 0.5f;

    private void OnEnable()
    {
        if (tamHinhMau != null)
        {
            // Reset về 0 ngay khi bảng hiện lên
            tamHinhMau.fillAmount = 0f;
            tamHinhMau.gameObject.SetActive(true);

            StartCoroutine(HieuUngChayMau());
        }
    }

    IEnumerator HieuUngChayMau()
    {
        float giaTriHienTai = 0f;

        while (giaTriHienTai < 1f)
        {
            // --- SỬA Ở ĐÂY: Dùng unscaledDeltaTime thay vì deltaTime ---
            // Để dù game có Pause (TimeScale = 0) thì máu vẫn chảy
            giaTriHienTai += Time.unscaledDeltaTime * tocDoChay;

            tamHinhMau.fillAmount = giaTriHienTai;
            yield return null;
        }

        tamHinhMau.fillAmount = 1f;
    }
}
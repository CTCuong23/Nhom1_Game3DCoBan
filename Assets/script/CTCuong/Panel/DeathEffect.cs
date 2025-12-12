using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DeathEffect : MonoBehaviour
{
    [SerializeField] Image BloodImage;
    [SerializeField] float FlowSpeed = 0.5f;

    private void OnEnable()
    {
        if (BloodImage != null)
        {
            // Reset về 0 ngay khi bảng hiện lên
            BloodImage.fillAmount = 0f;
            BloodImage.gameObject.SetActive(true);

            StartCoroutine(BleedingEffect());
        }
    }

    IEnumerator BleedingEffect()
    {
        float CurrentValue = 0f;

        while (CurrentValue < 1f)
        {
            // --- SỬA Ở ĐÂY: Dùng unscaledDeltaTime thay vì deltaTime ---
            // Để dù game có Pause (TimeScale = 0) thì máu vẫn chảy
            CurrentValue += Time.unscaledDeltaTime * FlowSpeed;

            BloodImage.fillAmount = CurrentValue;
            yield return null;
        }

        BloodImage.fillAmount = 1f;
    }
}
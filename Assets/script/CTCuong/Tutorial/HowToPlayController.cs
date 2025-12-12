using UnityEngine;

public class HowToPlayController : MonoBehaviour
{
    [SerializeField] private GameObject howToPanel;
    [SerializeField] private GameObject hintText;
    [SerializeField] private float hintDuration = 5f;

    private bool firstCloseDone = false;

    void Start()
    {
        // Khi vào game, panel mở sẵn
        howToPanel.SetActive(true);

        // Hint sẽ tự ẩn sau 5s
        if (hintText != null)
            Invoke(nameof(HideHint), hintDuration);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            bool isOpen = howToPanel.activeSelf;
            howToPanel.SetActive(!isOpen);

            // Khi người chơi bấm H lần đầu → hint chưa bị ẩn thì ẩn luôn cho gọn
            if (!firstCloseDone)
            {
                firstCloseDone = true;
                if (hintText != null) hintText.SetActive(false);
            }
        }
    }

    void HideHint()
    {
        if (hintText != null)
            hintText.SetActive(false);
    }
}

using UnityEngine;
using UnityEngine.Playables;
using StarterAssets; // Để gọi được script của nhân vật

public class OpeningCutscene : MonoBehaviour
{
    [Header("Setup Kéo Thả")]
    public PlayableDirector timelineDirector; // Kéo cái Timeline vào đây
    public GameObject player;                 // Kéo nhân vật vào đây
    public GameObject cutsceneCamHolder;      // Kéo cái GameObject cha chứa mấy cái Camera quay phim vào đây

    [Header("Cấu hình")]
    public float blendBackTime = 2f; // Thời gian camera bay từ góc phim về góc chơi

    void Start()
    {
        // --- 1. SETUP KHI VÀO GAME ---

        // Khóa cứng nhân vật, không cho đi lung tung
        var tpc = player.GetComponent<ThirdPersonController>();
        if (tpc != null) tpc.enabled = false;

        // Tắt khả năng xoay camera của chuột
        var input = player.GetComponent<StarterAssetsInputs>();
        if (input != null) input.cursorInputForLook = false;

        // Ẩn UI game đi cho giống phim (Gọi sang GameManager)
        if (GameManager.instance != null && GameManager.instance.gameplayUI != null)
            GameManager.instance.gameplayUI.SetActive(false);

        // --- 2. BẮT ĐẦU CHẠY PHIM ---
        if (timelineDirector != null)
        {
            timelineDirector.Play();
            // Đăng ký sự kiện: Khi Timeline chạy xong thì gọi hàm OnCutsceneFinish
            timelineDirector.stopped += OnCutsceneFinish;
        }
    }

    // Hàm này tự chạy khi Timeline kết thúc
    void OnCutsceneFinish(PlayableDirector director)
    {
        // --- 3. CHUYỂN CẢNH MƯỢT MÀ ---

        // Tắt cụm Camera Cutscene đi. 
        // -> Lúc này Unity sẽ thấy mất cam phim, nó sẽ tự động tìm cam có độ ưu tiên cao tiếp theo (là Cam của người chơi)
        // -> Nhờ CinemachineBrain, nó sẽ "blend" từ vị trí cũ về vị trí mới cực mượt.
        if (cutsceneCamHolder != null) cutsceneCamHolder.SetActive(false);

        // --- 4. TRẢ LẠI QUYỀN ĐIỀU KHIỂN ---

        // Mở khóa nhân vật
        var tpc = player.GetComponent<ThirdPersonController>();
        if (tpc != null) tpc.enabled = true;

        var input = player.GetComponent<StarterAssetsInputs>();
        if (input != null) input.cursorInputForLook = true;

        // Hiện lại UI
        if (GameManager.instance != null && GameManager.instance.gameplayUI != null)
            GameManager.instance.gameplayUI.SetActive(true);

        // Xong nhiệm vụ thì tắt script này đi
        this.enabled = false;
    }
}
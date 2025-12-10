using UnityEngine;
using UnityEngine.Playables;
using TMPro;
using System.Collections;
using StarterAssets;
using Cinemachine; // <--- NHỚ THÊM DÒNG NÀY

public class OpeningCutscene : MonoBehaviour
{
    [Header("Setup Kéo Thả")]
    public PlayableDirector timelineDirector;
    public GameObject player;
    public GameObject cutsceneCamHolder;

    [Header("UI Skip")]
    public GameObject skipHintObject;
    public float timeShowHint = 3f;

    private bool hasSkipped = false;
    private Vector3 startPos;
    private Quaternion startRot;

    // Biến lưu trữ cài đặt Camera cũ để khôi phục sau khi Cut
    private CinemachineBlendDefinition originalBlend;
    private CinemachineBrain mainCameraBrain;

    IEnumerator Start()
    {
        // Lưu vị trí gốc
        if (player != null)
        {
            startPos = player.transform.position;
            startRot = player.transform.rotation;
        }

        // Lấy Brain của Camera chính để sau này chỉnh Blend
        if (Camera.main != null)
            mainCameraBrain = Camera.main.GetComponent<CinemachineBrain>();

        yield return null;

        // Tắt UI Gameplay
        if (GameManager.instance != null && GameManager.instance.gameplayUI != null)
            GameManager.instance.gameplayUI.SetActive(false);

        // Khóa nhân vật
        var tpc = player.GetComponent<ThirdPersonController>();
        if (tpc != null) tpc.enabled = false;

        var input = player.GetComponent<StarterAssetsInputs>();
        if (input != null) input.cursorInputForLook = false;

        // Ẩn chữ Skip
        if (skipHintObject != null) skipHintObject.SetActive(false);

        // Chạy phim
        if (timelineDirector != null)
        {
            timelineDirector.Play();
            timelineDirector.stopped += OnCutsceneFinish;
            StartCoroutine(ShowSkipHint());
        }
    }

    void Update()
    {
        if (!hasSkipped && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Escape)))
        {
            SkipCutscene();
        }
    }

    IEnumerator ShowSkipHint()
    {
        yield return new WaitForSeconds(timeShowHint);
        if (!hasSkipped && skipHintObject != null)
        {
            skipHintObject.SetActive(true);
        }
    }

    void SkipCutscene()
    {
        hasSkipped = true;

        // --- XỬ LÝ INSTANT CUT (CẮT NGAY LẬP TỨC) ---
        if (mainCameraBrain != null)
        {
            // 1. Lưu lại kiểu Blend cũ (thường là EaseInOut 2s)
            originalBlend = mainCameraBrain.m_DefaultBlend;

            // 2. Chỉnh Blend về 0 (Cắt cái rụp)
            mainCameraBrain.m_DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Style.Cut, 0);
        }

        if (timelineDirector != null) timelineDirector.Stop();
    }

    void OnCutsceneFinish(PlayableDirector director)
    {
        if (this == null || !this.enabled) return;

        // --- FIX LỖI RƠI XUYÊN MAP ---
        if (player != null)
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            player.transform.position = startPos + Vector3.up * 0.1f;
            player.transform.rotation = startRot;

            Animator anim = player.GetComponent<Animator>();
            if (anim != null)
            {
                anim.Play("Grounded", 0, 0f);
                anim.SetFloat("Speed", 0f);
                anim.SetFloat("MotionSpeed", 1f);
            }

            if (cc != null) cc.enabled = true;
        }

        // Tắt Camera Cutscene -> Vì Blend đang là 0 nên nó sẽ nhảy bụp về Camera Player
        if (cutsceneCamHolder != null) cutsceneCamHolder.SetActive(false);
        if (skipHintObject != null) skipHintObject.SetActive(false);

        var tpc = player.GetComponent<ThirdPersonController>();
        if (tpc != null) tpc.enabled = true;

        var input = player.GetComponent<StarterAssetsInputs>();
        if (input != null) input.cursorInputForLook = true;

        if (GameManager.instance != null && GameManager.instance.gameplayUI != null)
            GameManager.instance.gameplayUI.SetActive(true);

        // --- KHÔI PHỤC LẠI BLEND CAMERA (Để chơi game bình thường nó còn mượt) ---
        if (mainCameraBrain != null && hasSkipped)
        {
            StartCoroutine(RestoreCameraBlend());
        }
        else
        {
            this.enabled = false;
        }
    }

    IEnumerator RestoreCameraBlend()
    {
        // Đợi 1 khung hình cho việc chuyển Camera hoàn tất
        yield return null;

        // Trả lại cài đặt cũ
        if (mainCameraBrain != null)
        {
            mainCameraBrain.m_DefaultBlend = originalBlend;
        }

        this.enabled = false;
    }
}
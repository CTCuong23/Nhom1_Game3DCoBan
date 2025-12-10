using UnityEngine;
using UnityEngine.Playables;
using TMPro;
using System.Collections;
using StarterAssets;

public class OpeningCutscene : MonoBehaviour
{
    [Header("Setup Kéo Thả")]
    public PlayableDirector timelineDirector;
    public GameObject player;
    public GameObject cutsceneCamHolder;

    [Header("UI Skip")]
    public GameObject skipHintObject;
    public float timeShowHint = 3f;

    [Header("Cấu hình")]
    public float blendBackTime = 2f;

    private bool hasSkipped = false;
    private Vector3 startPos;
    private Quaternion startRot;

    // --- ĐỔI TỪ VOID SANG IENUMERATOR ĐỂ CHỜ ĐỢI ---
    IEnumerator Start()
    {
        // 1. Lưu vị trí gốc
        if (player != null)
        {
            startPos = player.transform.position;
            startRot = player.transform.rotation;
        }

        // 2. CHỜ 1 KHUNG HÌNH (Để GameManager chạy xong việc bật UI của nó trước đã)
        yield return null;

        // 3. GIỜ MỚI BẮT ĐẦU XỬ LÝ (Quyền lực tối cao)

        // Tắt UI Gameplay (Lần này chắc chắn tắt được)
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

        // Dọn dẹp
        if (cutsceneCamHolder != null) cutsceneCamHolder.SetActive(false);
        if (skipHintObject != null) skipHintObject.SetActive(false);

        var tpc = player.GetComponent<ThirdPersonController>();
        if (tpc != null) tpc.enabled = true;

        var input = player.GetComponent<StarterAssetsInputs>();
        if (input != null) input.cursorInputForLook = true;

        // Bật lại UI
        if (GameManager.instance != null && GameManager.instance.gameplayUI != null)
            GameManager.instance.gameplayUI.SetActive(true);

        this.enabled = false;
    }
}
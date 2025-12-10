using UnityEngine;
using System.Collections;
using StarterAssets;
using UnityEngine.AI; // <--- CẦN CÁI NÀY ĐỂ DỊCH CHUYỂN QUÁI

public class HidingLocker : MonoBehaviour
{
    [Header("Setup")]
    public Animator doorAnimator;
    public Transform enterPos;
    public GameObject lockerCamera;
    public GameObject playerMesh;

    [Header("Thông số")]
    public float animDuration = 1f;

    private bool isHiding = false;
    private bool isBusy = false;
    private GameObject player;

    private CharacterController charController;
    private ThirdPersonController tpc;

    void Start()
    {
        if (lockerCamera != null) lockerCamera.SetActive(false);
        player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            charController = player.GetComponent<CharacterController>();
            tpc = player.GetComponent<ThirdPersonController>();
        }
    }

    void Update()
    {
        if (isHiding && !isBusy && Input.GetKeyDown(KeyCode.F))
        {
            StartCoroutine(ExitLockerFast());
        }
    }

    public void EnterLocker()
    {
        if (!isHiding && !isBusy) StartCoroutine(ProcessEnterSmooth());
    }

    // --- VÀO TỦ: MƯỢT MÀ (Giữ nguyên) ---
    IEnumerator ProcessEnterSmooth()
    {
        isBusy = true;

        if (doorAnimator) doorAnimator.Play("Open");

        if (tpc) tpc.enabled = false;
        if (charController) charController.enabled = false;

        yield return new WaitForSeconds(animDuration);

        if (playerMesh) playerMesh.SetActive(false);
        if (lockerCamera) lockerCamera.SetActive(true);

        if (doorAnimator) doorAnimator.Play("Close");

        GameManager.instance.isPlayerHiding = true;
        GameManager.instance.HideHint();
        isHiding = true;

        // --- TÍNH NĂNG MỚI: ĐÁ ĐÍT QUÁI ĐI CHỖ KHÁC ---
        TeleportEnemiesAway();
        // ----------------------------------------------

        yield return new WaitForSeconds(animDuration);
        isBusy = false;
    }

    // --- RA TỦ: TỨC THÌ (Teleport) ---
    IEnumerator ExitLockerFast()
    {
        isBusy = true;

        // 1. Không cần Animation mở cửa -> Chuyển cảnh ngay
        if (lockerCamera) lockerCamera.SetActive(false);
        if (playerMesh) playerMesh.SetActive(true);

        // 2. Dịch chuyển Player ra cửa ngay lập tức
        if (player && enterPos)
        {
            player.transform.position = enterPos.position;
            player.transform.rotation = enterPos.rotation;
        }

        // 3. Trả lại điều khiển
        if (charController) charController.enabled = true;
        if (tpc) tpc.enabled = true;

        // 4. Báo game hết trốn
        GameManager.instance.isPlayerHiding = false;
        isHiding = false;

        // (Tùy chọn) Đảm bảo cửa đóng (đề phòng animation bị lệch)
        if (doorAnimator) doorAnimator.Play("Idle"); // Hoặc "Close"

        yield return null; // Chờ 1 frame cho ổn định
        isBusy = false;
    }

    // --- HÀM DỊCH CHUYỂN QUÁI ---
    void TeleportEnemiesAway()
    {
        // Tìm tất cả GameObject có Tag là "Enemy"
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (var enemy in enemies)
        {
            NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
            EnemyPatrolData patrol = enemy.GetComponent<EnemyPatrolData>();

            // Chỉ dịch chuyển nếu quái có Agent và có dữ liệu đường đi (PatrolData)
            if (agent != null && patrol != null)
            {
                // Lấy 1 điểm ngẫu nhiên trong danh sách đi tuần
                Vector3 randomPoint = patrol.GetRandomWaypoint();

                // Dịch chuyển tức thời (Warp)
                agent.Warp(randomPoint);
                agent.ResetPath(); // Xóa lệnh đuổi cũ

                Debug.Log($"Đã dịch chuyển quái {enemy.name} đến điểm khác để chống Camp!");
            }
        }
    }
}
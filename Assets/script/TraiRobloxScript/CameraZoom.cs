using UnityEngine;
using Cinemachine;
using UnityEngine.UI;

public class CameraZoom : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] CinemachineVirtualCamera vCam;
    [SerializeField] Camera mainCamera;

    [Header("UI Crosshair (Tâm thường)")]
    [SerializeField] Image centerCrosshair;       // Kéo cái CrossHair (Màu trắng) vào đây
    [SerializeField] Image centerCrosshairHover;  // Kéo cái CrossHairHover (Màu xanh) vào đây

    [Header("UI Hand Cursor (Bàn tay Tâm ngắm)")]
    [SerializeField] Image handCursor;      // Bàn tay mặc định (Màu trắng/Mở)
    [SerializeField] Image handHover;       // Bàn tay khi tương tác (Màu đỏ/Nắm)

    [Header("Interaction (Tương tác)")]
    private float interactionDistance = 5f;
    [SerializeField] LayerMask interactableLayer;
    private float interactionRadius = 0.07f;

    [Header("Zoom Settings")]
    private float zoomFOV = 20f;
    private float zoomDistance = 2.0f;
    [Range(0, 1)] public float zoomSide = 1.0f;
    private Vector3 zoomOffset = new Vector3(0.7f, 0.29f, 0.67f);
    private Vector3 zoomDamping = Vector3.zero;
    private float smoothSpeed = 10f;

    [Header("Sensitivity")]
    [Range(0.1f, 1f)] private float mouseSensitivityMultiplier = 0.2f;

    public static float CurrentSensitivityFactor = 1f;

    // Private variables
    private float defaultFOV;
    private float defaultDistance;
    private Vector3 defaultOffset;
    private Vector3 defaultDamping;
    private bool isZooming = false;
    private Cinemachine3rdPersonFollow thirdPersonComponent;

    void Start()
    {
        if (vCam == null) vCam = GetComponent<CinemachineVirtualCamera>();
        if (mainCamera == null) mainCamera = Camera.main;

        thirdPersonComponent = vCam.GetCinemachineComponent<Cinemachine3rdPersonFollow>();

        if (thirdPersonComponent != null)
        {
            defaultFOV = vCam.m_Lens.FieldOfView;
            defaultDistance = thirdPersonComponent.CameraDistance;
            defaultOffset = thirdPersonComponent.ShoulderOffset;
            defaultDamping = thirdPersonComponent.Damping;
        }

        // --- SỬA LỖI 1: XÓA DÒNG GÁN BIẾN SAI Ở ĐÂY ---
        // (Đã xóa dòng: if (centerCrosshair != null) centerCrosshair = centerCrosshairHover;)

        // Khởi đầu: Chưa Zoom thì hiện Tâm thường (Trắng), ẩn Tâm Hover (Xanh)
        if (centerCrosshair != null) centerCrosshair.gameObject.SetActive(true);
        if (centerCrosshairHover != null) centerCrosshairHover.gameObject.SetActive(false);

        // Ẩn cả 2 bàn tay lúc đầu
        if (handCursor != null) handCursor.gameObject.SetActive(false);
        if (handHover != null) handHover.gameObject.SetActive(false);
    }

    void Update()
    {
        if (vCam == null || thirdPersonComponent == null) return;

        // 1. INPUT BẬT/TẮT ZOOM
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            isZooming = !isZooming;
            GameManager.instance.isAiming = !GameManager.instance.isAiming;

            // Cập nhật hiển thị UI
            UpdateCrosshairVisuals();
        }

        // 2. LOGIC TƯƠNG TÁC (Chỉ chạy khi đang Zoom)
        if (isZooming)
        {
            HandleCenterInteraction();
        }

        // 3. ZOOM PHYSICS (Luôn chạy)
        ApplyZoomPhysics();
    }

    // --- SỬA LỖI 2: DÙNG SetActive ĐỂ BẬT TẮT ---
    void UpdateCrosshairVisuals()
    {
        // Xử lý Tâm ngắm giữa màn hình
        if (isZooming)
        {
            // Đang ngắm -> Ẩn tâm trắng, Hiện tâm xanh
            if (centerCrosshair != null) centerCrosshair.gameObject.SetActive(false);
            if (centerCrosshairHover != null) centerCrosshairHover.gameObject.SetActive(true);
        }
        else
        {
            // Không ngắm -> Hiện tâm trắng, Ẩn tâm xanh
            if (centerCrosshair != null) centerCrosshair.gameObject.SetActive(true);
            if (centerCrosshairHover != null) centerCrosshairHover.gameObject.SetActive(false);
        }

        // Xử lý Bàn tay (Giữ nguyên logic của bạn)
        if (isZooming)
        {
            if (handCursor != null) handCursor.gameObject.SetActive(true);
            if (handHover != null) handHover.gameObject.SetActive(false);
        }
        else
        {
            if (handCursor != null) handCursor.gameObject.SetActive(false);
            if (handHover != null) handHover.gameObject.SetActive(false);
        }
    }

    void HandleCenterInteraction()
    {
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.SphereCast(ray, interactionRadius, out hit, interactionDistance, interactableLayer))
        {
            InteractableObject obj = hit.collider.GetComponent<InteractableObject>();

            if (obj != null)
            {
                // Có vật phẩm -> Tắt tay thường, Bật tay đỏ
                if (handCursor != null) handCursor.gameObject.SetActive(false);
                if (handHover != null) handHover.gameObject.SetActive(true);

                // --- LOGIC HIGHLIGHT CŨ CỦA BẠN (Cứ để comment nếu chưa dùng) ---
                /*
                InteractableHighlight highlight = hit.collider.GetComponent<InteractableHighlight>();
                if (highlight != null)
                {
                   if (currentHighlightObj != highlight)
                   {
                       if (currentHighlightObj != null) currentHighlightObj.ToggleHighlight(false);
                       currentHighlightObj = highlight;
                       currentHighlightObj.ToggleHighlight(true);
                   }
                }
                */

                if (Input.GetMouseButtonDown(0))
                {
                    Debug.Log("Đã tương tác với: " + hit.collider.name);
                }
                return;
            }
        }

        // Không trúng gì -> Bật lại tay thường
        if (handCursor != null) handCursor.gameObject.SetActive(true);
        if (handHover != null) handHover.gameObject.SetActive(false);

        /*
        if (currentHighlightObj != null)
        {
            currentHighlightObj.ToggleHighlight(false);
            currentHighlightObj = null;
        }
        */
    }

    void ApplyZoomPhysics()
    {
        float dt = Time.deltaTime * smoothSpeed;
        float targetFOV = isZooming ? zoomFOV : defaultFOV;

        CurrentSensitivityFactor = isZooming ? mouseSensitivityMultiplier : 1f;

        vCam.m_Lens.FieldOfView = Mathf.Lerp(vCam.m_Lens.FieldOfView, targetFOV, dt);
        thirdPersonComponent.CameraDistance = Mathf.Lerp(thirdPersonComponent.CameraDistance, isZooming ? zoomDistance : defaultDistance, dt);
        thirdPersonComponent.ShoulderOffset = Vector3.Lerp(thirdPersonComponent.ShoulderOffset, isZooming ? zoomOffset : defaultOffset, dt);

        Vector3 currentDamping = thirdPersonComponent.Damping;
        thirdPersonComponent.Damping = Vector3.Lerp(currentDamping, isZooming ? zoomDamping : defaultDamping, dt);
    }
}
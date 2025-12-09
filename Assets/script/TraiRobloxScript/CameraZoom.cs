using UnityEngine;
using Cinemachine;
using UnityEngine.UI;

public class CameraZoom : MonoBehaviour
{
    [Header("Highlight Settings (Vật liệu phát sáng)")]
    [SerializeField] Material highlightMaterial; // Kéo Material EdgeGlow vào đây
    private InteractableObject currentHighlightObj;
    private Material originalMaterial;
    private Renderer currentRenderer;

    [Header("Components")]
    [SerializeField] CinemachineVirtualCamera vCam;
    [SerializeField] Camera mainCamera;

    [Header("UI Crosshair (Tâm thường)")]
    [SerializeField] Image centerCrosshair;
    [SerializeField] Image centerCrosshairHover;

    [Header("UI Hand Cursor (Bàn tay Tâm ngắm)")]
    [SerializeField] Image handCursor;
    [SerializeField] Image handHover;

    [Header("Interaction (Tương tác)")]
    private float interactionDistance = 5f;
    [SerializeField] LayerMask interactableLayer;
    private float interactionRadius = 0.07f;
    private float currentHoldTime = 0f;

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

        UpdateCrosshairVisuals();
    }

    void Update()
    {
        if (vCam == null || thirdPersonComponent == null) return;

        // 1. INPUT BẬT/TẮT ZOOM (Phím Q)
        if (Input.GetKeyDown(KeyCode.Q))
        {
            isZooming = !isZooming;
            GameManager.instance.isAiming = !GameManager.instance.isAiming;

            // Nếu tắt ngắm thì reset mọi thứ
            if (!isZooming) ResetInteraction();

            UpdateCrosshairVisuals();
        }

        // 2. LOGIC TƯƠNG TÁC (Chỉ chạy khi đang Zoom)
        if (isZooming)
        {
            HandleCenterInteraction();
        }

        // 3. ZOOM PHYSICS (Luôn chạy để camera mượt)
        ApplyZoomPhysics();
    }

    void UpdateCrosshairVisuals()
    {
        if (isZooming)
        {
            // Đang ngắm: Tắt tâm thường, Bật tâm bàn tay
            if (centerCrosshair != null) centerCrosshair.gameObject.SetActive(false);
            if (centerCrosshairHover != null) centerCrosshairHover.gameObject.SetActive(true); // Cái này là tâm xanh nhỏ
            if (handCursor != null) handCursor.gameObject.SetActive(true);
            if (handHover != null) handHover.gameObject.SetActive(false);
        }
        else
        {
            // Không ngắm: Bật tâm thường
            if (centerCrosshair != null) centerCrosshair.gameObject.SetActive(true);
            if (centerCrosshairHover != null) centerCrosshairHover.gameObject.SetActive(false);
            if (handCursor != null) handCursor.gameObject.SetActive(false);
            if (handHover != null) handHover.gameObject.SetActive(false);
        }
    }

    void HandleCenterInteraction()
    {
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        // Bắn tia từ giữa màn hình
        if (Physics.SphereCast(ray, interactionRadius, out hit, interactionDistance, interactableLayer))
        {
            InteractableObject obj = hit.collider.GetComponent<InteractableObject>();

            // === QUAN TRỌNG: CHỈ XỬ LÝ NẾU LÀ ITEM (VẬT PHẨM) ===
            // Nếu là Cửa (Door) thì bỏ qua, để cho script PlayerInteraction lo
            if (obj != null && obj.type == InteractableObject.ObjectType.Item)
            {
                // 1. XỬ LÝ HIGHLIGHT (Phát sáng)
                if (obj != currentHighlightObj)
                {
                    ResetHighlightEffect(); // Tắt sáng cái cũ

                    // Gán cái mới và bật sáng
                    currentHighlightObj = obj;
                    currentRenderer = obj.GetComponent<Renderer>();

                    if (currentRenderer != null && highlightMaterial != null)
                    {
                        originalMaterial = currentRenderer.material; // Lưu áo cũ
                        currentRenderer.material = highlightMaterial; // Mặc áo mới
                    }
                }

                // 2. XỬ LÝ UI BÀN TAY (Nắm lại)
                if (handCursor != null) handCursor.gameObject.SetActive(false);
                if (handHover != null) handHover.gameObject.SetActive(true);

                // 3. HIỆN GỢI Ý
                GameManager.instance.ShowHint(obj.GetHintText());

                // 4. XỬ LÝ GIỮ PHÍM E
                if (Input.GetKey(KeyCode.E))
                {
                    currentHoldTime += Time.deltaTime;
                    GameManager.instance.UpdateLoading(currentHoldTime, obj.holdTime);

                    if (currentHoldTime >= obj.holdTime)
                    {
                        obj.PerformAction(); // Nhặt đồ
                        ResetInteraction();  // Reset sau khi nhặt
                    }
                }
                else
                {
                    // Thả tay ra thì reset thanh loading
                    if (currentHoldTime > 0)
                    {
                        currentHoldTime = 0f;
                        GameManager.instance.StopLoading();
                    }
                }

                return; // Kết thúc hàm tại đây (đã tìm thấy Item)
            }
        }

        // === NẾU KHÔNG NGẮM TRÚNG ITEM NÀO ===
        ResetInteraction();
    }

    // Hàm dọn dẹp trạng thái
    void ResetInteraction()
    {
        // Trả lại UI Bàn tay mở
        if (handCursor != null && isZooming) handCursor.gameObject.SetActive(true);
        if (handHover != null) handHover.gameObject.SetActive(false);

        // Tắt chữ gợi ý và Loading
        GameManager.instance.HideHint();
        GameManager.instance.StopLoading();
        currentHoldTime = 0f;

        // Tắt hiệu ứng sáng
        ResetHighlightEffect();
    }

    void ResetHighlightEffect()
    {
        if (currentHighlightObj != null && currentRenderer != null)
        {
            // Trả lại material gốc
            currentRenderer.material = originalMaterial;
        }

        currentHighlightObj = null;
        currentRenderer = null;
        originalMaterial = null;
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
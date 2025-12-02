using UnityEngine;
using Cinemachine;
using UnityEngine.UI;
// Không cần thư viện StarterAssets ở đây nữa vì ta không can thiệp vào Input

public class CameraZoom : MonoBehaviour
{
    [Header("Components")]
    public CinemachineVirtualCamera vCam;
    public Camera mainCamera;

    [Header("UI Crosshair (Tâm thường)")]
    public Image centerCrosshair;
    public Color normalColor = new Color(1, 1, 1, 0.2f); // Màu tâm thường
    public Color zoomModeColor = new Color(1, 1, 1, 0f); // Ẩn tâm thường khi zoom (alpha = 0)

    [Header("UI Hand Cursor (Bàn tay Tâm ngắm)")]
    public Image handCursor;            // Kéo hình Bàn Tay vào đây
    public Color handNormalColor = Color.white;
    public Color handHoverColor = Color.green; // Màu xanh khi chĩa vào đồ vật

    [Header("Interaction (Tương tác)")]
    public float interactionDistance = 100f;
    public LayerMask interactableLayer;

    [Header("Zoom Settings")]
    public float zoomFOV = 20f;
    public float zoomDistance = 2.0f;
    [Range(0, 1)] public float zoomSide = 1.0f;
    public Vector3 zoomOffset = new Vector3(0.7f, 0.29f, 0.67f);
    public Vector3 zoomDamping = new Vector3(0.1f, 0.25f, 0.3f);
    public float smoothSpeed = 10f;

    [Header("Sensitivity")]
    [Range(0.1f, 1f)] public float mouseSensitivityMultiplier = 0.2f;

    // Biến này để StarterAssetsInputs đọc (làm chậm chuột)
    public static float CurrentSensitivityFactor = 1f;

    // Private variables
    private float defaultFOV;
    private float defaultDistance;
    private float defaultSide;
    private Vector3 defaultOffset;
    private Vector3 defaultDamping;
    private bool isZooming = false;
    private Cinemachine3rdPersonFollow thirdPersonComponent;

    // Vị trí gốc của bàn tay (để giữ nó ở giữa màn hình)
    private Vector3 initialHandPos;

    void Start()
    {
        if (vCam == null) vCam = GetComponent<CinemachineVirtualCamera>();
        if (mainCamera == null) mainCamera = Camera.main;

        thirdPersonComponent = vCam.GetCinemachineComponent<Cinemachine3rdPersonFollow>();

        if (thirdPersonComponent != null)
        {
            defaultFOV = vCam.m_Lens.FieldOfView;
            defaultDistance = thirdPersonComponent.CameraDistance;
            defaultSide = thirdPersonComponent.CameraSide;
            defaultOffset = thirdPersonComponent.ShoulderOffset;
            defaultDamping = thirdPersonComponent.Damping;
        }

        if (centerCrosshair != null) centerCrosshair.color = normalColor;

        if (handCursor != null)
        {
            handCursor.gameObject.SetActive(false); // Ẩn bàn tay đi lúc đầu
            // Lưu vị trí giữa màn hình của bàn tay (nếu bạn đã đặt nó ở giữa trong Canvas)
            initialHandPos = handCursor.rectTransform.localPosition;
        }
    }

    void Update()
    {
        if (vCam == null || thirdPersonComponent == null) return;

        // 1. INPUT BẬT/TẮT ZOOM
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            isZooming = !isZooming;
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

    void UpdateCrosshairVisuals()
    {
        // Khi Zoom: Ẩn tâm vuông, Hiện bàn tay
        // Khi Tắt: Hiện tâm vuông, Ẩn bàn tay
        if (centerCrosshair != null)
            centerCrosshair.color = isZooming ? zoomModeColor : normalColor;

        if (handCursor != null)
            handCursor.gameObject.SetActive(isZooming);
    }

    void HandleCenterInteraction()
    {
        // Bắn tia từ CHÍNH GIỮA màn hình (0.5, 0.5)
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        // Kiểm tra va chạm
        if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayer))
        {
            // Trúng đồ vật -> Đổi màu bàn tay thành XANH
            if (handCursor != null) handCursor.color = handHoverColor;

            // Bấm chuột trái để tương tác
            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("Đã tương tác với: " + hit.collider.name);
                // GỌI HÀM LOGIC CỦA BẠN Ở ĐÂY
            }
        }
        else
        {
            // Không trúng gì -> Bàn tay màu TRẮNG
            if (handCursor != null) handCursor.color = handNormalColor;
        }
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
using UnityEngine;
using Cinemachine;
using UnityEngine.UI;

public class CameraZoom : MonoBehaviour
{
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

    // --- THÊM BIẾN ĐẾM THỜI GIAN GIỮ E ---
    private float currentHoldTime = 0f;

    // Biến lưu vật phẩm đang được Highlight
    //private InteractableHighlight currentHighlightObj;

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

        if (centerCrosshair != null) centerCrosshair.gameObject.SetActive(true);
        if (centerCrosshairHover != null) centerCrosshairHover.gameObject.SetActive(false);

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

            // Reset trạng thái tương tác khi tắt bật
            if (!isZooming) ResetInteraction();

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
        if (isZooming)
        {
            if (centerCrosshair != null) centerCrosshair.gameObject.SetActive(false);
            if (centerCrosshairHover != null) centerCrosshairHover.gameObject.SetActive(true);
            if (handCursor != null) handCursor.gameObject.SetActive(true);
            if (handHover != null) handHover.gameObject.SetActive(false);
        }
        else
        {
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

        if (Physics.SphereCast(ray, interactionRadius, out hit, interactionDistance, interactableLayer))
        {
            InteractableObject obj = hit.collider.GetComponent<InteractableObject>();

            if (obj != null)
            {
                // A. XỬ LÝ UI IMAGE
                if (handCursor != null) handCursor.gameObject.SetActive(false);
                if (handHover != null) handHover.gameObject.SetActive(true);

                // B. XỬ LÝ HIGHLIGHT (Code cũ của bạn)
                /*
                InteractableHighlight highlight = hit.collider.GetComponent<InteractableHighlight>();
                if (highlight != null && currentHighlightObj != highlight)
                {
                    if (currentHighlightObj != null) currentHighlightObj.ToggleHighlight(false);
                    currentHighlightObj = highlight;
                    currentHighlightObj.ToggleHighlight(true);
                }
                */

                // --- C. HIỆN GỢI Ý & GIỮ E (MỚI THÊM VÀO) ---

                // 1. Hiện chữ "Giữ E để nhặt"
                if (obj.type == InteractableObject.ObjectType.Item)
                    GameManager.instance.ShowHint("Giữ E để nhặt");
                else if (obj.type == InteractableObject.ObjectType.Door)
                {
                    if (GameManager.instance.currentItems >= 3)
                        GameManager.instance.ShowHint("Vật phẩm: 3/3\nGiữ E để thoát");
                    else
                        GameManager.instance.ShowHint($"Chưa đủ đồ ({GameManager.instance.currentItems}/3)");
                }

                // 2. Logic Giữ phím E
                // (Chỉ cho phép giữ nếu là Item hoặc là Cửa đã đủ đồ)
                bool canInteract = (obj.type == InteractableObject.ObjectType.Item) ||
                                   (obj.type == InteractableObject.ObjectType.Door && GameManager.instance.currentItems >= 3);

                if (canInteract && Input.GetKey(KeyCode.E))
                {
                    currentHoldTime += Time.deltaTime;
                    GameManager.instance.UpdateLoading(currentHoldTime, obj.holdTime);

                    if (currentHoldTime >= obj.holdTime)
                    {
                        obj.PerformAction(); // Nhặt đồ hoặc Thắng game
                        ResetInteraction();  // Reset sau khi xong
                    }
                }
                else
                {
                    // Thả tay ra thì reset loading
                    if (currentHoldTime > 0)
                    {
                        currentHoldTime = 0f;
                        GameManager.instance.StopLoading();
                    }
                }

                return; // Kết thúc hàm tại đây
            }
        }

        // --- NẾU KHÔNG NGẮM TRÚNG GÌ ---
        ResetInteraction();
    }

    // Hàm phụ để dọn dẹp trạng thái khi không nhìn thấy vật phẩm
    void ResetInteraction()
    {
        // Trả lại UI mặc định
        if (handCursor != null) handCursor.gameObject.SetActive(true);
        if (handHover != null) handHover.gameObject.SetActive(false);

        // Tắt chữ gợi ý và Loading
        GameManager.instance.HideHint();
        GameManager.instance.StopLoading();
        currentHoldTime = 0f;

        // Tắt Highlight (nếu đang dùng)
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
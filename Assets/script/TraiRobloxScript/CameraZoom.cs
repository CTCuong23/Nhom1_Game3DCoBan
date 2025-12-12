using UnityEngine;
using Cinemachine;
using UnityEngine.UI;

public class CameraZoom : MonoBehaviour
{
    [Header("Highlight Settings")]
    [SerializeField] Material highlightMaterial;
    private InteractableObject currentHighlightObj;
    private Material originalMaterial;
    private Renderer currentRenderer;

    [Header("Components")]
    [SerializeField] CinemachineVirtualCamera vCam;
    [SerializeField] Camera mainCamera;

    [Header("UI Crosshair")]
    [SerializeField] Image centerCrosshair;
    [SerializeField] Image centerCrosshairHover;
    [SerializeField] Image handCursor;
    [SerializeField] Image handHover;

    [Header("Interaction")]
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

        // 1. INPUT ZOOM (Phím Q)
        if (Input.GetKeyDown(KeyCode.Q))
        {
            isZooming = !isZooming;
            GameManager.instance.isAiming = !GameManager.instance.isAiming;
            if (!isZooming) ResetInteraction();
            UpdateCrosshairVisuals();
        }

        // 2. TƯƠNG TÁC KHI ZOOM
        if (isZooming)
        {
            HandleCenterInteraction();
        }

        // 3. ZOOM PHYSICS
        ApplyZoomPhysics();
    }

    void UpdateCrosshairVisuals()
    {
        if (isZooming)
        {
            if (centerCrosshair) centerCrosshair.gameObject.SetActive(false);
            if (centerCrosshairHover) centerCrosshairHover.gameObject.SetActive(true);
            if (handCursor) handCursor.gameObject.SetActive(true);
            if (handHover) handHover.gameObject.SetActive(false);
        }
        else
        {
            if (centerCrosshair) centerCrosshair.gameObject.SetActive(true);
            if (centerCrosshairHover) centerCrosshairHover.gameObject.SetActive(false);
            if (handCursor) handCursor.gameObject.SetActive(false);
            if (handHover) handHover.gameObject.SetActive(false);
        }
    }

    void HandleCenterInteraction()
    {
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.SphereCast(ray, interactionRadius, out hit, interactionDistance, interactableLayer))
        {
            InteractableObject obj = hit.collider.GetComponent<InteractableObject>();

            // CHỈ XỬ LÝ NẾU LÀ ITEM (Bao gồm Pin, Thẻ, và BLUE KEY)
            if (obj != null && obj.type == InteractableObject.ObjectType.Item)
            {
                // Highlight
                if (obj != currentHighlightObj)
                {
                    ResetHighlightEffect();
                    currentHighlightObj = obj;
                    currentRenderer = obj.GetComponent<Renderer>();
                    if (currentRenderer != null && highlightMaterial != null)
                    {
                        originalMaterial = currentRenderer.material;
                        currentRenderer.material = highlightMaterial;
                    }
                }

                // UI Bàn tay
                if (handCursor) handCursor.gameObject.SetActive(false);
                if (handHover) handHover.gameObject.SetActive(true);

                // Gợi ý
                GameManager.instance.ShowHint(obj.GetHintText());

                // Giữ E
                if (Input.GetKey(KeyCode.E))
                {
                    currentHoldTime += Time.deltaTime;
                    GameManager.instance.UpdateLoading(currentHoldTime, obj.holdTime);

                    if (currentHoldTime >= obj.holdTime)
                    {
                        obj.PerformAction();
                        ResetInteraction(); // Reset ngay lập tức
                    }
                }
                else
                {
                    if (currentHoldTime > 0)
                    {
                        currentHoldTime = 0f;
                        GameManager.instance.StopLoading();
                    }
                }
                return;
            }
        }

        ResetInteraction();
    }

    void ResetInteraction()
    {
        if (handCursor != null && isZooming) handCursor.gameObject.SetActive(true);
        if (handHover != null) handHover.gameObject.SetActive(false);

        // Đảm bảo tắt Loading UI
        if (GameManager.instance != null)
        {
            GameManager.instance.HideHint();
            GameManager.instance.StopLoading();
        }

        currentHoldTime = 0f;
        ResetHighlightEffect();
    }

    void ResetHighlightEffect()
    {
        if (currentHighlightObj != null && currentRenderer != null && originalMaterial != null)
        {
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
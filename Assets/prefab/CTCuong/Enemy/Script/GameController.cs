using UnityEngine;
using System.Collections.Generic;
// Nếu dòng dưới báo lỗi đỏ, hãy comment nó lại (thêm // vào đầu dòng)
using StarterAssets;

namespace Script.UI
{
    public class GameController : MonoBehaviour
    {
        // --- SINGLETON (Giúp gọi từ bất cứ đâu) ---
        private static GameController _instance;
        public static GameController Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("GameController");
                    _instance = go.AddComponent<GameController>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        // --- VARIABLES ---
        private bool _isGamePaused;
        private List<GameObject> _activePanels = new List<GameObject>();

        // Các biến lưu trạng thái component để bật lại sau khi Pause
        private Dictionary<Animator, bool> _animatorStates = new Dictionary<Animator, bool>();
        private Dictionary<Rigidbody, bool> _rigidbodyStates = new Dictionary<Rigidbody, bool>();
        private Dictionary<CharacterController, bool> _characterControllerStates = new Dictionary<CharacterController, bool>();
        private Dictionary<MonoBehaviour, bool> _scriptStates = new Dictionary<MonoBehaviour, bool>();

        void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        // --- HÀM CHÍNH (GỌI TỪ ENEMY HOẶC UI) ---

        // Hàm kiểm tra Pause (Dùng cho Enemy check)
        public static bool IsGamePaused()
        {
            return Instance._isGamePaused;
        }

        public static void PauseGame(GameObject panel)
        {
            // 1. Hiện Panel UI (ví dụ bảng chết)
            if (panel != null && !Instance._activePanels.Contains(panel))
            {
                Instance._activePanels.Add(panel);
                panel.SetActive(true);
            }

            if (Instance._isGamePaused) return;
            Instance._isGamePaused = true;

            // 2. Mở khóa chuột để bấm nút
            SetInputState(false);

            // 3. Đóng băng thế giới game
            DisableAllAnimators();
            DisableAllRigidbodies();
            DisableAllCharacterControllers();
            // DisableGameplayScripts(); // Tạm tắt cái này để tránh lỗi logic phức tạp
        }

        public static void ResumeGame(GameObject panel = null)
        {
            // 1. Ẩn Panel
            if (panel != null && Instance._activePanels.Contains(panel))
            {
                Instance._activePanels.Remove(panel);
                panel.SetActive(false);
            }

            // 2. Nếu không còn panel nào mở thì Resume game
            if (Instance._activePanels.Count == 0)
            {
                Instance._isGamePaused = false;

                // Khóa chuột lại để chơi tiếp
                SetInputState(true);

                EnableAllAnimators();
                EnableAllRigidbodies();
                EnableAllCharacterControllers();
                EnableGameplayScripts();
            }
        }

        public static void ClearAllPanels()
        {
            // Dùng khi bấm Restart: Ẩn hết UI, reset trạng thái
            foreach (GameObject panel in Instance._activePanels)
            {
                if (panel != null) panel.SetActive(false);
            }

            Instance._activePanels.Clear();
            Instance._isGamePaused = false;

            // Khóa chuột lại để chơi tiếp
            SetInputState(true);

            EnableAllAnimators();
            EnableAllRigidbodies();
            EnableAllCharacterControllers();
            EnableGameplayScripts();
        }

        // --- XỬ LÝ INPUT & CURSOR (PHẦN QUAN TRỌNG) ---
        private static void SetInputState(bool isGameActive)
        {
            // Xử lý chuột: Active = Khóa chuột (chơi), !Active = Hiện chuột (menu)
            if (isGameActive)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            // Xử lý Starter Assets (Tìm component trong scene để tránh lỗi null)
            // Dùng FindFirstObjectByType cho Unity bản mới, hoặc FindObjectOfType cho bản cũ
            var input = FindFirstObjectByType<StarterAssetsInputs>();
            if (input != null)
            {
                // Ngắt input nhìn/di chuyển khi game pause
                input.cursorInputForLook = isGameActive;
                input.cursorLocked = isGameActive;
                // Nếu StarterAssets của bạn có biến 'playerInput', có thể cần disable nó ở đây
            }
        }

        // --- CÁC HÀM "ĐÓNG BĂNG" HỆ THỐNG (GIỮ NGUYÊN LOGIC CỦA THẦY) ---

        private static void DisableAllAnimators()
        {
            Instance._animatorStates.Clear();
            Animator[] animators = FindObjectsByType<Animator>(FindObjectsSortMode.None);
            foreach (Animator anim in animators)
            {
                // Không tắt animator của UI
                if (anim.gameObject.activeInHierarchy && !IsUIComponent(anim.gameObject))
                {
                    Instance._animatorStates[anim] = anim.enabled;
                    anim.speed = 0f; // Dừng animation tại chỗ
                }
            }
        }

        private static void EnableAllAnimators()
        {
            foreach (var kvp in Instance._animatorStates)
            {
                if (kvp.Key != null) kvp.Key.speed = 1f; // Chạy lại bình thường
            }
            Instance._animatorStates.Clear();
        }

        private static void DisableAllRigidbodies()
        {
            Instance._rigidbodyStates.Clear();
            Rigidbody[] rigidbodies = FindObjectsByType<Rigidbody>(FindObjectsSortMode.None);
            foreach (Rigidbody rb in rigidbodies)
            {
                if (!IsUIComponent(rb.gameObject))
                {
                    Instance._rigidbodyStates[rb] = rb.isKinematic;
                    rb.isKinematic = true; // Dừng vật lý rơi/bay
                }
            }
        }

        private static void EnableAllRigidbodies()
        {
            foreach (var kvp in Instance._rigidbodyStates)
            {
                if (kvp.Key != null) kvp.Key.isKinematic = kvp.Value;
            }
            Instance._rigidbodyStates.Clear();
        }

        private static void DisableAllCharacterControllers()
        {
            Instance._characterControllerStates.Clear();
            CharacterController[] controllers = FindObjectsByType<CharacterController>(FindObjectsSortMode.None);
            foreach (CharacterController cc in controllers)
            {
                if (!IsUIComponent(cc.gameObject))
                {
                    Instance._characterControllerStates[cc] = cc.enabled;
                    cc.enabled = false;
                }
            }
        }

        private static void EnableAllCharacterControllers()
        {
            foreach (var kvp in Instance._characterControllerStates)
            {
                if (kvp.Key != null) kvp.Key.enabled = kvp.Value;
            }
            Instance._characterControllerStates.Clear();
        }

        // Tạm để trống hàm script để tránh lỗi logic game của bạn, 
        // vì disable nhầm script quan trọng sẽ gây bug.
        private static void DisableGameplayScripts() { }
        private static void EnableGameplayScripts() { }

        private static bool IsUIComponent(GameObject obj)
        {
            return obj.GetComponentInParent<Canvas>() != null;
        }
    }
}
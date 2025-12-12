using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using UnityEngine.SceneManagement; // Thư viện để check tên Scene

namespace StarterAssets
{
    public class StarterAssetsInputs : MonoBehaviour
    {
        [Header("Character Input Values")]
        public Vector2 move;
        public Vector2 look;
        public bool jump;
        public bool sprint;
        public bool steal;

        [Header("Movement Settings")]
        public bool analogMovement;

        [Header("Mouse Cursor Settings")]
        public bool cursorLocked = true;
        public bool cursorInputForLook = true;

        // --- KHAI BÁO BIẾN (Sửa lỗi "does not exist") ---
        public static bool isGameActive = true;
        // ------------------------------------------------

        void Start()
        {
            CheckSceneState();
        }

        private void CheckSceneState()
        {
            // Kiểm tra tên Scene hiện tại
            string currentScene = SceneManager.GetActiveScene().name;

            // 1. NẾU LÀ MENU CHÍNH (TraiRoblox2)
            if (currentScene == "TraiRoblox2")
            {
                SetGameActive(false); // Tắt điều khiển, hiện chuột
            }
            // 2. NẾU LÀ CÁC SCENE GAME (MainMap, v.v...)
            else
            {
                SetGameActive(true); // Bật điều khiển để chơi ngay
            }
        }

#if ENABLE_INPUT_SYSTEM
        public void OnMove(InputValue value)
        {
            if (isGameActive) MoveInput(value.Get<Vector2>());
        }

        public void OnLook(InputValue value)
        {
            if (cursorInputForLook && isGameActive) LookInput(value.Get<Vector2>());
        }

        public void OnJump(InputValue value)
        {
            if (isGameActive) JumpInput(value.isPressed);
        }

        public void OnSprint(InputValue value)
        {
            if (isGameActive) SprintInput(value.isPressed);
        }

        public void OnSteal(InputValue value)
        {
            if (isGameActive) StealInput(value.isPressed);
        }

        public void OnMenu(InputValue value)
        {
            // 1. Nếu đang ở Menu chính (TraiRoblox2) -> Chặn nút ESC
            if (SceneManager.GetActiveScene().name == "TraiRoblox2")
            {
                return;
            }

            // 2. Nếu đang trong Game (MainMap) -> Cho phép Pause
            if (value.isPressed && isGameActive)
            {
                // Tìm đối tượng quản lý Pause (GameController hoặc PauseMenuController)
                // Dùng SendMessage để không cần quan tâm script tên là gì (tránh lỗi thiếu script)

                GameObject gameController = GameObject.Find("GameController");
                if (gameController != null)
                {
                    // Gọi hàm "Pause" hoặc "ShowPauseMenu" bên đó
                    gameController.SendMessage("Pause", SendMessageOptions.DontRequireReceiver);
                    gameController.SendMessage("ShowPauseMenu", SendMessageOptions.DontRequireReceiver);
                }
                else
                {
                    // Fallback: Nếu chưa có menu thì tạm thời mở chuột ra để test
                    UnlockCursor();
                }
            }
        }
#endif

        public void MoveInput(Vector2 newMoveDirection)
        {
            move = newMoveDirection;
        }

        public void LookInput(Vector2 newLookDirection)
        {
            look = newLookDirection;
        }

        public void JumpInput(bool newJumpState)
        {
            jump = newJumpState;
        }

        public void SprintInput(bool newSprintState)
        {
            sprint = newSprintState;
        }

        public void StealInput(bool newStealState)
        {
            steal = newStealState;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (isGameActive)
            {
                SetCursorState(cursorLocked);
            }
        }

        private void SetCursorState(bool newState)
        {
            Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !newState;
        }

        // --- CÁC HÀM QUẢN LÝ TRẠNG THÁI (Dùng chung cho cả Game) ---

        public static void UnlockCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            isGameActive = false;
        }

        public static void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            isGameActive = true;
        }

        public static void SetGameActive(bool active)
        {
            isGameActive = active;
            if (active)
            {
                LockCursor();
            }
            else
            {
                UnlockCursor();
            }
        }

        // Hàm này để các script khác (như Zombie) check xem game có đang chạy không
        public static bool IsGameActive()
        {
            return isGameActive;
        }
    }
}
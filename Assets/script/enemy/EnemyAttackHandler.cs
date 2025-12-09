using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Script.UI; // Bắt buộc có dòng này để gọi GameController
using UnityEngine.SceneManagement; // Bắt buộc có để chuyển cảnh

namespace Script.Enemy
{
    public class EnemyAttackHandler : MonoBehaviour
    {
        [Header("Attack Settings")]
        [SerializeField] private float attackRange = 3f;
        [SerializeField] private Transform attackPoint;

        [Header("Timing Settings")]
        [Tooltip("Thời điểm bắt đầu gây sát thương (0.0 - 1.0)")]
        [SerializeField] private float damageStartTime = 0.35f;

        [Tooltip("Thời điểm kết thúc gây sát thương")]
        [SerializeField] private float damageEndTime = 0.5f;

        [Header("UI References")]
        [SerializeField] private GameObject youreDeadPanel;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button BackToMenuButton;
        [SerializeField] private Button QuitButton;
        [SerializeField] private TMP_Text deathText;

        [Header("References")]
        [SerializeField] private Transform player;
        [SerializeField] private Animator enemyAnimator;
        [SerializeField] private Transform playerCheckpoint;
        [SerializeField] private Transform enemyCheckpoint;

        private bool _hasKilledPlayer;
        private bool _isAttacking;
        private bool _hasCheckedThisAttack;
        private int _isAttackHash;
        private bool _hasIsAttackParameter;

        void Start()
        {
            _hasKilledPlayer = false;
            _isAttacking = false;
            _hasCheckedThisAttack = false;

            // --- Setup UI ---
            if (youreDeadPanel != null) youreDeadPanel.SetActive(false);
            if (restartButton != null)
            {
                restartButton.onClick.RemoveAllListeners();
                restartButton.onClick.AddListener(OnRestartButtonClicked);
            }

            // 1. Nút Menu
            if (BackToMenuButton != null)
            {
                BackToMenuButton.onClick.RemoveAllListeners(); // Xóa sạch cũ
                BackToMenuButton.onClick.AddListener(LoadMainMenu); // Gắn hàm LoadMainMenu vào
            }

            // 2. Nút Quit
            if (QuitButton != null)
            {
                QuitButton.onClick.RemoveAllListeners(); // Xóa sạch cũ
                QuitButton.onClick.AddListener(QuitGame); // Gắn hàm QuitGame vào
            }

            // --- Setup Animator ---
            if (enemyAnimator == null) enemyAnimator = GetComponent<Animator>();
            if (enemyAnimator != null)
            {
                _isAttackHash = Animator.StringToHash("IsAttack");
                foreach (AnimatorControllerParameter param in enemyAnimator.parameters)
                {
                    if (param.name == "IsAttack")
                    {
                        _hasIsAttackParameter = true;
                        enemyAnimator.SetBool(_isAttackHash, false);
                        break;
                    }
                }
            }

            // --- Setup Player ---
            if (player == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null) player = playerObj.transform;
            }
            if (attackPoint == null) attackPoint = transform;
        }

        void Update()
        {
            if (_hasKilledPlayer) return;

            // --- CÁCH MỚI: Check qua GameController (An toàn hơn) ---
            if (GameController.IsGamePaused()) return;
            // --------------------------------------------------------

            if (enemyAnimator == null) return;

            AnimatorStateInfo stateInfo = enemyAnimator.GetCurrentAnimatorStateInfo(0);

            bool isAttackParameter = false;
            if (_hasIsAttackParameter)
            {
                isAttackParameter = enemyAnimator.GetBool(_isAttackHash);
            }

            // Logic check tên animation
            bool isInAttackState = isAttackParameter ||
                                   stateInfo.IsName("Attack") ||
                                   CheckStateNameContains(stateInfo, "attack");

            if (isInAttackState)
            {
                if (!_isAttacking)
                {
                    _isAttacking = true;
                    _hasCheckedThisAttack = false;
                }

                // Logic tính sát thương theo thời gian tùy chỉnh
                float currentTime = stateInfo.normalizedTime % 1f;

                if (!_hasCheckedThisAttack && currentTime >= damageStartTime && currentTime <= damageEndTime)
                {
                    _hasCheckedThisAttack = true;
                    CheckAttackHit();
                }
            }
            else
            {
                if (_isAttacking)
                {
                    _isAttacking = false;
                    _hasCheckedThisAttack = false;
                }
            }
        }

        // --- Giữ nguyên các hàm phụ trợ bên dưới ---
        private bool CheckStateNameContains(AnimatorStateInfo stateInfo, string keyword)
        {
            string stateName = GetCurrentStateName(stateInfo);
            return stateName.ToLower().Contains(keyword.ToLower());
        }

        private string GetCurrentStateName(AnimatorStateInfo stateInfo)
        {
            if (enemyAnimator == null) return "Unknown";
            int layerIndex = 0;
            AnimatorClipInfo[] clipInfos = enemyAnimator.GetCurrentAnimatorClipInfo(layerIndex);
            if (clipInfos.Length > 0) return clipInfos[0].clip.name;
            return $"State_{stateInfo.shortNameHash}";
        }

        private void CheckAttackHit()
        {
            if (IsPlayerInAttackRange()) KillPlayer();
        }

        private bool IsPlayerInAttackRange()
        {
            if (player == null) return false;
            float distanceToPlayer = Vector3.Distance(attackPoint.position, player.position);
            return distanceToPlayer <= attackRange;
        }

        private void KillPlayer()
        {
            // Tìm script máu của người chơi
            PlayerStats stats = player.GetComponent<PlayerStats>();

            if (stats != null)
            {
                // Trừ 50 máu
                stats.TakeDamage(50f);

                // Đẩy người chơi ra xa chút hoặc reset attack để không bị trừ máu liên tục mỗi khung hình
                _hasKilledPlayer = true; // Tạm dùng biến này để chặn đánh tiếp (hoặc bro viết logic cooldown)

                // Logic Reset để quái đánh tiếp được sau 1-2 giây (Tùy chọn)
                Invoke("ResetAttackState", 2f);
            }
            else
            {
                // Không có script máu thì giết luôn như cũ
                _hasKilledPlayer = true;
                Script.UI.GameController.PauseGame(youreDeadPanel);
                if (deathText != null) deathText.text = "YOU'RE DEAD!";
            }
        }
        void ResetAttackState()
        {
            _hasKilledPlayer = false; // Cho phép đánh lại
        }

        private void OnRestartButtonClicked()
        {
            GameController.ClearAllPanels();
            if (youreDeadPanel != null) youreDeadPanel.SetActive(false);

            TeleportToCheckpoints();

            if (enemyAnimator != null && _hasIsAttackParameter) enemyAnimator.SetBool(_isAttackHash, false);

            _hasKilledPlayer = false;
            _isAttacking = false;
            _hasCheckedThisAttack = false;
        }

        public void LoadMainMenu()
        {
            SceneManager.LoadScene(0);
        }

        public void QuitGame()
        {
            // Nếu đang test trong Unity Editor thì dừng chơi
            #if UNITY_EDITOR
                        UnityEditor.EditorApplication.isPlaying = false;
            #else
                            // Nếu đã build ra file game thật (.exe) thì tắt ứng dụng
                            Application.Quit();
            #endif
        }

        private void TeleportToCheckpoints()
        {
            if (player != null && playerCheckpoint != null)
            {
                CharacterController playerController = player.GetComponent<CharacterController>();
                if (playerController != null)
                {
                    playerController.enabled = false;
                    player.position = playerCheckpoint.position;
                    player.rotation = playerCheckpoint.rotation;
                    playerController.enabled = true;
                }
                else
                {
                    player.position = playerCheckpoint.position;
                    player.rotation = playerCheckpoint.rotation;
                }
            }
            if (enemyCheckpoint != null)
            {
                transform.position = enemyCheckpoint.position;
                transform.rotation = enemyCheckpoint.rotation;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Transform point = attackPoint != null ? attackPoint : transform;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(point.position, attackRange);
            if (player != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(point.position, player.position);
            }
        }
    }
}
using System;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Chase Player (Throttled)", story: "Chase Player with delay", category: "Action/Navigation", id: "ChasePlayerFinal")]
public partial class MoveToPlayerAction : Action
{
    [SerializeReference]public BlackboardVariable<GameObject> TargetPlayer;
    [SerializeReference]public BlackboardVariable<float> Speed;
    [SerializeReference]public BlackboardVariable<float> StopChaseDistance;

    private NavMeshAgent agent;

    private float _nextUpdatePathTime; // Biến đếm thời gian

    // Biến kiểm tra để đảm bảo Start/Stop luôn đi theo cặp
    private bool hasStartedChase = false;

    protected override Status OnStart()
    {
        if (TargetPlayer.Value == null) return Status.Failure;
        agent = GameObject.GetComponent<NavMeshAgent>();
        if (agent == null) return Status.Failure;


        // Nếu Player đang trốn -> Thất bại ngay (Để quái chuyển sang hành động khác)
        if (GameManager.instance.isPlayerHiding) return Status.Failure;

        EnemySound soundScript = GameObject.GetComponent<EnemySound>();
        if (soundScript != null) soundScript.PlayAlertSound();

        // --- XỬ LÝ NHẠC (MỚI) ---
        if (MusicManager.instance != null)
        {
            MusicManager.instance.StartChase();
            hasStartedChase = true; // Đánh dấu là đã báo nhạc rồi
        }

        agent.speed = Speed.Value;
        agent.isStopped = false;
        agent.SetDestination(TargetPlayer.Value.transform.position);
        _nextUpdatePathTime = Time.time + 0.2f;

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (agent == null || TargetPlayer.Value == null) return Status.Failure;

        // --- NẾU PLAYER TRỐN THÌ DỪNG ĐUỔI ---
        if (GameManager.instance.isPlayerHiding)
        {
            agent.ResetPath();
            return Status.Failure; // Trả về Failure để Behavior Graph chuyển nhánh (ví dụ: Wander)
        }
        // --------------------------------------

        if (Time.time >= _nextUpdatePathTime)
        {
            agent.SetDestination(TargetPlayer.Value.transform.position);
            _nextUpdatePathTime = Time.time + 0.2f;
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            return Status.Success;
        }

        float distance = Vector3.Distance(agent.transform.position, TargetPlayer.Value.transform.position);
        if (distance > StopChaseDistance.Value + 1.0f)
        {
            agent.ResetPath();
            return Status.Failure;
        }

        return Status.Running;
    }

    // --- HÀM MỚI QUAN TRỌNG: OnEnd ---
    // Hàm này tự động chạy khi cái node Chase này kết thúc (dù thành công hay thất bại)
    protected override void OnEnd()
    {
        // Nếu trước đó đã bật nhạc Chase thì giờ tắt đi
        if (hasStartedChase && MusicManager.instance != null)
        {
            MusicManager.instance.StopChase();
            hasStartedChase = false;
        }
    }
}
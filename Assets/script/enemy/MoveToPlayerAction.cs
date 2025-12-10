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
    [SerializeReference] public BlackboardVariable<GameObject> TargetPlayer;
    [SerializeReference] public BlackboardVariable<float> Speed;
    [SerializeReference] public BlackboardVariable<float> StopChaseDistance;

    private NavMeshAgent agent;
    private float _nextUpdatePathTime; // Biến đếm thời gian
    private float _nextUpdatePathTime;
    // Biến kiểm tra để đảm bảo Start/Stop luôn đi theo cặp
    private bool hasStartedChase = false;

    protected override Status OnStart()
    {
        if (TargetPlayer.Value == null) return Status.Failure;
        agent = GameObject.GetComponent<NavMeshAgent>();
        if (agent == null) return Status.Failure;

        // Tìm script EnemySound và gọi tiếng báo động
        // Nếu Player đang trốn -> Thất bại ngay (Để quái chuyển sang hành động khác)
        if (GameManager.instance.isPlayerHiding) return Status.Failure;


        // --- XỬ LÝ ÂM THANH (CŨ) ---
        EnemySound soundScript = GameObject.GetComponent<EnemySound>();
        if (soundScript != null)
        {
            soundScript.PlayAlertSound();
        }

        // --- XỬ LÝ NHẠC (MỚI) ---
        if (MusicManager.instance != null)
        {
            MusicManager.instance.StartChase();
            hasStartedChase = true; // Đánh dấu là đã báo nhạc rồi
        }

        agent.speed = Speed.Value;
        agent.isStopped = false;

        // Gọi lần đầu tiên luôn
        agent.SetDestination(TargetPlayer.Value.transform.position);
        _nextUpdatePathTime = Time.time + 0.2f; // Hẹn 0.2s sau mới gọi lại

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (agent == null || TargetPlayer.Value == null) return Status.Failure;

        // --- SỬA LỖI TẠI ĐÂY: GIỚI HẠN TẦN SUẤT GỌI ---
        // Chỉ cập nhật đường đi mỗi 0.2 giây một lần
        if (Time.time >= _nextUpdatePathTime)
        {
            agent.SetDestination(TargetPlayer.Value.transform.position);
            _nextUpdatePathTime = Time.time + 0.2f; // Reset hẹn giờ
        }

        // 1. CHECK THÀNH CÔNG
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            return Status.Success;
        }

        // 2. CHECK BỎ CUỘC
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
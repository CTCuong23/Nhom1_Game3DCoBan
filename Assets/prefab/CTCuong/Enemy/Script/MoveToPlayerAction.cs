using System;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Move To Player (Instant)", story: "Update destination to Player and finish immediately", category: "Action/Navigation", id: "MoveToPlayerInstant")]
public partial class MoveToPlayerAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> TargetPlayer;
    [SerializeReference] public BlackboardVariable<float> Speed;

    protected override Status OnStart()
    {
        if (TargetPlayer.Value == null) return Status.Failure;

        NavMeshAgent agent = GameObject.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            // 1. Cập nhật tốc độ
            agent.speed = Speed.Value;
            // 2. Cập nhật điểm đến
            agent.SetDestination(TargetPlayer.Value.transform.position);
            // 3. Quan trọng: Mở phanh (đề phòng bị khóa ở nhánh kia)
            agent.isStopped = false;

            // 4. Báo cáo THÀNH CÔNG NGAY LẬP TỨC (để Graph quay lại kiểm tra khoảng cách)
            return Status.Success;
        }
        // Báo lỗi
        return Status.Failure;
    }
}
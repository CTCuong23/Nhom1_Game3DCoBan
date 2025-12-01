using System;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using Random = UnityEngine.Random;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Wander Around (Patrol)", story: "Move to random point within [Radius]", category: "Action/Navigation", id: "WanderAction")]
public partial class WanderAction : Action
{
    [SerializeReference] public BlackboardVariable<float> Radius;
    [SerializeReference] public BlackboardVariable<float> Speed;

    protected override Status OnStart()
    {
        NavMeshAgent agent = GameObject.GetComponent<NavMeshAgent>();
        if (agent == null) return Status.Failure;

        // 1. Kiểm tra xem AI đã đến nơi chưa?
        // Nếu AI đang di chuyển và chưa đến đích, ta cứ để nó đi tiếp (trả về Success để giữ graph chạy)
        if (agent.hasPath && agent.remainingDistance > agent.stoppingDistance)
        {
            return Status.Success;
        }

        // 2. Nếu đã đứng lại (hoặc chưa có đường đi), ta tìm điểm mới
        Vector3 randomDirection = Random.insideUnitSphere * Radius.Value;
        randomDirection += GameObject.transform.position; // Tính điểm xung quanh vị trí hiện tại của AI

        NavMeshHit hit;
        // NavMesh.SamplePosition tìm điểm hợp lệ gần nhất trên sàn điều hướng
        if (NavMesh.SamplePosition(randomDirection, out hit, Radius.Value, 1))
        {
            agent.speed = Speed.Value;
            agent.isStopped = false;
            agent.SetDestination(hit.position);
            return Status.Success;
        }

        return Status.Failure;
    }
}
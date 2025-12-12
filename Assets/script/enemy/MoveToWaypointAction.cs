using System;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Move To Patrol Waypoint", story: "Pick random waypoint and move", category: "Action/Navigation", id: "MoveToWaypoint")]
public partial class MoveToWaypointAction : Action
{
    [SerializeReference][SerializeField] BlackboardVariable<float> Speed;

    protected override Status OnStart()
    {
        NavMeshAgent agent = GameObject.GetComponent<NavMeshAgent>();
        EnemyPatrolData patrolData = GameObject.GetComponent<EnemyPatrolData>();

        if (agent == null || patrolData == null)
        {
            // Nếu quên gắn script EnemyPatrolData thì báo lỗi
            Debug.LogError("Chưa gắn script EnemyPatrolData vào Enemy kìa!");
            return Status.Failure;
        }

        // 1. Kiểm tra: Nếu đang đi dở thì thôi, đi tiếp
        if (!agent.isStopped && agent.hasPath && agent.remainingDistance > 0.5f)
        {
            return Status.Success;
        }

        // 2. Lấy điểm ngẫu nhiên từ danh sách Waypoint
        Vector3 dest = patrolData.GetRandomWaypoint();

        // 3. Set đích đến
        agent.speed = Speed.Value;
        agent.isStopped = false;
        agent.SetDestination(dest);

        return Status.Success;
    }
}
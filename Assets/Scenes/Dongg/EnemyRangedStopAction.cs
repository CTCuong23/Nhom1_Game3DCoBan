using System;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Ranged Enemy Stop", story: "Stop for attacking", category: "Action/FriendLogic", id: "RangedEnemyStop")]
public partial class EnemyRangedStopAction : Action
{
    protected override Status OnStart()
    {
        // Lấy NavMeshAgent từ con quái
        NavMeshAgent agent = GameObject.GetComponent<NavMeshAgent>();

        if (agent != null)
        {
            // Dừng hẳn việc di chuyển
            agent.isStopped = true;

            // Xóa đường đi cũ để quái không bị trượt theo quán tính
            agent.ResetPath();

            // Đảm bảo vận tốc về 0 để Animator chuyển về Idle ngay lập tức
            agent.velocity = Vector3.zero;
        }

        return Status.Success;
    }
}
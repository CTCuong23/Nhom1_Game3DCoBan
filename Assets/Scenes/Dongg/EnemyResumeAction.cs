using System;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Enemy Resume", story: "Resume moving", category: "Action/FriendLogic", id: "EnemyResume")]
public partial class EnemyResumeAction : Action
{
    protected override Status OnStart()
    {
        NavMeshAgent agent = GameObject.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            // Mở khóa để Agent có thể di chuyển lại
            agent.isStopped = false;
        }
        return Status.Success;
    }
}
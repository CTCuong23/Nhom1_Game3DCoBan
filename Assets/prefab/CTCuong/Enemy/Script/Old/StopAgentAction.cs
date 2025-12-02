using System;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Stop Agent", story: "Stop moving immediately", category: "Action/Navigation", id: "StopAgent")]
public partial class StopAgentAction : Action
{
    protected override Status OnStart()
    {
        NavMeshAgent agent = GameObject.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.isStopped = true; // Ra lệnh dừng lại
            agent.ResetPath();      // Xóa đường đi cũ
        }
        return Status.Success;
    }
}
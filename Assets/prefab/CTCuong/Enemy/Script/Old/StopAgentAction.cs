using System;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Stop Agent (Anti-Slide)", story: "Stop movement immediately", category: "Action/Navigation", id: "StopAgentAntiSlide")]
public partial class StopAgentAction : Action
{
    protected override Status OnStart()
    {
        NavMeshAgent agent = GameObject.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.isStopped = true;        // 1. Ngắt động cơ
            agent.velocity = Vector3.zero; // 2. TRIỆT TIÊU QUÁN TÍNH (Chống trượt)
            return Status.Success;
        }
        return Status.Failure;
    }
}
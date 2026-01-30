using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Enemy Look At Player", story: "Rotate to face [Target]", category: "Action/FriendLogic", id: "EnemyLookAt")]
public partial class EnemyLookAtAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    public float RotationSpeed = 10f;

    protected override Status OnUpdate()
    {
        if (Target.Value == null) return Status.Failure;

        // Tính toán hướng nhìn
        Vector3 direction = (Target.Value.transform.position - GameObject.transform.position).normalized;
        direction.y = 0; // Chỉ xoay quanh trục đứng

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            GameObject.transform.rotation = Quaternion.Slerp(GameObject.transform.rotation, targetRotation, Time.deltaTime * RotationSpeed);
        }

        // Nếu góc quay đã gần khớp, coi như hoàn thành để chạy node tiếp theo
        float angle = Vector3.Angle(GameObject.transform.forward, direction);
        if (angle < 5f) return Status.Success;

        return Status.Running;
    }
}
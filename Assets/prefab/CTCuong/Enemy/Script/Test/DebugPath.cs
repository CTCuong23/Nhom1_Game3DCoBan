using UnityEngine;
using UnityEngine.AI;

public class DebugPath : MonoBehaviour
{
    NavMeshAgent agent;
    LineRenderer line;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        // Tạo cái dây vẽ đường
        line = gameObject.AddComponent<LineRenderer>();
        line.startWidth = 0.1f;
        line.endWidth = 0.1f;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = Color.yellow;
        line.endColor = Color.red;
    }

    void Update()
    {
        if (agent == null || !agent.hasPath)
        {
            line.positionCount = 0;
            return;
        }

        // Vẽ đường mà AI đang tính toán trong đầu
        line.positionCount = agent.path.corners.Length;
        line.SetPositions(agent.path.corners);
    }
}
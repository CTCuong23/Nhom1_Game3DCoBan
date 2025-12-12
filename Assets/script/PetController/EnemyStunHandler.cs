using UnityEngine;
using UnityEngine.AI;

public class EnemyStunHandler : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator animator;
    private bool isStunned = false;
    private float originalSpeed;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        if (agent != null) originalSpeed = agent.speed;
    }

    public void ApplyStun(float duration)
    {
        // Nếu đang choáng rồi thì không choáng chồng (hoặc có thể reset thời gian tùy bro)
        if (isStunned) return;

        StartCoroutine(StunRoutine(duration));
    }

    System.Collections.IEnumerator StunRoutine(float duration)
    {
        isStunned = true;
        Debug.Log("QUÁI ĐÃ BỊ CHOÁNG!");

        // 1. Dừng di chuyển NavMesh
        if (agent != null)
        {
            agent.isStopped = true;
            agent.speed = 0;
            agent.velocity = Vector3.zero;
        }

        // 2. Đóng băng Animation (Đứng đơ ra như tượng)
        if (animator != null)
        {
            animator.speed = 0;
        }

        // 3. Đợi hết thời gian
        yield return new WaitForSeconds(duration);

        // 4. Tỉnh lại
        if (agent != null)
        {
            agent.isStopped = false;
            agent.speed = originalSpeed; // Trả lại tốc độ cũ
        }

        if (animator != null)
        {
            animator.speed = 1; // Chạy lại animation bình thường
        }

        isStunned = false;
        Debug.Log("QUÁI TỈNH LẠI RỒI!");
    }
}
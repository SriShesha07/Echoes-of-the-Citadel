using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public Transform player;
    private NavMeshAgent agent;
    public Animator animator;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (agent == null)
        {
            Debug.LogError($"{name}: Missing NavMeshAgent.");
        }

        if (player == null)
        {
            Debug.LogError($"{name}: Player reference is not assigned.");
        }
    }

    private void Update()
    {
        if (agent == null || player == null)
        {
            return;
        }

        if (!agent.isOnNavMesh)
        {
            Debug.LogWarning($"{name}: NavMeshAgent is not on a NavMesh.");
            return;
        }

        agent.SetDestination(player.position);

        if (animator != null)
        {
            animator.SetFloat("Speed", agent.velocity.magnitude);
        }
    }
}
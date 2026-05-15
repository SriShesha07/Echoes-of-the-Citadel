using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Chases the player until within firing range, then stops and lets
/// EnemyAttack handle the shooting. If the player moves away again the
/// enemy resumes the chase.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Animator animator;

    [Header("Combat")]
    [Tooltip("Stop moving and start shooting when within this distance.")]
    public float stoppingDistance = 12f;

    [Tooltip("If the player gets closer than this, back off slightly so the enemy isn't standing on top of the player.")]
    public float retreatDistance = 3f;

    [Tooltip("Speed used when retreating from the player.")]
    public float retreatSpeed = 2f;

    private NavMeshAgent _agent;
    private float _defaultSpeed;
    private Vector3 _animatorRootLocalPosition;
    private Quaternion _animatorRootLocalRotation;
    private bool _hasAnimatorRootPose;

    private void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _defaultSpeed = _agent.speed;
        _agent.stoppingDistance = Mathf.Max(stoppingDistance - 0.5f, 0.1f);

        // Make sure the NavMeshAgent — not the animation — owns the enemy's
        // position. Without this, any root translation curve in a clip (e.g.
        // the Firing Rifle shooting anim) will tug the character down through
        // the floor while NavMeshAgent fights back, producing the "sinking"
        // behaviour the user sees.
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }
        }
        if (animator != null)
        {
            animator.applyRootMotion = false;
            _animatorRootLocalPosition = animator.transform.localPosition;
            _animatorRootLocalRotation = animator.transform.localRotation;
            _hasAnimatorRootPose = true;
        }

        if (player == null)
        {
            PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();
            if (playerHealth != null)
            {
                player = playerHealth.transform;
            }
        }

        if (player == null)
        {
            Debug.LogError($"{name}: Player reference is not assigned on EnemyAI and could not be found in the scene.");
        }
    }

    private void Update()
    {
        if (_agent == null || player == null)
        {
            return;
        }

        if (!_agent.isOnNavMesh)
        {
            // Don't spam every frame — log only once.
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance < retreatDistance)
        {
            _agent.isStopped = false;
            // Back away: pick a point opposite the player.
            Vector3 away = (transform.position - player.position).normalized * retreatDistance;
            _agent.speed = retreatSpeed;
            _agent.SetDestination(transform.position + away);
        }
        else if (distance <= stoppingDistance)
        {
            // In firing range: stop the NavMeshAgent completely so its residual
            // velocity does not suppress EnemyAttack or blend back into walk.
            _agent.speed = _defaultSpeed;
            _agent.isStopped = true;
            _agent.ResetPath();
        }
        else
        {
            // Out of range — chase.
            _agent.speed = _defaultSpeed;
            _agent.isStopped = false;
            _agent.SetDestination(player.position);
        }

        if (animator != null)
        {
            float speed = _agent.isStopped ? 0f : _agent.velocity.magnitude;
            animator.SetFloat("Speed", speed);
        }
    }

    /// <summary>
    /// After Animator + NavMeshAgent finish writing the frame, re-snap to the
    /// NavMeshAgent's authoritative position. This kills any residual Y drift
    /// from animation curves on the root bone (the "enemy sinks into the
    /// floor while firing" bug). It's a one-line safety net — applyRootMotion
    /// is also off, so this normally just rewrites the same value.
    /// </summary>
    private void LateUpdate()
    {
        if (_agent == null || !_agent.enabled || !_agent.isOnNavMesh)
        {
            return;
        }
        Vector3 p = transform.position;
        Vector3 agentPos = _agent.nextPosition;
        // Snap Y to the agent so the visible mesh can't sink below the navmesh.
        if (Mathf.Abs(p.y - agentPos.y) > 0.001f)
        {
            transform.position = new Vector3(p.x, agentPos.y, p.z);
        }

        if (_hasAnimatorRootPose && animator != null)
        {
            animator.applyRootMotion = false;
            animator.transform.localPosition = _animatorRootLocalPosition;
            animator.transform.localRotation = _animatorRootLocalRotation;
        }
    }

    private void FacePlayer()
    {
        Vector3 flat = new Vector3(player.position.x - transform.position.x, 0f, player.position.z - transform.position.z);
        if (flat.sqrMagnitude < 0.0001f) return;
        Quaternion want = Quaternion.LookRotation(flat);
        transform.rotation = Quaternion.Slerp(transform.rotation, want, 8f * Time.deltaTime);
    }
}

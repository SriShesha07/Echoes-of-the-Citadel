using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Ranged enemy attack. The enemy must be standing within <see cref="attackRange"/>
/// of the player, have line of sight, and the cooldown must be ready - then it
/// fires a Projectile from the muzzle transform aimed at the player.
///
/// Falls back to direct damage if no projectile prefab is assigned (preserves
/// the old melee-on-contact behaviour during prefab setup).
/// </summary>
public class EnemyAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackDamage = 10f;

    [Tooltip("Maximum distance at which the enemy will start shooting at the player.")]
    public float attackRange = 18f;

    [Tooltip("Minimum distance - if the player is closer than this, the enemy stops shooting (so they don't shoot through their own collider).")]
    public float minAttackRange = 1.2f;

    [Tooltip("Seconds between shots.")]
    public float attackCooldown = 1.2f;

    [Tooltip("Random delay added on top of the cooldown so a group doesn't fire in perfect sync.")]
    public float cooldownJitter = 0.4f;

    [Header("References")]
    public Transform player;
    public Animator animator;

    [Header("Ranged")]
    [Tooltip("World-space transform the projectile spawns from. Place an empty GameObject at the gun muzzle and drag it here.")]
    public Transform muzzle;

    [Tooltip("Projectile prefab to fire. Must have a Projectile component (use the 'Projectile' script).")]
    public GameObject projectilePrefab;

    [Tooltip("Optional muzzle flash VFX spawned at the muzzle each shot.")]
    public GameObject muzzleFlash;

    [Tooltip("Optional gunshot SFX.")]
    public AudioSource gunAudio;

    [Header("Weapon Setup")]
    [Tooltip("Gun prefab spawned into the enemy hand if the scene instance does not already have one.")]
    public GameObject gunPrefab;

    [Tooltip("Bone or transform that should hold the spawned gun. Defaults to the assigned animator's right hand when empty.")]
    public Transform weaponParent;

    public Vector3 gunLocalPosition = new Vector3(-0.0149f, -0.1037f, -0.0149f);
    public Vector3 gunLocalEulerAngles = new Vector3(0f, 71.334f, -6.25f);
    public Vector3 gunLocalScale = Vector3.one;

    [Tooltip("Created under the gun when muzzle is empty.")]
    public Vector3 muzzleLocalPosition = new Vector3(0.013f, -0.002f, 0.886f);

    [Tooltip("Override damage carried by the spawned projectile. Defaults to attackDamage if 0.")]
    public float projectileDamage = 0f;

    [Tooltip("Override speed for the spawned projectile (units/sec). 0 = use prefab default.")]
    public float projectileSpeed = 0f;

    [Header("Line of Sight")]
    [Tooltip("Layers that block the line of sight (e.g., environment). Leave empty to skip the check.")]
    public LayerMask lineOfSightMask = ~0;

    [Tooltip("Vertical offset added to player position when aiming so the enemy aims at the chest, not the feet.")]
    public float aimAtPlayerHeight = 1.4f;

    [Header("Movement Gating")]
    [Tooltip("Maximum horizontal speed (units/sec) the enemy can be moving and still be considered 'planted' enough to fire. Above this, the Firing Rifle animation is suppressed and the enemy plays its walk/run anim instead.")]
    public float maxFireSpeed = 0.35f;

    [Tooltip("How quickly the enemy turns to face the player while shooting (Slerp factor per second).")]
    public float aimTurnSpeed = 12f;

    [Tooltip("Rotate the spawned weapon so its barrel visually points at the same target as the projectile.")]
    public bool aimWeaponAtPlayer = true;

    [Tooltip("How quickly the visual weapon turns toward the player.")]
    public float weaponAimTurnSpeed = 20f;

    [Header("Animation")]
    [Tooltip("Animator state played whenever this enemy fires. Must match the state name in EnemyAnim.controller.")]
    public string firingStateName = "Firing Rifle";

    [Tooltip("Fixed transition time used when blending into the firing animation.")]
    public float firingBlendTime = 0.05f;

    [Tooltip("Animator layer used for firing. Layer 1 should be the upper-body shooting layer.")]
    public int firingLayerIndex = 1;

    private float _nextAttackTime;
    private bool _warnedMissingAttackParam;
    private bool _warnedMissingFiringState;
    private NavMeshAgent _agent;
    private int _firingStateHash;
    private Transform _gunTransform;
    private Vector3 _barrelLocalDirection = Vector3.forward;

    private void Reset()
    {
        // Sensible defaults when added in the editor.
        attackRange = 18f;
        attackCooldown = 1.2f;
    }

    private void Awake()
    {
        // If the inspector reference wasn't wired up, grab the Animator from
        // self or children so the "Firing Rifle" animation actually plays.
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }
        }

        _agent = GetComponent<NavMeshAgent>();
        _firingStateHash = Animator.StringToHash(firingStateName);
    }

    private void Start()
    {
        SetupWeaponIfNeeded();
    }

    private void Update()
    {
        if (player == null)
        {
            PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();
            if (playerHealth != null)
            {
                player = playerHealth.transform;
            }
            else
            {
                return;
            }
        }

        Vector3 aimTarget = player.position + Vector3.up * aimAtPlayerHeight;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer > attackRange || distanceToPlayer < minAttackRange)
        {
            return;
        }

        // Only fire when the enemy is essentially stationary. NavMeshAgent
        // velocity is the authoritative source of "am I moving" - using it
        // means we don't fire during the gap between attackRange (18) and
        // EnemyAI.stoppingDistance (12) where the enemy is chasing.
        if (_agent != null && !_agent.isStopped && _agent.velocity.magnitude > maxFireSpeed)
        {
            return;
        }

        if (Time.time < _nextAttackTime)
        {
            return;
        }

        if (!HasLineOfSight(aimTarget))
        {
            return;
        }

        Attack(aimTarget);

        float jitter = cooldownJitter > 0f ? Random.Range(0f, cooldownJitter) : 0f;
        _nextAttackTime = Time.time + attackCooldown + jitter;
    }

    private bool HasLineOfSight(Vector3 aimTarget)
    {
        if (muzzle == null)
        {
            return true; // skip LoS if muzzle isn't set up yet
        }
        if (lineOfSightMask == 0)
        {
            return true;
        }

        Vector3 origin = muzzle.position;
        Vector3 dir = (aimTarget - origin);
        float dist = dir.magnitude;
        if (dist < 0.01f) return true;
        dir /= dist;

        if (Physics.Raycast(origin, dir, out RaycastHit hit, dist, lineOfSightMask))
        {
            // If we hit the player (or one of their children), LoS is fine.
            if (hit.collider.GetComponentInParent<PlayerHealth>() != null)
            {
                return true;
            }
            return false;
        }
        return true;
    }

    /// <summary>
    /// Run in LateUpdate so we have the final word on rotation each frame and
    /// don't fight the NavMeshAgent's own facing update. While the enemy is
    /// in firing range we always face the player, regardless of whether we're
    /// chasing or shooting - the Firing Rifle anim aims along local +Z, so
    /// the body must already be pointed at the player for shots to land.
    /// </summary>
    private void LateUpdate()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer > attackRange || distanceToPlayer < minAttackRange)
        {
            return;
        }

        Vector3 aimTarget = player.position + Vector3.up * aimAtPlayerHeight;
        FaceTarget(aimTarget);
        AimWeaponAtTarget(aimTarget);
    }

    private void FaceTarget(Vector3 worldTarget)
    {
        Vector3 flat = new Vector3(worldTarget.x - transform.position.x, 0f, worldTarget.z - transform.position.z);
        if (flat.sqrMagnitude < 0.0001f) return;
        Quaternion want = Quaternion.LookRotation(flat);
        transform.rotation = Quaternion.Slerp(transform.rotation, want, aimTurnSpeed * Time.deltaTime);
    }

    private void AimWeaponAtTarget(Vector3 worldTarget)
    {
        if (!aimWeaponAtPlayer || _gunTransform == null || muzzle == null)
        {
            return;
        }

        Vector3 aimDirection = worldTarget - muzzle.position;
        if (aimDirection.sqrMagnitude < 0.0001f)
        {
            return;
        }

        Quaternion targetBarrelRotation = Quaternion.LookRotation(aimDirection.normalized, Vector3.up);
        Quaternion localBarrelRotation = Quaternion.LookRotation(_barrelLocalDirection, Vector3.up);
        Quaternion targetGunRotation = targetBarrelRotation * Quaternion.Inverse(localBarrelRotation);
        _gunTransform.rotation = Quaternion.Slerp(_gunTransform.rotation, targetGunRotation, weaponAimTurnSpeed * Time.deltaTime);
    }

    private void Attack(Vector3 aimTarget)
    {
        if (animator != null)
        {
            animator.applyRootMotion = false;

            int layer = Mathf.Clamp(firingLayerIndex, 0, animator.layerCount - 1);
            if (layer > 0)
            {
                animator.SetLayerWeight(layer, 1f);
            }

            bool playedFiring = TryPlayFiringState(layer);

            if (!playedFiring && !_warnedMissingFiringState)
            {
                Debug.LogWarning($"{name}: Animator '{animator.runtimeAnimatorController?.name}' could not play '{firingStateName}' on the firing layer.", this);
                _warnedMissingFiringState = true;
            }

            if (HasAnimatorParameter(animator, "Attack"))
            {
                animator.ResetTrigger("Attack");
                animator.SetTrigger("Attack");
            }
            else if (!_warnedMissingAttackParam)
            {
                Debug.LogWarning($"{name}: Animator '{animator.runtimeAnimatorController?.name}' has no 'Attack' trigger.", this);
                _warnedMissingAttackParam = true;
            }
        }

        if (gunAudio != null)
        {
            gunAudio.Play();
        }

        if (projectilePrefab != null && muzzle != null)
        {
            FireProjectile(aimTarget);
        }
        else
        {
            // Fallback (keeps the old behaviour working while you're wiring up
            // the prefab + muzzle in the inspector).
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth == null)
            {
                playerHealth = player.GetComponentInParent<PlayerHealth>();
            }
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
            }
        }
    }

    private void FireProjectile(Vector3 aimTarget)
    {
        Vector3 origin = muzzle.position;
        Vector3 dir = (aimTarget - origin);
        if (dir.sqrMagnitude < 0.0001f)
        {
            dir = muzzle.forward;
        }
        dir.Normalize();

        if (muzzleFlash != null)
        {
            Instantiate(muzzleFlash, origin, Quaternion.LookRotation(dir));
        }

        GameObject go = Instantiate(projectilePrefab, origin, Quaternion.LookRotation(dir));
        Projectile proj = go.GetComponent<Projectile>();
        if (proj != null)
        {
            float dmg = projectileDamage > 0f ? projectileDamage : attackDamage;
            proj.Launch(dir, dmg, projectileSpeed);
        }
    }

    private static bool HasAnimatorParameter(Animator anim, string paramName)
    {
        if (anim == null || anim.runtimeAnimatorController == null) return false;
        foreach (AnimatorControllerParameter p in anim.parameters)
        {
            if (p.name == paramName) return true;
        }
        return false;
    }

    private bool TryPlayFiringState(int layer)
    {
        if (animator == null || string.IsNullOrEmpty(firingStateName))
        {
            return false;
        }

        string layerStateName = animator.GetLayerName(layer) + "." + firingStateName;
        int fullStateHash = Animator.StringToHash(layerStateName);

        if (animator.HasState(layer, fullStateHash))
        {
            animator.CrossFadeInFixedTime(fullStateHash, firingBlendTime, layer, 0f);
            return true;
        }

        if (animator.HasState(layer, _firingStateHash))
        {
            animator.CrossFadeInFixedTime(_firingStateHash, firingBlendTime, layer, 0f);
            return true;
        }

        return false;
    }

    private void SetupWeaponIfNeeded()
    {
        if (muzzle != null)
        {
            CacheWeaponAimReferences();
            return;
        }

        if (gunPrefab == null)
        {
            return;
        }

        Transform parent = weaponParent;
        if (parent == null && animator != null && animator.isHuman)
        {
            parent = animator.GetBoneTransform(HumanBodyBones.RightHand);
        }
        if (parent == null)
        {
            parent = transform;
        }

        GameObject gun = Instantiate(gunPrefab, parent);
        gun.name = gunPrefab.name;
        gun.transform.localPosition = gunLocalPosition;
        gun.transform.localRotation = Quaternion.Euler(gunLocalEulerAngles);
        gun.transform.localScale = gunLocalScale;
        _gunTransform = gun.transform;

        GameObject muzzleObject = new GameObject("Muzzle");
        muzzleObject.transform.SetParent(gun.transform, false);
        muzzleObject.transform.localPosition = muzzleLocalPosition;
        muzzleObject.transform.localRotation = Quaternion.identity;
        muzzleObject.transform.localScale = Vector3.one;
        muzzle = muzzleObject.transform;
        CacheWeaponAimReferences();
    }

    private void CacheWeaponAimReferences()
    {
        if (muzzle == null)
        {
            return;
        }

        if (_gunTransform == null)
        {
            _gunTransform = muzzle.parent;
        }

        if (_gunTransform != null)
        {
            Vector3 barrel = _gunTransform.InverseTransformPoint(muzzle.position);
            if (barrel.sqrMagnitude > 0.0001f)
            {
                _barrelLocalDirection = barrel.normalized;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, minAttackRange);
        if (muzzle != null && player != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(muzzle.position, player.position + Vector3.up * aimAtPlayerHeight);
        }
    }
}

using UnityEngine;

/// <summary>
/// Simple linear projectile. Fired by the enemy (or anyone) toward a target
/// position. Travels forward at <see cref="speed"/>, damages whatever it hits,
/// and self-destructs after <see cref="lifeTime"/> seconds.
///
/// Designed to work with PlayerHealth (for hits on the player) and Target
/// (for hits on other damageable things). Falls back to no-op if neither.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Units per second the projectile travels along its forward axis.")]
    public float speed = 40f;

    [Tooltip("Seconds before the projectile is auto-destroyed if it doesn't hit anything.")]
    public float lifeTime = 4f;

    [Header("Damage")]
    public float damage = 10f;

    [Header("FX")]
    [Tooltip("Particle/VFX prefab spawned at the hit point. Optional.")]
    public GameObject impactEffect;

    [Tooltip("Tags this projectile should ignore on collision (e.g., the shooter's own tag).")]
    public string[] ignoreTags = new string[] { "Enemy" };

    private Rigidbody _rb;
    private bool _consumed;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        // Make sure the rigidbody is set up sensibly for a fast projectile.
        _rb.useGravity = false;
        _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void OnEnable()
    {
        _consumed = false;
        // Push it forward.
        if (_rb != null)
        {
            _rb.linearVelocity = transform.forward * speed;
        }
        // Auto-cleanup if it never hits anything.
        Invoke(nameof(SelfDestruct), lifeTime);
    }

    private void OnDisable()
    {
        CancelInvoke(nameof(SelfDestruct));
    }

    /// <summary>Public entry-point used by the shooter to aim and arm the projectile.</summary>
    public void Launch(Vector3 direction, float overrideDamage = -1f, float overrideSpeed = -1f)
    {
        if (direction.sqrMagnitude > 0.0001f)
        {
            transform.forward = direction.normalized;
        }
        if (overrideDamage > 0f)
        {
            damage = overrideDamage;
        }
        if (overrideSpeed > 0f)
        {
            speed = overrideSpeed;
        }
        if (_rb != null)
        {
            _rb.linearVelocity = transform.forward * speed;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        HandleHit(collision.gameObject, collision.contacts.Length > 0 ? collision.contacts[0].point : transform.position,
                  collision.contacts.Length > 0 ? collision.contacts[0].normal : -transform.forward);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Support trigger colliders too (some player rigs use trigger capsules).
        HandleHit(other.gameObject, transform.position, -transform.forward);
    }

    private void HandleHit(GameObject hit, Vector3 point, Vector3 normal)
    {
        if (_consumed) return;
        if (hit == null) return;

        // Ignore shooter / friendly tags.
        if (ignoreTags != null)
        {
            for (int i = 0; i < ignoreTags.Length; i++)
            {
                if (!string.IsNullOrEmpty(ignoreTags[i]) && hit.CompareTag(ignoreTags[i]))
                {
                    return;
                }
            }
        }

        _consumed = true;

        // Damage routing.
        PlayerHealth playerHealth = hit.GetComponentInParent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
        }
        else
        {
            Target target = hit.GetComponentInParent<Target>();
            if (target != null)
            {
                target.TakeDamage(damage);
            }
        }

        if (impactEffect != null)
        {
            Quaternion rot = normal.sqrMagnitude > 0.0001f
                ? Quaternion.LookRotation(normal)
                : Quaternion.identity;
            Instantiate(impactEffect, point, rot);
        }

        Destroy(gameObject);
    }

    private void SelfDestruct()
    {
        if (this != null && gameObject != null)
        {
            Destroy(gameObject);
        }
    }
}

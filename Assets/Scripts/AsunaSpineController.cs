using UnityEngine;

/// <summary>
/// Spine / Chest stabiliser for the Free Test Character Asuna (Mecanim Humanoid).
///
/// What it fixes:
///   - The visible side-tilt (roll) on the spine while walking/running with a gun.
///   - Gives the upper body proper aim-up / aim-down so the gun follows the
///     camera pitch instead of staring straight ahead.
///
/// How it works:
///   - Asuna's FBX is imported as a Humanoid rig (Spine2 = humanoid Spine,
///     Spine3 = humanoid Chest). We fetch those bones via the Animator so we
///     don't rely on any hard-coded transform names — works even after Unity
///     renames bones internally.
///   - Runs in LateUpdate so it executes AFTER the Animator pose for the frame
///     has been written. Final word on bone rotation wins.
///
/// Drop this on the PLAYER GameObject (the one with the Animator) and assign:
///   - aimCamera     : your main camera (the one MouseLook drives).
///   - playerBody    : the player root that yaws left/right (MouseLook target).
///   - playerShooting: optional — used to only correct while the gun is out.
/// All three are optional — anything left blank uses sensible fallbacks.
///
/// Tuning:
///   - aimWeight: 0 disables completely; 1 fully overrides the anim.
///   - spinePitchShare / chestPitchShare: distribute camera pitch between
///     Spine and Chest for a natural bend (default 40 / 60).
///   - removeRoll: the key knob for the "tilted spine" bug. Leave on.
/// </summary>
[DefaultExecutionOrder(500)]
[RequireComponent(typeof(Animator))]
public class AsunaSpineController : MonoBehaviour
{
    [Header("References")]
    public Camera aimCamera;
    public Transform playerBody;
    public PlayerShooting playerShooting;

    [Header("Aim")]
    [Tooltip("0 = ignore camera pitch, 1 = full pitch override.")]
    [Range(0f, 1f)] public float aimWeight = 0.85f;

    [Tooltip("Maximum pitch (degrees) the spine + chest will bend up/down.")]
    public float maxPitch = 55f;

    [Tooltip("Fraction of total camera pitch applied to the Spine (Spine2). Spine + Chest should add up to ~1.")]
    [Range(0f, 1f)] public float spinePitchShare = 0.4f;

    [Tooltip("Fraction of total camera pitch applied to the Chest (Spine3). Spine + Chest should add up to ~1.")]
    [Range(0f, 1f)] public float chestPitchShare = 0.6f;

    [Header("Anti-Tilt")]
    [Tooltip("Zero out side-to-side roll on the spine. This is the actual fix for the visible tilt while walking with the gun.")]
    public bool removeRoll = true;

    [Tooltip("If true, the aim correction only runs while the gun is in hand (requires playerShooting).")]
    public bool armedOnly = true;

    [Header("Offsets (advanced)")]
    [Tooltip("Extra local Euler offset added to the spine after correction. Use to nudge rest pose if needed.")]
    public Vector3 spineLocalOffsetEuler = Vector3.zero;

    [Tooltip("Extra local Euler offset added to the chest after correction.")]
    public Vector3 chestLocalOffsetEuler = Vector3.zero;

    private Animator _animator;
    private Transform _spineBone; // humanoid Spine (Asuna's "Spine2")
    private Transform _chestBone; // humanoid Chest (Asuna's "Spine3")

    private void Awake()
    {
        _animator = ResolveHumanoidAnimator();
        if (_animator == null)
        {
            // No Humanoid Animator anywhere in this hierarchy. This is normal if
            // the script ends up on a non-character object (e.g. a weapon root
            // or an enemy with a Generic rig). Silently disable instead of
            // spamming the console — the spine fix simply isn't applicable here.
            enabled = false;
            return;
        }

        _spineBone = _animator.GetBoneTransform(HumanBodyBones.Spine);
        _chestBone = _animator.GetBoneTransform(HumanBodyBones.Chest);

        if (_spineBone == null)
        {
            Debug.LogWarning("[AsunaSpineController] Humanoid rig has no Spine bone mapped in the Avatar. Spine fix disabled.", this);
            enabled = false;
            return;
        }

        if (playerBody == null)
        {
            playerBody = transform;
        }

        if (aimCamera == null)
        {
            aimCamera = Camera.main;
        }

        if (playerShooting == null)
        {
            playerShooting = GetComponent<PlayerShooting>();
            if (playerShooting == null)
            {
                playerShooting = GetComponentInChildren<PlayerShooting>();
            }
        }
    }

    /// <summary>
    /// Find a Humanoid Animator: prefer the one on this GameObject, otherwise
    /// search children. Returns null if none of them are humanoid.
    /// </summary>
    private Animator ResolveHumanoidAnimator()
    {
        Animator local = GetComponent<Animator>();
        if (local != null && local.isHuman && local.avatar != null && local.avatar.isValid)
        {
            return local;
        }

        // Asuna's FBX is sometimes set up as a child of the root, so the script
        // gets attached to a parent that has only a stub Animator (or none).
        Animator[] children = GetComponentsInChildren<Animator>(true);
        for (int i = 0; i < children.Length; i++)
        {
            Animator a = children[i];
            if (a != null && a.isHuman && a.avatar != null && a.avatar.isValid)
            {
                return a;
            }
        }

        return null;
    }

    private void LateUpdate()
    {
        if (_spineBone == null)
        {
            return;
        }

        bool armed = (playerShooting == null) || playerShooting.isArmed;
        if (armedOnly && !armed)
        {
            return;
        }

        // --- camera pitch (look up/down) ---
        float pitch = 0f;
        if (aimCamera != null)
        {
            float camX = aimCamera.transform.eulerAngles.x;
            if (camX > 180f) camX -= 360f;
            pitch = Mathf.Clamp(camX, -maxPitch, maxPitch);
        }

        // Normalise pitch shares so they always sum to ~1 (so tuning sliders feel intuitive).
        float shareSum = Mathf.Max(0.0001f, spinePitchShare + chestPitchShare);
        float spineShare = spinePitchShare / shareSum;
        float chestShare = chestPitchShare / shareSum;

        Quaternion bodyYaw = Quaternion.Euler(0f, playerBody.eulerAngles.y, 0f);

        ApplyBoneAim(_spineBone, bodyYaw, pitch * spineShare, spineLocalOffsetEuler);

        if (_chestBone != null)
        {
            // Chest's "yaw reference" should follow whatever the spine just became,
            // otherwise chest rotation fights the spine. Take the parent's world rotation
            // (which is now the corrected spine) projected to yaw.
            Quaternion chestYaw = Quaternion.Euler(0f, _spineBone.eulerAngles.y, 0f);
            ApplyBoneAim(_chestBone, chestYaw, pitch * chestShare, chestLocalOffsetEuler);
        }
    }

    /// <summary>
    /// Re-orients a single bone:
    ///   1. Yaw it to face the same direction as <paramref name="yawReference"/> (kills accumulated twist).
    ///   2. Pitch it by <paramref name="pitchDegrees"/> (camera aim).
    ///   3. Optionally strip Z (roll) — this is the tilt fix.
    ///   4. Blend with the current rotation using aimWeight.
    /// </summary>
    private void ApplyBoneAim(Transform bone, Quaternion yawReference, float pitchDegrees, Vector3 extraLocalEuler)
    {
        Quaternion target = yawReference * Quaternion.Euler(pitchDegrees, 0f, 0f);
        Quaternion blended = Quaternion.Slerp(bone.rotation, target, aimWeight);

        if (removeRoll)
        {
            Vector3 e = blended.eulerAngles;
            blended = Quaternion.Euler(e.x, e.y, 0f);
        }

        bone.rotation = blended;

        if (extraLocalEuler != Vector3.zero)
        {
            bone.localRotation = bone.localRotation * Quaternion.Euler(extraLocalEuler);
        }
    }
}

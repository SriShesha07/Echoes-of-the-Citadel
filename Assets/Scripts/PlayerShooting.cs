using UnityEngine;
using System.Collections;

public class PlayerShooting : MonoBehaviour
{
    [Header("Weapon Swapping")]
    public Animator animator;
    public GameObject gunInHand;
    public GameObject gunOnBack;
    public bool isArmed = true;
    private bool isSwapping = false;

    [Header("References")]
    public Camera tpsCamera;
    public GameObject impactEffect;
    public GameObject wallEffect;
    public AudioSource gunAudio;
    public AudioSource continuousAudio;

    [Header("Settings")]
    public float weaponDamage = 30f;
    public float weaponRange = 100f;
    public float fireRate = 0.15f;
    private float nextTimeToFire = 0f;

    [Header("Visuals")]
    public Transform firePoint;
    public LineRenderer bulletTrail;

    void Start()
    {
        if (animator != null)
        {
            animator.SetBool("IsArmed", isArmed);
            animator.SetLayerWeight(1, isArmed ? 1f : 0f);
        }
        UpdateGunVisibility();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) && !isSwapping)
        {
            StartCoroutine(SwapWeaponRoutine());
        }

        if (isArmed && !isSwapping)
        {
            HandleFiring();
        }
    }

    void HandleFiring()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (animator != null) animator.SetBool("IsFiring", true);
            if (gunAudio != null) gunAudio.Play();
            Shoot();
            nextTimeToFire = Time.time + fireRate;
        }
        else if (Input.GetMouseButton(0) && Time.time >= nextTimeToFire)
        {
            if (continuousAudio != null && !continuousAudio.isPlaying) continuousAudio.Play();
            Shoot();
            nextTimeToFire = Time.time + fireRate;
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (animator != null) animator.SetBool("IsFiring", false);
            if (continuousAudio != null) continuousAudio.Stop();
        }
    }

    IEnumerator SwapWeaponRoutine()
    {
        isSwapping = true;
        isArmed = !isArmed;

        if (animator != null) animator.SetBool("IsArmed", isArmed);
        if (isArmed && animator != null) animator.SetLayerWeight(1, 1f);

        yield return new WaitForSeconds(0.3f);
        UpdateGunVisibility();
        yield return new WaitForSeconds(0.6f);

        if (!isArmed && animator != null) animator.SetLayerWeight(1, 0f);
        isSwapping = false;
    }

    void UpdateGunVisibility()
    {
        if (gunInHand != null) gunInHand.SetActive(isArmed);
        if (gunOnBack != null) gunOnBack.SetActive(!isArmed);
    }

    void Shoot()
    {
        Ray cameraRay = tpsCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
        Vector3 targetPoint;

        if (Physics.Raycast(cameraRay, out RaycastHit cameraHit, weaponRange))
        {
            targetPoint = cameraHit.point; 
        }
        else
        {
            targetPoint = cameraRay.GetPoint(weaponRange); 
        }

        Vector3 shootDirection = (targetPoint - firePoint.position).normalized;

        if (Physics.Raycast(firePoint.position, shootDirection, out RaycastHit hitInfo, weaponRange))
        {
            if (bulletTrail != null && firePoint != null) StartCoroutine(DrawTrail(hitInfo.point));

            if (hitInfo.transform.TryGetComponent(out Target target))
            {
                if (impactEffect) Instantiate(impactEffect, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
                target.TakeDamage(weaponDamage);
            }
            else if (wallEffect)
            {
                Instantiate(wallEffect, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
            }
        }
        else
        {
            if (bulletTrail != null && firePoint != null) StartCoroutine(DrawTrail(firePoint.position + (shootDirection * weaponRange)));
        }
    }

    private IEnumerator DrawTrail(Vector3 hitPoint)
    {
        if (bulletTrail != null && firePoint != null)
        {
            bulletTrail.SetPosition(0, firePoint.position);
            bulletTrail.SetPosition(1, hitPoint);

            bulletTrail.enabled = true;
            yield return new WaitForSeconds(0.04f); 
            bulletTrail.enabled = false;
        }
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (animator != null && isArmed)
        {
            animator.SetLookAtWeight(1.0f, 0.4f, 1.0f, 0.0f, 0.5f);
            Vector3 lookPos = tpsCamera.transform.position + tpsCamera.transform.forward * 50f;
            animator.SetLookAtPosition(lookPos);
        }
        else if (animator != null)
        {
            animator.SetLookAtWeight(0f);
        }
    }
}
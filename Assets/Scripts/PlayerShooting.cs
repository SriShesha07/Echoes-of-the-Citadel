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


    [Header("Aim Correction")]
    public Transform spineBone;
    public Vector3 aimOffset = new Vector3(0f, 40f, 0f);
    private float currentTwistWeight = 0f;

    [Header("Settings")]
    public float weaponDamage = 30f;
    public float weaponRange = 100f;
    public float fireRate = 0.15f;
    private float nextTimeToFire = 0f;

    void Start()
    {
        if (animator != null) animator.SetBool("IsArmed", isArmed);

        if (isArmed)
        {
            if (gunInHand != null) gunInHand.SetActive(true);
            if (gunOnBack != null) gunOnBack.SetActive(false);
        }
        else
        {
            if (gunInHand != null) gunInHand.SetActive(false);
            if (gunOnBack != null) gunOnBack.SetActive(true);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) && !isSwapping)
        {
            StartCoroutine(SwapWeaponRoutine());
        }

        if (isArmed && !isSwapping)
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
        else
        {
            if (animator != null) animator.SetBool("IsFiring", false);
            if (continuousAudio != null && continuousAudio.isPlaying) continuousAudio.Stop();
        }
    }

    IEnumerator SwapWeaponRoutine()
    {
        isSwapping = true;
        isArmed = !isArmed;

        if (animator != null) animator.SetBool("IsArmed", isArmed);

        yield return new WaitForSeconds(0.3f);

        if (isArmed)
        {
            if (gunInHand != null) gunInHand.SetActive(true);
            if (gunOnBack != null) gunOnBack.SetActive(false);
        }
        else
        {
            if (gunInHand != null) gunInHand.SetActive(false);
            if (gunOnBack != null) gunOnBack.SetActive(true);
        }

        yield return new WaitForSeconds(0.6f);
        isSwapping = false;
    }

    void Shoot()
    {

        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
        Ray ray = tpsCamera.ScreenPointToRay(screenCenter);
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo, weaponRange))
        {
            Target targetObject = hitInfo.transform.GetComponent<Target>();
            if (targetObject != null)
            {
                if (impactEffect != null)
                {
                    GameObject impact = Instantiate(impactEffect, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
                    Destroy(impact, 1f);
                }
                targetObject.TakeDamage(weaponDamage);
            }
            else
            {
                if (wallEffect != null)
                {
                    GameObject spark = Instantiate(wallEffect, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
                    Destroy(spark, 1f);
                }
            }
        }
    }

    void LateUpdate()
    {
        if (isArmed && spineBone != null)
        {
            bool isFiring = animator != null && animator.GetBool("IsFiring");

            float targetTwist = isFiring ? 1f : 0f;
            currentTwistWeight = Mathf.MoveTowards(currentTwistWeight, targetTwist, Time.deltaTime * 10f);

            if (currentTwistWeight > 0f)
            {
                spineBone.Rotate(aimOffset * currentTwistWeight, Space.Self);
            }
        }
    }

}
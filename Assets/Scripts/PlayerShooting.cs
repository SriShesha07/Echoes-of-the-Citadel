using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    [Header("References")]
    public Camera tpsCamera;
    public GameObject impactEffect;
    public GameObject wallEffect;
    public AudioSource gunAudio;
    public AudioSource continuousAudio;

    [Header("Settings")]
    public float weaponDamage = 25f;
    public float weaponRange = 100f;
    public float fireRate = 0.15f;
    private float nextTimeToFire = 0f;

    void Start()
    {

    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (gunAudio != null) gunAudio.Play();

            Shoot();
            nextTimeToFire = Time.time + fireRate;
        }
        else if (Input.GetMouseButton(0) && Time.time >= nextTimeToFire)
        {
            if (continuousAudio != null && !continuousAudio.isPlaying)
            {
                continuousAudio.Play();
            }

            Shoot();
            nextTimeToFire = Time.time + fireRate;
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (continuousAudio != null) continuousAudio.Stop();
        }
    }

    void Shoot()
    {
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);

        Ray ray = tpsCamera.ScreenPointToRay(screenCenter);
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo, weaponRange))
        {
            Debug.Log("Hit: " + hitInfo.transform.name);

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
}
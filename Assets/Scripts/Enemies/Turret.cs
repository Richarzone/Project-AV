using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : UnitHealth
{
    [SerializeField] private LayerMask playerLayer;

    [Header("Turret")]
    [SerializeField] private GameObject destroyedTurret;
    [SerializeField] private Transform turretPivot;
    [SerializeField] private Transform gunPivot;
    [SerializeField] private Transform gun;
    [SerializeField] private Transform laserPivot;
    [SerializeField] private float range;
    [SerializeField] private float followRange;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float fireTime;
    [SerializeField] private float reloadTime;
    
    [Header("Projectile")]
    [SerializeField] private GameObject projectile;
    [SerializeField] private float projectileVelocity;

    private LineRenderer lineRenderer;
    private AudioSource audioSource;
    private Transform target;
    private float currentFireTime;
    private bool reloading;

    protected override void Start()
    {
        health = new Health(maxHealth);
        health.OnDeath += Health_OnDeath;

        currentFireTime = fireTime;
        lineRenderer = GetComponent<LineRenderer>();
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        GetTarget();

        if (!target)
        {
            currentFireTime = fireTime;
            return;
        }
        else
        {
            Vector3 direction = target.position - turretPivot.position;
            
            RotateBase(direction);
            RotateGun(direction);

            RaycastHit hit;

            if (Physics.Raycast(gunPivot.position, gunPivot.forward, out hit))
            {
                lineRenderer.enabled = true;
                lineRenderer.SetPosition(0, laserPivot.position);
                lineRenderer.SetPosition(1, hit.point);

                if (1 << hit.collider.gameObject.layer == playerLayer.value && !reloading)
                {
                    currentFireTime -= Time.deltaTime;
                    
                    if (currentFireTime < 0)
                    {
                        Fire();
                        StartCoroutine(Reload());
                    }
                }
                else
                {
                    currentFireTime = fireTime;
                }
            }
            else
            {
                lineRenderer.enabled = false;
            }
        }
    }

    #region Turret
    private void Fire()
    {
        audioSource.Play();
        Vector3 direction = (target.position - gunPivot.position).normalized;

        GameObject projectileInstace = Instantiate(projectile, gunPivot.position, gunPivot.rotation);
        projectileInstace.GetComponent<Projectile>().StopAllCoroutines();
        projectileInstace.GetComponent<Rigidbody>().AddForce((direction * projectileVelocity), ForceMode.Impulse);
    }

    private IEnumerator Reload()
    {
        reloading = true;
        currentFireTime = fireTime;

        yield return new WaitForSeconds(reloadTime);

        reloading = false;
    }

    private void RotateBase(Vector3 direction)
    {
        direction.y = 0;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        turretPivot.rotation = Quaternion.Slerp(turretPivot.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private void RotateGun(Vector3 direction)
    {
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        
        gun.transform.rotation = Quaternion.Slerp(gun.transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        Vector3 gunRotation = gun.transform.rotation.eulerAngles;
        gunRotation.y = 0;
        gunRotation.z = 0;
        gun.transform.localRotation = Quaternion.Euler(gunRotation);
    }

    private void GetTarget()
    {
        if (!target)
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, range, playerLayer);

            if (hitColliders.Length > 0)
            {
                target = hitColliders[0].transform;
            }
        }
        else
        {
            float distance = Vector3.Distance(transform.position, target.position);

            if (distance > followRange)
            {
                target = null;
                lineRenderer.enabled = false;
            }
        }
    }
    #endregion

    private void Health_OnDeath(object sender, System.EventArgs e)
    {
        Instantiate(destroyedTurret, transform.position, transform.rotation);
        Destroy(gameObject);
    }
}
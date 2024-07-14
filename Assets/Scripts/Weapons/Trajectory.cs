using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(AudioSource))]
public class Trajectory : MonoBehaviour
{
    [Header("Weapon SO")]
    [SerializeField] private WeaponSO weaponSO;

    [Header("Weapon Object")]
    [Tooltip("Object that represents the base of weapon and where it will it rotate")]
    [SerializeField] private GameObject weaponBase;
    [Tooltip("Object that represents the weapon")]
    [SerializeField] private GameObject weaponObject;
    [Tooltip("Speed at which the base of the weapon will rotate to its target")]
    [SerializeField] private float weaponBaseRotationSpeed;

    [Header("Weapon Aim Settings")]
    [Tooltip("Object that the weapon will follow")]
    [SerializeField] private Transform aimObject;
    [Tooltip("Layers where the weapon can aim")]
    [SerializeField] private LayerMask aimingLayer;
    [SerializeField] private LayerMask wallLayer;
    [Tooltip("Maximum distance where the impact point will follow")]
    [SerializeField] private float maxAimDistance;
    [Tooltip("Offset of the local Y position of the aim object. Use so that is at global 0")]
    [SerializeField] private float aimObjectYOffset;
    [SerializeField] private float aimObjectZOffset;

    [Header("Projectile Pooling")]
    [SerializeField] private int objectsToPool;
    private List<GameObject> projectilePool = new List<GameObject>();

    [Header("Zoom Settings")]
    [SerializeField] private GameObject cameraPivot;
    [SerializeField] private float zoomThreshold;

    [Header("Origin Points")]
    [SerializeField] private Transform tankTransform;
    [SerializeField] private Transform weaponLineOrigin;
    [SerializeField] private Transform aimLineOrigin;

    [Header("Parabolic Weapon Line Settings")]
    [Min(2)] [Tooltip("Number of points to draw along the trajectory")]
    [SerializeField] private int resolution;
    [Range(0.01f, 0.25f)] [Tooltip("Number of points to draw along the trajectory")]
    [SerializeField] private float timeBetweenPoints;
    [Tooltip("Maximum height of the trajectory")]
    [SerializeField] private float maxHeight;
    [Tooltip("Width of the line")]
    [SerializeField] private float lineWidth;

    [Header("Aim Line Settings")]
    [SerializeField] private float yOriginOffset;

    [Header("Impact Point")]
    [SerializeField] private Transform impactPointObject;

    [Header("VFX")]
    [SerializeField] private bool hasEjectionEffect;
    [SerializeField] private ParticleSystem ejectionEffect;

    // Components
    private AudioSource audioSource;
    public LineRenderer weaponLineRenderer;
    public LineRenderer aimLineRenderer;

    // Mouse and base aiming
    private Vector3 mousePosition;
    private Vector3 targetPosition;
    private Quaternion turretTarget;

    // Weapon utilities
    private float timeBetweenShots;
    private int currentAmmo;
    private int currentReseveAmmo;
    private int totalAmmo;
    private bool reload;
    private bool active;

    private Collider playerCollider;

    private void Awake()
    {
        currentAmmo = weaponSO.ammo;
        currentReseveAmmo = weaponSO.reserveAmmo;
        totalAmmo = currentAmmo + currentReseveAmmo;
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        weaponLineRenderer = transform.GetChild(0).GetComponent<LineRenderer>();
        aimLineRenderer = transform.GetChild(1).GetComponent<LineRenderer>();

        DeactivateLines();
        CreateProjectilePool();
    }

    private void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit raycastHit, Mathf.Infinity, aimingLayer))
        {
            mousePosition = raycastHit.point;

            aimObject.localPosition = Vector3.forward * Vector3.Distance(tankTransform.position, mousePosition);
            aimObject.localPosition = Vector3.ClampMagnitude(aimObject.localPosition, maxAimDistance);
            aimObject.localPosition = new Vector3(aimObject.localPosition.x, aimObjectYOffset, aimObject.localPosition.z + aimObjectZOffset);

            weaponObject.transform.LookAt(aimObject.position);
            
            if (active)
            {
                CalculateTrajectory();
            }
        }
        
        TurretRotation();
        
        // If the time between shots is zero or less than zero return
        if (timeBetweenShots <= 0f)
        {
            return;
        }

        // Refresh the time between shots
        timeBetweenShots -= Time.deltaTime;
    }

    #region Weapon
    public void Shoot()
    {
        // If the time between shots is zero and its not reloading
        if (timeBetweenShots <= 0f && !reload && totalAmmo != 0)
        {
            // Get random audio settings
            ModifyAudioSettings();

            // Get random sound from the weapon sounds
            int randomSound = Random.Range(0, weaponSO.weaponSound.Count);

            // Play weapon fire sound
            audioSource.PlayOneShot(weaponSO.weaponSound[randomSound]);

            // Substract one round from the ammo counter
            currentAmmo--;
            totalAmmo--;

            // If there is no more ammo, set the ammo counter to red and reload
            if (currentAmmo == 0)
            {
                //weaponName.color = Color.red;
                CallReload();
            }

            if (weaponSO.weaponType == WeaponSO.WeaponType.ShotgunShot)
            {
                for (int i = 0; i <= weaponSO.shotsPerClick; i++)
                {
                    InstantiateProjectile();
                }
            }
            else
            {
                InstantiateProjectile();
            }

            if (hasEjectionEffect)
            {
                ejectionEffect.Play();
            }

            // Set the time between shots to its corresponding rate of fire
            timeBetweenShots = 1 / weaponSO.rateOfFire;
        }
    }

    private void InstantiateProjectile()
    {
        // Set the spread values
        float spreadX = Random.Range(-weaponSO.spread, weaponSO.spread);
        float spreadY = Random.Range(-weaponSO.spread, weaponSO.spread);
        float spreadZ = Random.Range(-weaponSO.spread, weaponSO.spread);
        Vector3 spread = (new Vector3(spreadX * weaponSO.spreadXMultiplier, spreadY * weaponSO.spreadYMultiplier, spreadZ * weaponSO.spreadZMultiplier));

        // Set the directin the projectile will fly to
        Vector3 direction = (targetPosition - transform.position).normalized;

        // Instantiate the projectile
        GameObject payloadInstance = GetPooledObject();

        payloadInstance.transform.position = transform.position;
        payloadInstance.transform.rotation = transform.rotation;
        payloadInstance.SetActive(true);
        payloadInstance.GetComponent<Projectile>().StopAllCoroutines();
        payloadInstance.GetComponent<Rigidbody>().AddForce((direction * weaponSO.projectileVelocity) + spread, ForceMode.Impulse);
        //projectile.GetComponent<Rigidbody>().velocity = (transform.forward * weaponSO.projectileVelocity) + spread;
    }

    public void RefillAmmo()
    {
        currentReseveAmmo = weaponSO.reserveAmmo;
        totalAmmo = currentReseveAmmo + currentAmmo;

        tankTransform.GetComponent<TankController>().UpdateWeaponInfo();
    }

    public void Reload()
    {
        int ammoToReload = weaponSO.ammo - currentAmmo;

        if (currentReseveAmmo >= ammoToReload)
        {
            // If there is enough reserve ammo, fully reload the magazine
            currentAmmo += ammoToReload;
            currentReseveAmmo -= ammoToReload;
        }
        else if (currentReseveAmmo > 0)
        {
            // If there isn't enough reserve ammo, add whatever is left
            currentAmmo += currentReseveAmmo;
            currentReseveAmmo = 0;
        }
        
        tankTransform.GetComponent<TankController>().UpdateWeaponInfo();

        reload = false;
    }

    public void CallReload()
    {
        if (currentReseveAmmo == 0)
        {
            return;
        }

        reload = true;
        //weaponSatus = WeaponSatus.Reloading;
        audioSource.PlayOneShot(weaponSO.reloadSound[0]);

        Invoke("Reload", weaponSO.reloadTime);
    }

    private void TurretRotation()
    {
        Vector3 direction = mousePosition - transform.position;
        direction.y = 0;

        turretTarget = Quaternion.LookRotation(direction);
        weaponBase.transform.rotation = Quaternion.RotateTowards(weaponBase.transform.rotation, turretTarget, weaponBaseRotationSpeed * Time.deltaTime);
    }

    //5.865f z offset of the cannon
    public void Zoom(GameObject cameraPivot)
    {
        // Set the camera target position when the player zooms in
        Vector3 zoomTarget = (transform.position + mousePosition) / 2f;

        zoomTarget.x = Mathf.Clamp(zoomTarget.x, -zoomThreshold + transform.position.x, zoomThreshold + transform.position.x);
        zoomTarget.y = 0f;
        zoomTarget.z = Mathf.Clamp(zoomTarget.z, -zoomThreshold + transform.position.z, zoomThreshold + transform.position.z);

        cameraPivot.transform.position = zoomTarget;

        DrawAimLine();
    }

    public void Active(bool value)
    {
        active = value;
    }

    /*public void MousePosition(Vector3 position)
    {
        mousePosition = position;
    }*/
    #endregion

    #region Weapon Properties
    public string GetWeaponName()
    {
        return weaponSO.name;
    }

    public int GetCurrentAmmo()
    {
        return currentAmmo;
    }

    public int GetReserveAmmo()
    {
        return currentReseveAmmo;
    }

    public int GetMaxAmmo()
    {
        return weaponSO.reserveAmmo;
    }

    public Sprite GetCrosshair()
    {
        return weaponSO.crosshair;
    }

    public Material GetCrosshairMaterial()
    {
        return weaponSO.material;
    }

    public Vector3 GetCrosshairSize()
    {
        return new Vector3(weaponSO.size, weaponSO.size, weaponSO.size);
    }
    #endregion

    #region Projectile Pooling
    private void CreateProjectilePool()
    {
        Transform globalPool = GameObject.FindGameObjectWithTag("ObjectPool").transform;

        for (int i = 0; i < objectsToPool; i++)
        {
            GameObject instance = Instantiate(weaponSO.projectile);
            instance.SetActive(false);
            instance.GetComponent<Projectile>().SetPlayerCollider(playerCollider);
            projectilePool.Add(instance);

            instance.transform.SetParent(globalPool);
        }
    }

    private GameObject GetPooledObject()
    {
        for (int i = 0; i < objectsToPool; i++)
        {
            if (!projectilePool[i].activeInHierarchy)
            {
                return projectilePool[i];
            }
        }
        return null;
    }
    #endregion

    #region Projectile Trajectoy & Line Rendering
    private void DrawAimLine()
    {
        Vector3 lineOrigin = new Vector3(aimLineOrigin.position.x, yOriginOffset, aimLineOrigin.position.z);

        aimLineRenderer.SetPosition(0, lineOrigin);
        aimLineRenderer.SetPosition(1, new Vector3(mousePosition.x, mousePosition.y, mousePosition.z));
    }

    private void CalculateTrajectory()
    {
        Vector3 direction = (aimObject.position - weaponLineOrigin.position).normalized;

        weaponLineRenderer.SetPosition(0, weaponLineOrigin.position);

        if (Physics.Raycast(weaponLineOrigin.position, direction, out RaycastHit raycastHit, Mathf.Infinity, wallLayer))
        {
            targetPosition = raycastHit.point;
            weaponLineRenderer.SetPosition(1, raycastHit.point);
            impactPointObject.position = raycastHit.point;
        }

        /*weaponLineRenderer.positionCount = Mathf.CeilToInt(resolution / timeBetweenPoints) + 1;

        Vector3 startPosition = weaponLineOrigin.position;
        Vector3 startVelocity = weaponSO.projectileVelocity * transform.forward / weaponSO.projectile.GetComponent<Rigidbody>().mass;

        int i = 0;
        weaponLineRenderer.SetPosition(i, startPosition);

        for (float t = 0; t < resolution; t += timeBetweenPoints)
        {
            i++;
            Vector3 point = startPosition + t * startVelocity;

            if (weaponSO.useGravity)
            {
                point.y = startPosition.y + startVelocity.y * t + (Physics.gravity.y / 2f * (t * t));
            }

            weaponLineRenderer.SetPosition(i, point);

            Vector3 lastPosition = weaponLineRenderer.GetPosition(i - 1);

            if (Physics.Raycast(lastPosition, (point - lastPosition).normalized, out RaycastHit hit, (point - lastPosition).magnitude, wallLayer))
            {
                weaponLineRenderer.SetPosition(i, hit.point);
                weaponLineRenderer.positionCount = i + 1;

                //targetPosition = hit.point;

                return;

                /*if (Vector3.Distance(weaponLineOrigin.position, mousePosition) < maxAimDistance)
                {
                    weaponLineRenderer.SetPosition(i, aimObject.position);
                    weaponLineRenderer.positionCount = i + 1;

                    targetPosition = aimObject.position;

                    impactPointObject.position = Camera.main.WorldToScreenPoint(aimObject.position);
                    return;
                }
                else
                {
                    weaponLineRenderer.SetPosition(i, hit.point);
                    weaponLineRenderer.positionCount = i + 1;

                    targetPosition = hit.point;

                    Debug.Log(hit.point);

                    impactPointObject.position = Camera.main.WorldToScreenPoint(hit.point);
                    return;
                }
            }
        }*/
    }

    private Vector3 CalculateParabolicPoint(float t)
    {
        float x = Mathf.Lerp(weaponLineOrigin.position.x, aimObject.position.x, t);
        float y = Mathf.Lerp(weaponLineOrigin.position.y, aimObject.position.y, t);
        float z = Mathf.Lerp(weaponLineOrigin.position.z, aimObject.position.z, t);

        float parabolaHeight = Mathf.Lerp(weaponLineOrigin.position.y, aimObject.position.y + maxHeight, t);

        y += parabolaHeight * (4f * t - 4f * t * t);

        if (y > (weaponLineOrigin.position.y))
        {
            y = weaponLineOrigin.position.y;
        }

        return new Vector3(x, y, z);
    }

    public void ActivateLines() 
    {
        aimLineRenderer.enabled = true;
        weaponLineRenderer.enabled = true;
    }

    public void DeactivateLines()
    {
        aimLineRenderer.enabled = false;
        weaponLineRenderer.enabled = false;
    }
    #endregion

    private void ModifyAudioSettings()
    {
        float pitch = Random.Range(weaponSO.lowerPitch, weaponSO.upperPitch);
        float volume = Random.Range(weaponSO.lowerVolume, weaponSO.upperVolume);
        audioSource.pitch = pitch;
        audioSource.volume = volume;
    }

    public void PlayerCollider(Collider value)
    {
        playerCollider = value;
    }
}
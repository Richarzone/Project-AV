using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class WeaponClass : MonoBehaviour
{
    //Try poitioning the gun pivot onn the negative X area
    public enum WeaponSatus
    {
        Armed,
        Reloading
    }

    [Header("Weapon Settings")]
    [SerializeField] private WeaponSO weaponSO;
    [SerializeField] private bool hasMultiplePivots;
    [SerializeField] private bool fireFromAllPivtos;
    [SerializeField] private bool isTopDown;
    [SerializeField] private bool canRotate;

    //Number of weapon pivots inside of a parent weapon GameObject
    private int childCount;

    [Header("Weapon Composition")]
    [SerializeField] private List<WeaponObject> weaponObjects = new List<WeaponObject>();

    [Header("UI")]
    [SerializeField] bool useUI;
    //[SerializeField] private GameObject graphics;
    //[SerializeField] private GameObject crosshair;
    //[SerializeField] private GameObject weaponIndicator;
    [SerializeField] private TextMeshProUGUI weaponName;
    [SerializeField] private TextMeshProUGUI ammoCounter;

    [Header("Crosshair")]
    [SerializeField] private bool canIncrementSize;
    [SerializeField] private float crosshairSizeIncrement;
    [SerializeField] private float sizeIncrementSpeed;
    private Vector3 graphicsPos;
    private Vector3 crosshairStartingSize;

    [Header("Audio")]
    [SerializeField] private bool useAudio;
    [SerializeField] private AudioSource audioSource;

    private WeaponSatus weaponSatus;
    private Vector3 targetPosition;
    private Vector3 direction;
    private float targetRotation;
    private float timeBetweenShots;
    private int currentWeapon;
    private int currentAmmo;
    private bool reload;
    private bool shoot;

    private void Awake()
    {
        if (useAudio)
        {
            audioSource = GetComponent<AudioSource>();
            //audioSource.clip = weaponSO.weaponSound;
        }
    }

    private void Start()
    {
        weaponSatus = WeaponSatus.Armed;
        childCount = transform.childCount;

        currentAmmo = weaponSO.ammo;

        if (useUI)
        {
            ammoCounter.text = currentAmmo.ToString();
        }

        timeBetweenShots = 0f;
    }

    private void Update()
    {
        // Resize the crosshair to its original size
        if (canIncrementSize)
        {
            //crosshair.transform.localScale = Vector3.Lerp(crosshair.transform.localScale, crosshairStartingSize, Time.deltaTime * sizeIncrementSpeed);
        }

        if (canRotate)
        {
            // Rotate the weapon
            RotateWeapon();
        }

        // Shoot the weapon
        if (Input.GetMouseButton(0))
        {
            Shoot();
        }

        // If the time between shots is zero or less than zero return
        if (timeBetweenShots <= 0f)
        {
            return;
        }

        // Refresh the time between shots
        timeBetweenShots -= Time.deltaTime;
    }

    public void OnChangeWeapon()
    {
        //crosshairStartingSize = crosshair.transform.localScale;
        //ammoCounter.text = currentAmmo.ToString();
        shoot = false;
    }

    public void Shoot()
    {
        // If the time between shots is zero and its not reloading
        if (timeBetweenShots <= 0f && !reload)
        {
            // Get random audio settings
            if (useAudio)
            {
                ModifyAudioSettings();
            }

            // Play weapon fire sound
            //audioSource.PlayOneShot(weaponSO.weaponSound);

            // Increment the size of the crosshair
            if (canIncrementSize)
            {
                //crosshair.transform.localScale = crosshair.transform.localScale * crosshairSizeIncrement;
            }

            // Substract one round from the ammo counter
            currentAmmo--;

            // Set the ammo updated count to the UI counter
            if (useUI)
            {
                ammoCounter.text = currentAmmo.ToString();
            }

            // If there is no more ammo, set the ammo counter to red and reload
            if (currentAmmo == 0)
            {
                weaponName.color = Color.red;
                StartCoroutine(Reload());
            }

            // Operation to calculate rate of fire
            timeBetweenShots = 1 / weaponSO.rateOfFire;

            // If the weapon does not have multiple pivots, instantiate projectile and return
            if (!hasMultiplePivots)
            {
                SetDirection();
                InstantiateProjectile(weaponObjects[currentWeapon].GunPivot(), targetRotation, weaponObjects[currentWeapon].GunPivot().forward + direction);
                return;
            }

            // If the weapon has multiple pivot points and fires from all pivots at the same time, instantiate the projectiles and return
            /*if (fireFromAllPivtos)
            {
                foreach (WeaponObject weapon in weaponObjects) //gunPivots[currentPivot] expand on this for weapon array solution (use the index of the wepon)
                {
                    foreach (Transform pivot in weapon.GunPivots())
                    {
                        //SetDirection();
                        InstantiateProjectile(pivot, targetRotation, direction);
                    }
                }

                return;
            }

            // If the weapon has multiple pivots and does not fire from all pivots at the same time, change to the next pivot point when the gun fires
            SetDirection();
            InstantiateProjectile(weaponObjects[currentWeapon].GunPivot(), targetRotation, direction);*/

            weaponObjects[currentWeapon].SetPivotNumber(weaponObjects[currentWeapon].GetPivotNumber() + 1);

            // If the pivot count is grater than the number of pivots, reset the count to first one
            if (weaponObjects[currentWeapon].GetPivotNumber() > weaponObjects[currentWeapon].GunPivots().Count - 1)
            {
                weaponObjects[currentWeapon].SetPivotNumber(0);
                currentWeapon++;
            }

            if (currentWeapon > weaponObjects.Count - 1)
            {
                currentWeapon = 0;
            }
        }
    }

    public IEnumerator Reload()
    {
        reload = true;
        //weaponSatus = WeaponSatus.Reloading;
        //audioSource.PlayOneShot(weaponSO.reloadSound);

        yield return new WaitForSeconds(weaponSO.reloadTime);

        weaponName.color = Color.white;
        currentAmmo = weaponSO.ammo;
        //weaponSatus = WeaponSatus.Armed;

        if (useUI)
        {
            ammoCounter.text = currentAmmo.ToString();
        }

        reload = false;
    }

    private void InstantiateProjectile(Transform pivot, float targetRotation, Vector3 direction)
    {
        GameObject instance = Instantiate(weaponSO.projectile, pivot.position, Quaternion.LookRotation(direction, Vector3.up));
    }

    private void SetDirection()
    {
        // Generate random values for spread
        float spreadX = Random.Range(-weaponSO.spread, weaponSO.spread);
        float spreadY = Random.Range(-weaponSO.spread, weaponSO.spread);

        // Get the direction for the projectile to fly to
        //targetRotation = Mathf.Atan2(targetPosition.y - weaponObjects[currentWeapon].WeaponPivot().position.y,
                                     //targetPosition.x - weaponObjects[currentWeapon].WeaponPivot().position.x) * Mathf.Rad2Deg;

        if (!isTopDown)
        {
            //direction = (new Vector3(targetPosition.x + spreadX, targetPosition.y, targetPosition.z + spreadY) - weaponObjects[currentWeapon].WeaponPivot().position).normalized;
            direction = (new Vector3(spreadX, spreadY, 0));
        }
        else
        {
            direction = (new Vector3(targetPosition.x + spreadX, targetPosition.y + spreadY, 0) - weaponObjects[currentWeapon].WeaponPivot().position).normalized;
        }
    }

    private void ModifyAudioSettings()
    {
        float pitch = UnityEngine.Random.Range(weaponSO.lowerPitch, weaponSO.upperPitch);
        float volume = UnityEngine.Random.Range(weaponSO.lowerVolume, weaponSO.upperVolume);
        audioSource.pitch = pitch;
        audioSource.volume = volume;
    }

    public void ReloadInput()
    {
        if (currentAmmo != weaponSO.ammo && !reload)
        {
            StartCoroutine(Reload());
        }
    }

    private void RotateWeapon()
    {
        // Calculate the rotation angle for the weapon to look a the mouse position
        Vector3 difference = targetPosition - transform.position;
        difference.Normalize();
        float rotationZ = (Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg) - 90;

        // Rotate the weapon to look at mouse position
        if (childCount == 0)
        {
            transform.rotation = Quaternion.Euler(0, 0, rotationZ);
        }
        else
        {
            foreach (WeaponObject weapon in weaponObjects)
            {
                weapon.WeaponPivot().transform.rotation = Quaternion.Euler(0, 0, rotationZ);
            }
        }
    }

    /*public WeaponSatus GetWeaponStatus()
    {
        return weaponSatus;
    }*/

    /*public GameObject GetWeaponGraphics()
    {
        return graphics;
    }

    public GameObject GetWeaponIndicator()
    {
        return weaponIndicator;
    }*/

    public string GetWeaponName()
    {
        return weaponName.text;
    }

    public bool IsTopDown()
    {
        return isTopDown;
    }

    public void ShootInput(bool input)
    {
        shoot = input;
    }

    public void SetTargetPosition(Vector3 target)
    {
        targetPosition = target;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackHelicopter : Ability
{
    [Header("Call In")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float indicatorDelay;
    [SerializeField] private float spawnDelay;

    [Header("Detection & Follow")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float detectionRange;

    [Header("Helicopter")]
    [SerializeField] private float duration;
    [SerializeField] private float movementSpeed;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float movementTime;
    [SerializeField] private float minimumMovement;
    [SerializeField] private float attackArea;

    [Header("Machinegun Attack")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed;
    [SerializeField] private float rateOfFire;
    [SerializeField] private float attackTime;
    [SerializeField] private float weaponSpread;
    [SerializeField] private float timeBetweenAttacks;
    [SerializeField] private float turretRotationSpeed;
    [SerializeField] private AudioClip machineGunStop;

    [Header("Rocket Attack Indicator")]
    [SerializeField] private ParticleSystem indicator;
    [SerializeField] private float indicatorDuration;
    [SerializeField] private float fireIndicatorDuration;

    [Header("Rocket Attack")]
    [SerializeField] private GameObject rocketPrefab;
    [SerializeField] private int payloadCount;
    [SerializeField] private float rocketSpeed;
    [SerializeField] private float payloadSpread;
    [SerializeField] private float delayBetweenShots;
    private GameObject indicatorInstance;

    [Header("Object Pool")]
    [SerializeField] private int bulletsToPool;
    private List<GameObject> bulletPool = new List<GameObject>();
    [SerializeField] private int rocketsToPool;
    private List<GameObject> rocketPool = new List<GameObject>();

    [Header("Missile Fire Point")]
    [SerializeField] private Vector3 firePointPosition;
    private Transform firePoint;

    private Vector3 direction;
    private Vector3 targetPosition;

    // Helicopter
    private Transform player;
    private Transform helicopter;
    private Transform turret;
    private Transform turretCannon;
    private Transform turretTarget;
    private Animator animator;
    private AudioSource helicopterAudio;
    private AudioSource turretAudio;
    private Vector3 randomMove;
    private Vector3 velocity;
    private Quaternion targetRotation;
    private Quaternion turretTargetRotation;
    private Quaternion turretStartingRotation;
    private float timeBetweenShots;
    private float closestDistance;
    private bool active;
    private bool attack;
    private bool attackColldown;
    private bool moveColldown;

    private void Awake()
    {
        CreateObjectPool(bulletPool, bulletPrefab, bulletsToPool);
        CreateObjectPool(rocketPool, rocketPrefab, rocketsToPool);
    }

    // Start is called before the first frame update
    void Start()
    {
        player = transform.parent;

        // Get helicopter components
        helicopter = transform.GetChild(0).transform;
        turret = helicopter.GetChild(0);
        turretCannon = turret.GetChild(0);

        animator = helicopter.GetComponent<Animator>();
        turretAudio = turret.GetComponent<AudioSource>();

        // Get the fire point to shoot projectiles from
        firePoint = indicatorInstance.transform.GetChild(1);

        // Set fire point's position
        firePoint.localPosition = firePointPosition;

        // Set the direction the projectile will fly to
        direction = (transform.position - firePoint.position).normalized;

        // Set fire point's rotation
        firePoint.rotation = Quaternion.LookRotation(direction);

        // Set the starting rotation of the turret
        turretStartingRotation = turret.rotation;

        // Set duration times
        ParticleSystem.MainModule mainIndicator = indicatorInstance.GetComponent<ParticleSystem>().main;
        mainIndicator.duration = indicatorDuration;
        mainIndicator.startLifetime = indicatorDuration;
        mainIndicator.startSize = aOE;

        ParticleSystem.MainModule timeIdicator = indicatorInstance.transform.GetChild(0).GetComponent<ParticleSystem>().main;
        timeIdicator.duration = fireIndicatorDuration;
        timeIdicator.startLifetime = fireIndicatorDuration;
        timeIdicator.startSize = aOE;

        rangeIndicator.transform.localScale = new Vector3(aOE, aOE, aOE);

        rangeIndicator.SetActive(false);

        active = false;
        attack = false;
        attackColldown = false;
        moveColldown = false;
    }

    private void Update()
    {
        if (active)
        {
            if (turretTarget == null)
            {
                Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRange, enemyLayer);

                closestDistance = Mathf.Infinity;

                foreach (Collider collider in hitColliders)
                {
                    float distance = Vector3.Distance(transform.position, collider.transform.position);

                    // Check if this is the closest collider so far
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        turretTarget = collider.transform;
                    }
                }

                if (!moveColldown)
                {
                    StartCoroutine(SetMovement());
                }

                Move(player.position);

                Vector3 direction = player.position - transform.position;
                targetRotation = Quaternion.LookRotation(direction);
                targetRotation *= Quaternion.Euler(0, 180f, 0);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

                // Return the turret to its starting position
                turret.rotation = Quaternion.Lerp(turret.rotation, turretStartingRotation, turretRotationSpeed * Time.deltaTime);
            }
            else
            {
                Vector3 turretDirection = turretTarget.position - turret.position;
                turretTargetRotation = Quaternion.LookRotation(turretDirection);
                turret.rotation = Quaternion.Lerp(turret.rotation, turretTargetRotation, turretRotationSpeed * Time.deltaTime);

                Vector3 direction = turretTarget.position - transform.position;
                targetRotation = Quaternion.LookRotation(direction);
                targetRotation *= Quaternion.Euler(0, 180f, 0);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                
                if (attack)
                {
                    if (!attackColldown)
                    {
                        StartCoroutine(Attack());
                    }

                    Shoot(turretDirection);
                }

                if (!moveColldown)
                {
                    StartCoroutine(SetMovement());
                }

                Move(turretTarget.position);
            }
        }

        if (timeBetweenShots <= 0f)
        {
            return;
        }

        timeBetweenShots -= Time.deltaTime;
    }

    public override void ActivateAbility()
    {
        if (!lockAbility)
        {
            StartCoroutine(SpawnAbility());
        }
    }

    private IEnumerator SpawnAbility()
    {
        // Select where to spawn the ability
        lockAbility = true;
        rangeIndicator.SetActive(true);

        while (!abilityInput)
        {
            GetMousePosition();

            yield return null;
        }

        abilityInput = false;
        abilityButton.interactable = false;
        rangeIndicator.SetActive(false);

        transform.position = targetPosition;
        transform.rotation = Quaternion.Euler(Vector3.zero);
        indicatorInstance.transform.position = targetPosition;

        yield return new WaitForSeconds(indicatorDelay);

        indicatorInstance.SetActive(true);

        Invoke("DeativateIndicator", indicatorInstance.GetComponent<ParticleSystem>().main.duration + (indicatorInstance.transform.childCount * delayBetweenShots));

        yield return new WaitForSeconds(fireIndicatorDuration + 1f);

        // Shoot missile attack
        for (int j = 0; j < payloadCount; j++)
        {
            // Set random spread values for each projectile
            float spreadX = Random.Range(-payloadSpread, payloadSpread);
            float spreadZ = Random.Range(-payloadSpread, payloadSpread);
            Vector3 spread = new Vector3(spreadX, 0f, spreadZ);

            // Get pooled object
            GameObject payloadInstance = GetPooledObject(rocketPool, rocketsToPool);

            // Set projectile's trnasform and velocity
            payloadInstance.transform.position = firePoint.position;
            payloadInstance.transform.rotation = firePoint.rotation;
            payloadInstance.SetActive(true);
            payloadInstance.GetComponent<Projectile>().StopAllCoroutines();
            payloadInstance.GetComponent<Rigidbody>().AddForce((direction * rocketSpeed) + spread, ForceMode.Impulse);

            yield return new WaitForSeconds(delayBetweenShots);
        }

        yield return new WaitForSeconds(spawnDelay);
        
        // Activate helicopter
        helicopter.gameObject.SetActive(true);
        animator.Play("Arrival");

        yield return new WaitForSeconds(3f);

        active = true;
        attack = true;

        yield return new WaitForSeconds(duration);

        //transform.rotation = Quaternion.Euler();

        // Helicopter leaves once time has run out
        animator.SetTrigger("Departure");

        active = false;

        if (attack)
        {
            turretAudio.Stop();
            turretAudio.PlayOneShot(machineGunStop);
        }

        StopCoroutine("Attack");
        StopCoroutine("SetMovement");

        yield return new WaitForSeconds(cooldown);

        lockAbility = false;
        abilityButton.interactable = true;
    }

    #region Attack
    private IEnumerator Attack()
    {
        attackColldown = true;
        turretAudio.Play();

        yield return new WaitForSeconds(attackTime);

        turretAudio.Stop();
        turretAudio.PlayOneShot(machineGunStop);
        attack = false;

        yield return new WaitForSeconds(timeBetweenAttacks);

        attack = true;
        attackColldown = false;
    }

    private void Shoot(Vector3 direction)
    {
        if (timeBetweenShots <= 0f)
        {
            // Set the spread values
            float spreadX = Random.Range(-weaponSpread, weaponSpread);
            float spreadY = Random.Range(-weaponSpread, weaponSpread);
            float spreadZ = Random.Range(-weaponSpread, weaponSpread);
            Vector3 spread = (new Vector3(spreadX, spreadY, spreadZ));

            // Instantiate the projectile
            GameObject bulletInstance = GetPooledObject(bulletPool, bulletsToPool);

            // Set projectile's trnasform and velocity
            bulletInstance.transform.position = turretCannon.position;
            bulletInstance.transform.rotation = turretCannon.rotation;
            bulletInstance.SetActive(true);
            bulletInstance.GetComponent<Projectile>().StopAllCoroutines();
            bulletInstance.GetComponent<Rigidbody>().AddForce((direction * bulletSpeed) + spread, ForceMode.Impulse);

            timeBetweenShots = 1 / rateOfFire;
        }
    }
    #endregion

    #region Movement
    private IEnumerator SetMovement()
    {
        moveColldown = true;

        // Set random movement values
        float moveX = CheckForMin(Random.Range(-attackArea, attackArea));
        float moveZ = CheckForMin(Random.Range(-attackArea, attackArea));
        randomMove = (new Vector3(moveX, 0f, moveZ));

        yield return new WaitForSeconds(movementTime);

        moveColldown = false;
    }

    private void Move(Vector3 target)
    {
        float distanceToTarget = (transform.position - (target + randomMove)).magnitude;

        if (distanceToTarget > 0.1f)
        {
            transform.position = Vector3.SmoothDamp(transform.position, target + randomMove, ref velocity, movementSpeed * Time.deltaTime);
        }
    }
    #endregion

    #region Helper Functions
    private void GetMousePosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit raycastHit, Mathf.Infinity, groundLayer))
        {
            targetPosition = raycastHit.point;
            rangeIndicator.transform.position = new Vector3(targetPosition.x, 0.01f, targetPosition.z);
        }
    }

    private float CheckForMin(float number)
    {
        if (Mathf.Abs(number) < minimumMovement && number > 0)
        {
            return number + minimumMovement;
        }
        else if (Mathf.Abs(number) < minimumMovement && number < 0)
        {
            return number - minimumMovement;
        }

        return number;
    }

    private void DeativateIndicator()
    {
        indicatorInstance.SetActive(false);
    }
    #endregion

    #region Object Pooling
    private void CreateObjectPool(List<GameObject> objectPool, GameObject projectile, int objectsToPool)
    {
        Transform globalPool = GameObject.FindGameObjectWithTag("ObjectPool").transform;

        // Instantiate and deactivate indicator
        indicatorInstance = Instantiate(indicator.gameObject, transform.position, transform.rotation);
        indicatorInstance.SetActive(false);
        indicatorInstance.transform.SetParent(globalPool);

        // Instantiate necesary projectiles
        for (int i = 0; i < objectsToPool; i++)
        {
            GameObject instance = Instantiate(projectile);
            instance.SetActive(false);
            objectPool.Add(instance);

            instance.transform.SetParent(globalPool);
        }
    }

    private GameObject GetPooledObject(List<GameObject> objectPool, int objectsInPool)
    {
        for (int i = 0; i < objectsInPool; i++)
        {
            if (!objectPool[i].activeInHierarchy)
            {
                return objectPool[i];
            }
        }

        return null;
    }
    #endregion

    private void OnDrawGizmosSelected()
    {
        // Draw a wire sphere to visualize the detection range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
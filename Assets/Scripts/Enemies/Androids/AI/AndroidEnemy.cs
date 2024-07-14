using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.AI;

public class AndroidEnemy : UnitHealth
{
    public enum EnemyState
    {
        Patrol,
        Combat,
    }

    [Header("Enemy Model References")]
    [SerializeField] private Transform torso;

    [Header("Patrolling State")]
    [SerializeField] private float minWaitTime;
    [SerializeField] private float maxWaitTime;
    [SerializeField] private float detectionRange;

    [Header("Waypoint Patrol")]
    [SerializeField] private bool useWaypointPatrol;
    [SerializeField] private List<Transform> patrolPoints = new List<Transform>();

    [Header("Randon Patrol")]
    [SerializeField] private bool useRandomPatrol;
    [SerializeField] private Transform center;
    [SerializeField] private float patrolRange;

    [Header("Combat State")]
    [SerializeField] private float targetRadius;
    [SerializeField] private float combatTimeOut;
    [SerializeField] private float combatCallCooldown;
    [SerializeField] private float minRetargetTime;
    [SerializeField] private float maxRetargetTime;

    [Header("Attack")]
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask detectionLayer;
    [SerializeField] private GameObject projectile;
    [SerializeField] private Transform weaponPivot;
    [SerializeField] private int ammo;
    [SerializeField] private float projectileVelocity;
    [SerializeField] private float spread;
    [SerializeField] private float fireTime;
    [SerializeField] private float reloadTime;
    [SerializeField] private float rateOfFire;

    [Header("SFX")]
    [SerializeField] private AudioSource weaponFireSource;
    [SerializeField] private AudioClip weaponFireSFX;
    [SerializeField] private float lowerPitch;
    [SerializeField] private float upperPitch;
    [SerializeField] private float lowerVolume;
    [SerializeField] private float upperVolume;

    [Header("Torso Animation")]
    [SerializeField] private float patrolRotationSpeed;
    [SerializeField] private float combatRotationSpeed;
    [SerializeField] private float minRotation;
    [SerializeField] private float maxRotation;
    [SerializeField] private float minTime;
    [SerializeField] private float maxTime;

    private EnemyState enemyState;

    // Enemy References
    private NavMeshAgent agent;
    private Animator animator;

    // Patrol Points
    private Transform patrolPointsHolder;
    private Vector3 lastPatrolPoint;
    private int targetPatrolPoint;

    // Combat
    private Transform targetEnemy;
    private float currentCombatTimer;
    private float timeBetweenShots;
    private float currentFireTime;
    private int currentAmmo;
    private bool canFire;
    private bool reloading;

    // Switches
    private bool canMove;
    private bool onCombat;
    private bool combatCallable;

    // Rotation Targets
    private Quaternion targetRotation;
    private Quaternion torsoTargetRotation;

    protected override void Start()
    {
        health = new Health(maxHealth);
        health.OnDeath += Health_OnDeath;

        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        enemyState = EnemyState.Patrol;

        currentAmmo = ammo;
        currentFireTime = fireTime;

        canMove = true;
        onCombat = false;
        combatCallable = true;
        reloading = false;

        if (useWaypointPatrol)
        {
            patrolPointsHolder = transform.GetChild(0);
            patrolPointsHolder.SetParent(null);
        }

        targetPatrolPoint = 0;

        UpdatePatrolPoint();
        Invoke("SetTorsoRotation", 3f);
    }

    void Update()
    {
        LookForTarget();

        if (enemyState == EnemyState.Patrol)
        {
            if (canMove)
            {
                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    StartCoroutine(Arrival());
                }
            }

            torso.rotation = Quaternion.Lerp(torso.rotation, torsoTargetRotation, patrolRotationSpeed * Time.deltaTime);
        }
        else if (enemyState == EnemyState.Combat)
        {
            CombatTimer();
            Attack();

            Vector3 torsoDirection = targetEnemy.position - torso.position;
            torsoTargetRotation = Quaternion.LookRotation(torsoDirection);
            torsoTargetRotation.eulerAngles = new Vector3(-90, torsoTargetRotation.eulerAngles.y, torsoTargetRotation.eulerAngles.z);
            torso.rotation = Quaternion.Lerp(torso.rotation, torsoTargetRotation, combatRotationSpeed * Time.deltaTime);
        }
    }

    private void LookForTarget()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRange, detectionLayer);

        foreach (Collider collider in hitColliders)
        {
            if (collider.GetComponent<PlayerStatus>() && !targetEnemy)
            {
                targetEnemy = collider.transform;

                enemyState = EnemyState.Combat;
                currentCombatTimer = combatTimeOut;
                onCombat = true;
                canMove = true;
                agent.isStopped = false;

                StopCoroutine(Arrival());
                CancelInvoke("SetTorsoRotation");

                SetTargetPosition();
                
                if (!onCombat)
                {
                    animator.SetTrigger("Walk");
                }
            }

            if (collider.gameObject != gameObject)
            {
                if (collider.GetComponent<AndroidEnemy>() && enemyState == EnemyState.Combat)
                {
                    collider.GetComponent<AndroidEnemy>().CombatCall(targetEnemy.transform);
                }
            }
        }

        if (targetEnemy)
        {
            float distance = Vector3.Distance(transform.position, targetEnemy.position);

            if (distance > detectionRange)
            {
                onCombat = false;
            }
        }
    }

    #region Patrolling
    private void UpdatePatrolPoint()
    {
        if (useWaypointPatrol)
        {
            targetPatrolPoint++;

            if (targetPatrolPoint == patrolPoints.Count)
            {
                targetPatrolPoint = 0;
            }

            lastPatrolPoint = patrolPoints[targetPatrolPoint].position;
            agent.destination = lastPatrolPoint;
        }
        else if (useRandomPatrol)
        {
            Vector3 randomPoint = new Vector3(center.position.x + GetRandomNumber(-patrolRange, patrolRange), 0f, center.position.z + GetRandomNumber(-patrolRange, patrolRange));
            NavMeshHit hit;

            if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
            {
                agent.destination = hit.position;
                lastPatrolPoint = hit.position;
            }
        }
    }

    private IEnumerator Arrival()
    {
        agent.isStopped = true;
        canMove = false;

        animator.SetTrigger("Idle");

        float randomWaitTime = GetRandomNumber(minWaitTime, maxWaitTime);

        UpdatePatrolPoint();

        yield return new WaitForSeconds(randomWaitTime);

        canMove = true;
        agent.isStopped = false;

        animator.SetTrigger("Walk");
    }
    #endregion

    #region Combat
    private void SetTargetPosition()
    {
        Vector3 randomPoint = new Vector3(targetEnemy.position.x + GetRandomNumber(-patrolRange, patrolRange), 0f, targetEnemy.position.z + GetRandomNumber(-patrolRange, patrolRange));
        agent.destination = randomPoint;

        Invoke("SetTargetPosition", GetRandomNumber(minRetargetTime, maxRetargetTime));
    }

    private void Attack()
    {
        Debug.Log(targetEnemy + " --> " + gameObject.name);
        Vector3 targetDirection = (weaponPivot.position - targetEnemy.position).normalized;
        RaycastHit hit;

        if (Physics.Raycast(weaponPivot.position, weaponPivot.forward * -1, out hit))
        {
            if (1 << hit.collider.gameObject.layer == playerLayer.value && !reloading)
            {
                currentFireTime -= Time.deltaTime;

                if (currentFireTime < 0)
                {
                    canFire = true;
                }
            }
            else
            {
                currentFireTime = fireTime;
            }
        }

        if (canFire)
        {
            if (timeBetweenShots <= 0f)
            {
                Shoot(targetDirection);
                return;
            }

            timeBetweenShots -= Time.deltaTime;
        }
    }

    private void Shoot(Vector3 direction)
    {
        if (currentAmmo != 0)
        {
            // Set the spread values
            float spreadX = Random.Range(-spread, spread);
            float spreadY = Random.Range(-spread, spread);
            float spreadZ = Random.Range(-spread, spread);
            Vector3 calculatedSpread = (new Vector3(spreadX, spreadY, spreadZ));

            // Instantiate Projectile
            GameObject projectileInstace = Instantiate(projectile, weaponPivot.position, weaponPivot.rotation);
            projectileInstace.GetComponent<Projectile>().StopAllCoroutines();
            projectileInstace.GetComponent<Rigidbody>().AddForce((direction * -projectileVelocity) + calculatedSpread, ForceMode.Impulse);

            ModifyAudioSettings();
            weaponFireSource.PlayOneShot(weaponFireSFX);

            timeBetweenShots = 1 / rateOfFire;
            currentAmmo--;
        }
        else
        {
            canFire = false;
            StartCoroutine(Reload());
        }
    }

    private IEnumerator Reload()
    {
        reloading = true;
        currentFireTime = fireTime;

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = ammo;
        reloading = false;
    }

    private void CombatTimer()
    {
        if (!onCombat)
        {
            currentCombatTimer -= Time.deltaTime;
        }

        if (currentCombatTimer <= 0f)
        {
            agent.isStopped = true;

            CancelInvoke("SetTargetPosition");
            ResetTorsoRotation();
            StartCoroutine(ReturnToPatrol());
        }
    }

    private void ResetCombatCall()
    {
        combatCallable = true;
    }

    private IEnumerator ReturnToPatrol()
    {
        enemyState = EnemyState.Patrol;
        animator.SetTrigger("Idle");

        yield return new WaitForSeconds(2f);

        targetEnemy = null;
        agent.destination = lastPatrolPoint;
        agent.isStopped = false;
        animator.SetTrigger("Walk");

        Invoke("ResetCombatCall", combatCallCooldown);
        Invoke("SetTorsoRotation", 3f);
    }

    public void CombatCall(Transform target)
    {
        if (combatCallable && target != null)
        {
            combatCallable = false;

            targetEnemy = target;
            agent.destination = target.position;
            enemyState = EnemyState.Combat;
        }
    }
    #endregion

    #region Animation
    private void SetTorsoRotation()
    {
        float randomRotation = GetRandomNumber(minRotation, maxRotation);
        randomRotation += transform.rotation.eulerAngles.y;

        float randomTime = GetRandomNumber(minTime, maxTime);

        torsoTargetRotation.eulerAngles = new Vector3(-90, randomRotation, 0);

        Invoke("SetTorsoRotation", randomTime);
    }

    private void ResetTorsoRotation()
    {
        torsoTargetRotation.eulerAngles = new Vector3(-90, 0, 0);
    }
    #endregion

    #region Helper Functions
    private float GetRandomNumber(float minValue, float maxValue)
    {
        float randomNumber = Random.Range(minValue, maxValue);
        return randomNumber;
    }

    private void ModifyAudioSettings()
    {
        float pitch = Random.Range(lowerPitch, upperPitch);
        float volume = Random.Range(lowerVolume, upperVolume);
        weaponFireSource.pitch = pitch;
        weaponFireSource.volume = volume;
    }
    #endregion

    private void Health_OnDeath(object sender, System.EventArgs e)
    {
        if (useWaypointPatrol)
        {
            Destroy(patrolPointsHolder.gameObject);
        }

        Destroy(gameObject);
    }
}

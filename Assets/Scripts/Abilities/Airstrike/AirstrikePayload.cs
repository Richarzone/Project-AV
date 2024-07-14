using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirstrikePayload : Ability
{
    [SerializeField] private LayerMask groundLayer;

    [Header("Indicators")]
    [SerializeField] private ParticleSystem indicator;
    [SerializeField] private float indicatorDuration;
    [SerializeField] private float fireIndicatorDuration;
    [SerializeField] private float spawnMinDelay;
    [SerializeField] private float spawnMaxDelay;
    private GameObject indicatorInstance;

    [Header("Payload")]
    [SerializeField] private GameObject projectile;
    [SerializeField] private int payloadCount;
    [SerializeField] private int volleyCount;
    [SerializeField] private float projectileSpeed;
    [SerializeField] private float payloadSpread;
    [SerializeField] private float delayBetweenShots;
    [SerializeField] private float delayBetweenVolleys;

    [Header("Projectile Pool")]
    [SerializeField] private int objectsToPool;
    private List<GameObject> projectilePool = new List<GameObject>();

    [Header("Fire Point")]
    [SerializeField] private Vector3 firePointPosition;
    private Transform firePoint;

    private Vector3 direction;
    private Vector3 targetPosition;

    private void Awake()
    {
        CreateObjectPool();
    }

    private void Start()
    {
        // Get the fire point to shoot projectiles from
        firePoint = indicatorInstance.transform.GetChild(1);

        // Set fire point's position
        firePoint.localPosition = firePointPosition;

        // Set the direction the projectile will fly to
        direction = (transform.position - firePoint.position).normalized;
        
        // Set fire point's rotation
        firePoint.rotation = Quaternion.LookRotation(direction);

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
    }

    public override void ActivateAbility()
    {
        if (!lockAbility)
        {
            float delay = Random.Range(spawnMinDelay, spawnMaxDelay);
            StartCoroutine(SpawnPayload(delay));
        }
    }

    private IEnumerator SpawnPayload(float indicatorDelay)
    {
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

        indicatorInstance.transform.position = targetPosition;
        
        yield return new WaitForSeconds(indicatorDelay);

        indicatorInstance.SetActive(true);
        Invoke("DeativateIndicator", indicatorInstance.GetComponent<ParticleSystem>().main.duration + (indicatorInstance.transform.childCount * delayBetweenShots));

        yield return new WaitForSeconds(fireIndicatorDuration + 1f);

        for (int i = 0; i < volleyCount; i++)
        {
            for (int j = 0; j < payloadCount; j++)
            {
                // Set random spread values for each projectile
                float spreadX = Random.Range(-payloadSpread, payloadSpread);
                float spreadZ = Random.Range(-payloadSpread, payloadSpread);
                Vector3 spread = new Vector3(spreadX, 0f, spreadZ);

                // Get instance of projectile
                GameObject payloadInstance = GetPooledObject();

                // Set projectile's trnasform and velocity
                payloadInstance.transform.position = firePoint.position;
                payloadInstance.transform.rotation = firePoint.rotation;
                payloadInstance.SetActive(true);
                payloadInstance.GetComponent<Projectile>().StopAllCoroutines();
                payloadInstance.GetComponent<Rigidbody>().AddForce((direction * projectileSpeed) + spread, ForceMode.Impulse);

                yield return new WaitForSeconds(delayBetweenShots);
            }

            yield return new WaitForSeconds(delayBetweenVolleys);
        }

        yield return new WaitForSeconds(cooldown);

        lockAbility = false;
        abilityButton.interactable = true;
    }

    private void DeativateIndicator()
    {
        indicatorInstance.SetActive(false);
    }

    private void CreateObjectPool()
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
            projectilePool.Add(instance);

            instance.transform.SetParent(GameObject.FindGameObjectWithTag("ObjectPool").transform);
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

    private void GetMousePosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit raycastHit, Mathf.Infinity, groundLayer))
        {
            targetPosition = raycastHit.point;
            rangeIndicator.transform.position = new Vector3(targetPosition.x, 0.01f, targetPosition.z);
        }
    }
}
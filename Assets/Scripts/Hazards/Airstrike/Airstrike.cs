using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class Airstrike : MonoBehaviour
{
    [Header("Layers")]
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask groundLayer;

    [Header("UI Indicator")]
    [SerializeField] private GameObject UINotification;

    [Header("Payload")]
    [SerializeField] private GameObject payload;
    private List<GameObject> payloadPool = new List<GameObject>();

    [Header("Strike Settings")]
    [SerializeField] private float range;
    [SerializeField] private float timeBetweenStrikes;
    [SerializeField] private float payloadCount;

    private PlayerStatus player;
    private Transform target;
    private float currentTime;


    private void Awake()
    {
        CreatePayloadPool();
    }

    private void Start()
    {
        currentTime = timeBetweenStrikes;
    }

    private void Update()
    {
        if (currentTime <= 0)
        {
            Collider[] hitColliders = Physics.OverlapSphere(target.position, range);

            foreach (Collider collider in hitColliders)
            {
                if ((1 << collider.gameObject.layer) == playerLayer.value)
                {
                    player = collider.GetComponent<PlayerStatus>();
                    player.AirstrikeWarning();
                    
                }
            }
            
            for (int i = 0; i < payloadCount; i++)
            {
                // Set random spawn position within the airstrike range
                float xPosition = Random.Range(-range, range);
                float yPosition = Random.Range(-range, range);
                Vector3 spawnPosition = new Vector3(target.position.x + xPosition, 0f, target.position.z + yPosition);

                GameObject airstrike = GetPooledObject();
                airstrike.transform.position = spawnPosition;
                airstrike.SetActive(true);
                airstrike.GetComponent<EnemyAirstrike>().Fire(airstrike.transform.position);
            }

            currentTime = timeBetweenStrikes;

            return;
        }

        if (player != null && player.GetDeathFlag())
        {
            Debug.Log("HERE");
            target = null;
            currentTime = timeBetweenStrikes;
            this.enabled = false;
            return;
        }

        currentTime -= Time.deltaTime;
    }

    public void ReserTimer()
    {
        currentTime = timeBetweenStrikes;
    }

    public void SetTarget(Transform airstrikeTarget)
    {
        target = airstrikeTarget;
    }

    public void ResetTarget()
    {
        target = null;
    }

    public void ClearAllPoolReferences()
    {
        foreach (GameObject pooledObject in payloadPool)
        {
            pooledObject.GetComponent<EnemyAirstrike>().ClearObjectPool();
        }
    }

    private void CreatePayloadPool()
    {
        Transform globalPool = GameObject.FindGameObjectWithTag("ObjectPool").transform;

        // Instantiate necesary projectiles
        for (int i = 0; i < payloadCount; i++)
        {
            GameObject instance = Instantiate(payload);
            instance.SetActive(false);
            payloadPool.Add(instance);

            instance.transform.SetParent(globalPool);
        }
    }

    private GameObject GetPooledObject()
    {
        for (int i = 0; i < payloadCount; i++)
        {
            if (!payloadPool[i].activeInHierarchy)
            {
                return payloadPool[i];
            }
        }
        return null;
    }

    private void OnDrawGizmosSelected()
    {
        // Set the color of the Gizmos
        Gizmos.color = UnityEngine.Color.white;

        // Draw a wire sphere using Gizmos
        Gizmos.DrawWireSphere(transform.position, range);
    }
}

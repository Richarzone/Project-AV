using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnPoint : MonoBehaviour
{
    [SerializeField] private ParticleSystem smoke;
    [SerializeField] private GameObject beacon;
    private bool spawnSet;

    private void Start()
    {
        spawnSet = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerStatus>() && !spawnSet)
        {
            spawnSet = true;
            smoke.Play();
            other.GetComponent<PlayerStatus>().SetSpawnPoint(transform);
            Destroy(beacon);
        }
    }
}
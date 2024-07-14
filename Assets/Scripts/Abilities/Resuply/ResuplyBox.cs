using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResuplyBox : MonoBehaviour
{
    [SerializeField] private LayerMask groundLayer;
    private bool active;

    private void OnCollisionEnter(Collision collision)
    {
        if (1 << collision.gameObject.layer == groundLayer)
        {
            Debug.Log("Activating...");
            Invoke("Activate", 1f);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.GetComponent<PlayerStatus>() && active)
        {
            other.GetComponent<PlayerStatus>().RefillRepairKit();
            other.GetComponent<TankController>().RefillReserveAmmo();
            Destroy(gameObject);
        }
    }

    private void Activate()
    {
        Debug.Log("Active");
        active = true;
    }
}

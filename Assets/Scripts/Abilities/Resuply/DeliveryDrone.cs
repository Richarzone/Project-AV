using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeliveryDrone : MonoBehaviour
{
    [SerializeField] private GameObject payload;
    [SerializeField] float initialFallForce;

    private void DestoyDrone()
    {
        Destroy(transform.parent.gameObject);
    }

    private void DropPackage()
    {
        Rigidbody rigidbody = payload.GetComponent<Rigidbody>();
        rigidbody.useGravity = true;
        rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        rigidbody.velocity = new Vector3(0, -initialFallForce, 0);

        payload.transform.parent = null;
    }
}

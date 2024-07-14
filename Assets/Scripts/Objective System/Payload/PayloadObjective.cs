using CurvedPathGenerator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PayloadObjective : MonoBehaviour
{
    private PathFollower pathFollower;
    private SphereCollider sphereCollider;

    private void Start()
    {
        pathFollower = GetComponent<PathFollower>();
        sphereCollider = GetComponent<SphereCollider>();
        sphereCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<PlayerStatus>())
        {
            pathFollower.IsMove = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.GetComponent<PlayerStatus>())
        {
            pathFollower.IsMove = false;
        }
    }

    private void SetMove()
    {
        sphereCollider.enabled = true;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirstrikeZone : MonoBehaviour
{
    private Airstrike airstrike;

    private void Start()
    {
        airstrike = GetComponent<Airstrike>();
        airstrike.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerStatus playerStatus = other.gameObject.GetComponent<PlayerStatus>();

        if (playerStatus)
        {
            airstrike.ReserTimer();
            airstrike.SetTarget(other.transform);
            airstrike.enabled = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerStatus playerStatus = other.gameObject.GetComponent<PlayerStatus>();

        if (playerStatus)
        {
            airstrike.ResetTarget();
            airstrike.enabled = false;
        }
    }
}

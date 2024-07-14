using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutOfBounds : MonoBehaviour
{
    private void OnTriggerExit(Collider other)
    {
        PlayerStatus playerStatus = other.gameObject.GetComponent<PlayerStatus>();

        if (playerStatus)
        {
            playerStatus.SetOutOfBounds();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerStatus playerStatus = other.gameObject.GetComponent<PlayerStatus>();

        if (playerStatus)
        {
            playerStatus.ResetOutOfBounds();
        }
    }
}

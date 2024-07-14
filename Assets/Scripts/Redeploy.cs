using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Redeploy : MonoBehaviour
{
    [SerializeField] private GameObject drone;
    [SerializeField] private List<GameObject> lines;

    private Animator animator;

    // ANIMATION EVENTS

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        foreach (GameObject line in lines)
        {
            LineRenderer lr = line.GetComponent<LineRenderer>();
            lr.SetPosition(0, line.transform.parent.position);
            lr.SetPosition(1, line.transform.GetChild(0).position);
        }
    }

    private void DestroyDrone()
    {
        Destroy(transform.parent.gameObject);
    }
}
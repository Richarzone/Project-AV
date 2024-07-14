using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyEnemy : MonoBehaviour
{
    [SerializeField] private Transform parts;
    [SerializeField] private Transform explosionPivot;
    [SerializeField] private float explosionForce;

    void Start()
    {
        foreach (Transform part in parts)
        {
            Rigidbody rigidbody = part.GetComponent<Rigidbody>();
            rigidbody.AddExplosionForce(explosionForce, explosionPivot.position, 10f);
        }
    }
}

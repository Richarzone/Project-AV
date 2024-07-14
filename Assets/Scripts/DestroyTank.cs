using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyTank : MonoBehaviour
{
    [SerializeField] private Transform explosionPivot;
    [SerializeField] private float explosionForce;

    private Rigidbody tankTurret;
    private Rigidbody secondaryWeapon;
    private Rigidbody weaponStation;

    private void Start()
    {
        tankTurret = transform.GetChild(0).GetComponent<Rigidbody>();
        secondaryWeapon = transform.GetChild(1).GetComponent<Rigidbody>();
        weaponStation = transform.GetChild(2).GetComponent<Rigidbody>();

        tankTurret.AddExplosionForce(explosionForce, explosionPivot.position, 10f);
        secondaryWeapon.AddExplosionForce(explosionForce * 0.1f, explosionPivot.position, 10f);
        weaponStation.AddExplosionForce(explosionForce * 0.1f, explosionPivot.position, 10f);
    }
}

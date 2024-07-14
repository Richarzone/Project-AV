using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WeaponObject
{
    [SerializeField] private Transform weaponPivot;
    [SerializeField] private List<Transform> gunPivots = new List<Transform>();
    private int currentPivot = 0;

    public Transform WeaponPivot()
    {
        return weaponPivot;
    }

    public Transform GunPivot()
    {
        return gunPivots[currentPivot];
    }

    public List<Transform> GunPivots()
    {
        return gunPivots;
    }

    public int GetPivotNumber()
    {
        return currentPivot;
    }
    public void SetPivotNumber(int newPivot)
    {
        currentPivot = newPivot;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseDefensesTriger : ObjectiveTrigger
{
    [SerializeField] private Airstrike airstrike;
    [SerializeField] private List<Animator> doorAnimators = new List<Animator>();

    public override void FireTrigger()
    {
        airstrike.ClearAllPoolReferences();
        Invoke("DestroyAirStrike", 3f);

        foreach (Animator animator in doorAnimators)
        {
            animator.SetTrigger("Open");
        }
    }

    private void DestroyAirStrike()
    {
        Destroy(airstrike.gameObject);
    }
}

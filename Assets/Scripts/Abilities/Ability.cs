using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Ability : MonoBehaviour
{
    [Header("Ability Settings")]
    [SerializeField] protected GameObject rangeIndicator;
    [SerializeField] protected float cooldown;
    [SerializeField] protected float aOE;

    protected AbilityWheel abilityManager;
    protected Button abilityButton;
    protected bool abilityInput;
    protected bool lockAbility;

    public virtual void ActivateAbility()
    {

    }

    public virtual void DeactivateAbility() 
    {
        StopAllCoroutines();
        rangeIndicator.SetActive(false);
        lockAbility = false;
    }

    public void UseInput(bool input)
    {
        abilityInput = input;
    }

    public void SetUIButton(Button button)
    {
        abilityButton = button;
    }

    public float GetCoolDown()
    {
        return cooldown;
    }
}

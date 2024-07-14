using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitHealth : MonoBehaviour
{
    protected Health health;
    protected Objective objective;
    protected SubOjectiveManager subObjective;
    protected bool invulnerable;
    [SerializeField] protected int maxHealth;

    protected virtual void Start()
    {
        health = new Health(maxHealth);
        invulnerable = false;
}

    public void Healing(int value)
    {
        health.Heal(value);
    }

    public void DealDamage(int value)
    {
        if (!invulnerable)
        {
            health.Damage(value);
        }
    }

    public void SetToObjective(Objective value)
    {
        objective = value;
    }

    public void SetToSubObjective(SubOjectiveManager value)
    {
        subObjective = value;
    }
}

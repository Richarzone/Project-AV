using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generator : UnitHealth
{
    [Header("Generator")]
    [SerializeField] private GameObject destroyedGenerator;
    [SerializeField] private GameObject mediumDamage;
    [SerializeField] private GameObject heavyDamage;
    [Range(0f, 1f)]
    [SerializeField] private float mediumDamageThreshold;
    [Range(0f, 1f)]
    [SerializeField] private float heavyDamageThreshold;

    private Animator animator;
    private bool destroyed;

    protected override void Start()
    {
        health = new Health(maxHealth);
        health.OnHealthChange += Health_OnHealthChange;
        health.OnDeath += Health_OnDeath;

        destroyed = false;

        animator = GetComponent<Animator>();
    }

    private void Health_OnHealthChange(object sender, System.EventArgs e)
    {
        Debug.Log(health.GetHelthPercentage());

        if (health.GetHelthPercentage() <= heavyDamageThreshold)
        {
            heavyDamage.SetActive(true);
        }
        else if (health.GetHelthPercentage() <= mediumDamageThreshold)
        {
            mediumDamage.SetActive(true);
        }
    }

    private void Health_OnDeath(object sender, System.EventArgs e)
    {
        animator.SetTrigger("Destroyed");

        if (objective != null && !destroyed)
        {
            destroyed = true;
            objective.AddProgress(1);
        }
        else if (subObjective != null && !destroyed)
        {
            destroyed = true;
            subObjective.AddProgress();
        }
    }

    private void SpawnDestroyed()
    {
        Instantiate(destroyedGenerator, transform.position, destroyedGenerator.transform.rotation);
        Destroy(gameObject);
    }
}

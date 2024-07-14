using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Slider healthBar;
    [SerializeField] private Image healthBarImage;
    [Range(0f,1f)]
    [SerializeField] private float criticalHealth;
    [SerializeField] private Color criticalHealthColor;

    private Health healthManager;

    private void Start()
    {
        healthBar = transform.GetComponent<Slider>();
    }

    private void HealthManager_OnHealthChange(object sender, System.EventArgs e)
    {
        healthBar.value = healthManager.GetHelthPercentage();

        if (healthBar.value <= criticalHealth)
        {
            healthBarImage.color = criticalHealthColor;
        }
        else
        {
            healthBarImage.color = Color.white;
        }
    }

    public void ResetHealthBar()
    {
        healthBarImage.color = Color.white;
        healthBar.value = 1f;
    }

    public void EmptyHealthBar()
    {
        healthBar.value = 0f;
    }

    public void SetHealthManager(Health manager)
    {
        healthManager = manager;

        healthManager.OnHealthChange += HealthManager_OnHealthChange;
    }
}

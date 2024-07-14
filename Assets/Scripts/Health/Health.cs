using System;
using System.Diagnostics;

public class Health
{
    public event EventHandler OnHealthChange;
    public event EventHandler OnDeath;

    public int health;
    private int maxHealth;
    
    public Health(int value) 
    {
        maxHealth = value;
        health = maxHealth;
    }

    public int GetHealth() 
    { 
        return health; 
    }

    public float GetHelthPercentage()
    {
        return (float)health / maxHealth;
    }

    public void Damage(int damage)
    {
        health -= damage;

        if (health <= 0)
        {
            health = 0;

            if (OnDeath != null)
            {
                OnDeath(this, EventArgs.Empty);
            }
        }

        if (OnHealthChange != null)
        {
            OnHealthChange(this, EventArgs.Empty);
        }
    }

    public void Heal(int healAmount)
    {
        health += healAmount;

        if (health > maxHealth)
        {
            health = maxHealth;
        }

        if (OnHealthChange != null)
        {
            OnHealthChange(this, EventArgs.Empty);
        }
    }
}

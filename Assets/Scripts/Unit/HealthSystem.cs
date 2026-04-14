using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    public event EventHandler OnDead;
    public event EventHandler OnHealthChanged;
    [SerializeField] private int healthMax = 100;
    [SerializeField] private int health;

    private void Awake()
    {
        health = healthMax;
    }

    public void Damage(int damageAmount)
    {
        health -= damageAmount;

        if (health < 0) health = 0;

        OnHealthChanged?.Invoke(this, EventArgs.Empty);

        if (health == 0) Die();       
    }

    private void Die()
    {
        OnDead?.Invoke(this, EventArgs.Empty);
    }

    public float GetHealthNormalized()
    {
        return (float)health / healthMax;
    }
}

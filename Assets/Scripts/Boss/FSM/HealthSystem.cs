﻿using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    public float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    public delegate void OnDeathDelegate();
    public event OnDeathDelegate OnDeath;

    private void Start()
    {
        currentHealth = maxHealth;
    }


    public void TakeDamage(float amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            OnDeath?.Invoke();

            EnemyManager.Instance.UnregisterEnemy();
        }
    }

    public float GetCurrentHealth() => currentHealth;
}
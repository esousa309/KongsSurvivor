using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float current = 100f;
    public bool autoDestroyOnDeath = true;
    
    [Header("Events")]
    public Action OnDeath;
    public Action<float> OnDamaged;
    public Action<float> OnHealed;
    public Action<float> OnHealthChanged;
    
    protected bool isDead = false;
    
    protected virtual void Start()
    {
        current = maxHealth;
    }
    
    public virtual void TakeDamage(float amount)
    {
        if (isDead) return;
        
        float previousHealth = current;
        current -= amount;
        current = Mathf.Clamp(current, 0f, maxHealth);
        
        OnDamaged?.Invoke(amount);
        OnHealthChanged?.Invoke(current);
        
        // Visual feedback
        StartCoroutine(DamageFlash());
        
        if (current <= 0 && !isDead)
        {
            Die();
        }
    }
    
    public virtual void Heal(float amount)
    {
        if (isDead) return;
        
        float previousHealth = current;
        current += amount;
        current = Mathf.Clamp(current, 0f, maxHealth);
        
        OnHealed?.Invoke(amount);
        OnHealthChanged?.Invoke(current);
    }
    
    protected virtual void Die()
    {
        if (isDead) return;
        
        isDead = true;
        OnDeath?.Invoke();
        
        if (autoDestroyOnDeath)
        {
            Destroy(gameObject, 0.1f);
        }
    }
    
    protected IEnumerator DamageFlash()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color originalColor = sr.color;
            sr.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            
            if (sr != null)
                sr.color = originalColor;
        }
    }
    
    public float GetHealthPercent()
    {
        return current / maxHealth;
    }
    
    public bool IsDead()
    {
        return isDead;
    }
    
    public void SetMaxHealth(float newMax, bool healToFull = false)
    {
        maxHealth = newMax;
        
        if (healToFull)
        {
            current = maxHealth;
        }
        else
        {
            current = Mathf.Min(current, maxHealth);
        }
        
        OnHealthChanged?.Invoke(current);
    }
}
using UnityEngine;

public class Health : MonoBehaviour
{
    public float maxHealth = 100f;
    public float current;
    
    [Header("Death Behavior")]
    public bool autoDestroyOnDeath = true;

    public System.Action<float, float> OnDamaged;
    public System.Action OnDied;

    void Awake()
    {
        // CRITICAL: Always reset to full health on start
        current = maxHealth;
        Debug.Log($"[Health] {gameObject.name} initialized with {current}/{maxHealth} HP");
    }

    void OnEnable()
    {
        // Extra safety: Reset HP when re-enabled
        if (current <= 0f)
        {
            current = maxHealth;
            Debug.Log($"[Health] {gameObject.name} HP restored to {maxHealth}");
        }
    }

    public void Damage(float amount)
    {
        if (current <= 0f) return;
        current -= amount;
        OnDamaged?.Invoke(current, maxHealth);
        if (current <= 0f)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        current = Mathf.Min(maxHealth, current + amount);
    }

    void Die()
    {
        OnDied?.Invoke();
        
        if (autoDestroyOnDeath)
        {
            Destroy(gameObject);
        }
    }
}
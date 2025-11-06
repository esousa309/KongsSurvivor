using UnityEngine;

public class PlayerDamageHandler : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 100;
    [Tooltip("Current player health at runtime.")]
    public int currentHealth = 100;

    [Header("Damage Handling")]
    [Tooltip("Player won't take damage again until this time passes (seconds).")]
    public float invulnerabilityTime = 0.5f;

    [Tooltip("Optional knockback impulse when taking damage.")]
    public float defaultKnockback = 5f;

    private float invulnTimer;
    private Rigidbody rb;
    private PlayerController controller;

    // Optional callbacks you can hook from other scripts/UI
    public System.Action<int, int> onHealthChanged; // (current, max)
    public System.Action onDeath;

    void Awake()
    {
        // Self-heal required components (no editor warnings)
        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.useGravity = true;
        rb.isKinematic = false;

        var col = GetComponent<Collider>();
        if (col == null) col = gameObject.AddComponent<CapsuleCollider>();

        controller = GetComponent<PlayerController>();
        if (controller == null) controller = gameObject.AddComponent<PlayerController>();

        // Clamp initial values
        maxHealth = Mathf.Max(1, maxHealth);
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        if (currentHealth == 0) currentHealth = maxHealth;
    }

    void Update()
    {
        if (invulnTimer > 0f)
            invulnTimer -= Time.deltaTime;
    }

    public void TakeDamage(int amount, Vector3? hitDirection = null)
    {
        if (amount <= 0) return;
        if (invulnTimer > 0f) return;

        currentHealth = Mathf.Max(0, currentHealth - amount);
        onHealthChanged?.Invoke(currentHealth, maxHealth);

        if (rb != null)
        {
            Vector3 impulse;
            if (hitDirection.HasValue && hitDirection.Value != Vector3.zero)
                impulse = hitDirection.Value.normalized * defaultKnockback + Vector3.up * 0.5f;
            else
                impulse = -transform.forward * defaultKnockback * 0.5f + Vector3.up * 0.5f;

            rb.AddForce(impulse, ForceMode.Impulse);
        }

        invulnTimer = invulnerabilityTime;

        if (currentHealth == 0)
            Die();
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;
        int prev = currentHealth;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        if (currentHealth != prev)
            onHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void Die()
    {
        if (controller) controller.enabled = false;
        enabled = false;
        onDeath?.Invoke();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        maxHealth = Mathf.Max(1, maxHealth);
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    }
#endif
}


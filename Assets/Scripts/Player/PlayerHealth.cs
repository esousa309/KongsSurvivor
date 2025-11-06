using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : Health
{
    [Header("Player Specific")]
    public float invulnerabilityTime = 0.5f;
    public float defaultKnockback = 5f;
    
    private bool isInvulnerable = false;
    private Rigidbody2D rb;
    private PlayerMovement playerMovement;
    
    protected override void Start()
    {
        base.Start();
        
        rb = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<PlayerMovement>();
        
        // Override auto destroy for player
        autoDestroyOnDeath = false;
    }
    
    public void TakeDamage(float amount, Vector3 damageSource)
    {
        if (isInvulnerable || isDead) return;
        
        // Call base damage
        base.TakeDamage(amount);
        
        // Apply knockback
        ApplyKnockback(damageSource);
        
        // Start invulnerability
        if (!isDead)
        {
            StartCoroutine(InvulnerabilityFrames());
        }
    }
    
    public override void TakeDamage(float amount)
    {
        if (isInvulnerable || isDead) return;
        
        base.TakeDamage(amount);
        
        // Start invulnerability
        if (!isDead)
        {
            StartCoroutine(InvulnerabilityFrames());
        }
    }
    
    void ApplyKnockback(Vector3 damageSource)
    {
        if (rb != null)
        {
            Vector2 knockbackDir = (transform.position - damageSource).normalized;
            rb.AddForce(knockbackDir * defaultKnockback * 100f);
        }
    }
    
    IEnumerator InvulnerabilityFrames()
    {
        isInvulnerable = true;
        
        // Visual feedback - flashing
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            float flashDuration = 0.1f;
            int flashCount = Mathf.FloorToInt(invulnerabilityTime / flashDuration);
            
            for (int i = 0; i < flashCount; i++)
            {
                sr.enabled = false;
                yield return new WaitForSeconds(flashDuration / 2f);
                sr.enabled = true;
                yield return new WaitForSeconds(flashDuration / 2f);
            }
        }
        else
        {
            yield return new WaitForSeconds(invulnerabilityTime);
        }
        
        isInvulnerable = false;
    }
    
    protected override void Die()
    {
        base.Die();
        
        // Disable player controls
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }
        
        PlayerCombat combat = GetComponent<PlayerCombat>();
        if (combat != null)
        {
            combat.enabled = false;
        }
        
        // Play death animation or effect
        StartCoroutine(DeathSequence());
    }
    
    IEnumerator DeathSequence()
    {
        // Visual death effect
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            // Fade out
            float fadeTime = 1f;
            float elapsedTime = 0f;
            Color originalColor = sr.color;
            
            while (elapsedTime < fadeTime)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeTime);
                sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                yield return null;
            }
        }
        
        // Notify GameManager
        if (GameManager.Instance != null)
        {
            // GameManager will handle game over
        }
    }
    
    public void Revive()
    {
        isDead = false;
        current = maxHealth;
        
        // Re-enable components
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }
        
        PlayerCombat combat = GetComponent<PlayerCombat>();
        if (combat != null)
        {
            combat.enabled = true;
        }
        
        // Reset visuals
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.enabled = true;
            sr.color = Color.white;
        }
        
        OnHealthChanged?.Invoke(current);
    }
}
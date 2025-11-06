using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float damage = 50f;
    public float speed = 16f;
    public float maxRange = 10f;
    public bool isExplosive = false;
    public float explosionRadius = 2f;
    public float lifeStealPercent = 0f;
    
    private Vector3 startPosition;
    private Vector3 direction;
    private GameObject owner;
    private Rigidbody2D rb;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
    }
    
    public void Setup(float dmg, float spd, float range, Vector3 dir, GameObject own)
    {
        damage = dmg;
        speed = spd;
        maxRange = range;
        direction = dir.normalized;
        owner = own;
        startPosition = transform.position;
        
        // Set velocity
        if (rb != null)
        {
            rb.velocity = direction * speed;
        }
        
        // Destroy after max range
        Destroy(gameObject, maxRange / speed);
    }
    
    void Update()
    {
        // Check if exceeded max range
        if (Vector3.Distance(startPosition, transform.position) >= maxRange)
        {
            Destroy(gameObject);
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        HandleCollision(other);
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollision(collision.collider);
    }
    
    void HandleCollision(Collider2D other)
    {
        // Ignore collision with owner
        if (other.gameObject == owner) return;
        
        // Check if hit enemy
        if (other.CompareTag("Enemy"))
        {
            DamageTarget(other.gameObject);
            
            // Handle explosive shots
            if (isExplosive)
            {
                Explode();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        // Check if hit wall or ground
        else if (other.CompareTag("Wall") || other.CompareTag("Ground"))
        {
            if (isExplosive)
            {
                Explode();
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
    
    void DamageTarget(GameObject target)
    {
        Health targetHealth = target.GetComponent<Health>();
        if (targetHealth != null)
        {
            targetHealth.TakeDamage(damage);
            
            // Apply life steal
            if (lifeStealPercent > 0 && owner != null)
            {
                PlayerCombat playerCombat = owner.GetComponent<PlayerCombat>();
                if (playerCombat != null)
                {
                    playerCombat.ApplyLifeSteal(damage);
                }
            }
        }
    }
    
    void Explode()
    {
        // Visual effect (if you have one)
        // GameObject explosionEffect = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        // Destroy(explosionEffect, 1f);
        
        // Damage all enemies in radius
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        
        foreach (Collider2D col in colliders)
        {
            if (col.CompareTag("Enemy"))
            {
                // Calculate damage falloff based on distance
                float distance = Vector2.Distance(transform.position, col.transform.position);
                float damageMultiplier = 1f - (distance / explosionRadius);
                
                Health enemyHealth = col.GetComponent<Health>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(damage * damageMultiplier * 0.5f); // Explosion does half damage
                }
                
                // Apply knockback
                Rigidbody2D enemyRb = col.GetComponent<Rigidbody2D>();
                if (enemyRb != null)
                {
                    Vector2 knockbackDir = (col.transform.position - transform.position).normalized;
                    enemyRb.AddForce(knockbackDir * 500f);
                }
            }
        }
        
        Destroy(gameObject);
    }
    
    void OnDrawGizmosSelected()
    {
        if (isExplosive)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }
}

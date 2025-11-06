using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Settings")]
    public float moveSpeed = 2f;
    public float damage = 10f;
    public float attackRange = 1.5f;
    public float attackCooldown = 1f;
    public bool isBoss = false;
    
    [Header("Target")]
    private Transform target;
    private float lastAttackTime = 0f;
    
    [Header("Components")]
    private Rigidbody2D rb;
    private Health health;
    private SpriteRenderer spriteRenderer;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
        
        health = GetComponent<Health>();
        if (health == null)
        {
            health = gameObject.AddComponent<Health>();
            health.maxHealth = isBoss ? 500f : 100f;
            health.current = health.maxHealth;
        }
        
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        
        // Set tag
        gameObject.tag = "Enemy";
        
        // Scale up if boss
        if (isBoss)
        {
            transform.localScale = Vector3.one * 2f;
            moveSpeed *= 0.7f; // Bosses move slower
            damage *= 2f; // But hit harder
        }
    }
    
    void Start()
    {
        // Find player if no target set
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }
    }
    
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
    
    void Update()
    {
        if (target == null) return;
        
        // Calculate distance to target
        float distanceToTarget = Vector2.Distance(transform.position, target.position);
        
        // Move towards target if not in attack range
        if (distanceToTarget > attackRange)
        {
            MoveTowardsTarget();
        }
        else
        {
            // Stop moving and attack
            rb.velocity = Vector2.zero;
            
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                Attack();
                lastAttackTime = Time.time;
            }
        }
        
        // Face target
        if (target.position.x < transform.position.x)
        {
            spriteRenderer.flipX = true;
        }
        else
        {
            spriteRenderer.flipX = false;
        }
    }
    
    void MoveTowardsTarget()
    {
        if (target == null || rb == null) return;
        
        Vector2 direction = (target.position - transform.position).normalized;
        rb.velocity = direction * moveSpeed;
    }
    
    void Attack()
    {
        if (target == null) return;
        
        // Check if target is still in range
        float distance = Vector2.Distance(transform.position, target.position);
        if (distance <= attackRange)
        {
            // Deal damage to player
            PlayerHealth playerHealth = target.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage, transform.position);
            }
            
            // Visual feedback
            StartCoroutine(AttackAnimation());
        }
    }
    
    IEnumerator AttackAnimation()
    {
        // Simple scale punch for attack
        Vector3 originalScale = transform.localScale;
        transform.localScale = originalScale * 1.2f;
        
        yield return new WaitForSeconds(0.1f);
        
        transform.localScale = originalScale;
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Continuous damage on contact with player
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null && Time.time >= lastAttackTime + attackCooldown)
            {
                playerHealth.TakeDamage(damage, transform.position);
                lastAttackTime = Time.time;
            }
        }
    }
    
    void OnCollisionStay2D(Collision2D collision)
    {
        // Continuous damage while in contact
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null && Time.time >= lastAttackTime + attackCooldown)
            {
                playerHealth.TakeDamage(damage, transform.position);
                lastAttackTime = Time.time;
            }
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Draw line to target
        if (target != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, target.position);
        }
    }
}

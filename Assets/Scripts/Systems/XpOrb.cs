using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XPOrb : MonoBehaviour
{
    [Header("XP Settings")]
    public float xpValue = 1f;
    public float magnetRange = 3f;
    public float magnetSpeed = 8f;
    public float lifetime = 30f;
    
    private Transform player;
    private bool isBeingAttracted = false;
    private Rigidbody2D rb;
    
    void Start()
    {
        // Find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        
        // Get or add Rigidbody2D
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
        }
        
        // Set tag
        gameObject.tag = "XPOrb";
        
        // Auto-destroy after lifetime
        Destroy(gameObject, lifetime);
        
        // Visual setup
        SetupVisuals();
    }
    
    void SetupVisuals()
    {
        // Add sprite renderer if missing
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            sr = gameObject.AddComponent<SpriteRenderer>();
        }
        
        // Create a simple circle sprite if none exists
        if (sr.sprite == null)
        {
            // Use Unity's default sprite (white square)
            sr.sprite = Resources.Load<Sprite>("Sprites/Square");
            sr.color = new Color(0.5f, 0f, 1f, 1f); // Purple color for XP
            transform.localScale = Vector3.one * 0.3f;
        }
        
        // Add circle collider if missing
        CircleCollider2D col = GetComponent<CircleCollider2D>();
        if (col == null)
        {
            col = gameObject.AddComponent<CircleCollider2D>();
            col.radius = 0.2f;
            col.isTrigger = true;
        }
    }
    
    void Update()
    {
        if (player == null) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // Check if within magnet range
        if (distanceToPlayer <= magnetRange && !isBeingAttracted)
        {
            isBeingAttracted = true;
        }
        
        // Move towards player if attracted
        if (isBeingAttracted)
        {
            MoveTowardsPlayer();
        }
    }
    
    void MoveTowardsPlayer()
    {
        if (player == null || rb == null) return;
        
        Vector2 direction = (player.position - transform.position).normalized;
        
        // Increase speed as it gets closer
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        float speedMultiplier = Mathf.Lerp(2f, 1f, distanceToPlayer / magnetRange);
        
        rb.velocity = direction * magnetSpeed * speedMultiplier;
    }
    
    public void SetXPValue(float value)
    {
        xpValue = value;
        
        // Scale orb based on value
        float scale = 0.3f + (value / 10f) * 0.2f;
        scale = Mathf.Clamp(scale, 0.3f, 1f);
        transform.localScale = Vector3.one * scale;
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Player will handle the XP collection
            // The PlayerXP script will destroy this object
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw magnet range
        Gizmos.color = new Color(0.5f, 0f, 1f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, magnetRange);
    }
}
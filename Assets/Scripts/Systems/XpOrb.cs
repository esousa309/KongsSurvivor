using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class XpOrb : MonoBehaviour
{
    public int xp = 1;
    public float attractRadius = 1.8f;
    public float attractSpeed = 6f;
    
    [Header("Visual Effects")]
    public float pulseSpeed = 3f;          // Faster pulsing
    public float pulseAmount = 0.3f;       // BIGGER pulse (30% instead of 15%)
    public float rotationSpeed = 100f;     // Faster rotation

    Transform target;
    Vector3 originalScale;
    float pulseTimer;

    void Awake()  // Changed from Start to Awake - runs immediately!
    {
        originalScale = transform.localScale;
    }

    void Update()
    {
        // PULSING animation - more noticeable now
        pulseTimer += Time.deltaTime * pulseSpeed;
        float pulse = 1f + Mathf.Sin(pulseTimer) * pulseAmount;
        transform.localScale = originalScale * pulse;
        
        // ROTATION animation - faster now
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);

        // Find and move towards player
        if (target == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) target = p.transform;
        }
        else
        {
            float d = Vector2.Distance(transform.position, target.position);
            if (d <= attractRadius)
            {
                transform.position = Vector2.MoveTowards(transform.position, target.position, attractSpeed * Time.deltaTime);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var level = other.GetComponent<LevelSystem>();
        if (level != null)
        {
            level.AddXp(xp);
            
            // Spawn collection effect
            CreateCollectionEffect();
            
            Destroy(gameObject);
        }
    }
    
    void CreateCollectionEffect()
    {
        // Create a simple particle burst
        GameObject particle = new GameObject("XP_Particle");
        particle.transform.position = transform.position;
        
        // Add sprite renderer for visual
        SpriteRenderer sr = particle.AddComponent<SpriteRenderer>();
        sr.sprite = GetComponent<SpriteRenderer>().sprite;
        sr.color = new Color(0.3f, 0.8f, 1f, 1f); // Bright blue
        
        // Add animation component
        ParticleBurst pb = particle.AddComponent<ParticleBurst>();
        pb.lifetime = 0.3f;
    }
}
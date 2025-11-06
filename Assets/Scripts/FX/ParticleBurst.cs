using UnityEngine;

public class ParticleBurst : MonoBehaviour
{
    public float lifetime = 0.3f;
    
    private float timer;
    private Vector3 startScale;
    private SpriteRenderer sr;

    void Start()
    {
        startScale = transform.localScale;
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        timer += Time.deltaTime;
        
        // Scale up and fade out
        float progress = timer / lifetime;
        transform.localScale = startScale * (1f + progress * 2f);
        
        if (sr != null)
        {
            Color c = sr.color;
            c.a = 1f - progress;
            sr.color = c;
        }
        
        // Destroy when done
        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}
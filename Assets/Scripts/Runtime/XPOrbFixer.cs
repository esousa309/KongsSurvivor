using UnityEngine;

public class XPOrbFixer : MonoBehaviour
{
    public float attractionSpeed = 8f;
    public float collectionDistance = 0.5f;
    public float verticalFloat = 0.5f;
    public float floatSpeed = 2f;
    
    private Transform playerTransform;
    private bool isBeingCollected = false;
    private float initialY;
    
    void Start()
    {
        // Find the player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        
        initialY = transform.position.y;
        
        // Ensure the orb is 3D
        Fix3DVisuals();
    }
    
    void Fix3DVisuals()
    {
        // Remove 2D components
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            DestroyImmediate(spriteRenderer);
        }
        
        // Add 3D mesh
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            meshFilter = gameObject.AddComponent<MeshFilter>();
            GameObject tempSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            meshFilter.mesh = tempSphere.GetComponent<MeshFilter>().mesh;
            DestroyImmediate(tempSphere);
        }
        
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
            Material orbMaterial = new Material(Shader.Find("Standard"));
            orbMaterial.color = new Color(0.2f, 0.5f, 1f, 1f); // Blue color
            orbMaterial.SetFloat("_Metallic", 0.8f);
            orbMaterial.SetFloat("_Glossiness", 0.9f);
            
            // Make it glow
            orbMaterial.EnableKeyword("_EMISSION");
            orbMaterial.SetColor("_EmissionColor", new Color(0.2f, 0.5f, 1f) * 1.5f);
            meshRenderer.material = orbMaterial;
        }
        
        // Set scale
        transform.localScale = Vector3.one * 0.3f;
    }
    
    void Update()
    {
        if (playerTransform == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        
        // Start collection when player is close enough
        if (distanceToPlayer < 3f)
        {
            isBeingCollected = true;
        }
        
        if (isBeingCollected)
        {
            // Move towards player
            Vector3 targetPos = playerTransform.position;
            transform.position = Vector3.MoveTowards(transform.position, targetPos, attractionSpeed * Time.deltaTime);
            
            // Check if close enough to collect
            if (distanceToPlayer < collectionDistance)
            {
                CollectOrb();
            }
        }
        else
        {
            // Float animation
            float newY = initialY + Mathf.Sin(Time.time * floatSpeed) * verticalFloat;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }
    
    void CollectOrb()
    {
        // Add XP to player
        PlayerLevel playerLevel = playerTransform.GetComponent<PlayerLevel>();
        if (playerLevel != null)
        {
            playerLevel.AddXP(1);
        }
        
        // Destroy this orb
        Destroy(gameObject);
    }
}
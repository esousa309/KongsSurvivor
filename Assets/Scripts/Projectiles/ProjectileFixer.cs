using UnityEngine;

public class ProjectileFixer : MonoBehaviour
{
    public float scaleMultiplier = 0.3f;
    
    void Start()
    {
        // Remove any 2D components
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            DestroyImmediate(spriteRenderer);
        }
        
        // Ensure we have 3D components
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
            Material projectileMaterial = new Material(Shader.Find("Standard"));
            projectileMaterial.color = Color.yellow;
            projectileMaterial.SetFloat("_Metallic", 0.5f);
            projectileMaterial.SetFloat("_Glossiness", 0.8f);
            
            // Make it slightly emissive so it's more visible
            projectileMaterial.EnableKeyword("_EMISSION");
            projectileMaterial.SetColor("_EmissionColor", Color.yellow * 0.5f);
            meshRenderer.material = projectileMaterial;
        }
        
        // Set appropriate scale
        transform.localScale = Vector3.one * scaleMultiplier;
    }
}
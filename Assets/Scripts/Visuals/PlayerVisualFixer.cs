using UnityEngine;

public class PlayerVisualFixer : MonoBehaviour
{
    void Start()
    {
        // Remove the 2D sprite renderer if it exists
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            DestroyImmediate(spriteRenderer);
        }
        
        // Add 3D mesh components if they don't exist
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            meshFilter = gameObject.AddComponent<MeshFilter>();
            // Use a capsule mesh for the player
            GameObject tempCapsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            meshFilter.mesh = tempCapsule.GetComponent<MeshFilter>().mesh;
            DestroyImmediate(tempCapsule);
        }
        
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
            
            // Create a simple green material for the player
            Material playerMaterial = new Material(Shader.Find("Standard"));
            playerMaterial.color = new Color(0.035f, 0.977f, 0.087f, 1f); // Green color from your sprite
            meshRenderer.material = playerMaterial;
        }
    }
}
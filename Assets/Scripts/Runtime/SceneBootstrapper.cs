using UnityEngine;

public class SceneBootstrapFixer : MonoBehaviour
{
    void Awake()
    {
        FixScene();
    }
    
    void FixScene()
    {
        // Fix Player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            if (player.GetComponent<PlayerVisualFixer>() == null)
            {
                player.AddComponent<PlayerVisualFixer>();
            }
        }
        
        // Fix Camera
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            if (mainCam.GetComponent<CameraFixer>() == null)
            {
                mainCam.gameObject.AddComponent<CameraFixer>();
            }
        }
        
        // Fix existing XP Orbs
        GameObject[] orbs = GameObject.FindGameObjectsWithTag("XPOrb");
        foreach (GameObject orb in orbs)
        {
            if (orb.GetComponent<XPOrbFixer>() == null)
            {
                orb.AddComponent<XPOrbFixer>();
            }
        }
        
        Debug.Log("Scene fixes applied! All 2D elements converted to 3D.");
    }
}
using UnityEngine;

/// <summary>
/// Ensures the Player Rigidbody is configured for 3D XZ motion.
/// </summary>
[DefaultExecutionOrder(-1000)]
public class EnsurePlayer3DSetup : MonoBehaviour
{
    void Awake()
    {
        var rb = GetComponent<Rigidbody>();
        if (rb)
        {
            rb.useGravity = false; // custom gravity in PlayerController
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ; // allow XZ translation
        }

        // Make sure the Player is tagged correctly so other systems find it.
        if (gameObject.tag != "Player") gameObject.tag = "Player";
    }
}

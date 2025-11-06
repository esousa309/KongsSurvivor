using UnityEngine;

[DisallowMultipleComponent]
public class EnemyGroundClamp : MonoBehaviour
{
    [Tooltip("Layers considered as walkable ground.")]
    public LayerMask groundMask = ~0;

    [Tooltip("How high above the object to start the ground ray (safety).")]
    public float castHeight = 50f;

    [Tooltip("Small gap to avoid z-fighting with ground.")]
    public float skin = 0.02f;

    void LateUpdate()
    {
        Vector3 origin = transform.position + Vector3.up * castHeight;
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, castHeight * 2f, groundMask, QueryTriggerInteraction.Ignore))
        {
            float bottomOffset = 0f;
            var col = GetComponent<Collider>();
            if (col != null)
            {
                // distance from center to bottom of the collider
                bottomOffset = col.bounds.extents.y;
            }

            Vector3 p = transform.position;
            p.y = hit.point.y + bottomOffset + skin;
            transform.position = p;
        }
    }
}

using UnityEngine;

[DisallowMultipleComponent]
public class ProjectileGroundClamp : MonoBehaviour
{
    public LayerMask groundMask = ~0;
    public float castHeight = 50f;
    public float skin = 0.02f;

    [Tooltip("Extra height above the ground so projectiles read clearly.")]
    public float heightOffset = 1.0f;

    void LateUpdate()
    {
        Vector3 origin = transform.position + Vector3.up * castHeight;
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, castHeight * 2f, groundMask, QueryTriggerInteraction.Ignore))
        {
            float bottom = 0f;
            var col = GetComponent<Collider>();
            if (col) bottom = col.bounds.extents.y;

            var p = transform.position;
            p.y = hit.point.y + bottom + skin + heightOffset;
            transform.position = p;
        }
    }
}

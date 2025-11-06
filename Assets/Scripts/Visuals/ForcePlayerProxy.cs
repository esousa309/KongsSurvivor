using UnityEngine;

[DefaultExecutionOrder(-1000)]
[RequireComponent(typeof(Transform))]
public class ForcePlayerProxy : MonoBehaviour
{
    public Vector3 capsuleWorldScale = new Vector3(1.2f, 2.4f, 1.2f);
    public Color   capsuleColor      = new Color(0.2f, 1f, 0.2f);

    void Awake()  { Ensure(); }
    void OnEnable(){ Ensure(); }

    void Ensure()
    {
        // Hide any 2D sprite on the player
        var sr = GetComponent<SpriteRenderer>();
        if (sr) sr.enabled = false;

        // Remove stale proxy (wrong primitive, wrong scale, etc.)
        var existing = transform.Find("Visual3D");
        if (existing) DestroyImmediate(existing.gameObject);

        // Create a fresh capsule proxy
        var proxy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        proxy.name = "Visual3D";
        proxy.transform.SetParent(transform, false);
        proxy.transform.localPosition = Vector3.zero;
        proxy.transform.localRotation = Quaternion.identity;

        // strictly visual—no physics
        var col = proxy.GetComponent<Collider>(); if (col) Destroy(col);
        var rb  = proxy.GetComponent<Rigidbody>(); if (rb) Destroy(rb);

        // set world scale, regardless of parent
        SetWorldScale(proxy.transform, capsuleWorldScale);

        var r = proxy.GetComponent<Renderer>();
        if (r && r.material)
        {
            if (r.material.HasProperty("_Color")) r.material.color = capsuleColor;
            r.material.DisableKeyword("_EMISSION");
        }
    }

    static void SetWorldScale(Transform t, Vector3 worldScale)
    {
        var parent = t.parent;
        t.localScale = Vector3.one;
        if (parent != null)
        {
            var lossy = t.lossyScale;
            t.localScale = new Vector3(
                lossy.x == 0f ? 0f : worldScale.x / lossy.x,
                lossy.y == 0f ? 0f : worldScale.y / lossy.y,
                lossy.z == 0f ? 0f : worldScale.z / lossy.z
            );
        }
        else t.localScale = worldScale;
    }
}

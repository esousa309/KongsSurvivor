using UnityEngine;

[DefaultExecutionOrder(-200)]
[DisallowMultipleComponent]
public class VisualAuthoringService : MonoBehaviour
{
    public float scanInterval = 0.35f;

    public Vector3 enemyScale  = new Vector3(1.2f, 1.2f, 1.2f);
    public Vector3 projScale   = new Vector3(1.8f, 1.8f, 1.8f); // bigger = easy to see
    public Vector3 orbScale    = new Vector3(0.8f, 0.8f, 0.8f);
    public float   projYOffset = 1.0f;
    public float   orbYOffset  = 0.0f;

    float _timer;

    void Start() { ApplyOnce(); }
    void Update()
    {
        _timer -= Time.deltaTime;
        if (_timer <= 0f) { ApplyOnce(); _timer = scanInterval; }
    }

    void ApplyOnce()
    {
        foreach (var tr in FindObjectsOfType<Transform>(false))
        {
            var go = tr.gameObject; if (!go) continue;
            string n = (go.name ?? "").ToLowerInvariant();

            // Skip scene infrastructure
            if (n == "main" || n.Contains("manager") || n.Contains("spawner") || n.Contains("service") ||
                n.Contains("canvas") || n.Contains("eventsystem") || n.Contains("camera") ||
                n.Contains("ground") || n.Contains("wall"))
            { RemoveProxy(go); continue; }

            // Magnets stay invisible & proxy-free
            if (IsMagnet(go))
            { RemoveProxy(go); var sr = go.GetComponent<SpriteRenderer>(); if (sr) sr.enabled = false; continue; }

            // Player is handled by ForcePlayerProxy
            if (n == "player" || n == "kong" || go.CompareTag("Player")) continue;

            // Enemies -> cubes
            if (go.CompareTag("Enemy") || n.Contains("enemy"))
            { EnsureExactProxy(go, PrimitiveType.Cube, enemyScale, Color.white, 0f, false); HideSprite(go); continue; }

            // Projectiles -> glowing big spheres (lifted)
            if (n.Contains("projectile") || n.Contains("bullet") || n.Contains("missile") || n.Contains("shot"))
            { EnsureExactProxy(go, PrimitiveType.Sphere, projScale, new Color(0.3f, 0.6f, 1f), projYOffset, true); HideSprite(go); continue; }

            // XP orbs / pickups -> glowing spheres
            if (n.Contains("xp") || n.Contains("orb") || n.Contains("pickup") || n.Contains("loot"))
            { EnsureExactProxy(go, PrimitiveType.Sphere, orbScale, new Color(0.2f, 1f, 0.6f), orbYOffset, true); HideSprite(go); continue; }
        }
    }

    static bool IsMagnet(GameObject go)
    {
        string n = (go.name ?? "").ToLowerInvariant();
        if (n.Contains("magnet")) return true;
        foreach (var c in go.GetComponents<Component>())
            if (c && c.GetType().Name.ToLowerInvariant().Contains("magnet")) return true;
        return false;
    }

    static void HideSprite(GameObject go)
    { var sr = go.GetComponent<SpriteRenderer>(); if (sr) sr.enabled = false; }

    static void RemoveProxy(GameObject go)
    { var p = go.transform.Find("Visual3D"); if (p) Object.DestroyImmediate(p.gameObject); }

    void EnsureExactProxy(GameObject parent, PrimitiveType type, Vector3 worldScale, Color color, float yOffset, bool emissive)
    {
        RemoveProxy(parent);

        var proxy = GameObject.CreatePrimitive(type);
        proxy.name = "Visual3D";
        proxy.transform.SetParent(parent.transform, false);
        proxy.transform.localPosition = new Vector3(0f, yOffset, 0f);
        proxy.transform.localRotation = Quaternion.identity;

        // strictly visual
        var col = proxy.GetComponent<Collider>(); if (col) Destroy(col);
        var rb  = proxy.GetComponent<Rigidbody>(); if (rb) Destroy(rb);

        SetWorldScale(proxy.transform, worldScale);

        var r = proxy.GetComponent<Renderer>();
        if (r && r.material)
        {
            if (r.material.HasProperty("_Color")) r.material.color = color;
            if (emissive)
            {
                r.material.EnableKeyword("_EMISSION");
                if (r.material.HasProperty("_EmissionColor"))
                    r.material.SetColor("_EmissionColor", color * 2.4f);
            }
            else r.material.DisableKeyword("_EMISSION");
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

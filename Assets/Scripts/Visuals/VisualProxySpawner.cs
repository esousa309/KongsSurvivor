using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
public class VisualProxySpawner : MonoBehaviour
{
    [Tooltip("How often we scan the scene for new/unknown spawned objects.")]
    public float scanInterval = 0.35f;

    [Header("Projectile visuals")]
    public float projectileYOffset = 1.0f; // keep them visible above ground
    public Vector3 projectileScale = new Vector3(0.9f, 0.9f, 0.9f);

    [Header("Orb / pickup visuals")]
    public float orbYOffset = 0.0f;
    public Vector3 orbScale = new Vector3(0.6f, 0.6f, 0.6f);

    [Header("Enemy / boss visuals")]
    public Vector3 enemyScale = new Vector3(1.15f, 1.15f, 1.15f);
    public Vector3 bossScale  = new Vector3(2.0f,  2.0f,  2.0f);

    [Header("Player visuals")]
    public Vector3 playerScale = new Vector3(1.2f, 2.4f, 1.2f);

    float _timer;

    void Start() => ApplyOnce();

    void Update()
    {
        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            ApplyOnce();
            _timer = scanInterval;
        }
    }

    // ---- main scan ----
    void ApplyOnce()
    {
        foreach (var t in FindObjectsOfType<Transform>(false))
        {
            var go = t.gameObject;
            if (!go) continue;

            string name = (go.name ?? "").ToLowerInvariant();

            // never proxy magnets (keep them invisible/functional)
            if (LooksLikeMagnet(go)) continue;

            // PLAYER
            if (name == "kong" || name == "player" || go.CompareTag("Player"))
            {
                EnsureProxy(go, PrimitiveType.Capsule, playerScale, new Color(0.2f, 1f, 0.2f), 0f, emissive:false);
                continue;
            }

            // BOSSES (by name or tag)
            if (name.Contains("boss") || go.CompareTag("Boss"))
            {
                // big cylinder so it reads distinct from regular enemies
                EnsureProxy(go, PrimitiveType.Cylinder, bossScale, new Color(1.0f, 0.6f, 0.2f), 0f, emissive:false);
                continue;
            }

            // ENEMIES
            if (go.CompareTag("Enemy") || name.Contains("enemy"))
            {
                EnsureProxy(go, PrimitiveType.Cube, enemyScale, Color.white, 0f, emissive:false);
                continue;
            }

            // PROJECTILES
            if (LooksLikeProjectile(name))
            {
                EnsureProxy(go, PrimitiveType.Cube, projectileScale, new Color(0.3f, 0.6f, 1f), projectileYOffset, emissive:true);
                continue;
            }

            // XP ORBS / PICKUPS
            if (LooksLikePickup(name))
            {
                EnsureProxy(go, PrimitiveType.Sphere, orbScale, new Color(0.2f, 1f, 0.6f), orbYOffset, emissive:true);
                continue;
            }
        }
    }

    // ---- helpers ----
    static bool LooksLikeProjectile(string lowerName)
    {
        return lowerName.Contains("projectile") ||
               lowerName.Contains("bullet")     ||
               lowerName.Contains("missile")    ||
               lowerName.Contains("shot");
    }

    static bool LooksLikePickup(string lowerName)
    {
        return lowerName.Contains("xp")     ||
               lowerName.Contains("orb")    ||
               lowerName.Contains("pickup") ||
               lowerName.Contains("loot");
    }

    static bool LooksLikeMagnet(GameObject go)
    {
        string n = (go.name ?? "").ToLowerInvariant();
        if (n.Contains("magnet")) return true;

        // any component whose type name contains "magnet" (e.g., XpMagnet)
        var comps = go.GetComponents<Component>();
        return comps.Any(c => c && c.GetType().Name.ToLowerInvariant().Contains("magnet"));
    }

    void EnsureProxy(GameObject parent, PrimitiveType type, Vector3 localScale, Color color, float yOffset, bool emissive)
    {
        var existing = parent.transform.Find("Visual3D");
        if (existing == null)
        {
            var proxy = GameObject.CreatePrimitive(type);
            proxy.name = "Visual3D";
            proxy.transform.SetParent(parent.transform, false);
            existing = proxy.transform;

            // visual-only: remove physics from proxy
            var col = proxy.GetComponent<Collider>(); if (col) Destroy(col);
            var rb  = proxy.GetComponent<Rigidbody>(); if (rb) Destroy(rb);
        }

        existing.localPosition = new Vector3(0f, yOffset, 0f);
        existing.localRotation = Quaternion.identity;
        existing.localScale    = localScale;

        var r = existing.GetComponent<Renderer>();
        if (r && r.material)
        {
            if (r.material.HasProperty("_Color")) r.material.color = color;
            if (emissive)
            {
                r.material.EnableKeyword("_EMISSION");
                if (r.material.HasProperty("_EmissionColor"))
                    r.material.SetColor("_EmissionColor", color * 2.2f);
            }
            else r.material.DisableKeyword("_EMISSION");
        }

        // We DO NOT hide the parent SpriteRenderer; keeping it visible avoids “paper-thin” regressions.
    }
}

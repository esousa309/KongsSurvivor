using UnityEngine;

[DefaultExecutionOrder(-50)]
public class OrbAutoClampService : MonoBehaviour
{
    public float scanInterval = 0.5f;
    float timer;

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f) { Attach(); timer = scanInterval; }
    }

    void Attach()
    {
        foreach (var tr in FindObjectsOfType<Transform>(false))
        {
            var go = tr.gameObject; if (!go) continue;
            string n = (go.name ?? "").ToLowerInvariant();

            if (n.Contains("magnet") || HasComponentLike(go, "magnet"))
            {   // magnets stay invisible
                var p = tr.Find("Visual3D"); if (p) DestroyImmediate(p.gameObject);
                var sr = go.GetComponent<SpriteRenderer>(); if (sr) sr.enabled = false;
                continue;
            }

            if (n.Contains("xp") || n.Contains("orb") || n.Contains("pickup") || n.Contains("loot"))
            {
                if (!go.GetComponent<OrbGroundClamp>()) go.AddComponent<OrbGroundClamp>();
                if (!go.GetComponent<OrbAutoClean>())   go.AddComponent<OrbAutoClean>();
            }
        }
    }

    static bool HasComponentLike(GameObject go, string lowerSubstr)
    {
        foreach (var c in go.GetComponents<Component>())
            if (c && c.GetType().Name.ToLowerInvariant().Contains(lowerSubstr)) return true;
        return false;
    }
}

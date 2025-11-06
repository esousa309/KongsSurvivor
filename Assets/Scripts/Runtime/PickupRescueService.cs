using UnityEngine;

public class PickupRescueService : MonoBehaviour
{
    [Header("Targets")]
    public string[] pickupKeywords = new[] { "xp", "orb", "pickup" };

    [Header("Behavior")]
    public float maxLifetime = 20f;
    public float idleSpeed = 0.03f;
    public float idleNudge = 5f;
    public float collectRadius = 0.9f;
    public float groundSkin = 0.02f;
    public float groundCastHeight = 50f;

    Transform player;

    bool LooksLikeMagnet(GameObject go)
    {
        string n = go.name.ToLowerInvariant();
        if (n.Contains("magnet")) return true;
        var comps = go.GetComponents<Component>();
        foreach (var c in comps)
        {
            if (!c) continue;
            var tn = c.GetType().Name.ToLowerInvariant();
            if (tn.Contains("magnet")) return true;
        }
        return false;
    }

    void LateUpdate()
    {
        if (player == null)
        {
            var go = GameObject.FindWithTag("Player") ?? GameObject.Find("Kong") ?? GameObject.Find("Player");
            if (go) player = go.transform;
        }

        var all = FindObjectsOfType<Transform>(false);
        foreach (var t in all)
        {
            var go = t.gameObject;

            // Never touch magnets
            if (LooksLikeMagnet(go)) continue;

            string n = go.name.ToLowerInvariant();
            bool looksPickup = false;
            for (int i = 0; i < pickupKeywords.Length; i++)
                if (n.Contains(pickupKeywords[i])) { looksPickup = true; break; }

            if (!looksPickup) continue;

            var life = go.GetComponent<_PickupAge>() ?? go.AddComponent<_PickupAge>();
            life.age += Time.deltaTime;
            if (life.age > maxLifetime) { Destroy(go); continue; }

            // clamp to ground
            Vector3 origin = t.position + Vector3.up * groundCastHeight;
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, groundCastHeight * 2f, ~0, QueryTriggerInteraction.Ignore))
            {
                float bottom = 0f;
                var col = go.GetComponent<Collider>(); if (col) bottom = col.bounds.extents.y;
                var p = t.position; p.y = hit.point.y + bottom + groundSkin; t.position = p;
            }

            if (player != null)
            {
                if ((player.position - t.position).sqrMagnitude < collectRadius * collectRadius)
                {
                    Destroy(go);
                    continue;
                }
            }

            var rb = go.GetComponent<Rigidbody>();
            float speed = rb ? rb.velocity.magnitude : 0f;
            if (speed < idleSpeed && player != null)
            {
                Vector3 dir = (player.position - t.position);
                dir.y = 0f;
                if (dir.sqrMagnitude > 0.0001f)
                {
                    dir = dir.normalized;
                    t.position += dir * idleNudge * Time.deltaTime;
                }
            }
        }
    }

    class _PickupAge : MonoBehaviour { public float age; }
}

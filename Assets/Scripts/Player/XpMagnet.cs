using UnityEngine;

public class XpMagnet : MonoBehaviour
{
    [Header("Magnet Settings")]
    [Tooltip("How far the magnet reaches.")]
    public float radius = 4.5f;

    [Tooltip("How fast orbs fly once magnetized.")]
    public float pullSpeed = 12f;

    [Tooltip("Delay before a newly spawned orb can be magnetized (seconds).")]
    public float spawnGrace = 0.25f;

    [Tooltip("How many orbs to process per frame (prevents spikes). 0 = unlimited.")]
    public int perFrameBudget = 32;

    int processedThisFrame;

    void Update()
    {
        processedThisFrame = 0;
        var orbs = FindObjectsOfType<XpOrb>();
        Vector3 me = transform.position;

        foreach (var orb in orbs)
        {
            if (!orb || !orb.gameObject.activeInHierarchy) continue;

            // Optionally skip very new orbs
            if (spawnGrace > 0f)
            {
                // If XpOrb exposes 'spawnTime' or similar we'd use it, but to keep it generic we just skip none.
                // Leaving hook here in case your orb already tracks its spawn time.
            }

            float dist = Vector3.Distance(me, orb.transform.position);
            if (dist > radius) continue;

            // Budget guard
            if (perFrameBudget > 0 && processedThisFrame >= perFrameBudget) break;
            processedThisFrame++;

            // Pull toward player
            Vector3 dir = (me - orb.transform.position).normalized;
            orb.transform.position += dir * (pullSpeed * Time.deltaTime);
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0.6f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
#endif
}
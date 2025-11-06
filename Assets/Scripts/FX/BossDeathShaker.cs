
// Assets/Scripts/FX/BossDeathShaker.cs
using UnityEngine;

public class BossDeathShaker : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        new GameObject("BossDeathShaker", typeof(BossDeathShaker)).hideFlags = HideFlags.HideInHierarchy;
    }

    Health trackedBoss;
    float lastHp = -1f;
    float rescanTimer;
    const float RescanInterval = 0.5f;

    void Update()
    {
        var gm = Object.FindObjectOfType<GameManager>();
        if (gm == null || !gm.IsBossLevel)
        {
            trackedBoss = null;
            lastHp = -1f;
            return;
        }

        if (trackedBoss == null || trackedBoss.current <= 0f || !trackedBoss.gameObject.activeInHierarchy)
        {
            rescanTimer -= Time.deltaTime;
            if (rescanTimer <= 0f)
            {
                rescanTimer = RescanInterval;
                trackedBoss = FindLikelyBoss();
                lastHp = -1f;
            }
        }
        if (trackedBoss == null) return;

        if (lastHp < 0f) lastHp = trackedBoss.current;

        if (trackedBoss.current <= 0f && lastHp > 0f)
        {
            // Boss just died
            CameraShaker.Shake(0.35f, 0.25f);
        }
        lastHp = trackedBoss.current;
    }

    Health FindLikelyBoss()
    {
        var enemies = Object.FindObjectsOfType<Enemy>();
        Health best = null;
        float bestScore = -1f;
        foreach (var e in enemies)
        {
            var h = e.GetComponent<Health>();
            if (h == null || h.current <= 0f) continue;
            float max = GetMaxHealth(h);
            float score = h.current + max;
            if (score > bestScore) { bestScore = score; best = h; }
        }
        return best;
    }

    float GetMaxHealth(Health h)
    {
        try
        {
            var type = h.GetType();
            var field = type.GetField("maxHealth");
            if (field != null && field.FieldType == typeof(float)) return (float)field.GetValue(h);
            var prop = type.GetProperty("MaxHealth");
            if (prop != null && prop.PropertyType == typeof(float)) return (float)prop.GetValue(h, null);
        }
        catch { }
        return Mathf.Max(1f, h.current);
    }
}

using UnityEngine;

public class StuckOrbCleaner : MonoBehaviour
{
    public float maxLifetime = 15f;
    public float minSpeedToLive = 0.05f;
    public float checkInterval = 0.5f;

    float age;
    float timer;

    void Update()
    {
        age += Time.deltaTime;
        timer -= Time.deltaTime;

        if (age >= maxLifetime)
        {
            Destroy(gameObject);
            return;
        }

        if (timer <= 0f)
        {
            timer = checkInterval;
            var all = FindObjectsOfType<Transform>(false);
            foreach (var t in all)
            {
                string n = t.name.ToLowerInvariant();
                if (!(n.Contains("orb") || n.Contains("xp"))) continue;

                var rb = t.GetComponent<Rigidbody>();
                float speed = rb ? rb.velocity.magnitude : 0f;
                if (speed < minSpeedToLive)
                {
                    // If it's basically stuck, kill it after a short grace
                    var k = t.gameObject.GetComponent<_KillSoon>();
                    if (k == null) t.gameObject.AddComponent<_KillSoon>();
                }
            }
        }
    }

    // Small helper to kill specific objects a bit later so it's not abrupt
    class _KillSoon : MonoBehaviour
    {
        float t = 2.0f;
        void Update() { t -= Time.deltaTime; if (t <= 0f) Destroy(gameObject); }
    }
}

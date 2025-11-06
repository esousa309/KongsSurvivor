using UnityEngine;

public class ProjectileAutoClampService : MonoBehaviour
{
    public LayerMask groundMask = ~0;
    public float scanInterval = 0.35f;
    float timer;

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            AttachClamp();
            timer = scanInterval;
        }
    }

    void AttachClamp()
    {
        var all = FindObjectsOfType<Transform>(false);
        foreach (var t in all)
        {
            var go = t.gameObject;
            string n = go.name.ToLowerInvariant();
            if (n.Contains("projectile") || n.Contains("bullet") || n.Contains("missile") || n.Contains("shot"))
            {
                var clamp = go.GetComponent<ProjectileGroundClamp>();
                if (clamp == null)
                {
                    clamp = go.AddComponent<ProjectileGroundClamp>();
                    clamp.groundMask = groundMask;
                }
            }
        }
    }
}

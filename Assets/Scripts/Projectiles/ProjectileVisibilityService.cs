using UnityEngine;

public class ProjectileVisibilityService : MonoBehaviour
{
    public float scanInterval = 0.25f;
    float timer;

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            Attach();
            timer = scanInterval;
        }
    }

    void Attach()
    {
        var all = FindObjectsOfType<Transform>(false);
        foreach (var t in all)
        {
            var go = t.gameObject;
            string n = go.name.ToLowerInvariant();
            if (n.Contains("projectile") || n.Contains("bullet") || n.Contains("missile") || n.Contains("shot"))
            {
                if (go.GetComponent<ProjectileVisibilityBooster>() == null)
                    go.AddComponent<ProjectileVisibilityBooster>();
            }
        }
    }
}

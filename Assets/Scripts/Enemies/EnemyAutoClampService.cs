using UnityEngine;

public class EnemyAutoClampService : MonoBehaviour
{
    [Tooltip("Layers considered as walkable ground for enemies.")]
    public LayerMask groundMask = ~0;

    [Tooltip("How often to scan for new enemies (seconds).")]
    public float scanInterval = 0.5f;

    private float timer;

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            AttachClampToEnemies();
            timer = scanInterval;
        }
    }

    void AttachClampToEnemies()
    {
        var all = FindObjectsOfType<Transform>(includeInactive: false);
        foreach (var t in all)
        {
            GameObject go = t.gameObject;
            string n = go.name.ToLowerInvariant();
            if (go.CompareTag("Enemy") || n.Contains("enemy"))
            {
                var clamp = go.GetComponent<EnemyGroundClamp>();
                if (clamp == null)
                {
                    clamp = go.AddComponent<EnemyGroundClamp>();
                    clamp.groundMask = groundMask;
                }
            }
        }
    }
}

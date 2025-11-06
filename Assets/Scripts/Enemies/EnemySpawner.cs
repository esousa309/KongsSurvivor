using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Prefabs & References")]
    public Enemy enemyPrefab;
    public Enemy bossPrefab;
    public Transform player;

    [Header("Pickups")]
    public GameObject xpOrbPrefab;

    [Header("Spawn Settings")]
    public float spawnInterval = 1.5f;
    public int maxEnemies = 80;
    public float spawnRadius = 12f;

    // Difficulty context (set by GameManager)
    int planetIndex = 0;
    int subLevel = 1;

    // Internal
    float timer;
    public bool SpawnedAnyThisLevel { get; private set; }

    public int LiveEnemies
    {
        get
        {
            // Fast enough for debugging scale
            return FindObjectsOfType<Enemy>().Length;
        }
    }

    void OnEnable()
    {
        timer = 0f;
        SpawnedAnyThisLevel = false;
        TryAutoAssignPlayer();
    }

    void Update()
    {
        if (!IsReady(out string whyNot))
        {
            return;
        }

        if (LiveEnemies >= maxEnemies) return;

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            if (!SpawnEnemy(false))
            {
                Debug.LogWarning("[EnemySpawner] SpawnEnemy(false) failed. Check prefabs and Player reference.");
            }
            timer = 0f;
        }
    }

    public void SetDifficulty(int planetIdx, int subLvl, bool isBossLevel)
    {
        planetIndex = planetIdx;
        subLevel = subLvl;
        spawnInterval = Mathf.Clamp(1.8f - 0.06f * (subLevel - 1) - 0.05f * planetIndex, 0.35f, 2f);
        maxEnemies = Mathf.Clamp(40 + 4 * (subLevel - 1) + 8 * planetIndex, 20, 200);

        if (isBossLevel)
        {
            spawnInterval *= 2.5f;
            maxEnemies = Mathf.Max(15, maxEnemies / 3);
        }
    }

    public void ResetForLevelStart(int initialBurstCount)
    {
        timer = 0f;
        SpawnedAnyThisLevel = false;
        if (!IsReady(out string whyNot))
        {
            Debug.LogWarning("[EnemySpawner] Not ready at level start: " + whyNot);
            return;
        }

        for (int i = 0; i < initialBurstCount; i++)
        {
            bool ok = SpawnEnemy(false);
            if (!ok)
            {
                Debug.LogWarning($"[EnemySpawner] Initial burst spawn #{i+1} failed.");
                break;
            }
        }
    }

    public void SpawnBossNow()
    {
        if (!SpawnEnemy(true))
        {
            Debug.LogWarning("[EnemySpawner] SpawnBossNow failed. Check bossPrefab and Player reference.");
        }
    }

    /// <summary>
    /// Forces a single regular enemy spawn at current player position + radius.
    /// Useful for debugging.
    /// </summary>
    public bool ForceSpawnOne()
    {
        if (!IsReady(out string whyNot))
        {
            Debug.LogWarning("[EnemySpawner] ForceSpawnOne blocked: " + whyNot);
            return false;
        }
        return SpawnEnemy(false);
    }

    /// <summary>
    /// Forces a boss spawn for debugging.
    /// </summary>
    public bool ForceSpawnBoss()
    {
        if (!IsReady(out string whyNot))
        {
            Debug.LogWarning("[EnemySpawner] ForceSpawnBoss blocked: " + whyNot);
            return false;
        }
        return SpawnEnemy(true);
    }

    bool SpawnEnemy(bool boss)
    {
        var prefab = boss ? bossPrefab : enemyPrefab;
        if (prefab == null) { return false; }
        if (player == null) { TryAutoAssignPlayer(); if (player == null) return false; }

        Vector2 offset = Random.insideUnitCircle.normalized * Mathf.Max(1f, spawnRadius);
        Vector2 pos = (Vector2)player.position + offset;

        var e = Instantiate(prefab, pos, Quaternion.identity);
        if (e == null) return false;

        // Safety: ensure base components exist
        var h = e.GetComponent<Health>();
        if (h == null) h = e.gameObject.AddComponent<Health>();
        if (e.GetComponent<Rigidbody2D>() == null)
        {
            var rb = e.gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
        }
        if (e.GetComponent<Collider2D>() == null)
        {
            e.gameObject.AddComponent<CircleCollider2D>();
        }

        // Scale difficulty
        float hpMult = 1f + 0.25f * planetIndex + 0.15f * (subLevel - 1);
        if (boss) hpMult *= 12f;
        if (h != null)
        {
            h.maxHealth = Mathf.Max(1f, h.maxHealth * hpMult);
            h.current = h.maxHealth;
        }

        e.moveSpeed *= 1f + 0.02f * (subLevel - 1) + 0.01f * planetIndex;
        e.xpAmount = boss ? Mathf.Max(1, 15 + 3 * planetIndex) : Mathf.Max(1, 1 + subLevel / 5);

        // Ensure XP drop
        if (e.xpOrbPrefab == null && xpOrbPrefab != null) e.xpOrbPrefab = xpOrbPrefab;

        SpawnedAnyThisLevel = true;
        return true;
    }

    bool IsReady(out string reason)
    {
        if (enemyPrefab == null) { reason = "Enemy Prefab missing"; return false; }
        if (player == null) { reason = "Player not assigned"; return false; }
        reason = null;
        return true;
    }

    void TryAutoAssignPlayer()
    {
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, Mathf.Max(0.5f, spawnRadius));
    }
}
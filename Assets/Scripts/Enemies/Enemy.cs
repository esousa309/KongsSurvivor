
// Assets/Scripts/Enemies/Enemy.cs
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;

    [Header("Rewards")]
    public int xpAmount = 1;
    public GameObject xpOrbPrefab;

    private Transform player;
    private Health health;
    private bool hasDied;
    private bool orbDropped;
    private static bool suppressGlobalRewards; // used during level cleanup if you want

    void Awake()
    {
        health = GetComponent<Health>();
        if (health == null) health = gameObject.AddComponent<Health>();

        // If an orb was accidentally nested under this enemy in the prefab, purge it.
        var childOrbs = GetComponentsInChildren<XpOrb>(true);
        foreach (var c in childOrbs)
        {
            if (c != null && c.gameObject != null) Destroy(c.gameObject);
        }
    }

    void Update()
    {
        // If Health.Damage destroys us immediately on 0 HP, Update might never see it.
        // We still keep this check to catch normal cases.
        if (!hasDied && health != null && health.current <= 0f)
        {
            hasDied = true;
            TryDropOrb();
            Destroy(gameObject);
            return;
        }

        // Chase player (simple survivors-like movement)
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
        if (player == null) return;

        var dir = (player.position - transform.position).normalized;
        transform.position += dir * (moveSpeed * Time.deltaTime);
    }

    void OnDestroy()
    {
        // If destroyed without Update running (e.g., Health immediately calls Destroy),
        // we still want to drop an orb ONLY if the enemy actually died (HP <= 0).
        if (!orbDropped && !suppressGlobalRewards && health != null && health.current <= 0f)
        {
            TryDropOrb();
        }
    }

    private void TryDropOrb()
    {
        if (orbDropped) return;
        orbDropped = true;

        if (xpOrbPrefab == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"[Enemy] No xpOrbPrefab set on {name}; cannot drop XP.");
#endif
            return;
        }
        if (xpAmount <= 0) return;

        var orbGO = Instantiate(xpOrbPrefab, transform.position, Quaternion.identity);
        orbGO.transform.SetParent(null); // ensure not parented under this enemy
        var xo = orbGO.GetComponent<XpOrb>();
        if (xo != null) xo.xp = xpAmount;
    }

    // Optional: call this before mass cleanup to avoid reward drops (e.g., at level end)
    public static void SetSuppressRewards(bool on) { suppressGlobalRewards = on; }
}

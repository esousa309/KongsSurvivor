
using UnityEngine;

public class AutoAimWeapon : MonoBehaviour
{
    public Projectile projectilePrefab;
    public Transform muzzle; // optional, defaults to this transform
    public float fireRate = 1.5f;
    public float damage = 10f;
    public float range = 10f;
    public float projectileSpeed = 16f;

    private float fireTimer;

    void Update()
    {
        fireTimer += Time.deltaTime;
        if (fireTimer >= 1f / Mathf.Max(0.01f, fireRate))
        {
            var target = FindTarget();
            if (target != null)
            {
                Shoot(target);
                fireTimer = 0f;
            }
        }
    }

    Transform FindTarget()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, range);
        Transform best = null;
        float bestSqr = float.MaxValue;
        foreach (var h in hits)
        {
            Enemy e = h.GetComponent<Enemy>();
            if (e == null) continue;
            float d2 = (h.transform.position - transform.position).sqrMagnitude;
            if (d2 < bestSqr)
            {
                bestSqr = d2;
                best = h.transform;
            }
        }
        return best;
    }

    void Shoot(Transform target)
    {
        if (projectilePrefab == null) return;
        Vector3 spawnPos = muzzle != null ? muzzle.position : transform.position;
        Vector2 dir = (target.position - spawnPos).normalized;
        var proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        proj.Launch(dir, projectileSpeed, damage);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}

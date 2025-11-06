using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Combat Settings")]
    public GameObject projectilePrefab;
    public Transform muzzle;
    public float fireRate = 1.5f;
    public float damage = 50f;
    public float range = 10f;
    public float projectileSpeed = 16f;
    
    [Header("Upgrades")]
    public int projectileCount = 1;
    public float lifeStealPercent = 0f;
    public bool explosiveShots = false;
    public float explosionRadius = 2f;
    
    [Header("Auto-Aim")]
    public bool autoAim = true;
    public float aimRadius = 10f;
    
    private float nextFireTime = 0f;
    private Transform nearestEnemy;
    private PlayerHealth playerHealth;
    
    void Start()
    {
        playerHealth = GetComponent<PlayerHealth>();
        
        // Create muzzle if not assigned
        if (muzzle == null)
        {
            GameObject muzzleObj = new GameObject("Muzzle");
            muzzleObj.transform.SetParent(transform);
            muzzleObj.transform.localPosition = Vector3.up * 0.5f;
            muzzle = muzzleObj.transform;
        }
    }
    
    void Update()
    {
        // Find nearest enemy for auto-aim
        if (autoAim)
        {
            FindNearestEnemy();
        }
        
        // Auto-fire when enemy is in range
        if (Time.time >= nextFireTime)
        {
            if (nearestEnemy != null && Vector3.Distance(transform.position, nearestEnemy.position) <= range)
            {
                Fire();
                nextFireTime = Time.time + (1f / fireRate);
            }
        }
    }
    
    void FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float nearestDistance = Mathf.Infinity;
        nearestEnemy = null;
        
        foreach (GameObject enemy in enemies)
        {
            if (enemy != null)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance < nearestDistance && distance <= aimRadius)
                {
                    nearestDistance = distance;
                    nearestEnemy = enemy.transform;
                }
            }
        }
    }
    
    void Fire()
    {
        if (projectilePrefab == null || muzzle == null) return;
        
        // Calculate spread for multiple projectiles
        float spreadAngle = 15f;
        float startAngle = -(projectileCount - 1) * spreadAngle / 2f;
        
        for (int i = 0; i < projectileCount; i++)
        {
            // Create projectile
            GameObject projectile = Instantiate(projectilePrefab, muzzle.position, Quaternion.identity);
            
            // Calculate direction
            Vector3 direction = Vector3.up;
            
            if (nearestEnemy != null)
            {
                direction = (nearestEnemy.position - muzzle.position).normalized;
            }
            
            // Apply spread for multiple projectiles
            if (projectileCount > 1)
            {
                float angle = startAngle + (i * spreadAngle);
                direction = Quaternion.Euler(0, 0, angle) * direction;
            }
            
            // Setup projectile
            Projectile proj = projectile.GetComponent<Projectile>();
            if (proj == null)
            {
                proj = projectile.AddComponent<Projectile>();
            }
            
            proj.Setup(damage, projectileSpeed, range, direction, gameObject);
            proj.isExplosive = explosiveShots;
            proj.explosionRadius = explosionRadius;
            proj.lifeStealPercent = lifeStealPercent;
            
            // Set projectile rotation
            float rot_z = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            projectile.transform.rotation = Quaternion.Euler(0f, 0f, rot_z - 90);
        }
    }
    
    // Called by projectiles when they hit enemies
    public void ApplyLifeSteal(float damageDealt)
    {
        if (playerHealth != null && lifeStealPercent > 0)
        {
            float healAmount = damageDealt * lifeStealPercent;
            playerHealth.Heal(healAmount);
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, range);
        
        // Draw aim radius
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, aimRadius);
        
        // Draw line to nearest enemy
        if (nearestEnemy != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, nearestEnemy.position);
        }
    }
}
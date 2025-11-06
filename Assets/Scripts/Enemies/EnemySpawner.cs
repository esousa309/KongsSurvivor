using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject enemyPrefab;
    public GameObject bossPrefab;
    public Transform player;
    public GameObject xpOrbPrefab;
    
    [Header("Spawn Configuration")]
    public float spawnInterval = 1.5f;
    public int maxEnemies = 80;
    public float spawnRadius = 12f;
    public float minSpawnDistance = 5f;
    
    [Header("Runtime")]
    private bool isSpawning = false;
    private List<GameObject> activeEnemies = new List<GameObject>();
    private Coroutine spawnCoroutine;
    
    void Start()
    {
        // Find player if not assigned
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
        
        // Clean up list
        activeEnemies.Clear();
    }
    
    public void StartSpawning()
    {
        if (!isSpawning)
        {
            isSpawning = true;
            spawnCoroutine = StartCoroutine(SpawnLoop());
        }
    }
    
    public void StopSpawning()
    {
        isSpawning = false;
        
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }
    
    IEnumerator SpawnLoop()
    {
        while (isSpawning)
        {
            if (activeEnemies.Count < maxEnemies)
            {
                SpawnEnemy();
            }
            
            yield return new WaitForSeconds(spawnInterval);
        }
    }
    
    public void SpawnEnemy()
    {
        if (enemyPrefab == null || player == null) return;
        
        Vector3 spawnPosition = GetSpawnPosition();
        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        
        // Set enemy target
        Enemy enemyScript = enemy.GetComponent<Enemy>();
        if (enemyScript != null)
        {
            enemyScript.SetTarget(player);
        }
        
        // Track enemy
        activeEnemies.Add(enemy);
        
        // Setup enemy death handling
        Health enemyHealth = enemy.GetComponent<Health>();
        if (enemyHealth != null)
        {
            enemyHealth.OnDeath += () => HandleEnemyDeath(enemy);
        }
    }
    
    public void SpawnBoss()
    {
        if (bossPrefab == null || player == null) return;
        
        Vector3 spawnPosition = GetSpawnPosition();
        GameObject boss = Instantiate(bossPrefab, spawnPosition, Quaternion.identity);
        
        // Set boss target
        Enemy bossScript = boss.GetComponent<Enemy>();
        if (bossScript != null)
        {
            bossScript.SetTarget(player);
            bossScript.isBoss = true;
        }
        
        // Track boss
        activeEnemies.Add(boss);
        
        // Setup boss death handling
        Health bossHealth = boss.GetComponent<Health>();
        if (bossHealth != null)
        {
            bossHealth.OnDeath += () => HandleEnemyDeath(boss);
        }
    }
    
    Vector3 GetSpawnPosition()
    {
        if (player == null)
        {
            return transform.position + Random.insideUnitSphere * spawnRadius;
        }
        
        Vector3 spawnPos;
        int attempts = 0;
        
        do
        {
            // Get random angle
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            
            // Get random distance between min and max
            float distance = Random.Range(minSpawnDistance, spawnRadius);
            
            // Calculate position
            spawnPos = player.position + new Vector3(
                Mathf.Cos(angle) * distance,
                Mathf.Sin(angle) * distance,
                0f
            );
            
            attempts++;
        }
        while (Vector3.Distance(spawnPos, player.position) < minSpawnDistance && attempts < 10);
        
        return spawnPos;
    }
    
    void HandleEnemyDeath(GameObject enemy)
    {
        if (enemy == null) return;
        
        // Remove from active list
        activeEnemies.Remove(enemy);
        
        // Spawn XP orb
        if (xpOrbPrefab != null)
        {
            SpawnXPOrb(enemy.transform.position);
        }
        
        // Check if this was a boss
        Enemy enemyScript = enemy.GetComponent<Enemy>();
        if (enemyScript != null && enemyScript.isBoss)
        {
            // Spawn multiple XP orbs for boss
            for (int i = 0; i < 5; i++)
            {
                Vector3 offset = Random.insideUnitCircle * 1f;
                SpawnXPOrb(enemy.transform.position + offset);
            }
            
            // Notify game manager if all bosses defeated
            CheckBossCompletion();
        }
    }
    
    void SpawnXPOrb(Vector3 position)
    {
        if (xpOrbPrefab != null)
        {
            GameObject orb = Instantiate(xpOrbPrefab, position, Quaternion.identity);
            
            // Add slight random force
            Rigidbody2D rb = orb.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 randomForce = Random.insideUnitCircle * 2f;
                rb.AddForce(randomForce, ForceMode2D.Impulse);
            }
        }
    }
    
    void CheckBossCompletion()
    {
        bool bossesRemaining = false;
        
        foreach (GameObject enemy in activeEnemies)
        {
            if (enemy != null)
            {
                Enemy enemyScript = enemy.GetComponent<Enemy>();
                if (enemyScript != null && enemyScript.isBoss)
                {
                    bossesRemaining = true;
                    break;
                }
            }
        }
        
        if (!bossesRemaining && GameManager.Instance != null && GameManager.Instance.IsBossLevel)
        {
            // All bosses defeated, complete level
            GameManager.Instance.CompleteLevel();
        }
    }
    
    public int GetActiveEnemyCount()
    {
        // Clean up null entries
        activeEnemies.RemoveAll(e => e == null);
        return activeEnemies.Count;
    }
    
    public void ClearAllEnemies()
    {
        foreach (GameObject enemy in activeEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }
        
        activeEnemies.Clear();
    }
    
    void OnDrawGizmosSelected()
    {
        Vector3 center = player != null ? player.position : transform.position;
        
        // Draw spawn radius
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(center, spawnRadius);
        
        // Draw minimum spawn distance
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(center, minSpawnDistance);
    }
}
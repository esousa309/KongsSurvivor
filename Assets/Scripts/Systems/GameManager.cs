using UnityEngine;
using System;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public EnemySpawner spawner;
    public float levelDurationSeconds = 90f;
    public float nextLevelDelay = 2f;
    public int initialBurstBase = 4;
    public bool endNonBossWhenCleared = true;

    [Header("Run Settings")]
    public bool resetOmegaOnFirstLevel = true;

    [Header("State (debug)")]
    public int planetIndex = 0;
    public int subLevel = 1;
    public bool isBossLevel;

    float timer;
    float levelStartTime;
    bool bossSpawned;
    bool running;
    int lastOmegaReward;
    LevelSystem playerLevel;

    // Boss tracking
    Health bossHealth;
    float bossRescanTimer;
    const float BossRescanInterval = 0.25f;

    bool runInitialized = false;
    
    // NEW: Track if we're waiting for upgrade selection
    private bool waitingForUpgrade = false;

    public event Action OnLevelWon;

    public bool IsRunning => running;
    public bool IsBossLevel => isBossLevel;
    public int PlanetIndex => planetIndex;
    public int SubLevel => subLevel;
    public float TimeRemaining => Mathf.Max(0f, timer);
    public int LastOmegaReward => lastOmegaReward;

    void Start() { BeginLevel(planetIndex, subLevel); }

    public void RestartRun()
    {
        runInitialized = false;
        BeginLevel(0, 1);
    }

    public void BeginLevel(int planetIdx, int subLvl)
    {
        StopAllCoroutines();

        planetIndex = Mathf.Max(0, planetIdx);
        subLevel = Mathf.Clamp(subLvl, 1, 25);

        if (!runInitialized)
        {
            if (resetOmegaOnFirstLevel && planetIndex == 0 && subLevel == 1) CurrencyManager.ResetTotal();
            runInitialized = true;
        }

        isBossLevel = (subLevel >= 5) && (subLevel % 5 == 0);

        timer = levelDurationSeconds;
        bossSpawned = false;
        running = true;
        lastOmegaReward = 0;
        levelStartTime = Time.time;
        waitingForUpgrade = false; // NEW: Reset flag

        bossHealth = null;
        bossRescanTimer = 0f;

        // Clean leftovers
        foreach (var orb in FindObjectsOfType<XpOrb>()) if (orb) Destroy(orb.gameObject);
        foreach (var e in FindObjectsOfType<Enemy>()) if (e) Destroy(e.gameObject);

        if (spawner != null)
        {
            spawner.enabled = true;
            spawner.SetDifficulty(planetIndex, subLevel, isBossLevel);

            int burst = isBossLevel ? 0 : Mathf.Clamp(initialBurstBase + planetIndex, initialBurstBase, initialBurstBase + 6);
            spawner.ResetForLevelStart(burst);
        }

        var playerGO = GameObject.FindGameObjectWithTag("Player");
        playerLevel = playerGO ? playerGO.GetComponent<LevelSystem>() : null;
    }

    void Update()
    {
        // NEW: Don't update game logic while waiting for upgrade
        if (waitingForUpgrade) return;
        
        if (!running) return;

        if (isBossLevel)
        {
            // ensure boss present
            if (!bossSpawned && spawner != null && spawner.bossPrefab != null)
            {
                spawner.SpawnBossNow();
                bossSpawned = true;

                // Lock the fight: stop further spawns
                spawner.enabled = false;

                // Try to acquire immediately
                bossHealth = FindBossHealth();
            }

            // ----- Boss end conditions -----
            // 1) If we have a reference and it's dead or disabled -> win
            if (bossHealth != null)
            {
                if (bossHealth.current <= 0f || !bossHealth.gameObject.activeInHierarchy)
                {
                    LevelWon();
                    return;
                }
            }
            else
            {
                // 2) If reference lost (likely destroyed), rescan quickly
                bossRescanTimer -= Time.deltaTime;
                if (bossRescanTimer <= 0f)
                {
                    bossRescanTimer = BossRescanInterval;
                    bossHealth = FindBossHealth();
                }

                // 3) Safety: if boss was spawned and there are NO live enemies, end level
                if (bossSpawned && CountLiveEnemies() == 0)
                {
                    LevelWon();
                    return;
                }
            }

            // No timer UI on boss levels
            return;
        }

        // Non-boss: timer or clear
        timer -= Time.deltaTime;
        if (timer <= 0f) { LevelWon(); return; }

        if (endNonBossWhenCleared && spawner != null)
        {
            if (Time.time - levelStartTime > 2f && spawner.SpawnedAnyThisLevel && spawner.LiveEnemies == 0)
            {
                LevelWon();
                return;
            }
        }
    }

    int CountLiveEnemies()
    {
        int count = 0;
        var enemies = FindObjectsOfType<Enemy>();
        foreach (var e in enemies)
        {
            var h = e.GetComponent<Health>();
            if (h != null && h.current > 0f) count++;
        }
        return count;
    }

    Health FindBossHealth()
    {
        // Prefer any component with type name starting with "Boss"
        foreach (var mb in FindObjectsOfType<MonoBehaviour>())
        {
            if (mb == null) continue;
            var t = mb.GetType();
            if (t.Name.StartsWith("Boss", StringComparison.OrdinalIgnoreCase))
            {
                var h2 = mb.GetComponent<Health>();
                if (h2 != null) return h2;
                h2 = mb.GetComponentInChildren<Health>();
                if (h2 != null) return h2;
            }
        }

        // Fallback: highest-HP Enemy
        Health best = null; float bestScore = -1f;
        foreach (var e in FindObjectsOfType<Enemy>())
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

    void LevelWon()
    {
        if (!running) return;
        running = false;

        // Stop spawner
        if (spawner) spawner.enabled = false;

        // Award any floating orbs
        var orbs = FindObjectsOfType<XpOrb>();
        if (playerLevel != null) { foreach (var orb in orbs) if (orb) playerLevel.AddXp(orb.xp); }
        foreach (var orb in orbs) if (orb) Destroy(orb.gameObject);

        // Clean up enemies
        foreach (var e in FindObjectsOfType<Enemy>()) if (e) Destroy(e.gameObject);

        lastOmegaReward = CalculateOmegaReward();
        CurrencyManager.AddOmega(lastOmegaReward);

        try { OnLevelWon?.Invoke(); } catch {}

        // NEW: Show upgrade UI and wait for player selection
        waitingForUpgrade = true;
        LevelUpWatcher.TriggerLevelUpUI();
    }
    
    // NEW: Public method called when player selects an upgrade
    public void OnUpgradeSelected()
    {
        waitingForUpgrade = false;
        StartCoroutine(LoadNextLevel());
    }

    int CalculateOmegaReward()
    {
        if (isBossLevel)
        {
            if (subLevel == 25) return 500;
            int tier = Mathf.Max(0, (subLevel / 5) - 1);
            return 100 + 25 * tier;
        }
        int baseReward = 10 + (subLevel * 2) + (planetIndex * 3);
        return Mathf.Clamp(baseReward, 10, 50);
    }

    IEnumerator LoadNextLevel()
    {
        yield return new WaitForSecondsRealtime(nextLevelDelay); // NEW: Use Realtime so it works even if Time.timeScale = 0

        if (subLevel < 25) subLevel += 1;
        else { subLevel = 1; planetIndex += 1;
            var prog = GetComponent<PlanetProgression>();
            if (prog && prog.PlanetCount > 0) planetIndex = Mathf.Clamp(planetIndex, 0, Mathf.Max(0, prog.PlanetCount - 1));
        }

        BeginLevel(planetIndex, subLevel);
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 520, 20), $"Planet {planetIndex}  Sub-level {subLevel}  {(isBossLevel ? "BOSS" : "Survive")}");
        if (!isBossLevel) GUI.Label(new Rect(10, 30, 300, 20), $"Time Left: {Mathf.CeilToInt(timer)}s");
        GUI.Label(new Rect(Screen.width - 210, 10, 200, 20), $"Î© OMEGA: {CurrencyManager.TotalOmega}");
        if (waitingForUpgrade) GUI.Label(new Rect(10, 50, 350, 20), $"LEVEL COMPLETE! +Î©{lastOmegaReward}  Choose upgrade..."); // NEW: Different message
        else if (!running && lastOmegaReward > 0) GUI.Label(new Rect(10, 50, 350, 20), $"LEVEL COMPLETE! +Î©{lastOmegaReward}  Loading next...");
    }
}
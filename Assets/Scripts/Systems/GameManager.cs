using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameManager>();
            }
            return instance;
        }
    }
    
    [Header("Level Configuration")]
    public EnemySpawner spawner;
    public float levelDurationSeconds = 90f;
    public float nextLevelDelay = 2f;
    public int initialBurstBase = 5;
    public bool endNonBossWhenCleared = true;
    public bool resetOmegaOnFirstLevel = true;
    
    [Header("Current Level Info")]
    public int planetIndex = 0;
    public int subLevel = 0;
    public bool isBossLevel = false;
    
    [Header("Rewards")]
    private int lastOmegaReward = 0;
    private float timeRemaining;
    private bool levelComplete = false;
    private bool gameOver = false;
    
    [Header("Player References")]
    private PlayerHealth playerHealth;
    private PlayerXP playerXP;
    
    [Header("UI References")]
    public GameObject endLevelRewardUI;
    public GameObject upgradeSelectionUI;
    
    // Public Properties for other scripts to access
    public float TimeRemaining => timeRemaining;
    public int LastOmegaReward => lastOmegaReward;
    public int PlanetIndex => planetIndex;
    public int SubLevel => subLevel;
    public bool IsBossLevel => isBossLevel;
    
    // Events
    public Action<int> OnOmegaRewardEarned;
    public Action OnLevelComplete;
    public Action OnGameOver;
    public Action<List<string>> OnUpgradeChoicesReady;
    
    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
    }
    
    void Start()
    {
        InitializeLevel();
        FindReferences();
        
        // Start the level timer
        timeRemaining = levelDurationSeconds;
        StartCoroutine(LevelTimer());
        
        // Spawn initial enemies
        if (spawner != null)
        {
            StartCoroutine(InitialEnemyBurst());
        }
    }
    
    void InitializeLevel()
    {
        // Load saved progress or set defaults
        if (PlayerPrefs.HasKey("CurrentPlanet"))
        {
            planetIndex = PlayerPrefs.GetInt("CurrentPlanet", 0);
            subLevel = PlayerPrefs.GetInt("CurrentSubLevel", 0);
        }
        
        // Determine if this is a boss level
        // Every 5th sublevel is Elite Boss, every 25th is Planet Boss
        isBossLevel = (subLevel > 0 && subLevel % 5 == 0);
        
        // Reset Omega coins on first level if configured
        if (resetOmegaOnFirstLevel && planetIndex == 0 && subLevel == 0)
        {
            PlayerPrefs.SetInt("OmegaCoins", 0);
        }
    }
    
    void FindReferences()
    {
        playerHealth = FindObjectOfType<PlayerHealth>();
        playerXP = FindObjectOfType<PlayerXP>();
        
        if (playerHealth != null)
        {
            playerHealth.OnDeath += HandlePlayerDeath;
        }
    }
    
    void Update()
    {
        // Update timer
        if (!levelComplete && !gameOver)
        {
            timeRemaining -= Time.deltaTime;
            
            if (timeRemaining <= 0)
            {
                timeRemaining = 0;
                CompleteLevel();
            }
            
            // Check if all enemies cleared (for non-boss levels)
            if (endNonBossWhenCleared && !isBossLevel)
            {
                if (spawner != null && spawner.GetActiveEnemyCount() == 0 && timeRemaining < levelDurationSeconds - 5f)
                {
                    CompleteLevel();
                }
            }
        }
    }
    
    IEnumerator InitialEnemyBurst()
    {
        yield return new WaitForSeconds(0.5f);
        
        int enemiesToSpawn = initialBurstBase + (subLevel / 5);
        
        for (int i = 0; i < enemiesToSpawn; i++)
        {
            if (isBossLevel && i == 0)
            {
                spawner.SpawnBoss();
            }
            else
            {
                spawner.SpawnEnemy();
            }
            yield return new WaitForSeconds(0.2f);
        }
        
        // Start regular spawning
        if (!isBossLevel)
        {
            spawner.StartSpawning();
        }
    }
    
    IEnumerator LevelTimer()
    {
        while (timeRemaining > 0 && !levelComplete && !gameOver)
        {
            yield return new WaitForSeconds(1f);
        }
    }
    
    public void CompleteLevel()
    {
        if (levelComplete) return;
        
        levelComplete = true;
        
        // Stop spawning
        if (spawner != null)
        {
            spawner.StopSpawning();
        }
        
        // Calculate rewards
        CalculateRewards();
        
        // Show reward UI
        ShowEndLevelReward();
        
        OnLevelComplete?.Invoke();
        
        // Auto-proceed to next level after delay
        StartCoroutine(NextLevelSequence());
    }
    
    void CalculateRewards()
    {
        // Base reward calculation
        int baseReward = 10 + (subLevel * 5);
        
        // Boss level multiplier
        if (isBossLevel)
        {
            baseReward *= 3;
        }
        
        // Time bonus
        float timeBonus = Mathf.Max(0, timeRemaining / levelDurationSeconds);
        baseReward = Mathf.RoundToInt(baseReward * (1f + timeBonus));
        
        lastOmegaReward = baseReward;
        
        // Add to player's total
        int currentOmega = PlayerPrefs.GetInt("OmegaCoins", 0);
        PlayerPrefs.SetInt("OmegaCoins", currentOmega + lastOmegaReward);
        PlayerPrefs.Save();
        
        OnOmegaRewardEarned?.Invoke(lastOmegaReward);
    }
    
    void ShowEndLevelReward()
    {
        if (endLevelRewardUI != null)
        {
            endLevelRewardUI.SetActive(true);
            // The UI should display LastOmegaReward
        }
    }
    
    IEnumerator NextLevelSequence()
    {
        yield return new WaitForSeconds(nextLevelDelay);
        
        // Hide reward UI
        if (endLevelRewardUI != null)
        {
            endLevelRewardUI.SetActive(false);
        }
        
        // Show upgrade selection
        ShowUpgradeSelection();
    }
    
    void ShowUpgradeSelection()
    {
        // Generate upgrade choices
        List<string> upgradeChoices = GenerateUpgradeChoices();
        
        if (upgradeSelectionUI != null)
        {
            upgradeSelectionUI.SetActive(true);
        }
        
        OnUpgradeChoicesReady?.Invoke(upgradeChoices);
    }
    
    List<string> GenerateUpgradeChoices()
    {
        List<string> choices = new List<string>();
        
        // Add 3 random upgrade options
        string[] possibleUpgrades = {
            "Increase Damage",
            "Increase Fire Rate",
            "Increase Move Speed",
            "Increase Max Health",
            "Add Projectile",
            "Increase Range",
            "Life Steal",
            "Explosive Shots"
        };
        
        // Shuffle and pick 3
        for (int i = 0; i < 3 && i < possibleUpgrades.Length; i++)
        {
            int randomIndex = UnityEngine.Random.Range(i, possibleUpgrades.Length);
            string temp = possibleUpgrades[i];
            possibleUpgrades[i] = possibleUpgrades[randomIndex];
            possibleUpgrades[randomIndex] = temp;
            
            choices.Add(possibleUpgrades[i]);
        }
        
        return choices;
    }
    
    public void OnUpgradeSelected(string upgradeName)
    {
        // Apply the upgrade based on selection
        ApplyUpgrade(upgradeName);
        
        // Hide upgrade UI
        if (upgradeSelectionUI != null)
        {
            upgradeSelectionUI.SetActive(false);
        }
        
        // Proceed to next level
        LoadNextLevel();
    }
    
    void ApplyUpgrade(string upgradeName)
    {
        // Find player components and apply upgrades
        var playerCombat = FindObjectOfType<PlayerCombat>();
        var playerMovement = FindObjectOfType<PlayerMovement>();
        
        switch (upgradeName)
        {
            case "Increase Damage":
                if (playerCombat != null)
                    playerCombat.damage += 10;
                break;
                
            case "Increase Fire Rate":
                if (playerCombat != null)
                    playerCombat.fireRate *= 1.2f;
                break;
                
            case "Increase Move Speed":
                if (playerMovement != null)
                    playerMovement.moveSpeed *= 1.1f;
                break;
                
            case "Increase Max Health":
                if (playerHealth != null)
                {
                    playerHealth.maxHealth += 25;
                    playerHealth.currentHealth += 25;
                }
                break;
                
            case "Add Projectile":
                if (playerCombat != null)
                    playerCombat.projectileCount++;
                break;
                
            case "Increase Range":
                if (playerCombat != null)
                    playerCombat.range *= 1.25f;
                break;
                
            case "Life Steal":
                if (playerCombat != null)
                    playerCombat.lifeStealPercent = 0.1f;
                break;
                
            case "Explosive Shots":
                if (playerCombat != null)
                    playerCombat.explosiveShots = true;
                break;
        }
        
        Debug.Log($"Applied upgrade: {upgradeName}");
    }
    
    void LoadNextLevel()
    {
        // Increment sublevel
        subLevel++;
        
        // Check if moving to next planet (every 25 levels)
        if (subLevel >= 25)
        {
            planetIndex++;
            subLevel = 0;
        }
        
        // Save progress
        PlayerPrefs.SetInt("CurrentPlanet", planetIndex);
        PlayerPrefs.SetInt("CurrentSubLevel", subLevel);
        PlayerPrefs.Save();
        
        // Reload the scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    void HandlePlayerDeath()
    {
        if (gameOver) return;
        
        gameOver = true;
        
        // Stop spawning
        if (spawner != null)
        {
            spawner.StopSpawning();
        }
        
        OnGameOver?.Invoke();
        
        // Show game over UI and options
        StartCoroutine(GameOverSequence());
    }
    
    IEnumerator GameOverSequence()
    {
        yield return new WaitForSeconds(2f);
        
        // Show restart options
        // For now, just restart
        RestartRun();
    }
    
    public void RestartRun()
    {
        // Reset to first level
        planetIndex = 0;
        subLevel = 0;
        
        PlayerPrefs.SetInt("CurrentPlanet", 0);
        PlayerPrefs.SetInt("CurrentSubLevel", 0);
        
        if (resetOmegaOnFirstLevel)
        {
            PlayerPrefs.SetInt("OmegaCoins", 0);
        }
        
        PlayerPrefs.Save();
        
        // Reload scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
    
    void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnDeath -= HandlePlayerDeath;
        }
    }
    
    // Debug methods
    [ContextMenu("Complete Level")]
    void Debug_CompleteLevel()
    {
        CompleteLevel();
    }
    
    [ContextMenu("Add Omega Coins")]
    void Debug_AddOmegaCoins()
    {
        int current = PlayerPrefs.GetInt("OmegaCoins", 0);
        PlayerPrefs.SetInt("OmegaCoins", current + 100);
        Debug.Log($"Omega Coins: {current + 100}");
    }
}
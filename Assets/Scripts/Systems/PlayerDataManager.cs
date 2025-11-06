using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public class PlayerData
{
    public int blueSerum = 0;
    public int omegaCoins = 0;
    public int currentLevel = 1;
    public int currentPlanet = 1;
    public int highestLevelUnlocked = 1;
    public string selectedKongType = "Alpha"; // Alpha, Omega, or Absolute
    public List<string> unlockedKongs = new List<string>() { "Alpha_Kong_001" };
    public Dictionary<string, int> kongLevels = new Dictionary<string, int>();
}

public class PlayerDataManager : MonoBehaviour
{
    private static PlayerDataManager instance;
    public static PlayerDataManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<PlayerDataManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("PlayerDataManager");
                    instance = go.AddComponent<PlayerDataManager>();
                }
            }
            return instance;
        }
    }
    
    [Header("Player Data")]
    public PlayerData playerData;
    
    [Header("Save System")]
    private string saveKey = "KongSurvivorsPlayerData";
    
    [Header("Events")]
    public Action<int> OnBlueSerumChanged;
    public Action<int> OnOmegaCoinsChanged;
    public Action<int> OnLevelChanged;
    
    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        DontDestroyOnLoad(gameObject);
        
        LoadPlayerData();
    }
    
    void Start()
    {
        // Initialize player data if new
        if (playerData == null)
        {
            playerData = new PlayerData();
            SavePlayerData();
        }
    }
    
    public void AddBlueSerum(int amount)
    {
        if (playerData == null) return;
        
        playerData.blueSerum += amount;
        playerData.blueSerum = Mathf.Max(0, playerData.blueSerum);
        
        OnBlueSerumChanged?.Invoke(playerData.blueSerum);
        SavePlayerData();
    }
    
    public void AddOmegaCoins(int amount)
    {
        if (playerData == null) return;
        
        playerData.omegaCoins += amount;
        playerData.omegaCoins = Mathf.Max(0, playerData.omegaCoins);
        
        OnOmegaCoinsChanged?.Invoke(playerData.omegaCoins);
        SavePlayerData();
    }
    
    public bool SpendBlueSerum(int amount)
    {
        if (playerData == null || playerData.blueSerum < amount)
            return false;
        
        playerData.blueSerum -= amount;
        OnBlueSerumChanged?.Invoke(playerData.blueSerum);
        SavePlayerData();
        
        return true;
    }
    
    public bool SpendOmegaCoins(int amount)
    {
        if (playerData == null || playerData.omegaCoins < amount)
            return false;
        
        playerData.omegaCoins -= amount;
        OnOmegaCoinsChanged?.Invoke(playerData.omegaCoins);
        SavePlayerData();
        
        return true;
    }
    
    public void SetCurrentLevel(int level)
    {
        if (playerData == null) return;
        
        playerData.currentLevel = level;
        
        // Update highest level unlocked if necessary
        if (level > playerData.highestLevelUnlocked)
        {
            playerData.highestLevelUnlocked = level;
        }
        
        OnLevelChanged?.Invoke(level);
        SavePlayerData();
    }
    
    public void SetCurrentPlanet(int planet)
    {
        if (playerData == null) return;
        
        playerData.currentPlanet = Mathf.Clamp(planet, 1, 20);
        SavePlayerData();
    }
    
    public void UnlockKong(string kongId)
    {
        if (playerData == null) return;
        
        if (!playerData.unlockedKongs.Contains(kongId))
        {
            playerData.unlockedKongs.Add(kongId);
            
            // Initialize kong level
            if (!playerData.kongLevels.ContainsKey(kongId))
            {
                playerData.kongLevels[kongId] = 1;
            }
            
            SavePlayerData();
        }
    }
    
    public void UpgradeKong(string kongId, int newLevel)
    {
        if (playerData == null) return;
        
        if (playerData.unlockedKongs.Contains(kongId))
        {
            playerData.kongLevels[kongId] = newLevel;
            SavePlayerData();
        }
    }
    
    public int GetKongLevel(string kongId)
    {
        if (playerData == null || !playerData.kongLevels.ContainsKey(kongId))
            return 1;
        
        return playerData.kongLevels[kongId];
    }
    
    public void SetSelectedKong(string kongType)
    {
        if (playerData == null) return;
        
        playerData.selectedKongType = kongType;
        SavePlayerData();
    }
    
    public void SavePlayerData()
    {
        if (playerData == null) return;
        
        try
        {
            string jsonData = JsonUtility.ToJson(playerData);
            PlayerPrefs.SetString(saveKey, jsonData);
            PlayerPrefs.Save();
            
            Debug.Log("Player data saved successfully");
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to save player data: " + e.Message);
        }
    }
    
    public void LoadPlayerData()
    {
        try
        {
            if (PlayerPrefs.HasKey(saveKey))
            {
                string jsonData = PlayerPrefs.GetString(saveKey);
                playerData = JsonUtility.FromJson<PlayerData>(jsonData);
                
                Debug.Log("Player data loaded successfully");
            }
            else
            {
                playerData = new PlayerData();
                SavePlayerData();
                
                Debug.Log("Created new player data");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to load player data: " + e.Message);
            playerData = new PlayerData();
        }
    }
    
    public void ResetPlayerData()
    {
        playerData = new PlayerData();
        SavePlayerData();
        
        // Trigger events to update UI
        OnBlueSerumChanged?.Invoke(playerData.blueSerum);
        OnOmegaCoinsChanged?.Invoke(playerData.omegaCoins);
        OnLevelChanged?.Invoke(playerData.currentLevel);
        
        Debug.Log("Player data reset");
    }
    
    // Getter methods for easy access
    public int GetBlueSerum() => playerData?.blueSerum ?? 0;
    public int GetOmegaCoins() => playerData?.omegaCoins ?? 0;
    public int GetCurrentLevel() => playerData?.currentLevel ?? 1;
    public int GetCurrentPlanet() => playerData?.currentPlanet ?? 1;
    public int GetHighestLevelUnlocked() => playerData?.highestLevelUnlocked ?? 1;
    public string GetSelectedKongType() => playerData?.selectedKongType ?? "Alpha";
    public List<string> GetUnlockedKongs() => playerData?.unlockedKongs ?? new List<string>();
}
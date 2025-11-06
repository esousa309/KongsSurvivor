using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

[System.Serializable]
public class LevelData
{
    public int levelNumber;
    public int planetNumber;
    public string sceneName;
    public bool isEliteBoss;
    public bool isPlanetBoss;
    public int requiredBlueSerum;
    public float difficultyMultiplier = 1.0f;
}

public class LevelManager : MonoBehaviour
{
    private static LevelManager instance;
    public static LevelManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<LevelManager>();
            }
            return instance;
        }
    }
    
    [Header("Level Configuration")]
    public List<LevelData> allLevels = new List<LevelData>();
    public LevelData currentLevelData;
    
    [Header("Scene Management")]
    public string mainMenuSceneName = "MainMenu";
    public string gameplaySceneName = "Gameplay";
    public bool useAsyncLoading = true;
    
    [Header("Loading Screen")]
    public GameObject loadingScreenPrefab;
    private GameObject activeLoadingScreen;
    
    [Header("Events")]
    public Action<int> OnLevelLoaded;
    public Action<float> OnLoadingProgress;
    
    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        
        // Initialize level data if empty
        if (allLevels.Count == 0)
        {
            GenerateDefaultLevelData();
        }
    }
    
    void Start()
    {
        // Get current level from PlayerDataManager
        if (PlayerDataManager.Instance != null)
        {
            int currentLevel = PlayerDataManager.Instance.GetCurrentLevel();
            LoadLevel(currentLevel);
        }
    }
    
    private void GenerateDefaultLevelData()
    {
        // Generate 500 levels (20 planets x 25 levels each)
        for (int i = 1; i <= 500; i++)
        {
            LevelData levelData = new LevelData();
            levelData.levelNumber = i;
            levelData.planetNumber = ((i - 1) / 25) + 1;
            levelData.sceneName = gameplaySceneName;
            
            // Every 5th level is Elite Boss (except Planet Boss levels)
            levelData.isEliteBoss = (i % 5 == 0) && (i % 25 != 0);
            
            // Every 25th level is Planet Boss
            levelData.isPlanetBoss = (i % 25 == 0);
            
            // Blue Serum requirement increases with level
            levelData.requiredBlueSerum = 0; // First level is free, others may require serum
            if (i > 1)
            {
                levelData.requiredBlueSerum = (i - 1) * 10;
            }
            
            // Difficulty increases with level
            levelData.difficultyMultiplier = 1.0f + (i * 0.02f);
            
            allLevels.Add(levelData);
        }
    }
    
    public void LoadLevel(int levelNumber)
    {
        if (levelNumber < 1 || levelNumber > allLevels.Count)
        {
            Debug.LogError($"Invalid level number: {levelNumber}");
            return;
        }
        
        currentLevelData = allLevels[levelNumber - 1];
        
        // Check if player has enough Blue Serum
        if (PlayerDataManager.Instance != null)
        {
            if (currentLevelData.requiredBlueSerum > 0)
            {
                int playerBlueSerum = PlayerDataManager.Instance.GetBlueSerum();
                if (playerBlueSerum < currentLevelData.requiredBlueSerum)
                {
                    Debug.LogWarning($"Not enough Blue Serum! Need {currentLevelData.requiredBlueSerum}, have {playerBlueSerum}");
                    return;
                }
            }
        }
        
        // Load the level
        if (useAsyncLoading)
        {
            StartCoroutine(LoadLevelAsync(currentLevelData.sceneName));
        }
        else
        {
            SceneManager.LoadScene(currentLevelData.sceneName);
            OnLevelLoaded?.Invoke(levelNumber);
        }
    }
    
    public void LoadNextLevel()
    {
        if (currentLevelData != null)
        {
            int nextLevel = currentLevelData.levelNumber + 1;
            
            // Check if next level exists
            if (nextLevel <= allLevels.Count)
            {
                // Save progress
                if (PlayerDataManager.Instance != null)
                {
                    PlayerDataManager.Instance.SetCurrentLevel(nextLevel);
                }
                
                LoadLevel(nextLevel);
            }
            else
            {
                Debug.Log("Congratulations! You've completed all levels!");
                LoadMainMenu();
            }
        }
    }
    
    public void ReloadCurrentLevel()
    {
        if (currentLevelData != null)
        {
            LoadLevel(currentLevelData.levelNumber);
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
    
    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        
        if (useAsyncLoading)
        {
            StartCoroutine(LoadLevelAsync(mainMenuSceneName));
        }
        else
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
    
    private IEnumerator LoadLevelAsync(string sceneName)
    {
        // Show loading screen
        ShowLoadingScreen();
        
        // Start async loading
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;
        
        float progress = 0f;
        
        while (!asyncLoad.isDone)
        {
            // Calculate progress (Unity stops at 0.9 until allowSceneActivation is true)
            progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            
            // Update loading progress
            OnLoadingProgress?.Invoke(progress);
            
            // Update loading screen if it exists
            if (activeLoadingScreen != null)
            {
                // Update loading bar or text here if your loading screen has them
            }
            
            // Check if loading is complete
            if (asyncLoad.progress >= 0.9f)
            {
                // Small delay for better UX
                yield return new WaitForSecondsRealtime(0.5f);
                
                // Activate the scene
                asyncLoad.allowSceneActivation = true;
            }
            
            yield return null;
        }
        
        // Hide loading screen
        HideLoadingScreen();
        
        // Trigger level loaded event
        if (currentLevelData != null)
        {
            OnLevelLoaded?.Invoke(currentLevelData.levelNumber);
        }
    }
    
    private void ShowLoadingScreen()
    {
        if (loadingScreenPrefab != null && activeLoadingScreen == null)
        {
            activeLoadingScreen = Instantiate(loadingScreenPrefab);
            DontDestroyOnLoad(activeLoadingScreen);
        }
    }
    
    private void HideLoadingScreen()
    {
        if (activeLoadingScreen != null)
        {
            Destroy(activeLoadingScreen);
            activeLoadingScreen = null;
        }
    }
    
    public LevelData GetLevelData(int levelNumber)
    {
        if (levelNumber < 1 || levelNumber > allLevels.Count)
            return null;
        
        return allLevels[levelNumber - 1];
    }
    
    public bool IsLevelUnlocked(int levelNumber)
    {
        if (PlayerDataManager.Instance == null)
            return levelNumber == 1;
        
        return levelNumber <= PlayerDataManager.Instance.GetHighestLevelUnlocked();
    }
    
    public int GetCurrentPlanet()
    {
        if (currentLevelData != null)
            return currentLevelData.planetNumber;
        
        return 1;
    }
    
    public string GetPlanetName(int planetNumber)
    {
        string[] planetNames = {
            "Kong Prime", "Banana Nebula", "Gorilla Galaxy", "Primate Paradise",
            "Ape Asteroid", "Simian System", "Monkey Moon", "Orangutan Orbit",
            "Baboon Belt", "Chimp Cluster", "Lemur Land", "Mandrill Mars",
            "Gibbon Globe", "Macaque Matrix", "Tamarin Terra", "Bonobo Base",
            "Silverback Station", "Howler Haven", "Capuchin Core", "Kong Omega"
        };
        
        if (planetNumber >= 1 && planetNumber <= planetNames.Length)
            return planetNames[planetNumber - 1];
        
        return $"Planet {planetNumber}";
    }
}
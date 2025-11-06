using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public enum GameState
{
    MainMenu,
    Playing,
    Paused,
    LevelComplete,
    GameOver
}

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
                if (instance == null)
                {
                    GameObject go = new GameObject("GameManager");
                    instance = go.AddComponent<GameManager>();
                }
            }
            return instance;
        }
    }
    
    [Header("Game State")]
    public GameState currentGameState = GameState.MainMenu;
    
    [Header("Level Information")]
    public int currentLevelNumber = 1;
    public int currentPlanetNumber = 1;
    public bool isEliteBossLevel = false;
    public bool isPlanetBossLevel = false;
    
    [Header("Game Settings")]
    public float gameSpeed = 1.0f;
    public bool soundEnabled = true;
    public bool musicEnabled = true;
    
    [Header("References")]
    public EndLevelRewardManager endLevelRewardManager;
    public PlayerDataManager playerDataManager;
    public LevelManager levelManager;
    
    [Header("Events")]
    public Action<GameState> OnGameStateChanged;
    public Action OnLevelStarted;
    public Action OnLevelCompleted;
    public Action OnGameOver;
    
    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        DontDestroyOnLoad(gameObject);
        
        InitializeManagers();
    }
    
    void Start()
    {
        // Load saved settings
        LoadGameSettings();
    }
    
    private void InitializeManagers()
    {
        // Find managers if not assigned
        if (endLevelRewardManager == null)
            endLevelRewardManager = FindObjectOfType<EndLevelRewardManager>();
        
        if (playerDataManager == null)
            playerDataManager = PlayerDataManager.Instance;
        
        if (levelManager == null)
            levelManager = FindObjectOfType<LevelManager>();
    }
    
    public void SetGameState(GameState newState)
    {
        if (currentGameState == newState)
            return;
        
        GameState previousState = currentGameState;
        currentGameState = newState;
        
        switch (newState)
        {
            case GameState.MainMenu:
                Time.timeScale = 1f;
                break;
                
            case GameState.Playing:
                Time.timeScale = gameSpeed;
                if (previousState == GameState.MainMenu)
                {
                    OnLevelStarted?.Invoke();
                }
                break;
                
            case GameState.Paused:
                Time.timeScale = 0f;
                break;
                
            case GameState.LevelComplete:
                HandleLevelComplete();
                break;
                
            case GameState.GameOver:
                Time.timeScale = 0f;
                OnGameOver?.Invoke();
                break;
        }
        
        OnGameStateChanged?.Invoke(newState);
    }
    
    public void StartGame()
    {
        // Load player data
        if (playerDataManager != null)
        {
            currentLevelNumber = playerDataManager.GetCurrentLevel();
            currentPlanetNumber = playerDataManager.GetCurrentPlanet();
        }
        
        // Determine level type
        UpdateLevelType();
        
        // Start the game
        SetGameState(GameState.Playing);
    }
    
    public void PauseGame()
    {
        if (currentGameState == GameState.Playing)
        {
            SetGameState(GameState.Paused);
        }
    }
    
    public void ResumeGame()
    {
        if (currentGameState == GameState.Paused)
        {
            SetGameState(GameState.Playing);
        }
    }
    
    public void CompleteLevel()
    {
        SetGameState(GameState.LevelComplete);
    }
    
    private void HandleLevelComplete()
    {
        // Save progress
        if (playerDataManager != null)
        {
            playerDataManager.SetCurrentLevel(currentLevelNumber + 1);
        }
        
        // Show reward popup
        if (endLevelRewardManager != null)
        {
            endLevelRewardManager.ShowRewardPopup(currentLevelNumber, isEliteBossLevel, isPlanetBossLevel);
        }
        
        OnLevelCompleted?.Invoke();
    }
    
    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        currentGameState = GameState.MainMenu;
        SceneManager.LoadScene("MainMenu");
    }
    
    private void UpdateLevelType()
    {
        // Every 5th level is an Elite Boss (5, 10, 15, 20, etc.)
        isEliteBossLevel = (currentLevelNumber % 5 == 0) && (currentLevelNumber % 25 != 0);
        
        // Every 25th level is a Planet Boss (25, 50, 75, etc.)
        isPlanetBossLevel = (currentLevelNumber % 25 == 0);
        
        // Update planet number (25 levels per planet)
        currentPlanetNumber = ((currentLevelNumber - 1) / 25) + 1;
    }
    
    public void SetGameSpeed(float speed)
    {
        gameSpeed = Mathf.Clamp(speed, 0.5f, 3f);
        
        if (currentGameState == GameState.Playing)
        {
            Time.timeScale = gameSpeed;
        }
    }
    
    public void ToggleSound()
    {
        soundEnabled = !soundEnabled;
        SaveGameSettings();
        
        // Update audio sources
        AudioListener.volume = soundEnabled ? 1f : 0f;
    }
    
    public void ToggleMusic()
    {
        musicEnabled = !musicEnabled;
        SaveGameSettings();
        
        // Update music sources if you have a music manager
        // MusicManager.Instance?.SetEnabled(musicEnabled);
    }
    
    private void SaveGameSettings()
    {
        PlayerPrefs.SetFloat("GameSpeed", gameSpeed);
        PlayerPrefs.SetInt("SoundEnabled", soundEnabled ? 1 : 0);
        PlayerPrefs.SetInt("MusicEnabled", musicEnabled ? 1 : 0);
        PlayerPrefs.Save();
    }
    
    private void LoadGameSettings()
    {
        gameSpeed = PlayerPrefs.GetFloat("GameSpeed", 1.0f);
        soundEnabled = PlayerPrefs.GetInt("SoundEnabled", 1) == 1;
        musicEnabled = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
        
        // Apply loaded settings
        AudioListener.volume = soundEnabled ? 1f : 0f;
    }
    
    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && currentGameState == GameState.Playing)
        {
            PauseGame();
        }
    }
    
    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && currentGameState == GameState.Playing)
        {
            PauseGame();
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EndLevelRewardManager : MonoBehaviour
{
    [Header("Reward Popup UI")]
    public GameObject rewardPopupPanel;
    public TextMeshProUGUI levelCompleteText;
    public TextMeshProUGUI blueSerumRewardText;
    public TextMeshProUGUI omegaCoinsRewardText;
    public Button continueButton;
    public Button retryButton;
    
    [Header("Reward Values")]
    public int baseBlueSerumReward = 100;
    public int baseOmegaCoinsReward = 10;
    public float eliteBossMultiplier = 2.0f;
    public float planetBossMultiplier = 5.0f;
    
    [Header("Animation")]
    public float popupAnimationDuration = 0.5f;
    public AnimationCurve popupScaleCurve;
    
    private GameManager gameManager;
    private PlayerDataManager playerDataManager;
    private LevelManager levelManager;
    
    void Start()
    {
        // Find managers
        gameManager = FindObjectOfType<GameManager>();
        playerDataManager = FindObjectOfType<PlayerDataManager>();
        levelManager = FindObjectOfType<LevelManager>();
        
        // Set up button listeners
        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinuePressed);
        
        if (retryButton != null)
            retryButton.onClick.AddListener(OnRetryPressed);
        
        // Hide popup initially
        if (rewardPopupPanel != null)
            rewardPopupPanel.SetActive(false);
    }
    
    public void ShowRewardPopup(int levelNumber, bool isEliteBoss, bool isPlanetBoss)
    {
        if (rewardPopupPanel == null)
        {
            Debug.LogError("Reward popup panel is not assigned!");
            return;
        }
        
        // Calculate rewards
        int blueSerumAmount = CalculateBlueSerumReward(isEliteBoss, isPlanetBoss);
        int omegaCoinsAmount = CalculateOmegaCoinsReward(isEliteBoss, isPlanetBoss);
        
        // Update UI texts
        if (levelCompleteText != null)
        {
            string levelType = isPlanetBoss ? "PLANET BOSS DEFEATED!" : 
                              isEliteBoss ? "ELITE BOSS DEFEATED!" : 
                              "LEVEL COMPLETE!";
            levelCompleteText.text = levelType;
        }
        
        if (blueSerumRewardText != null)
            blueSerumRewardText.text = "Blue Serum: +" + blueSerumAmount.ToString();
        
        if (omegaCoinsRewardText != null)
            omegaCoinsRewardText.text = "$OMEGA Coins: +" + omegaCoinsAmount.ToString();
        
        // Add rewards to player data
        if (playerDataManager != null)
        {
            playerDataManager.AddBlueSerum(blueSerumAmount);
            playerDataManager.AddOmegaCoins(omegaCoinsAmount);
        }
        
        // Show and animate popup
        StartCoroutine(ShowPopupAnimation());
    }
    
    private int CalculateBlueSerumReward(bool isEliteBoss, bool isPlanetBoss)
    {
        float multiplier = 1.0f;
        
        if (isPlanetBoss)
            multiplier = planetBossMultiplier;
        else if (isEliteBoss)
            multiplier = eliteBossMultiplier;
        
        return Mathf.RoundToInt(baseBlueSerumReward * multiplier);
    }
    
    private int CalculateOmegaCoinsReward(bool isEliteBoss, bool isPlanetBoss)
    {
        float multiplier = 1.0f;
        
        if (isPlanetBoss)
            multiplier = planetBossMultiplier;
        else if (isEliteBoss)
            multiplier = eliteBossMultiplier;
        
        return Mathf.RoundToInt(baseOmegaCoinsReward * multiplier);
    }
    
    private IEnumerator ShowPopupAnimation()
    {
        rewardPopupPanel.SetActive(true);
        
        // Pause game
        Time.timeScale = 0f;
        
        // Animate scale
        float elapsedTime = 0f;
        Vector3 startScale = Vector3.zero;
        Vector3 targetScale = Vector3.one;
        
        while (elapsedTime < popupAnimationDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = elapsedTime / popupAnimationDuration;
            
            if (popupScaleCurve != null && popupScaleCurve.length > 0)
                t = popupScaleCurve.Evaluate(t);
            
            rewardPopupPanel.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            
            yield return null;
        }
        
        rewardPopupPanel.transform.localScale = targetScale;
    }
    
    private void OnContinuePressed()
    {
        // Resume game
        Time.timeScale = 1f;
        
        // Hide popup
        if (rewardPopupPanel != null)
            rewardPopupPanel.SetActive(false);
        
        // Load next level
        if (levelManager != null)
            levelManager.LoadNextLevel();
    }
    
    private void OnRetryPressed()
    {
        // Resume game
        Time.timeScale = 1f;
        
        // Hide popup
        if (rewardPopupPanel != null)
            rewardPopupPanel.SetActive(false);
        
        // Reload current level
        if (levelManager != null)
            levelManager.ReloadCurrentLevel();
    }
    
    void OnDestroy()
    {
        // Clean up button listeners
        if (continueButton != null)
            continueButton.onClick.RemoveListener(OnContinuePressed);
        
        if (retryButton != null)
            retryButton.onClick.RemoveListener(OnRetryPressed);
    }
}

using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    public GameManager gameManager;
    public Text planetSublevelText;
    public Text modeOrTimerText;
    public Text omegaText;
    public GameObject levelCompletePanel;
    public Text levelCompleteText;
    public bool showBossLabel = true;

    void Start()
    {
        if (gameManager == null) gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null) gameManager.OnLevelWon += HandleLevelWon;
        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);
    }

    void OnDestroy()
    {
        if (gameManager != null) gameManager.OnLevelWon -= HandleLevelWon;
    }

    void Update()
    {
        if (gameManager == null) return;
        if (planetSublevelText != null) planetSublevelText.text = $"Planet {gameManager.PlanetIndex} — Lvl {gameManager.SubLevel}";
        if (modeOrTimerText != null)
            modeOrTimerText.text = (gameManager.IsBossLevel && showBossLabel) ? "BOSS" : $"Survive: {Mathf.CeilToInt(gameManager.TimeRemaining)}s";
        if (omegaText != null) omegaText.text = $"Ω {CurrencyManager.TotalOmega}";
    }

    void HandleLevelWon()
    {
        if (levelCompletePanel != null)
        {
            if (levelCompleteText != null) levelCompleteText.text = $"LEVEL COMPLETE  +Ω{gameManager.LastOmegaReward}";
            levelCompletePanel.SetActive(true);
            CancelInvoke(nameof(HideLevelComplete));
            Invoke(nameof(HideLevelComplete), 1.5f);
        }
    }

    void HideLevelComplete()
    {
        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);
    }
}

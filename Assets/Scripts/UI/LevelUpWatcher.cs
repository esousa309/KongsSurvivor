using System.Reflection;
using UnityEngine;

public class LevelUpWatcher : MonoBehaviour
{
    GameObject player;
    Component levelSystem;
    LevelUpUI ui;
    
    // NEW: Manual trigger instead of automatic detection
    private bool shouldShowUI = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        new GameObject("LevelUpWatcher", typeof(LevelUpWatcher));
    }

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        ui = new GameObject("LevelUpUI").AddComponent<LevelUpUI>();
        ui.Build();
        ui.Hide();
    }

    void Update()
    {
        if (player == null) player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
        if (ui != null && ui.player == null) ui.player = player;

        if (levelSystem == null)
        {
            levelSystem = player.GetComponent("LevelSystem");
            return;
        }

        // NEW: Only show UI when explicitly triggered
        if (shouldShowUI)
        {
            shouldShowUI = false;
            ui.WireHandlers();
            ui.Show();
        }
    }
    
    // NEW: Public method to trigger UI manually
    public void ShowLevelUpUI()
    {
        shouldShowUI = true;
    }

    // NEW: Static method for easy access from other scripts
    public static void TriggerLevelUpUI()
    {
        var watcher = FindObjectOfType<LevelUpWatcher>();
        if (watcher != null)
        {
            watcher.ShowLevelUpUI();
        }
    }
}
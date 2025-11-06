using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script helps identify common compiler errors in Unity projects
// Place it in your Scripts/Diagnostics folder

public class CompilerErrorDiagnostic : MonoBehaviour
{
    [Header("Diagnostic Information")]
    [TextArea(10, 20)]
    public string diagnosticReport = "Click 'Run Diagnostic' in the Inspector to check for issues.";
    
    void Start()
    {
        RunDiagnostic();
    }
    
    public void RunDiagnostic()
    {
        diagnosticReport = "=== Kong Survivors Diagnostic Report ===\n";
        diagnosticReport += $"Time: {System.DateTime.Now}\n\n";
        
        // Check for essential managers
        CheckManager<GameManager>("GameManager");
        CheckManager<PlayerDataManager>("PlayerDataManager");
        CheckManager<LevelManager>("LevelManager");
        CheckManager<EndLevelRewardManager>("EndLevelRewardManager");
        
        // Check for UI components
        CheckUIComponents();
        
        // Check for common prefab issues
        CheckPrefabs();
        
        // Check PlayerPrefs
        CheckPlayerPrefs();
        
        diagnosticReport += "\n=== Diagnostic Complete ===\n";
        diagnosticReport += "If you see any [ERROR] messages above, those need to be fixed.\n";
        diagnosticReport += "If all checks pass but you still have compiler errors:\n";
        diagnosticReport += "1. Check the Console window for specific error messages\n";
        diagnosticReport += "2. Look for scripts with 'using UnityEditor' (remove these)\n";
        diagnosticReport += "3. Check for missing semicolons or brackets\n";
        diagnosticReport += "4. Verify all script names match their class names\n";
        
        Debug.Log(diagnosticReport);
    }
    
    private void CheckManager<T>(string managerName) where T : MonoBehaviour
    {
        T manager = FindObjectOfType<T>();
        if (manager != null)
        {
            diagnosticReport += $"[OK] {managerName} found\n";
        }
        else
        {
            diagnosticReport += $"[WARNING] {managerName} not found in scene\n";
        }
    }
    
    private void CheckUIComponents()
    {
        diagnosticReport += "\n=== UI Components Check ===\n";
        
        // Check for Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            diagnosticReport += "[OK] Canvas found\n";
        }
        else
        {
            diagnosticReport += "[ERROR] No Canvas found - UI will not work\n";
        }
        
        // Check for EventSystem
        UnityEngine.EventSystems.EventSystem eventSystem = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
        if (eventSystem != null)
        {
            diagnosticReport += "[OK] EventSystem found\n";
        }
        else
        {
            diagnosticReport += "[ERROR] No EventSystem found - UI input will not work\n";
        }
    }
    
    private void CheckPrefabs()
    {
        diagnosticReport += "\n=== Prefab Check ===\n";
        
        // Check for reward popup
        GameObject rewardPopup = GameObject.Find("RewardPopupPanel");
        if (rewardPopup != null)
        {
            diagnosticReport += "[OK] Reward Popup Panel found\n";
            
            // Check for required UI components
            var texts = rewardPopup.GetComponentsInChildren<TMPro.TextMeshProUGUI>();
            var buttons = rewardPopup.GetComponentsInChildren<UnityEngine.UI.Button>();
            
            diagnosticReport += $"    - TextMeshPro components: {texts.Length}\n";
            diagnosticReport += $"    - Button components: {buttons.Length}\n";
            
            if (texts.Length < 3)
            {
                diagnosticReport += "    [WARNING] Expected at least 3 text components\n";
            }
            if (buttons.Length < 2)
            {
                diagnosticReport += "    [WARNING] Expected at least 2 button components\n";
            }
        }
        else
        {
            diagnosticReport += "[INFO] Reward Popup Panel not found (may be disabled)\n";
        }
    }
    
    private void CheckPlayerPrefs()
    {
        diagnosticReport += "\n=== PlayerPrefs Check ===\n";
        
        if (PlayerPrefs.HasKey("KongSurvivorsPlayerData"))
        {
            diagnosticReport += "[OK] Player data save found\n";
            string data = PlayerPrefs.GetString("KongSurvivorsPlayerData");
            diagnosticReport += $"    - Data length: {data.Length} characters\n";
        }
        else
        {
            diagnosticReport += "[INFO] No saved player data (normal for first run)\n";
        }
        
        if (PlayerPrefs.HasKey("GameSpeed"))
        {
            float speed = PlayerPrefs.GetFloat("GameSpeed");
            diagnosticReport += $"[OK] Game speed setting: {speed}\n";
        }
    }
    
    // Context menu option for easy access in Inspector
    [ContextMenu("Run Diagnostic")]
    void RunDiagnosticFromMenu()
    {
        RunDiagnostic();
    }
}
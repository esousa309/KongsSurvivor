using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    private static GameOverUI instance;
    private static float gameStartTime;
    
    [Header("UI References")]
    public GameObject panel;
    public Text titleText;
    public Text statsText;
    public Button restartButton;
    public Button quitButton;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        if (instance == null)
        {
            gameStartTime = Time.time;
            new GameObject("GameOverUIBootstrap", typeof(GameOverUIBootstrap));
        }
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[GameOverUI] Instance created");
            Hide();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public static void Show()
    {
        Debug.Log("[GameOverUI] SHOW called");
        
        if (Time.time - gameStartTime < 2f)
        {
            Debug.LogWarning("[GameOverUI] Ignoring Show() call - game just started!");
            return;
        }
        
        if (instance != null)
        {
            instance.ShowInternal();
        }
        else
        {
            Debug.LogError("[GameOverUI] Show() called but instance is NULL!");
        }
    }

    void ShowInternal()
    {
        Debug.Log("[GameOverUI] ShowInternal - Making panel visible");
        if (panel != null)
        {
            panel.SetActive(true);
            Debug.Log("[GameOverUI] Panel activated");
        }
        else
        {
            Debug.LogError("[GameOverUI] Panel is NULL!");
        }
        
        var gm = FindObjectOfType<GameManager>();
        if (gm != null && statsText != null)
        {
            int planet = gm.PlanetIndex;
            int sublevel = gm.SubLevel;
            int omega = CurrencyManager.TotalOmega;
            
            statsText.text = "Planet " + planet + " - Level " + sublevel + "\nTotal Omega: " + omega;
            Debug.Log("[GameOverUI] Stats updated");
        }
        
        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(Restart);
            Debug.Log("[GameOverUI] Restart button wired");
        }
        
        if (quitButton != null)
        {
            quitButton.onClick.RemoveAllListeners();
            quitButton.onClick.AddListener(Quit);
            Debug.Log("[GameOverUI] Quit button wired");
        }
    }

    void Hide()
    {
        Debug.Log("[GameOverUI] HIDE called");
        if (panel != null)
        {
            panel.SetActive(false);
            Debug.Log("[GameOverUI] Panel hidden");
        }
    }

    void Restart()
    {
        Debug.Log("========== RESTART BUTTON CLICKED ==========");
        Hide();
        Time.timeScale = 1f;
        gameStartTime = Time.time;
        Debug.Log("[GameOverUI] Time.timeScale reset to 1");
        
        var gm = FindObjectOfType<GameManager>();
        if (gm != null)
        {
            Debug.Log("[GameOverUI] Calling GameManager.RestartRun()");
            gm.RestartRun();
        }
        else
        {
            Debug.LogWarning("[GameOverUI] GameManager not found! Reloading scene...");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    void Quit()
    {
        Debug.Log("[GameOverUI] QUIT clicked");
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}

public class GameOverUIBootstrap : MonoBehaviour
{
    void Start()
    {
        var canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Invoke(nameof(TryAgain), 0.1f);
            return;
        }
        
        BuildGameOverUI(canvas);
        Destroy(gameObject);
    }
    
    void TryAgain()
    {
        Start();
    }

    void BuildGameOverUI(Canvas canvas)
    {
        Debug.Log("[GameOverUI] Building UI...");
        
        var panelGO = new GameObject("GameOverPanel", typeof(RectTransform));
        panelGO.transform.SetParent(canvas.transform, false);
        var rt = panelGO.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        
        var overlay = panelGO.AddComponent<Image>();
        overlay.color = new Color(0f, 0f, 0f, 0.85f);
        
        var container = CreateUI("Container", panelGO.transform);
        var crt = container.GetComponent<RectTransform>();
        crt.anchorMin = new Vector2(0.5f, 0.5f);
        crt.anchorMax = new Vector2(0.5f, 0.5f);
        crt.pivot = new Vector2(0.5f, 0.5f);
        crt.sizeDelta = new Vector2(500f, 350f);
        
        var containerBg = container.AddComponent<Image>();
        containerBg.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
        
        var titleGO = CreateUI("Title", container.transform);
        var trt = titleGO.GetComponent<RectTransform>();
        trt.anchorMin = new Vector2(0.5f, 1f);
        trt.anchorMax = new Vector2(0.5f, 1f);
        trt.pivot = new Vector2(0.5f, 1f);
        trt.anchoredPosition = new Vector2(0f, -20f);
        trt.sizeDelta = new Vector2(450f, 60f);
        
        var titleText = titleGO.AddComponent<Text>();
        Font f = null;
        try { f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch {}
        titleText.font = f;
        titleText.fontSize = 48;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.text = "GAME OVER";
        titleText.color = new Color(1f, 0.3f, 0.3f, 1f);
        
        var statsGO = CreateUI("Stats", container.transform);
        var srt = statsGO.GetComponent<RectTransform>();
        srt.anchorMin = new Vector2(0.5f, 0.5f);
        srt.anchorMax = new Vector2(0.5f, 0.5f);
        srt.pivot = new Vector2(0.5f, 0.5f);
        srt.anchoredPosition = new Vector2(0f, 20f);
        srt.sizeDelta = new Vector2(450f, 80f);
        
        var statsText = statsGO.AddComponent<Text>();
        statsText.font = f;
        statsText.fontSize = 24;
        statsText.alignment = TextAnchor.MiddleCenter;
        statsText.text = "Planet 0 - Level 1\nTotal Omega: 0";
        statsText.color = Color.white;
        
        var restartBtn = CreateButton("RestartButton", container.transform, "RESTART", new Vector2(0f, -80f));
        
        var quitBtn = CreateButton("QuitButton", container.transform, "QUIT", new Vector2(0f, -150f));
        
        var controller = panelGO.AddComponent<GameOverUI>();
        controller.panel = panelGO;
        controller.titleText = titleText;
        controller.statsText = statsText;
        controller.restartButton = restartBtn;
        controller.quitButton = quitBtn;
        
        panelGO.SetActive(false);
        
        Debug.Log("[GameOverUI] UI Built successfully and hidden");
    }
    
    GameObject CreateUI(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }
    
    Button CreateButton(string name, Transform parent, string label, Vector2 position)
    {
        var btnGO = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        btnGO.transform.SetParent(parent, false);
        var rt = btnGO.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;
        rt.sizeDelta = new Vector2(220f, 50f);
        
        var img = btnGO.GetComponent<Image>();
        img.color = new Color(0.3f, 0.3f, 0.3f, 1f);
        
        var btn = btnGO.GetComponent<Button>();
        var colors = btn.colors;
        colors.normalColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        colors.highlightedColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        colors.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
        btn.colors = colors;
        
        var textGO = CreateUI("Text", btnGO.transform);
        var trt = textGO.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;
        
        var text = textGO.AddComponent<Text>();
        Font f = null;
        try { f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch {}
        text.font = f;
        text.fontSize = 24;
        text.alignment = TextAnchor.MiddleCenter;
        text.text = label;
        text.color = Color.white;
        
        return btn;
    }
}
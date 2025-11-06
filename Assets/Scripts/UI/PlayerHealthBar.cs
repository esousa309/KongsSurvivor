using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    [Header("References")]
    public Image fillImage;
    public Text healthText;
    public GameObject rootPanel;
    
    [Header("Settings")]
    public float smoothSpeed = 5f;
    
    private Health playerHealth;
    private float targetFillAmount = 1f;
    private float currentFillAmount = 1f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        // Wait a frame for Canvas to exist, then create health bar
        new GameObject("PlayerHealthBarBootstrap", typeof(PlayerHealthBarBootstrap));
    }

    void Start()
    {
        FindPlayerHealth();
    }

    void Update()
    {
        if (playerHealth == null)
        {
            FindPlayerHealth();
            return;
        }
        
        // Calculate target fill
        float maxHP = Mathf.Max(1f, playerHealth.maxHealth);
        float curHP = Mathf.Clamp(playerHealth.current, 0f, maxHP);
        targetFillAmount = curHP / maxHP;
        
        // Smooth lerp
        currentFillAmount = Mathf.Lerp(currentFillAmount, targetFillAmount, Time.deltaTime * smoothSpeed);
        
        // Update UI
        if (fillImage != null) 
        {
            fillImage.fillAmount = currentFillAmount;
        }
        
        if (healthText != null)
        {
            healthText.text = $"{Mathf.CeilToInt(curHP)} / {Mathf.CeilToInt(maxHP)}";
        }
        
        // Show/hide based on whether player is alive
        if (rootPanel != null && rootPanel.activeSelf != (curHP > 0f))
        {
            rootPanel.SetActive(curHP > 0f);
        }
    }

    void FindPlayerHealth()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerHealth = player.GetComponent<Health>();
        }
    }
}

// Bootstrap helper to build the health bar UI
public class PlayerHealthBarBootstrap : MonoBehaviour
{
    void Start()
    {
        // Find or wait for canvas
        var canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Invoke(nameof(TryAgain), 0.1f);
            return;
        }
        
        BuildHealthBar(canvas);
        Destroy(gameObject);
    }
    
    void TryAgain()
    {
        Start();
    }

    void BuildHealthBar(Canvas canvas)
    {
        // Create root panel (bottom-left)
        var root = new GameObject("PlayerHealthBar", typeof(RectTransform));
        root.transform.SetParent(canvas.transform, false);
        var rt = root.GetComponent<RectTransform>();
        
        // Anchor bottom-left
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(0f, 0f);
        rt.pivot = new Vector2(0f, 0f);
        rt.anchoredPosition = new Vector2(10f, 10f);
        rt.sizeDelta = new Vector2(300f, 45f);
        
        var bg = root.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.5f);
        
        // Health label
        var labelGO = CreateUI("Label", root.transform);
        var lrt = labelGO.GetComponent<RectTransform>();
        lrt.anchorMin = new Vector2(0f, 1f);
        lrt.anchorMax = new Vector2(0f, 1f);
        lrt.pivot = new Vector2(0f, 1f);
        lrt.anchoredPosition = new Vector2(8f, -4f);
        lrt.sizeDelta = new Vector2(280f, 16f);
        
        var label = labelGO.AddComponent<Text>();
        Font f = null;
        try { f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch {}
        label.font = f;
        label.fontSize = 14;
        label.alignment = TextAnchor.UpperLeft;
        label.text = "HEALTH";
        label.color = new Color(1f, 1f, 1f, 0.8f);
        
        // Bar background
        var barBG = CreateUI("BarBG", root.transform);
        var bgrt = barBG.GetComponent<RectTransform>();
        bgrt.anchorMin = new Vector2(0f, 0f);
        bgrt.anchorMax = new Vector2(1f, 0f);
        bgrt.pivot = new Vector2(0f, 0f);
        bgrt.anchoredPosition = new Vector2(8f, 8f);
        bgrt.sizeDelta = new Vector2(-16f, 20f);
        
        var bgImg = barBG.AddComponent<Image>();
        bgImg.color = new Color(1f, 1f, 1f, 0.15f);
        
        // Bar fill
        var barFill = CreateUI("BarFill", barBG.transform);
        var fillrt = barFill.GetComponent<RectTransform>();
        fillrt.anchorMin = new Vector2(0f, 0f);
        fillrt.anchorMax = new Vector2(1f, 1f);
        fillrt.pivot = new Vector2(0.5f, 0.5f);
        fillrt.anchoredPosition = Vector2.zero;
        fillrt.sizeDelta = Vector2.zero;
        
        var fillImg = barFill.AddComponent<Image>();
        fillImg.color = new Color(0.2f, 0.85f, 0.3f, 0.9f); // Green
        fillImg.type = Image.Type.Filled;
        fillImg.fillMethod = Image.FillMethod.Horizontal;
        fillImg.fillOrigin = (int)Image.OriginHorizontal.Left;
        fillImg.fillAmount = 1f;
        
        // Health text (100/100)
        var textGO = CreateUI("HealthText", barBG.transform);
        var trt = textGO.GetComponent<RectTransform>();
        trt.anchorMin = new Vector2(0f, 0f);
        trt.anchorMax = new Vector2(1f, 1f);
        trt.pivot = new Vector2(0.5f, 0.5f);
        trt.anchoredPosition = Vector2.zero;
        trt.sizeDelta = Vector2.zero;
        
        var healthText = textGO.AddComponent<Text>();
        healthText.font = f;
        healthText.fontSize = 14;
        healthText.alignment = TextAnchor.MiddleCenter;
        healthText.text = "100 / 100";
        healthText.color = Color.white;
        
        // Add controller
        var controller = root.AddComponent<PlayerHealthBar>();
        controller.fillImage = fillImg;
        controller.healthText = healthText;
        controller.rootPanel = root;
        
        Debug.Log("[PlayerHealthBar] Created health bar UI");
    }
    
    GameObject CreateUI(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }
}

using System;
using UnityEngine;
using UnityEngine.UI;

public class BossBarSelfCanvas : MonoBehaviour
{
    public bool showAlwaysForDebug = false;
    public int sortingOrder = 500;

    private Canvas canvas;
    private GameObject root;
    private Image fillImage;
    private Text label;

    private GameManager gm;
    private Health trackedBoss;
    private bool bossLocked; // <- once we lock to a boss, don't retarget to minions
    private float rescanTimer;
    private const float RescanInterval = 0.5f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        if (FindObjectOfType<BossBarSelfCanvas>() == null)
            new GameObject("BossBarSelfCanvas", typeof(BossBarSelfCanvas));
    }

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        gm = FindObjectOfType<GameManager>();
        BuildUI();
    }

    void BuildUI()
    {
        var canvasGO = new GameObject("BossCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGO.transform.SetParent(transform, false);
        canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;

        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        root = new GameObject("BossBar", typeof(RectTransform));
        root.transform.SetParent(canvasGO.transform, false);
        var rt = root.GetComponent<RectTransform>();
        AnchorTopCenter(rt, new Vector2(540, 38), 70f);

        var bg = root.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.35f);

        var barBG = CreateUI("BarBG", root.transform);
        var bgrt = barBG.GetComponent<RectTransform>();
        bgrt.anchorMin = new Vector2(0f, 0.15f);
        bgrt.anchorMax = new Vector2(1f, 0.85f);
        bgrt.pivot = new Vector2(0.5f, 0.5f);
        bgrt.offsetMin = new Vector2(6f, 0f);
        bgrt.offsetMax = new Vector2(-6f, 0f);
        var bgImg = barBG.AddComponent<Image>();
        bgImg.color = new Color(1f,1f,1f,0.15f);

        var barFill = CreateUI("BarFill", barBG.transform);
        var fillrt = barFill.GetComponent<RectTransform>();
        fillrt.anchorMin = new Vector2(0f, 0f);
        fillrt.anchorMax = new Vector2(1f, 1f);
        fillrt.pivot = new Vector2(0.5f, 0.5f);
        fillImage = barFill.AddComponent<Image>();
        fillImage.color = new Color(0.85f, 0.2f, 0.2f, 0.9f);
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        fillImage.fillAmount = 1f;

        var labelGO = CreateUI("Label", root.transform);
        var lrt = labelGO.GetComponent<RectTransform>();
        lrt.anchorMin = new Vector2(0.5f, 0f);
        lrt.anchorMax = new Vector2(0.5f, 0f);
        lrt.pivot = new Vector2(0.5f, 1f);
        lrt.anchoredPosition = new Vector2(0f, -2f);
        lrt.sizeDelta = new Vector2(520f, 16f);

        label = labelGO.AddComponent<Text>();
        Font f = null;
        try { f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch {}
        label.font = f;
        label.fontSize = 16;
        label.alignment = TextAnchor.LowerCenter;
        label.text = "BOSS";
        label.color = Color.white;

        Show(false);
    }

    void Update()
    {
        if (gm == null) gm = FindObjectOfType<GameManager>();

        bool bossStage = showAlwaysForDebug || (gm != null && gm.IsBossLevel);
        if (!bossStage)
        {
            Show(false);
            trackedBoss = null;
            bossLocked = false;
            return;
        }

        // Lock onto the boss once, do not retarget to minions
        if (!bossLocked)
        {
            rescanTimer -= Time.deltaTime;
            if (rescanTimer <= 0f)
            {
                rescanTimer = RescanInterval;
                trackedBoss = FindLikelyBoss();
                if (trackedBoss != null) bossLocked = true;
            }
        }

        if (trackedBoss == null)
        {
            Show(false);
            return;
        }

        float maxHP = Mathf.Max(1f, GetMaxHealth(trackedBoss));
        float curHP = Mathf.Clamp(trackedBoss.current, 0f, maxHP);

        if (fillImage) fillImage.fillAmount = curHP / maxHP;
        if (label) label.text = $"BOSS  {Mathf.CeilToInt(curHP)}/{Mathf.CeilToInt(maxHP)}";

        // Hide when dead
        if (curHP <= 0f) { Show(false); return; }

        Show(true);
    }

    Health FindLikelyBoss()
    {
        // Prefer type name starting with "Boss"
        foreach (var mb in FindObjectsOfType<MonoBehaviour>())
        {
            if (mb == null) continue;
            var t = mb.GetType();
            if (t.Name.StartsWith("Boss", StringComparison.OrdinalIgnoreCase))
            {
                var h2 = mb.GetComponent<Health>();
                if (h2 != null) return h2;
                h2 = mb.GetComponentInChildren<Health>();
                if (h2 != null) return h2;
            }
        }
        // Fallback: highest-HP Enemy
        Health best = null; float bestScore = -1f;
        foreach (var e in FindObjectsOfType<Enemy>())
        {
            var h = e.GetComponent<Health>();
            if (h == null || h.current <= 0f) continue;
            float score = h.current + GetMaxHealth(h);
            if (score > bestScore) { bestScore = score; best = h; }
        }
        return best;
    }

    float GetMaxHealth(Health h)
    {
        try
        {
            var type = h.GetType();
            var field = type.GetField("maxHealth");
            if (field != null && field.FieldType == typeof(float)) return (float)field.GetValue(h);
            var prop = type.GetProperty("MaxHealth");
            if (prop != null && prop.PropertyType == typeof(float)) return (float)prop.GetValue(h, null);
        }
        catch { }
        return Mathf.Max(1f, h.current);
    }

    void Show(bool on)
    {
        if (root != null && root.activeSelf != on) root.SetActive(on);
    }

    GameObject CreateUI(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    void AnchorTopCenter(RectTransform rt, Vector2 size, float yOffset)
    {
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.sizeDelta = size;
        rt.anchoredPosition = new Vector2(0f, -yOffset);
    }
}

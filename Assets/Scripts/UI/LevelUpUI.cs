using UnityEngine;
using UnityEngine.UI;

public class LevelUpUI : MonoBehaviour
{
    Canvas canvas;
    GameObject panel;
    Button b1, b2, b3;
    Text t1, t2, t3, title;

    public GameObject player;

    public void Build()
    {
        var canvasGO = new GameObject("LevelUpCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        DontDestroyOnLoad(canvasGO);

        panel = new GameObject("Panel", typeof(RectTransform));
        panel.transform.SetParent(canvasGO.transform, false);
        var rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(680, 260);
        var bg = panel.AddComponent<Image>();
        bg.color = new Color(0,0,0,0.65f);

        var titleGO = MakeText(panel.transform, "Choose an upgrade", 28, TextAnchor.UpperCenter);
        title = titleGO.GetComponent<Text>();
        var trt = titleGO.GetComponent<RectTransform>();
        trt.anchoredPosition = new Vector2(0, -14);
        trt.sizeDelta = new Vector2(650, 40);

        b1 = MakeButton(" +Magnet Radius (+1.5) ", out t1, panel.transform, new Vector2(-210, -80));
        b2 = MakeButton(" +Max HP (+10) ",        out t2, panel.transform, new Vector2(  0 , -80));
        b3 = MakeButton(" +Move Speed (+10%) ",   out t3, panel.transform, new Vector2( 210, -80));

        Hide();
    }

    GameObject MakeText(Transform parent, string text, int size, TextAnchor anchor)
    {
        var go = new GameObject("Text", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var r = go.GetComponent<RectTransform>();
        r.anchorMin = r.anchorMax = new Vector2(0.5f, 1f);
        r.pivot = new Vector2(0.5f, 1f);
        var txt = go.AddComponent<Text>();
        Font f = null;
        try { f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch {}
        txt.font = f;
        txt.fontSize = size;
        txt.alignment = anchor;
        txt.text = text;
        txt.color = Color.white;
        return go;
    }

    Button MakeButton(string label, out Text labelRef, Transform parent, Vector2 anchored)
    {
        var go = new GameObject("Button", typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var r = go.GetComponent<RectTransform>();
        r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
        r.pivot = new Vector2(0.5f, 0.5f);
        r.sizeDelta = new Vector2(200, 110);
        r.anchoredPosition = anchored;

        var img = go.GetComponent<Image>();
        img.color = new Color(1f,1f,1f,0.08f);

        var textGO = MakeText(go.transform, label, 18, TextAnchor.MiddleCenter);
        var tr = textGO.GetComponent<RectTransform>();
        tr.anchorMin = tr.anchorMax = new Vector2(0.5f, 0.5f);
        tr.pivot = new Vector2(0.5f, 0.5f);
        tr.sizeDelta = new Vector2(190, 90);
        labelRef = textGO.GetComponent<Text>();

        var btn = go.GetComponent<Button>();
        var colors = btn.colors; colors.highlightedColor = new Color(1f,1f,1f,0.2f); colors.pressedColor = new Color(1f,1f,1f,0.3f); btn.colors = colors;
        return btn;
    }

    public void Show()
    {
        panel.SetActive(true);
        Time.timeScale = 0f;
    }
    
    public void Hide()
    {
        panel.SetActive(false);
        Time.timeScale = 1f;
        
        // NEW: Tell GameManager that upgrade was selected so next level can start
        NotifyGameManagerUpgradeSelected();
    }

    public void WireHandlers()
    {
        b1.onClick.RemoveAllListeners();
        b1.onClick.AddListener(() =>
        {
            SimpleUpgrades.IncreaseMagnetRadius(player, 1.5f);
            Hide();
        });
        b2.onClick.RemoveAllListeners();
        b2.onClick.AddListener(() =>
        {
            SimpleUpgrades.IncreaseMaxHealth(player, 10f);
            Hide();
        });
        b3.onClick.RemoveAllListeners();
        b3.onClick.AddListener(() =>
        {
            SimpleUpgrades.IncreaseMoveSpeed(player, 0.10f);
            Hide();
        });
    }
    
    // NEW: Helper method to notify GameManager
    void NotifyGameManagerUpgradeSelected()
    {
        var gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.OnUpgradeSelected();
        }
    }
}
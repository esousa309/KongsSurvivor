
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public static class UIUtil
{
    public static GameObject CreateUIObject(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    public static Text AddText(GameObject go, int fontSize, TextAnchor align, string text, bool bestFit = false)
    {
        var txt = go.AddComponent<Text>();
        // Unity 2022+: Arial.ttf is no longer a valid built-in. Use LegacyRuntime.ttf.
        Font f = null;
        try { f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch {}
        txt.font = f;
        txt.fontSize = fontSize;
        txt.alignment = align;
        txt.text = text;
        txt.resizeTextForBestFit = bestFit;
        txt.color = Color.white;
        return txt;
    }

    public static Image AddPanel(GameObject go, Color color, float alpha)
    {
        var img = go.AddComponent<Image>();
        img.color = new Color(color.r, color.g, color.b, alpha);
        return img;
    }
}

public static class RectUtil
{
    public static void AnchorTopLeft(RectTransform rt, Vector2 size, Vector2 offset)
    {
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.sizeDelta = size;
        rt.anchoredPosition = new Vector2(offset.x, -offset.y);
    }

    public static void AnchorTopRight(RectTransform rt, Vector2 size, Vector2 offset)
    {
        rt.anchorMin = new Vector2(1, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(1, 1);
        rt.sizeDelta = size;
        rt.anchoredPosition = new Vector2(-offset.x, -offset.y);
    }

    public static void AnchorCenter(RectTransform rt, Vector2 size)
    {
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = Vector2.zero;
    }
}

public class AutoHUDBootstrapper
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void BuildHUDOnLoad()
    {
        // If a HUD already exists, do nothing.
        var existing = Object.FindObjectOfType<HUDController>();
        if (existing != null) return;

        // Find or create a GameManager
        var gm = Object.FindObjectOfType<GameManager>();

        // Ensure an EventSystem exists
        if (Object.FindObjectOfType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            Object.DontDestroyOnLoad(es); // safe to persist
        }

        // Create a Canvas
        var canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // Create TopLeft panel
        var topLeft = UIUtil.CreateUIObject("TopLeft", canvasGO.transform);
        var tlrt = topLeft.GetComponent<RectTransform>();
        RectUtil.AnchorTopLeft(tlrt, new Vector2(380, 70), new Vector2(10, 10));
        UIUtil.AddPanel(topLeft, Color.black, 0.25f);

        // Planet/Sublevel text
        var planetTxtGO = UIUtil.CreateUIObject("PlanetSublevelText", topLeft.transform);
        var plrt = planetTxtGO.GetComponent<RectTransform>();
        RectUtil.AnchorTopLeft(plrt, new Vector2(360, 34), new Vector2(10, 8));
        var planetTxt = UIUtil.AddText(planetTxtGO, 22, TextAnchor.UpperLeft, "Planet 0 — Lvl 1");

        // Mode/Timer text
        var modeTxtGO = UIUtil.CreateUIObject("ModeOrTimerText", topLeft.transform);
        var mtrt = modeTxtGO.GetComponent<RectTransform>();
        RectUtil.AnchorTopLeft(mtrt, new Vector2(360, 28), new Vector2(10, 38));
        var modeTxt = UIUtil.AddText(modeTxtGO, 18, TextAnchor.UpperLeft, "Survive: 90s");

        // Omega (top-right)
        var omegaTxtGO = UIUtil.CreateUIObject("OmegaText", canvasGO.transform);
        var ort = omegaTxtGO.GetComponent<RectTransform>();
        RectUtil.AnchorTopRight(ort, new Vector2(220, 34), new Vector2(10, 10));
        var omegaTxt = UIUtil.AddText(omegaTxtGO, 22, TextAnchor.UpperRight, "Ω 0");

        // Level Complete panel
        var lcPanelGO = UIUtil.CreateUIObject("LevelCompletePanel", canvasGO.transform);
        var lcrt = lcPanelGO.GetComponent<RectTransform>();
        RectUtil.AnchorCenter(lcrt, new Vector2(600, 120));
        var panelImg = UIUtil.AddPanel(lcPanelGO, Color.black, 0.4f);

        var lcTextGO = UIUtil.CreateUIObject("LevelCompleteText", lcPanelGO.transform);
        var lctrt = lcTextGO.GetComponent<RectTransform>();
        RectUtil.AnchorCenter(lctrt, new Vector2(560, 80));
        var lcText = UIUtil.AddText(lcTextGO, 36, TextAnchor.MiddleCenter, "LEVEL COMPLETE +Ω100", true);

        // Add controller & wire references
        var hud = canvasGO.AddComponent<HUDController>();
        hud.gameManager = gm;
        hud.planetSublevelText = planetTxt;
        hud.modeOrTimerText = modeTxt;
        hud.omegaText = omegaTxt;
        hud.levelCompletePanel = lcPanelGO;
        hud.levelCompleteText = lcText;

        // Hide banner by default
        lcPanelGO.SetActive(false);

        Debug.Log("[AutoHUDBootstrapper] Canvas HUD created automatically.");
    }
}

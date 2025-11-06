using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class AutoBossBarWaiter : MonoBehaviour
{
    void Update()
    {
        // Wait until a Canvas exists (AutoHUDBootstrapper might create it this frame)
        var canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;

        // Build once then destroy this helper
        BuildBossBar(canvas);
        Destroy(gameObject);
    }

    void BuildBossBar(Canvas canvas)
    {
        // Root panel
        var root = new GameObject("BossBar", typeof(RectTransform));
        root.transform.SetParent(canvas.transform, false);
        var rt = root.GetComponent<RectTransform>();
        AnchorTopCenter(rt, new Vector2(540, 38), 70f);

        var bg = root.AddComponent<Image>();
        bg.color = new Color(0,0,0,0.35f);

        // Bar background
        var barBG = CreateUI("BarBG", root.transform);
        var bgrt = barBG.GetComponent<RectTransform>();
        bgrt.anchorMin = new Vector2(0f, 0.15f);
        bgrt.anchorMax = new Vector2(1f, 0.85f);
        bgrt.pivot = new Vector2(0.5f, 0.5f);
        bgrt.offsetMin = new Vector2(6f, 0f);
        bgrt.offsetMax = new Vector2(-6f, 0f);
        var bgImg = barBG.AddComponent<Image>();
        bgImg.color = new Color(1f,1f,1f,0.15f);

        // Bar fill
        var barFill = CreateUI("BarFill", barBG.transform);
        var fillrt = barFill.GetComponent<RectTransform>();
        fillrt.anchorMin = new Vector2(0f, 0f);
        fillrt.anchorMax = new Vector2(1f, 1f);
        fillrt.pivot = new Vector2(0.5f, 0.5f);
        var fillImg = barFill.AddComponent<Image>();
        fillImg.color = new Color(0.85f, 0.2f, 0.2f, 0.9f);
        fillImg.type = Image.Type.Filled;
        fillImg.fillMethod = Image.FillMethod.Horizontal;
        fillImg.fillOrigin = (int)Image.OriginHorizontal.Left;
        fillImg.fillAmount = 1f;

        // Label
        var labelGO = CreateUI("Label", root.transform);
        var lrt = labelGO.GetComponent<RectTransform>();
        lrt.anchorMin = new Vector2(0.5f, 0f);
        lrt.anchorMax = new Vector2(0.5f, 0f);
        lrt.pivot = new Vector2(0.5f, 1f);
        lrt.anchoredPosition = new Vector2(0f, -2f);
        lrt.sizeDelta = new Vector2(520f, 16f);

        var label = labelGO.AddComponent<Text>();
        Font f = null;
        try { f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch {}
        label.font = f;
        label.fontSize = 16;
        label.alignment = TextAnchor.LowerCenter;
        label.text = "BOSS";
        label.color = Color.white;

        // Controller
        var ctrl = root.AddComponent<BossBarController>();
        ctrl.fillImage = fillImg;
        ctrl.label = label;
        ctrl.rootPanel = root;

        Debug.Log("[AutoBossBar] Boss bar created (waited for Canvas).");
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

public static class AutoBossBarBootstrapperV2
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void EnsureBossBar()
    {
        if (Object.FindObjectOfType<BossBarController>() != null) return;
        // Spawn a tiny helper that waits until a Canvas exists, then builds the bar
        new GameObject("AutoBossBarWaiter", typeof(AutoBossBarWaiter));
    }
}
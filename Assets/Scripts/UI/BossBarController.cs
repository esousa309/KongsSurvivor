using UnityEngine;
using UnityEngine.UI;

public class BossBarController : MonoBehaviour
{
    [Header("Refs")]
    public GameManager gameManager;
    public Image fillImage;           // UI Image with Fill method Horizontal
    public Text label;                // "BOSS"
    public GameObject rootPanel;      // whole bar panel

    [Header("Scan Settings")]
    public float rescanInterval = 0.5f;

    private Health trackedBoss;
    private float rescanTimer;

    void Start()
    {
        if (gameManager == null) gameManager = FindObjectOfType<GameManager>();
        Show(false);
    }

    void Update()
    {
        if (gameManager == null || !gameManager.IsBossLevel)
        {
            Show(false);
            trackedBoss = null;
            return;
        }

        if (trackedBoss == null || trackedBoss.current <= 0f || !trackedBoss.gameObject.activeInHierarchy)
        {
            rescanTimer -= Time.deltaTime;
            if (rescanTimer <= 0f)
            {
                rescanTimer = rescanInterval;
                trackedBoss = FindLikelyBoss();
            }
        }

        if (trackedBoss == null)
        {
            Show(false);
            return;
        }

        float maxHP = Mathf.Max(1f, GetMaxHealth(trackedBoss));
        float curHP = Mathf.Clamp(trackedBoss.current, 0f, maxHP);
        float pct = curHP / maxHP;

        if (fillImage != null) fillImage.fillAmount = pct;
        if (label != null) label.text = $"BOSS  {Mathf.CeilToInt(curHP)}/{Mathf.CeilToInt(maxHP)}";

        Show(true);
    }

    Health FindLikelyBoss()
    {
        var enemies = FindObjectsOfType<Enemy>();
        Health best = null;
        float bestScore = -1f;
        foreach (var e in enemies)
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
        if (rootPanel != null && rootPanel.activeSelf != on) rootPanel.SetActive(on);
    }
}
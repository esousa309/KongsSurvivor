
using System;
using System.Reflection;
using UnityEngine;

public static class SimpleUpgrades
{
    public static bool IncreaseMagnetRadius(GameObject player, float addRadius)
    {
        if (player == null) return false;
        var mag = player.GetComponent<XpMagnet>();
        if (mag == null) return false;
        mag.radius += addRadius;
#if UNITY_EDITOR
        Debug.Log($"[Upgrade] Magnet radius +{addRadius} → {mag.radius}");
#endif
        return true;
    }

    public static bool IncreaseMaxHealth(GameObject player, float addHp)
    {
        if (player == null) return false;
        var h = player.GetComponent<Health>();
        if (h == null) return false;

        var type = h.GetType();
        var maxField = type.GetField("maxHealth", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var maxProp  = type.GetProperty("MaxHealth", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        float currentMax = 0f;
        bool hasMax = false;

        if (maxField != null && maxField.FieldType == typeof(float))
        {
            currentMax = (float)maxField.GetValue(h);
            maxField.SetValue(h, currentMax + addHp);
            hasMax = true;
        }
        else if (maxProp != null && maxProp.PropertyType == typeof(float) && maxProp.CanRead && maxProp.CanWrite)
        {
            currentMax = (float)maxProp.GetValue(h, null);
            maxProp.SetValue(h, currentMax + addHp, null);
            hasMax = true;
        }

        var curField = type.GetField("current", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var curProp  = type.GetProperty("Current", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (curField != null && curField.FieldType == typeof(float))
        {
            float cur = (float)curField.GetValue(h);
            cur += addHp;
            if (hasMax) cur = Mathf.Min(cur, currentMax + addHp);
            curField.SetValue(h, cur);
        }
        else if (curProp != null && curProp.PropertyType == typeof(float) && curProp.CanRead && curProp.CanWrite)
        {
            float cur = (float)curProp.GetValue(h, null);
            cur += addHp;
            if (hasMax) cur = Mathf.Min(cur, currentMax + addHp);
            curProp.SetValue(h, cur, null);
        }

#if UNITY_EDITOR
        Debug.Log($"[Upgrade] Max HP +{addHp}");
#endif
        return true;
    }

    public static bool IncreaseMoveSpeed(GameObject player, float percent)
    {
        if (player == null) return false;
        var comps = player.GetComponents<MonoBehaviour>();
        foreach (var c in comps)
        {
            if (c == null) continue;
            var t = c.GetType();
            var f = t.GetField("moveSpeed", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (f != null && f.FieldType == typeof(float))
            {
                float v = (float)f.GetValue(c);
                v *= (1f + percent);
                f.SetValue(c, v);
#if UNITY_EDITOR
                Debug.Log($"[Upgrade] {t.Name}.moveSpeed x{1f+percent:F2}");
#endif
                return true;
            }
            var p = t.GetProperty("moveSpeed", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (p != null && p.PropertyType == typeof(float) && p.CanRead && p.CanWrite)
            {
                float v = (float)p.GetValue(c, null);
                v *= (1f + percent);
                p.SetValue(c, v, null);
#if UNITY_EDITOR
                Debug.Log($"[Upgrade] {t.Name}.moveSpeed x{1f+percent:F2}");
#endif
                return true;
            }
        }
        return false;
    }
}

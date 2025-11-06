using UnityEngine;

public static class ComponentNameUtility
{
    public static bool HasComponentNameLike(this GameObject go, string lowercaseSubstring)
    {
        foreach (var c in go.GetComponents<Component>())
        {
            if (!c) continue;
            var n = c.GetType().Name.ToLowerInvariant();
            if (n.Contains(lowercaseSubstring)) return true;
        }
        return false;
    }
}

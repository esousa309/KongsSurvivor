using UnityEngine;

public static class AuthoringBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Install()
    {
        Ensure<VisualAuthoringService>("VisualAuthoringService");
        Ensure<EnemyAutoClampService>("EnemyAutoClampService");
        Ensure<ProjectileAutoClampService>("ProjectileAutoClampService");
        Ensure<ProjectileVisibilityService>("ProjectileVisibilityService");
        Ensure<OrbAutoClampService>("OrbAutoClampService"); // handles the stuck XP orb
    }

    static T Ensure<T>(string goName) where T : Component
    {
        var existing = Object.FindObjectOfType<T>();
        if (existing) return existing;

        var go = new GameObject(goName);
        Object.DontDestroyOnLoad(go);
        return go.AddComponent<T>();
    }
}

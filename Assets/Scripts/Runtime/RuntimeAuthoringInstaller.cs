using UnityEngine;

[DefaultExecutionOrder(-500)]
public class RuntimeAuthoringInstaller : MonoBehaviour
{
    void Awake()
    {
        Ensure<VisualAuthoringService>("VisualAuthoringService");
        Ensure<EnemyAutoClampService>("EnemyAutoClampService");
        Ensure<ProjectileAutoClampService>("ProjectileAutoClampService");
        Ensure<ProjectileVisibilityService>("ProjectileVisibilityService");
        Ensure<OrbAutoClampService>("OrbAutoClampService");   // <-- this kills the stuck XP-orb problem
    }

    static T Ensure<T>(string goName) where T : Component
    {
        var found = FindObjectOfType<T>();
        if (found) return found;

        var go = new GameObject(goName);
        return go.AddComponent<T>();
    }
}

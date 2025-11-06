
// Assets/Scripts/Enemies/AutoAttachEnemyFlash.cs
using UnityEngine;

public class AutoAttachEnemyFlash : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        // Create a tiny manager that periodically ensures new enemies get the flash
        new GameObject("AutoAttachEnemyFlash_Manager", typeof(AutoAttachEnemyFlash)).hideFlags = HideFlags.HideInHierarchy;
    }

    float scanInterval = 0.5f;
    float t;

    void Update()
    {
        t -= Time.deltaTime;
        if (t > 0f) return;
        t = scanInterval;

        var enemies = FindObjectsOfType<Enemy>();
        foreach (var e in enemies)
        {
            if (e.GetComponent<EnemyDamageFlash>() == null && e.GetComponent<SpriteRenderer>() != null)
            {
                e.gameObject.AddComponent<EnemyDamageFlash>();
            }
        }
    }
}

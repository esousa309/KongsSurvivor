
// Assets/Scripts/Player/XpOrbSafety.cs  (new helper - safe to keep)
using UnityEngine;

public class XpOrbSafety : MonoBehaviour
{
    void Awake()
    {
        // If someone accidentally made this orb a child of an Enemy at runtime,
        // detach so it doesn't get deleted with the parent.
        var parent = transform.parent;
        if (parent != null && parent.GetComponentInParent<Enemy>() != null)
        {
            transform.SetParent(null, true);
        }
    }
}

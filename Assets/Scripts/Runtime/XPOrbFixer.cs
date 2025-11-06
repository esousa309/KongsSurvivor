using UnityEngine;

/// <summary>
/// Keeps XP orbs sane while the scene loads / recompiles:
/// - Avoids referencing missing PlayerLevel-type singletons
/// - Clamps orb Y to player/ground height to prevent “floating mid-line” bugs
/// - Works even if the Player is spawned later
/// </summary>
[DisallowMultipleComponent]
public sealed class XPOrbFixer : MonoBehaviour
{
    [Tooltip("Optional override. If not set, will look for a GameObject tagged 'Player'.")]
    [SerializeField] private Transform player;

    [Tooltip("If true, the orb's Y will be matched to the player's Y each frame.")]
    [SerializeField] private bool matchPlayerY = true;

    [Tooltip("Optional ground Y to clamp to when player is missing.")]
    [SerializeField] private float fallbackGroundY = 0f;

    void Awake()
    {
        // Try to find the player if not wired in the prefab/scene
        if (!player)
        {
            var pObj = GameObject.FindWithTag("Player");
            if (pObj) player = pObj.transform;
        }
    }

    void LateUpdate()
    {
        // If the only thing this script used to do was consult PlayerLevel,
        // keep the orb stable on the plane instead of erroring out.
        if (!matchPlayerY) return;

        var pos = transform.position;
        if (player)
            pos.y = player.position.y;      // keep orb on the same height as the player
        else
            pos.y = fallbackGroundY;        // keep orb on ground until player exists

        transform.position = pos;
    }

    /// <summary>
    /// Helper for external spawners to inform the fixer about the player transform.
    /// </summary>
    public void SetPlayer(Transform playerTransform) => player = playerTransform;
}

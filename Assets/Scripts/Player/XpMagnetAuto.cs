using UnityEngine;

public static class XpMagnetAuto
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AddToPlayer()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        var magnet = player.GetComponent<XpMagnet>();
        if (magnet == null) magnet = player.AddComponent<XpMagnet>();

        // Reasonable defaults; tweak in Inspector later
        if (magnet.radius < 0.1f) magnet.radius = 4.5f;
        if (magnet.pullSpeed < 0.1f) magnet.pullSpeed = 12f;
        magnet.perFrameBudget = 32;

        Debug.Log("[XpMagnet] Attached to Player with radius=" + magnet.radius + ", speed=" + magnet.pullSpeed);
    }
}
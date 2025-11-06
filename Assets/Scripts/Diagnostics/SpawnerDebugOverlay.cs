using UnityEngine;

public class SpawnerDebugOverlay : MonoBehaviour
{
    public EnemySpawner spawner;

    void OnGUI()
    {
        if (spawner == null) spawner = FindObjectOfType<EnemySpawner>();
        if (spawner == null) return;

        var rect = new Rect(10, Screen.height - 120, 520, 110);
        GUI.Box(rect, "Spawner Debug");
        GUILayout.BeginArea(new Rect(rect.x + 8, rect.y + 22, rect.width - 16, rect.height - 30));
        GUILayout.Label($"Enabled: {spawner.enabled}    LiveEnemies: {spawner.LiveEnemies}    SpawnedAnyThisLevel: {spawner.SpawnedAnyThisLevel}");
        GUILayout.Label($"EnemyPrefab: {(spawner.enemyPrefab ? spawner.enemyPrefab.name : "NULL")}    BossPrefab: {(spawner.bossPrefab ? spawner.bossPrefab.name : "NULL")}");
        GUILayout.Label($"Player: {(spawner.player ? spawner.player.name : "NULL")}    XpOrbPrefab: {(spawner.xpOrbPrefab ? spawner.xpOrbPrefab.name : "NULL")}");
        GUILayout.Label($"spawnInterval: {spawner.spawnInterval:0.00}    maxEnemies: {spawner.maxEnemies}    radius: {spawner.spawnRadius:0.0}");
        GUILayout.EndArea();
    }
}
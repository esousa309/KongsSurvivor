#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnemySpawner))]
public class EnemySpawnerInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var spawner = (EnemySpawner)target;
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Debug Controls", EditorStyles.boldLabel);

        GUI.enabled = Application.isPlaying;
        if (GUILayout.Button("Spawn One Now"))
        {
            spawner.ForceSpawnOne();
        }
        if (GUILayout.Button("Spawn Boss Now"))
        {
            spawner.ForceSpawnBoss();
        }
        GUI.enabled = true;

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Play the scene to enable debug spawn buttons.", MessageType.Info);
        }
    }
}
#endif
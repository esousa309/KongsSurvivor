#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class AddBootstrapperToScene
{
    [MenuItem("Tools/Scene Helpers/Add Scene Bootstrapper")]
    public static void Add()
    {
        var existing = Object.FindObjectOfType<SceneBootstrapper>();
        if (existing != null)
        {
            Selection.activeObject = existing.gameObject;
            Debug.Log("SceneBootstrapper already exists; selected it.");
            return;
        }

        var go = new GameObject("SceneBootstrapper");
        go.AddComponent<SceneBootstrapper>();
        Selection.activeObject = go;
        Debug.Log("Added SceneBootstrapper to the scene.");
    }
}
#endif

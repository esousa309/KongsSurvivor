#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public static class CreateTest3DScene
{
    [MenuItem("Tools/Scene Helpers/Create & Open Clean Test3D Scene")]
    public static void CreateAndOpen()
    {
        string scenesFolder = "Assets/Scenes";
        if (!AssetDatabase.IsValidFolder(scenesFolder))
            AssetDatabase.CreateFolder("Assets", "Scenes");

        string path = $"{scenesFolder}/Test3D.unity";
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var go = new GameObject("SceneBootstrapper");
        go.AddComponent<SceneBootstrapper>();
        EditorSceneManager.SaveScene(scene, path);
        EditorSceneManager.OpenScene(path);
        Debug.Log($"Created and opened clean scene at: {path}");
    }
}
#endif

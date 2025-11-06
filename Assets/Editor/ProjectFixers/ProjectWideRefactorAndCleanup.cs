#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;

public static class ProjectWideRefactorAndCleanup
{
    [MenuItem("Tools/Project Fixers/1) Refactor PlayerController2D → PlayerController")]
    public static void RefactorType()
    {
        string assetsPath = Application.dataPath;
        var csFiles = Directory.GetFiles(assetsPath, "*.cs", SearchOption.AllDirectories)
            .Where(p => !p.Replace("\\","/").EndsWith("/ProjectWideRefactorAndCleanup.cs"))
            .ToArray();

        int filesChanged = 0, replacements = 0;
        foreach (var path in csFiles)
        {
            string text = File.ReadAllText(path, Encoding.UTF8);
            if (!text.Contains("PlayerController2D")) continue;

            string updated = text.Replace("PlayerController2D", "PlayerController");
            if (updated != text)
            {
                File.WriteAllText(path, updated, Encoding.UTF8);
                filesChanged++;
                replacements += CountOccurrences(text, "PlayerController2D");
                Debug.Log($"Updated: {Relative(path)}");
            }
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Refactor Complete",
            $"Files changed: {filesChanged}\nOccurrences replaced: {replacements}",
            "OK");
    }

    [MenuItem("Tools/Project Fixers/2) Remove Missing Scripts (Scenes & Prefabs)")]
    public static void RemoveMissingScriptsEverywhere()
    {
        int totalRemoved = 0;

        // Prefabs
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
        foreach (var guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            int removed = RemoveMissingScriptsInGameObject(prefab);
            if (removed > 0)
            {
                totalRemoved += removed;
                EditorUtility.SetDirty(prefab);
                Debug.Log($"Removed {removed} missing script(s) from prefab: {path}");
            }
        }

        // Scenes from Build Settings + any scenes under Assets
        var scenePaths = new HashSet<string>();
        foreach (var s in EditorBuildSettings.scenes)
            if (s.enabled && FileExistsInProject(s.path)) scenePaths.Add(s.path);
        foreach (var guid in AssetDatabase.FindAssets("t:Scene"))
        {
            string p = AssetDatabase.GUIDToAssetPath(guid);
            if (FileExistsInProject(p)) scenePaths.Add(p);
        }

        string currentScenePath = SceneManager.GetActiveScene().path;

        foreach (var scenePath in scenePaths)
        {
            if (!FileExistsInProject(scenePath)) continue;

            try
            {
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                int removed = 0;
                foreach (var root in scene.GetRootGameObjects())
                    removed += RemoveMissingScriptsInGameObject(root);

                if (removed > 0)
                {
                    totalRemoved += removed;
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                    Debug.Log($"Removed {removed} missing script(s) from scene: {scenePath}");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Skipped scene '{scenePath}': {ex.Message}");
            }
        }

        // Try to re-open previous scene only if it still exists
        if (!string.IsNullOrEmpty(currentScenePath) && FileExistsInProject(currentScenePath))
        {
            try { EditorSceneManager.OpenScene(currentScenePath, OpenSceneMode.Single); }
            catch { /* ignore */ }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Cleanup Complete",
            $"Total Missing (MonoBehaviour) components removed: {totalRemoved}",
            "OK");
    }

    [MenuItem("Tools/Project Fixers/3) Fix Build Settings: Remove Missing & Add Existing Scenes")]
    public static void FixBuildSettingsScenes()
    {
        // Remove missing entries; add all real scenes under Assets/ (non-duplicate)
        var validScenePaths = new HashSet<string>();

        foreach (var s in EditorBuildSettings.scenes)
            if (FileExistsInProject(s.path)) validScenePaths.Add(s.path);

        foreach (var guid in AssetDatabase.FindAssets("t:Scene"))
        {
            string p = AssetDatabase.GUIDToAssetPath(guid);
            if (FileExistsInProject(p)) validScenePaths.Add(p);
        }

        var entries = validScenePaths.Select(p => new EditorBuildSettingsScene(p, true)).ToArray();
        EditorBuildSettings.scenes = entries;

        EditorUtility.DisplayDialog("Build Settings Fixed",
            $"Scenes in Build set to {entries.Length} existing scene(s).",
            "OK");
    }

    private static int RemoveMissingScriptsInGameObject(GameObject go)
    {
        int removed = 0;
        removed += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
        foreach (Transform child in go.transform)
            removed += RemoveMissingScriptsInGameObject(child.gameObject);
        return removed;
    }

    private static bool FileExistsInProject(string assetPath)
    {
        if (string.IsNullOrEmpty(assetPath)) return false;
        string projectRoot = Application.dataPath.Replace("/Assets", "");
        string fullPath = Path.Combine(projectRoot, assetPath);
        return File.Exists(fullPath);
    }

    private static int CountOccurrences(string s, string sub)
    {
        int count = 0, idx = 0;
        while ((idx = s.IndexOf(sub, idx)) != -1) { count++; idx += sub.Length; }
        return count;
    }

    private static string Relative(string absolute)
    {
        string proj = Application.dataPath.Replace("/Assets", "");
        return absolute.Replace(proj + Path.DirectorySeparatorChar, "");
    }
}
#endif

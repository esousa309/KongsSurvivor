#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Text;

public static class RefactorPlayerControllerTo3D
{
    [MenuItem("Tools/Project Fixers/Refactor PlayerController → PlayerController")]
    public static void Run()
    {
        string assetsPath = Application.dataPath;
        var csFiles = Directory.GetFiles(assetsPath, "*.cs", SearchOption.AllDirectories)
            .Where(p => !p.Replace("\\","/").EndsWith("/RefactorPlayerControllerTo3D.cs")); // skip self

        int filesChanged = 0, replacements = 0;

        foreach (var path in csFiles)
        {
            string text = File.ReadAllText(path, Encoding.UTF8);
            if (text.Contains("PlayerController"))
            {
                string updated = text.Replace("PlayerController", "PlayerController");
                if (updated != text)
                {
                    File.WriteAllText(path, updated, Encoding.UTF8);
                    filesChanged++;
                    replacements += CountOccurrences(text, "PlayerController");
                    Debug.Log($"Updated: {Relative(path)}");
                }
            }
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog(
            "Refactor Complete",
            $"Converted references:\nFiles changed: {filesChanged}\nReplacements: {replacements}",
            "OK"
        );
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

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class EnsureRequiredTags
{
    [MenuItem("Tools/Project Fixers/Add Required Tags (Enemy, Player)")]
    public static void AddTags()
    {
        var tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]
        );
        var tagsProp = tagManager.FindProperty("tags");

        AddTagIfMissing(tagsProp, "Enemy");
        AddTagIfMissing(tagsProp, "Player");

        tagManager.ApplyModifiedProperties();
        EditorUtility.DisplayDialog("Tags",
            "Ensured tags exist: Enemy, Player", "OK");
    }

    private static void AddTagIfMissing(SerializedProperty tagsProp, string tag)
    {
        for (int i = 0; i < tagsProp.arraySize; i++)
            if (tagsProp.GetArrayElementAtIndex(i).stringValue == tag)
                return;

        int index = tagsProp.arraySize;
        tagsProp.InsertArrayElementAtIndex(index);
        tagsProp.GetArrayElementAtIndex(index).stringValue = tag;
    }
}
#endif

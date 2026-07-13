using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class Lesson01AutoOpen
{
    private const string MarkerPath = "Assets/Editor/Learning/Lesson01.autoopen";
    private const string ScenePath = "Assets/Scenes/Learning/Lesson01_Demo.unity";

    static Lesson01AutoOpen()
    {
        if (Application.isBatchMode)
        {
            return;
        }

        EditorApplication.delayCall += TryOpen;
    }

    private static void TryOpen()
    {
        if (!File.Exists(MarkerPath))
        {
            return;
        }

        if (!File.Exists(ScenePath))
        {
            return;
        }

        File.Delete(MarkerPath);
        AssetDatabase.Refresh();
        EditorSceneManager.OpenScene(ScenePath);
    }
}

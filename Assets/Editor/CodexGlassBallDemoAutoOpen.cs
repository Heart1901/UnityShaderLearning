using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class CodexGlassBallDemoAutoOpen
{
    private const string MarkerPath = "Assets/Editor/CodexGlassBallDemo.autoopen";
    private const string DemoScenePath = "Assets/Scenes/Stylized/GlassBallDemo.unity";

    static CodexGlassBallDemoAutoOpen()
    {
        if (Application.isBatchMode)
        {
            return;
        }

        EditorApplication.delayCall += OpenDemoOnce;
    }

    private static void OpenDemoOnce()
    {
        if (!File.Exists(MarkerPath))
        {
            return;
        }

        var scenePath = File.ReadAllText(MarkerPath).Trim();
        if (string.IsNullOrEmpty(scenePath))
        {
            return;
        }

        if (!File.Exists(scenePath))
        {
            CodexGlassBallDemoBuilder.Build();
            scenePath = DemoScenePath;
        }

        if (!File.Exists(scenePath))
        {
            return;
        }

        File.Delete(MarkerPath);
        EditorSceneManager.OpenScene(scenePath);

        var glassBall = GameObject.Find("Finished Glass Ball");
        if (glassBall != null)
        {
            Selection.activeGameObject = glassBall;
            SceneView.FrameLastActiveSceneView();
        }
    }
}

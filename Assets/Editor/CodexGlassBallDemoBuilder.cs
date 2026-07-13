using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_RENDER_PIPELINE_UNIVERSAL
using UnityEngine.Rendering.Universal;
#endif

public static class CodexGlassBallDemoBuilder
{
    private const string ScenePath = "Assets/Scenes/Stylized/GlassBallDemo.unity";
    private const string MaterialPath = "Assets/Materials/Stylized/CommercialGlassBall.mat";
    private const string AutoOpenMarkerPath = "Assets/Editor/CodexGlassBallDemo.autoopen";

    [MenuItem("Codex Demo/Rebuild Glass Ball Demo")]
    public static void BuildFromMenu()
    {
        Build();
    }

    public static void Build()
    {
        Directory.CreateDirectory("Assets/Scenes/Stylized");
        Directory.CreateDirectory("Assets/Materials/Stylized");
        Directory.CreateDirectory("Assets/Editor");

        ConfigureOpaqueTextureIfURP();

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "GlassBallDemo";

        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.42f, 0.54f, 0.68f);
        RenderSettings.ambientEquatorColor = new Color(0.25f, 0.30f, 0.35f);
        RenderSettings.ambientGroundColor = new Color(0.08f, 0.09f, 0.10f);
        RenderSettings.defaultReflectionMode = DefaultReflectionMode.Skybox;
        RenderSettings.reflectionIntensity = 1.2f;

        CreateCamera();
        CreateLights();
        CreateGlassBall();
        CreateReferenceWorld();
        CreateReflectionProbe();

        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorSceneManager.OpenScene(ScenePath);
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(ScenePath, true)
        };

        File.WriteAllText(AutoOpenMarkerPath, ScenePath);
        AssetDatabase.Refresh();
        Debug.Log($"Glass ball demo generated: {ScenePath}");
    }

    private static void ConfigureOpaqueTextureIfURP()
    {
        var pipelineAsset = GraphicsSettings.currentRenderPipeline;
        if (pipelineAsset == null)
        {
            return;
        }

        var serialized = new SerializedObject(pipelineAsset);
        var opaqueTexture = serialized.FindProperty("m_RequireOpaqueTexture");
        if (opaqueTexture != null)
        {
            opaqueTexture.boolValue = true;
        }

        var opaqueDownsampling = serialized.FindProperty("m_OpaqueDownsampling");
        if (opaqueDownsampling != null)
        {
            opaqueDownsampling.intValue = 0;
        }

        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(pipelineAsset);
    }

    private static void CreateCamera()
    {
        var cameraObject = new GameObject("Main Camera");
        var camera = cameraObject.AddComponent<Camera>();
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0f, 1.25f, -6.0f);
        cameraObject.transform.rotation = Quaternion.Euler(6f, 0f, 0f);
        camera.fieldOfView = 42f;
        camera.clearFlags = CameraClearFlags.Skybox;
        camera.nearClipPlane = 0.05f;
        camera.farClipPlane = 80f;

#if UNITY_RENDER_PIPELINE_UNIVERSAL
        var cameraData = cameraObject.GetComponent<UniversalAdditionalCameraData>();
        if (cameraData == null)
        {
            cameraData = cameraObject.AddComponent<UniversalAdditionalCameraData>();
        }

        cameraData.renderPostProcessing = true;
        cameraData.requiresColorOption = CameraOverrideOption.On;
        cameraData.requiresDepthOption = CameraOverrideOption.On;
#endif
    }

    private static void CreateLights()
    {
        var sunObject = new GameObject("Key Directional Light");
        sunObject.transform.rotation = Quaternion.Euler(42f, -32f, 0f);
        var sun = sunObject.AddComponent<Light>();
        sun.type = LightType.Directional;
        sun.color = new Color(0.95f, 0.98f, 1f);
        sun.intensity = 1.4f;

        CreatePointLight("Cool Rim Light", new Vector3(-2.9f, 2.2f, -1.6f), new Color(0.35f, 0.72f, 1f), 2.8f, 6f);
        CreatePointLight("Warm Spark Light", new Vector3(2.4f, 1.4f, -2.2f), new Color(1f, 0.72f, 0.35f), 1.25f, 5f);
    }

    private static void CreatePointLight(string name, Vector3 position, Color color, float intensity, float range)
    {
        var lightObject = new GameObject(name);
        lightObject.transform.position = position;
        var light = lightObject.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = color;
        light.intensity = intensity;
        light.range = range;
    }

    private static void CreateGlassBall()
    {
        var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = "Finished Glass Ball";
        sphere.transform.position = new Vector3(0f, 0.65f, 0f);
        sphere.transform.localScale = Vector3.one * 1.65f;

        var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        if (material == null)
        {
            var shader = Shader.Find("Codex/Stylized/Commercial Glass Ball");
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, MaterialPath);
        }

        sphere.GetComponent<Renderer>().sharedMaterial = material;
        Selection.activeGameObject = sphere;
    }

    private static void CreateReferenceWorld()
    {
        var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "Glossy Dark Floor";
        floor.transform.position = new Vector3(0f, -0.22f, 0.9f);
        floor.transform.localScale = new Vector3(7.5f, 0.08f, 7.5f);
        floor.GetComponent<Renderer>().sharedMaterial = CreateMaterial("M_GlassDemo_Floor", new Color(0.06f, 0.07f, 0.08f), 0.15f, 0.65f);

        var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = "Refraction Background Wall";
        wall.transform.position = new Vector3(0f, 1.25f, 2.8f);
        wall.transform.localScale = new Vector3(7.2f, 3.7f, 0.08f);
        wall.GetComponent<Renderer>().sharedMaterial = CreateMaterial("M_GlassDemo_BackWall", new Color(0.11f, 0.13f, 0.16f), 0f, 0.4f);

        Color[] palette =
        {
            new Color(0.95f, 0.20f, 0.18f),
            new Color(1.00f, 0.72f, 0.20f),
            new Color(0.25f, 0.82f, 0.45f),
            new Color(0.12f, 0.70f, 1.00f),
            new Color(0.58f, 0.35f, 1.00f),
            new Color(1.00f, 0.28f, 0.72f),
        };

        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                var tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tile.name = $"Refraction Color Tile {x}_{y}";
                tile.transform.position = new Vector3(-3.1f + x * 0.88f, 0.05f + y * 0.55f, 2.68f);
                tile.transform.localScale = new Vector3(0.68f, 0.36f, 0.06f);
                var color = Color.Lerp(palette[(x + y) % palette.Length], Color.white, ((x * 17 + y * 11) % 5) * 0.08f);
                tile.GetComponent<Renderer>().sharedMaterial = CreateMaterial($"M_GlassDemo_Tile_{x}_{y}", color, 0f, 0.25f);
            }
        }

        for (int i = 0; i < 6; i++)
        {
            var pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pillar.name = $"Vertical Refraction Pillar {i}";
            pillar.transform.position = new Vector3(-2.8f + i * 1.12f, 0.72f, 1.45f + (i % 2) * 0.22f);
            pillar.transform.localScale = new Vector3(0.08f, 0.92f, 0.08f);
            pillar.GetComponent<Renderer>().sharedMaterial = CreateMaterial($"M_GlassDemo_Pillar_{i}", palette[(i * 2) % palette.Length], 0f, 0.45f);
        }

        var label = new GameObject("Readable Refraction Text");
        label.transform.position = new Vector3(-1.85f, 2.5f, 2.45f);
        label.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        var mesh = label.AddComponent<TextMesh>();
        mesh.text = "GLASS BALL";
        mesh.characterSize = 0.22f;
        mesh.anchor = TextAnchor.MiddleLeft;
        mesh.alignment = TextAlignment.Left;
        mesh.color = new Color(0.92f, 0.98f, 1f);
        var textRenderer = label.GetComponent<MeshRenderer>();
        if (textRenderer == null)
        {
            textRenderer = label.AddComponent<MeshRenderer>();
        }

        textRenderer.sharedMaterial = CreateMaterial("M_GlassDemo_Text", new Color(0.92f, 0.98f, 1f), 0f, 0.1f);
    }

    private static void CreateReflectionProbe()
    {
        var probeObject = new GameObject("Realtime Reflection Probe");
        probeObject.transform.position = new Vector3(0f, 0.85f, 0.05f);
        var probe = probeObject.AddComponent<ReflectionProbe>();
        probe.mode = ReflectionProbeMode.Realtime;
        probe.refreshMode = ReflectionProbeRefreshMode.OnAwake;
        probe.timeSlicingMode = ReflectionProbeTimeSlicingMode.IndividualFaces;
        probe.size = new Vector3(8f, 5f, 8f);
        probe.intensity = 1.35f;
    }

    private static Material CreateMaterial(string name, Color color, float metallic, float smoothness)
    {
        var path = $"Assets/Materials/Stylized/{name}.mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material != null)
        {
            return material;
        }

        var shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        material = new Material(shader);
        material.color = color;
        material.SetColor("_BaseColor", color);
        material.SetColor("_Color", color);
        material.SetFloat("_Metallic", metallic);
        material.SetFloat("_Smoothness", smoothness);
        material.SetFloat("_Glossiness", smoothness);
        AssetDatabase.CreateAsset(material, path);
        return material;
    }
}

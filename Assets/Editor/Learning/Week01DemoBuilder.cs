using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_RENDER_PIPELINE_UNIVERSAL
using UnityEngine.Rendering.Universal;
#endif

public static class Week01DemoBuilder
{
    public const string ScenePath = "Assets/Scenes/Learning/Week01_ShaderBasics_Demo.unity";
    public const string MarkerPath = "Assets/Editor/Learning/Week01.autobuild";

    private const string MaterialFolder = "Assets/Materials/Learning/Week01";
    private const string TextureFolder = "Assets/Textures/Learning/Week01";
    private const string ShaderFolder = "Assets/Shaders/Learning/Week01_Basics";
    private const string CheckerTexturePath = TextureFolder + "/T_Week01_Checker.png";

    [MenuItem("Shader Learning/Rebuild Week 01 Demo")]
    public static void BuildFromMenu()
    {
        Build();
    }

    public static void Build()
    {
        EnsureFolders();
        ConfigureCameraIfURP();

        var checker = CreateCheckerTexture();
        var materials = CreateMaterials(checker);

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Week01_ShaderBasics_Demo";

        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.5f, 0.58f, 0.72f);
        RenderSettings.ambientEquatorColor = new Color(0.28f, 0.32f, 0.38f);
        RenderSettings.ambientGroundColor = new Color(0.1f, 0.11f, 0.13f);
        RenderSettings.reflectionIntensity = 0.7f;

        var camera = CreateCamera();
        CreateLights();
        CreateGround(materials["Ground"]);
        CreateLearningStations(materials, camera);
        CreateFinalShowcase(materials["Final"], camera);
        CreateSceneHeader(camera);

        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorSceneManager.OpenScene(ScenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Week 01 shader learning demo generated: {ScenePath}");
    }

    private static void EnsureFolders()
    {
        Directory.CreateDirectory("Assets/Scenes/Learning");
        Directory.CreateDirectory("Assets/Materials/Learning");
        Directory.CreateDirectory(MaterialFolder);
        Directory.CreateDirectory("Assets/Textures/Learning");
        Directory.CreateDirectory(TextureFolder);
        Directory.CreateDirectory(ShaderFolder);
    }

    private static void ConfigureCameraIfURP()
    {
#if UNITY_RENDER_PIPELINE_UNIVERSAL
        var pipelineAsset = GraphicsSettings.currentRenderPipeline;
        if (pipelineAsset == null)
        {
            return;
        }

        var serialized = new SerializedObject(pipelineAsset);
        var supportsHDR = serialized.FindProperty("m_SupportsHDR");
        if (supportsHDR != null)
        {
            supportsHDR.boolValue = true;
        }

        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(pipelineAsset);
#endif
    }

    private static Texture2D CreateCheckerTexture()
    {
        const int size = 256;
        const int tile = 32;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, true);
        var dark = new Color32(38, 48, 72, 255);
        var light = new Color32(236, 208, 126, 255);
        var line = new Color32(54, 173, 206, 255);

        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                var checker = ((x / tile) + (y / tile)) % 2 == 0;
                var isLine = x % tile < 2 || y % tile < 2;
                texture.SetPixel(x, y, isLine ? line : checker ? light : dark);
            }
        }

        texture.Apply();
        File.WriteAllBytes(CheckerTexturePath, texture.EncodeToPNG());
        AssetDatabase.ImportAsset(CheckerTexturePath, ImportAssetOptions.ForceUpdate);

        var importer = AssetImporter.GetAtPath(CheckerTexturePath) as TextureImporter;
        if (importer != null)
        {
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = true;
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.filterMode = FilterMode.Point;
            importer.SaveAndReimport();
        }

        return AssetDatabase.LoadAssetAtPath<Texture2D>(CheckerTexturePath);
    }

    private static Dictionary<string, Material> CreateMaterials(Texture2D checker)
    {
        var materials = new Dictionary<string, Material>();

        materials["Day01"] = MaterialAt("M_W01D01_UnlitColor", "Learning/Week01/01 Unlit Color");
        materials["Day01"].SetColor("_BaseColor", new Color(0.2f, 0.55f, 1.0f));

        materials["Day02"] = MaterialAt("M_W01D02_ObjectSpaceGradient", "Learning/Week01/02 Object Space Gradient");
        materials["Day02"].SetColor("_BottomColor", new Color(0.08f, 0.24f, 0.95f));
        materials["Day02"].SetColor("_TopColor", new Color(1.0f, 0.68f, 0.18f));
        materials["Day02"].SetFloat("_HeightMin", -1.0f);
        materials["Day02"].SetFloat("_HeightMax", 1.0f);

        materials["Day03"] = MaterialAt("M_W01D03_NormalVisualizer", "Learning/Week01/03 Normal Visualizer");
        materials["Day03"].SetFloat("_Strength", 1.0f);

        materials["Day04"] = MaterialAt("M_W01D04_LambertDiffuse", "Learning/Week01/04 Lambert Diffuse");
        materials["Day04"].SetColor("_BaseColor", new Color(0.95f, 0.58f, 0.22f));
        materials["Day04"].SetColor("_AmbientColor", new Color(0.08f, 0.10f, 0.14f));

        materials["Day05"] = MaterialAt("M_W01D05_ToonBandLighting", "Learning/Week01/05 Toon Band Lighting");
        materials["Day05"].SetColor("_LitColor", new Color(1.0f, 0.82f, 0.34f));
        materials["Day05"].SetColor("_ShadowColor", new Color(0.12f, 0.18f, 0.38f));
        materials["Day05"].SetFloat("_BandCenter", 0.45f);
        materials["Day05"].SetFloat("_BandSoftness", 0.035f);

        materials["Day06"] = MaterialAt("M_W01D06_TextureUV", "Learning/Week01/06 Texture UV");
        materials["Day06"].SetTexture("_BaseMap", checker);
        materials["Day06"].SetColor("_TintColor", Color.white);
        materials["Day06"].SetTextureScale("_BaseMap", new Vector2(2.0f, 2.0f));

        materials["Final"] = MaterialAt("M_W01D07_StylizedFinal", "Learning/Week01/07 Stylized Final");
        materials["Final"].SetTexture("_BaseMap", checker);
        materials["Final"].SetColor("_BaseColor", new Color(0.96f, 0.72f, 0.34f));
        materials["Final"].SetColor("_ShadowColor", new Color(0.12f, 0.16f, 0.34f));
        materials["Final"].SetColor("_RimColor", new Color(0.34f, 0.88f, 1.0f));
        materials["Final"].SetColor("_OutlineColor", new Color(0.03f, 0.04f, 0.08f));
        materials["Final"].SetFloat("_BandCenter", 0.48f);
        materials["Final"].SetFloat("_BandSoftness", 0.035f);
        materials["Final"].SetFloat("_RimPower", 2.7f);
        materials["Final"].SetFloat("_RimStrength", 0.52f);
        materials["Final"].SetFloat("_OutlineWidth", 0.025f);
        materials["Final"].SetTextureScale("_BaseMap", new Vector2(1.4f, 1.4f));

        materials["Ground"] = MaterialAt("M_W01_Ground", "Learning/Week01/01 Unlit Color");
        materials["Ground"].SetColor("_BaseColor", new Color(0.16f, 0.18f, 0.22f));

        foreach (var material in materials.Values)
        {
            EditorUtility.SetDirty(material);
        }

        return materials;
    }

    private static Material MaterialAt(string fileName, string shaderName)
    {
        var path = $"{MaterialFolder}/{fileName}.mat";
        var shader = Shader.Find(shaderName);
        if (shader == null)
        {
            Debug.LogError($"Missing shader: {shaderName}");
            shader = Shader.Find("Standard");
        }

        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, path);
        }

        material.shader = shader;
        return material;
    }

    private static Camera CreateCamera()
    {
        var cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0.0f, 4.25f, -9.4f);
        cameraObject.transform.rotation = Quaternion.Euler(25.0f, 0.0f, 0.0f);

        var camera = cameraObject.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.Skybox;
        camera.fieldOfView = 38.0f;
        camera.nearClipPlane = 0.05f;
        camera.farClipPlane = 100.0f;
        camera.allowHDR = true;

#if UNITY_RENDER_PIPELINE_UNIVERSAL
        var cameraData = cameraObject.GetComponent<UniversalAdditionalCameraData>();
        if (cameraData == null)
        {
            cameraData = cameraObject.AddComponent<UniversalAdditionalCameraData>();
        }

        cameraData.renderPostProcessing = true;
#endif
        return camera;
    }

    private static void CreateLights()
    {
        var keyObject = new GameObject("Key Directional Light");
        keyObject.transform.rotation = Quaternion.Euler(42.0f, -34.0f, 0.0f);
        var key = keyObject.AddComponent<Light>();
        key.type = LightType.Directional;
        key.color = new Color(1.0f, 0.96f, 0.86f);
        key.intensity = 1.45f;

        CreatePointLight("Cool Rim Practice Light", new Vector3(-4.5f, 3.2f, -2.0f), new Color(0.35f, 0.72f, 1.0f), 2.5f, 9.0f);
        CreatePointLight("Warm Fill Practice Light", new Vector3(4.2f, 2.6f, -2.4f), new Color(1.0f, 0.62f, 0.32f), 1.5f, 7.0f);
    }

    private static void CreatePointLight(string name, Vector3 position, Color color, float intensity, float range)
    {
        var obj = new GameObject(name);
        obj.transform.position = position;
        var light = obj.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = color;
        light.intensity = intensity;
        light.range = range;
    }

    private static void CreateGround(Material material)
    {
        var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ground.name = "Week01_Ground";
        ground.transform.position = new Vector3(0.0f, -0.08f, 1.2f);
        ground.transform.localScale = new Vector3(15.5f, 0.12f, 8.8f);
        ground.GetComponent<Renderer>().sharedMaterial = material;
    }

    private static void CreateLearningStations(Dictionary<string, Material> materials, Camera camera)
    {
        var stations = new[]
        {
            new Station("01 Color", "Properties -> frag", "Day01", new Vector3(-6.0f, 0.75f, 2.65f)),
            new Station("02 Varyings", "vertex data -> fragment", "Day02", new Vector3(-4.0f, 0.75f, 2.65f)),
            new Station("03 Normal", "normalWS as color", "Day03", new Vector3(-2.0f, 0.75f, 2.65f)),
            new Station("04 Lambert", "dot(N, L)", "Day04", new Vector3(0.0f, 0.75f, 2.65f)),
            new Station("05 Toon", "banded lighting", "Day05", new Vector3(2.0f, 0.75f, 2.65f)),
            new Station("06 UV", "texture sampling", "Day06", new Vector3(4.0f, 0.75f, 2.65f)),
            new Station("07 Final", "toon + rim + outline", "Final", new Vector3(6.0f, 0.75f, 2.65f))
        };

        foreach (var station in stations)
        {
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = "Week01_" + station.Title.Replace(" ", "_");
            sphere.transform.position = station.Position;
            sphere.transform.localScale = Vector3.one * 0.72f;
            sphere.GetComponent<Renderer>().sharedMaterial = materials[station.MaterialKey];

            CreateLabel(station.Title + "\n" + station.Subtitle, station.Position + new Vector3(0.0f, 1.05f, 0.0f), 0.075f, camera);
            CreatePedestal(station.Position + new Vector3(0.0f, -0.48f, 0.0f), materials["Ground"]);
        }

        CreateNormalLines(new Vector3(-2.0f, 0.75f, 2.65f), materials["Day01"]);
    }

    private static void CreateFinalShowcase(Material material, Camera camera)
    {
        var display = LoadDisplayModel();
        display.name = "Week01_Final_Stylized_Product";
        display.transform.position = new Vector3(0.0f, 1.05f, -1.15f);
        display.transform.rotation = Quaternion.Euler(0.0f, -25.0f, 0.0f);
        display.transform.localScale = Vector3.one * 1.45f;
        AssignMaterial(display, material);

        CreateLabel("Week 01 Final Product\nTexture + Toon Light + Rim + Outline", new Vector3(0.0f, 2.85f, -1.15f), 0.095f, camera);
        CreatePedestal(new Vector3(0.0f, 0.08f, -1.15f), material, new Vector3(2.2f, 0.18f, 2.2f));
    }

    private static GameObject LoadDisplayModel()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Common/Teapot.prefab");
        if (prefab != null)
        {
            var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance != null)
            {
                return instance;
            }
        }

        var model = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/Teapot.FBX");
        if (model != null)
        {
            var instance = PrefabUtility.InstantiatePrefab(model) as GameObject;
            if (instance != null)
            {
                return instance;
            }
        }

        return GameObject.CreatePrimitive(PrimitiveType.Sphere);
    }

    private static void AssignMaterial(GameObject root, Material material)
    {
        foreach (var renderer in root.GetComponentsInChildren<Renderer>())
        {
            renderer.sharedMaterial = material;
        }
    }

    private static void CreatePedestal(Vector3 position, Material material)
    {
        CreatePedestal(position, material, new Vector3(1.05f, 0.14f, 1.05f));
    }

    private static void CreatePedestal(Vector3 position, Material material, Vector3 scale)
    {
        var pedestal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pedestal.name = "Week01_Pedestal";
        pedestal.transform.position = position;
        pedestal.transform.localScale = scale;
        pedestal.GetComponent<Renderer>().sharedMaterial = material;
    }

    private static void CreateNormalLines(Vector3 center, Material material)
    {
        var directions = new[]
        {
            new Vector3(-0.35f, 0.9f, -0.12f).normalized,
            new Vector3(0.0f, 1.0f, 0.0f).normalized,
            new Vector3(0.38f, 0.82f, 0.18f).normalized
        };

        foreach (var direction in directions)
        {
            var lineObject = new GameObject("Week01_Normal_Direction_Line");
            var line = lineObject.AddComponent<LineRenderer>();
            line.sharedMaterial = material;
            line.startWidth = 0.045f;
            line.endWidth = 0.015f;
            line.positionCount = 2;
            var start = center + direction * 0.42f;
            var end = start + direction * 0.55f;
            line.SetPosition(0, start);
            line.SetPosition(1, end);
        }
    }

    private static void CreateSceneHeader(Camera camera)
    {
        CreateLabel("Week 01 Shader Basics\nFrom vertices to a stylized material", new Vector3(0.0f, 4.25f, 1.6f), 0.12f, camera);
    }

    private static void CreateLabel(string text, Vector3 position, float size, Camera camera)
    {
        var labelObject = new GameObject("Label_" + text.Split('\n')[0].Replace(" ", "_"));
        labelObject.transform.position = position;
        labelObject.transform.rotation = Quaternion.LookRotation(labelObject.transform.position - camera.transform.position);

        var mesh = labelObject.AddComponent<TextMesh>();
        mesh.text = text;
        mesh.anchor = TextAnchor.MiddleCenter;
        mesh.alignment = TextAlignment.Center;
        mesh.characterSize = size;
        mesh.fontSize = 64;
        mesh.color = new Color(0.9f, 0.94f, 1.0f);
    }

    private readonly struct Station
    {
        public Station(string title, string subtitle, string materialKey, Vector3 position)
        {
            Title = title;
            Subtitle = subtitle;
            MaterialKey = materialKey;
            Position = position;
        }

        public string Title { get; }
        public string Subtitle { get; }
        public string MaterialKey { get; }
        public Vector3 Position { get; }
    }
}

[InitializeOnLoad]
public static class Week01DemoAutoBuild
{
    static Week01DemoAutoBuild()
    {
        if (Application.isBatchMode)
        {
            return;
        }

        EditorApplication.delayCall += TryBuild;
    }

    private static void TryBuild()
    {
        if (!File.Exists(Week01DemoBuilder.MarkerPath))
        {
            return;
        }

        AssetDatabase.DeleteAsset(Week01DemoBuilder.MarkerPath);
        Week01DemoBuilder.Build();
    }
}

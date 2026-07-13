using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_RENDER_PIPELINE_UNIVERSAL
using UnityEngine.Rendering.Universal;
#endif

public static class Week02DemoBuilder
{
    public const string ScenePath = "Assets/Scenes/Learning/Week02_StylizedToon_Demo.unity";
    public const string MarkerPath = "Assets/Editor/Learning/Week02.autobuild";

    private const string MaterialFolder = "Assets/Materials/Learning/Week02";
    private const string TextureFolder = "Assets/Textures/Learning/Week02";
    private const string RampTexturePath = TextureFolder + "/T_W02_Ramp.png";
    private const string BaseTexturePath = TextureFolder + "/T_W02_BasePattern.png";
    private const string MatCapTexturePath = TextureFolder + "/T_W02_MatCap.png";
    private const string ShadowPatternPath = TextureFolder + "/T_W02_ShadowPattern.png";

    [MenuItem("Shader Learning/Rebuild Week 02 Demo")]
    public static void BuildFromMenu()
    {
        Build();
    }

    public static void Build()
    {
        EnsureFolders();

        var textures = CreateTextures();
        var materials = CreateMaterials(textures);

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Week02_StylizedToon_Demo";

        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.46f, 0.54f, 0.68f);
        RenderSettings.ambientEquatorColor = new Color(0.22f, 0.25f, 0.32f);
        RenderSettings.ambientGroundColor = new Color(0.08f, 0.09f, 0.12f);
        RenderSettings.reflectionIntensity = 0.65f;

        var camera = CreateCamera();
        CreateLights();
        CreateGround(materials["Ground"]);
        CreateLearningStations(materials, camera);
        CreateFinalShowcase(materials["Final"], camera);
        CreateSceneHeader(camera);

        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorSceneManager.OpenScene(ScenePath);
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(ScenePath, true)
        };

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        FocusSceneObject("Week02_Final_Advanced_Toon_Product");
        Debug.Log($"Week 02 stylized toon demo generated: {ScenePath}");
    }

    private static void FocusSceneObject(string objectName)
    {
        var focus = GameObject.Find(objectName);
        if (focus == null)
        {
            return;
        }

        Selection.activeGameObject = focus;
        var sceneView = SceneView.lastActiveSceneView;
        if (sceneView == null)
        {
            return;
        }

        sceneView.FrameSelected();
        sceneView.Repaint();
    }

    private static void EnsureFolders()
    {
        Directory.CreateDirectory("Assets/Scenes/Learning");
        Directory.CreateDirectory("Assets/Materials/Learning");
        Directory.CreateDirectory(MaterialFolder);
        Directory.CreateDirectory("Assets/Textures/Learning");
        Directory.CreateDirectory(TextureFolder);
    }

    private static Dictionary<string, Texture2D> CreateTextures()
    {
        var textures = new Dictionary<string, Texture2D>
        {
            ["Ramp"] = CreateRampTexture(),
            ["Base"] = CreateBasePatternTexture(),
            ["MatCap"] = CreateMatCapTexture(),
            ["Pattern"] = CreateShadowPatternTexture()
        };

        return textures;
    }

    private static Texture2D CreateRampTexture()
    {
        const int width = 256;
        const int height = 8;
        var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var t = x / (float)(width - 1);
                Color color;
                if (t < 0.38f)
                {
                    color = new Color(0.23f, 0.26f, 0.48f);
                }
                else if (t < 0.66f)
                {
                    color = new Color(0.72f, 0.52f, 0.42f);
                }
                else
                {
                    color = new Color(1.08f, 0.88f, 0.48f);
                }

                texture.SetPixel(x, y, color);
            }
        }

        return SaveTexture(texture, RampTexturePath, FilterMode.Point, TextureWrapMode.Clamp);
    }

    private static Texture2D CreateBasePatternTexture()
    {
        const int size = 256;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, true);

        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                var u = x / (float)(size - 1);
                var v = y / (float)(size - 1);
                var stripe = Mathf.Sin((u + v) * Mathf.PI * 12.0f) * 0.5f + 0.5f;
                var grid = x % 64 < 3 || y % 64 < 3 ? 0.18f : 0.0f;
                var color = new Color(0.84f + stripe * 0.08f, 0.66f + v * 0.12f, 0.36f + u * 0.08f);
                color = Color.Lerp(color, new Color(0.2f, 0.55f, 0.75f), grid);
                texture.SetPixel(x, y, color);
            }
        }

        return SaveTexture(texture, BaseTexturePath, FilterMode.Bilinear, TextureWrapMode.Repeat);
    }

    private static Texture2D CreateMatCapTexture()
    {
        const int size = 256;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, true);

        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                var uv = new Vector2(x / (float)(size - 1), y / (float)(size - 1));
                var p = uv * 2.0f - Vector2.one;
                var r = Mathf.Clamp01(p.magnitude);
                var main = Mathf.Pow(Mathf.Clamp01(1.0f - r), 1.8f);
                var highlight = Mathf.Exp(-Vector2.SqrMagnitude(p - new Vector2(-0.35f, 0.42f)) * 28.0f);
                var rim = Mathf.SmoothStep(0.78f, 1.0f, r) * 0.35f;
                var color = new Color(0.08f, 0.13f, 0.18f) + new Color(0.35f, 0.72f, 1.0f) * main + new Color(1.0f, 0.92f, 0.55f) * highlight + new Color(0.2f, 0.85f, 1.0f) * rim;
                texture.SetPixel(x, y, color);
            }
        }

        return SaveTexture(texture, MatCapTexturePath, FilterMode.Bilinear, TextureWrapMode.Clamp);
    }

    private static Texture2D CreateShadowPatternTexture()
    {
        const int size = 256;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, true);

        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                var diagonal = (x + y) % 28 < 7 ? 1.0f : 0.0f;
                var cross = Mathf.Abs((x - y) % 42) < 3 ? 0.55f : 0.0f;
                var dot = ((x / 16 + y / 16) % 2 == 0) ? 0.18f : 0.0f;
                var value = Mathf.Clamp01(diagonal + cross + dot);
                texture.SetPixel(x, y, new Color(value, value, value, 1.0f));
            }
        }

        return SaveTexture(texture, ShadowPatternPath, FilterMode.Point, TextureWrapMode.Repeat);
    }

    private static Texture2D SaveTexture(Texture2D texture, string path, FilterMode filterMode, TextureWrapMode wrapMode)
    {
        texture.Apply();
        File.WriteAllBytes(path, texture.EncodeToPNG());
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = true;
            importer.filterMode = filterMode;
            importer.wrapMode = wrapMode;
            importer.SaveAndReimport();
        }

        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }

    private static Dictionary<string, Material> CreateMaterials(Dictionary<string, Texture2D> textures)
    {
        var materials = new Dictionary<string, Material>();

        materials["Ramp"] = MaterialAt("M_W02D01_RampDiffuse", "Learning/Week02/01 Ramp Diffuse");
        materials["Ramp"].SetTexture("_BaseMap", textures["Base"]);
        materials["Ramp"].SetTexture("_RampMap", textures["Ramp"]);
        materials["Ramp"].SetColor("_BaseColor", new Color(1.0f, 0.74f, 0.36f));
        materials["Ramp"].SetFloat("_ShadowStrength", 0.22f);

        materials["Outline"] = MaterialAt("M_W02D02_OutlinePass", "Learning/Week02/02 Outline Pass");
        materials["Outline"].SetColor("_BaseColor", new Color(0.95f, 0.66f, 0.3f));
        materials["Outline"].SetColor("_ShadowColor", new Color(0.13f, 0.17f, 0.34f));
        materials["Outline"].SetColor("_OutlineColor", new Color(0.02f, 0.025f, 0.045f));
        materials["Outline"].SetFloat("_OutlineWidth", 0.028f);

        materials["Rim"] = MaterialAt("M_W02D03_RimLight", "Learning/Week02/03 Rim Light");
        materials["Rim"].SetColor("_BaseColor", new Color(0.68f, 0.48f, 1.0f));
        materials["Rim"].SetColor("_ShadowColor", new Color(0.12f, 0.12f, 0.28f));
        materials["Rim"].SetColor("_RimColor", new Color(0.25f, 0.95f, 1.0f));
        materials["Rim"].SetFloat("_RimStrength", 0.72f);

        materials["Spec"] = MaterialAt("M_W02D04_ToonSpecular", "Learning/Week02/04 Toon Specular");
        materials["Spec"].SetColor("_BaseColor", new Color(0.32f, 0.68f, 1.0f));
        materials["Spec"].SetColor("_ShadowColor", new Color(0.08f, 0.16f, 0.32f));
        materials["Spec"].SetColor("_ToonSpecColor", new Color(1.0f, 0.94f, 0.64f));
        materials["Spec"].SetFloat("_SpecStrength", 0.95f);

        materials["MatCap"] = MaterialAt("M_W02D05_MatCap", "Learning/Week02/05 MatCap");
        materials["MatCap"].SetTexture("_BaseMap", textures["Base"]);
        materials["MatCap"].SetTexture("_MatCapMap", textures["MatCap"]);
        materials["MatCap"].SetColor("_BaseColor", new Color(0.72f, 0.66f, 0.52f));
        materials["MatCap"].SetFloat("_MatCapStrength", 0.7f);

        materials["Pattern"] = MaterialAt("M_W02D06_ShadowPattern", "Learning/Week02/06 Shadow Pattern");
        materials["Pattern"].SetTexture("_BaseMap", textures["Base"]);
        materials["Pattern"].SetTexture("_PatternMap", textures["Pattern"]);
        materials["Pattern"].SetColor("_BaseColor", new Color(0.92f, 0.62f, 0.38f));
        materials["Pattern"].SetColor("_ShadowColor", new Color(0.12f, 0.16f, 0.32f));
        materials["Pattern"].SetFloat("_PatternStrength", 0.45f);

        materials["Final"] = MaterialAt("M_W02D07_AdvancedToonProduct", "Learning/Week02/07 Advanced Toon Product");
        materials["Final"].SetTexture("_BaseMap", textures["Base"]);
        materials["Final"].SetTexture("_RampMap", textures["Ramp"]);
        materials["Final"].SetTexture("_MatCapMap", textures["MatCap"]);
        materials["Final"].SetTexture("_PatternMap", textures["Pattern"]);
        materials["Final"].SetColor("_BaseColor", new Color(0.96f, 0.68f, 0.34f));
        materials["Final"].SetColor("_ShadowColor", new Color(0.12f, 0.15f, 0.32f));
        materials["Final"].SetColor("_RimColor", new Color(0.28f, 0.92f, 1.0f));
        materials["Final"].SetColor("_ToonSpecColor", new Color(1.0f, 0.92f, 0.58f));
        materials["Final"].SetColor("_OutlineColor", new Color(0.02f, 0.026f, 0.05f));
        materials["Final"].SetFloat("_OutlineWidth", 0.027f);
        materials["Final"].SetFloat("_BandContrast", 1.05f);
        materials["Final"].SetFloat("_RimStrength", 0.58f);
        materials["Final"].SetFloat("_SpecStrength", 0.78f);
        materials["Final"].SetFloat("_MatCapStrength", 0.34f);
        materials["Final"].SetFloat("_PatternStrength", 0.22f);

        materials["Ground"] = MaterialAt("M_W02_Ground", "Learning/Week02/01 Ramp Diffuse");
        materials["Ground"].SetTexture("_BaseMap", textures["Base"]);
        materials["Ground"].SetTexture("_RampMap", textures["Ramp"]);
        materials["Ground"].SetColor("_BaseColor", new Color(0.18f, 0.2f, 0.24f));
        materials["Ground"].SetFloat("_ShadowStrength", 0.5f);

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
            shader = Shader.Find("Universal Render Pipeline/Lit");
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
        cameraObject.transform.position = new Vector3(0.0f, 4.6f, -10.0f);
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
        key.color = new Color(1.0f, 0.96f, 0.82f);
        key.intensity = 1.55f;

        CreatePointLight("Cyan Rim Light", new Vector3(-4.8f, 3.2f, -2.4f), new Color(0.25f, 0.82f, 1.0f), 2.4f, 9.0f);
        CreatePointLight("Warm Accent Light", new Vector3(4.2f, 2.8f, -2.0f), new Color(1.0f, 0.62f, 0.28f), 1.4f, 7.5f);
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
        ground.name = "Week02_Ground";
        ground.transform.position = new Vector3(0.0f, -0.09f, 1.15f);
        ground.transform.localScale = new Vector3(15.8f, 0.12f, 9.0f);
        ground.GetComponent<Renderer>().sharedMaterial = material;
    }

    private static void CreateLearningStations(Dictionary<string, Material> materials, Camera camera)
    {
        var stations = new[]
        {
            new Station("01 Ramp", "texture controlled light", "Ramp", new Vector3(-6.0f, 0.75f, 2.75f)),
            new Station("02 Outline", "extra pass + cull front", "Outline", new Vector3(-4.0f, 0.75f, 2.75f)),
            new Station("03 Rim", "view dependent edge", "Rim", new Vector3(-2.0f, 0.75f, 2.75f)),
            new Station("04 Spec", "cartoon highlight", "Spec", new Vector3(0.0f, 0.75f, 2.75f)),
            new Station("05 MatCap", "view-space material", "MatCap", new Vector3(2.0f, 0.75f, 2.75f)),
            new Station("06 Pattern", "textured shadow", "Pattern", new Vector3(4.0f, 0.75f, 2.75f)),
            new Station("07 Product", "all effects combined", "Final", new Vector3(6.0f, 0.75f, 2.75f))
        };

        foreach (var station in stations)
        {
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = "Week02_" + station.Title.Replace(" ", "_");
            sphere.transform.position = station.Position;
            sphere.transform.localScale = Vector3.one * 0.72f;
            sphere.GetComponent<Renderer>().sharedMaterial = materials[station.MaterialKey];

            CreatePedestal(station.Position + new Vector3(0.0f, -0.48f, 0.0f), materials["Ground"]);
            CreateLabel(station.Title + "\n" + station.Subtitle, station.Position + new Vector3(0.0f, 1.08f, 0.0f), 0.072f, camera);
        }
    }

    private static void CreateFinalShowcase(Material material, Camera camera)
    {
        var display = LoadDisplayModel();
        display.name = "Week02_Final_Advanced_Toon_Product";
        display.transform.position = new Vector3(0.0f, 1.1f, -1.25f);
        display.transform.rotation = Quaternion.Euler(0.0f, -24.0f, 0.0f);
        display.transform.localScale = Vector3.one * 1.55f;
        AssignMaterial(display, material);

        CreatePedestal(new Vector3(0.0f, 0.06f, -1.25f), material, new Vector3(2.25f, 0.18f, 2.25f));
        CreateLabel("Week 02 Final Product\nRamp + Outline + Rim + Spec + MatCap + Pattern", new Vector3(0.0f, 3.05f, -1.25f), 0.085f, camera);
    }

    private static GameObject LoadDisplayModel()
    {
        var candidates = new[]
        {
            "Assets/Prefabs/Common/Teapot.prefab",
            "Assets/Prefabs/Common/Suzanne.prefab",
            "Assets/Models/Teapot.FBX",
            "Assets/Models/suzanne.obj"
        };

        foreach (var path in candidates)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (asset == null)
            {
                continue;
            }

            var instance = PrefabUtility.InstantiatePrefab(asset) as GameObject;
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
        pedestal.name = "Week02_Pedestal";
        pedestal.transform.position = position;
        pedestal.transform.localScale = scale;
        pedestal.GetComponent<Renderer>().sharedMaterial = material;
    }

    private static void CreateSceneHeader(Camera camera)
    {
        CreateLabel("Week 02 Stylized Toon\nInterview-ready material building blocks", new Vector3(0.0f, 4.45f, 1.55f), 0.11f, camera);
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
        mesh.color = new Color(0.92f, 0.95f, 1.0f);
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
public static class Week02DemoAutoBuild
{
    static Week02DemoAutoBuild()
    {
        if (Application.isBatchMode)
        {
            return;
        }

        EditorApplication.delayCall += TryBuild;
    }

    private static void TryBuild()
    {
        if (!File.Exists(Week02DemoBuilder.MarkerPath))
        {
            return;
        }

        AssetDatabase.DeleteAsset(Week02DemoBuilder.MarkerPath);
        Week02DemoBuilder.Build();
    }
}

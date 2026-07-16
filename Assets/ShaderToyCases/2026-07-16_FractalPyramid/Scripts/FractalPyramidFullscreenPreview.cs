using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public sealed class FractalPyramidFullscreenPreview : MonoBehaviour
{
    [SerializeField] private Material displayMaterial;
    [SerializeField, Range(0.1f, 50f)] private float distance = 5f;
    [SerializeField] private string quadName = "Fractal Pyramid Fullscreen Quad";
    [SerializeField] private bool drawGuiFallback = true;

    private Mesh previewMesh;

    private void OnEnable()
    {
        ConfigureCamera();
        EnsurePreviewQuad();
    }

    private void OnValidate()
    {
        ConfigureCamera();
    }

    private void LateUpdate()
    {
        ConfigureCamera();
        EnsurePreviewQuad();
    }

    private void OnGUI()
    {
        if (!drawGuiFallback || displayMaterial == null || Event.current.type != EventType.Repaint)
        {
            return;
        }

        Graphics.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture, displayMaterial);
    }

    private void ConfigureCamera()
    {
        Camera previewCamera = GetComponent<Camera>();
        previewCamera.clearFlags = CameraClearFlags.SolidColor;
        previewCamera.backgroundColor = Color.black;
        previewCamera.orthographic = true;
        previewCamera.orthographicSize = 5f;
        previewCamera.nearClipPlane = 0.01f;
        previewCamera.farClipPlane = 100f;
    }

    private void EnsurePreviewQuad()
    {
        if (displayMaterial == null)
        {
            return;
        }

        Transform previewTransform = transform.Find(quadName);
        if (previewTransform == null)
        {
            GameObject previewObject = new GameObject(quadName);
            previewObject.transform.SetParent(transform, false);
            previewTransform = previewObject.transform;
        }

        MeshFilter meshFilter = previewTransform.GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            meshFilter = previewTransform.gameObject.AddComponent<MeshFilter>();
        }

        MeshRenderer meshRenderer = previewTransform.GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            meshRenderer = previewTransform.gameObject.AddComponent<MeshRenderer>();
        }

        if (previewMesh == null)
        {
            previewMesh = CreateQuadMesh();
        }

        Camera previewCamera = GetComponent<Camera>();
        float aspect = previewCamera.aspect > 0f ? previewCamera.aspect : 16f / 9f;
        float height = previewCamera.orthographicSize * 2f;
        float width = height * aspect;

        previewTransform.localPosition = new Vector3(0f, 0f, distance);
        previewTransform.localRotation = Quaternion.identity;
        previewTransform.localScale = new Vector3(width, height, 1f);
        meshFilter.sharedMesh = previewMesh;
        meshRenderer.sharedMaterial = displayMaterial;
    }

    private static Mesh CreateQuadMesh()
    {
        Mesh mesh = new Mesh
        {
            name = "Fractal Pyramid Preview Quad"
        };

        mesh.vertices = new[]
        {
            new Vector3(-0.5f, -0.5f, 0f),
            new Vector3(0.5f, -0.5f, 0f),
            new Vector3(0.5f, 0.5f, 0f),
            new Vector3(-0.5f, 0.5f, 0f)
        };
        mesh.uv = new[]
        {
            new Vector2(0f, 0f),
            new Vector2(1f, 0f),
            new Vector2(1f, 1f),
            new Vector2(0f, 1f)
        };
        mesh.triangles = new[]
        {
            0, 2, 1,
            0, 3, 2
        };
        mesh.RecalculateBounds();
        return mesh;
    }
}

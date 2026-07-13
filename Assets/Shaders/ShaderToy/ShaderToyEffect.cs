using UnityEngine;

// 允许在编辑模式下预览效果，方便调试
[ExecuteInEditMode]
public class ShaderToyEffect : MonoBehaviour
{
    [Tooltip("要显示的后处理Shader")]
    public Shader shaderToy;

    private Material material = null;

    // 材质属性，负责动态创建和管理Shader所使用的材质
    public Material Material
    {
        get
        {
            // 当Shader为空，打印错误日志
            if (shaderToy == null)
            {
                Debug.LogError("ShaderToyEffect: 尚未指定Shader，请将Shader文件拖拽至 'shaderToy' 属性上。");
                return null;
            }
            // 当Shader不被当前的渲染管线支持，打印警告
            if (!shaderToy.isSupported)
            {
                Debug.LogWarning("ShaderToyEffect: 当前Shader可能不受支持，请检查是否与项目渲染管线兼容。");
                return null;
            }

            // 如果材质未创建或Shader发生变化，则创建新材质
            if (material == null || material.shader != shaderToy)
            {
                if (material != null)
                    DestroyImmediate(material);
                material = new Material(shaderToy);
                // 防止材质被意外保存到场景中
                material.hideFlags = HideFlags.HideAndDontSave;
            }
            return material;
        }
    }

    // OnRenderImage 是 Unity 的生命周期方法，它在相机渲染完所有物体后被调用。
    // source: 当前相机渲染好的图像，是只读的。
    // destination: 经过后处理后的图像将输出到这里，最终显示在屏幕上。
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        // 确保材质有效，避免对空引用进行操作
        if (Material == null) return;
        // Graphics.Blit是高效的GPU拷贝与处理函数，它将source图像通过指定的材质进行处理，
        // 将处理结果写入destination，完成一次全屏后处理特效。
        Graphics.Blit(source, destination, Material);
    }
}
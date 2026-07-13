using UnityEditor;
using UnityEngine;


public static class EditorResourceFinder
{
    // 在编辑器环境下通过文件名查找资源（无需放入特定文件夹）
    public static T FindAssetByFileName<T>(string fileName) where T : Object
    {
        // 移除文件扩展名（如果有）
        string cleanName = System.IO.Path.GetFileNameWithoutExtension(fileName);

        // 查找所有匹配名称的资源 GUID
        string[] guids = UnityEditor.AssetDatabase.FindAssets($"t:{typeof(T).Name} {cleanName}");

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            string assetName = System.IO.Path.GetFileNameWithoutExtension(assetPath);

            // 精确匹配文件名（而非包含搜索）
            if (assetName == cleanName)
            {
                return AssetDatabase.LoadAssetAtPath<T>(assetPath);
            }
        }

        Debug.LogError($"未找到资源: {fileName} (类型: {typeof(T).Name})");
        return null;
    }
}


using System;
using System.Collections.Generic;

using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class RegexAssetSearcher
{
    // 原代码中修改为静态字段，方便在编辑器窗口中修改
    public static string TargetPath = "Assets/Art/ModuleRes"; // 只关注此路径下的资源
    public static string RegexPattern = @"^UI_.+_Panel$";
    public static  Regex AssetRegex = new Regex(RegexPattern, RegexOptions.IgnoreCase);

    // 缓存：存储符合规则的文件名，保证文件名唯一
    private static readonly List<string> _cachedFileNames = new List<string>();

    // 接口：获取匹配的文件名称列表
    public static List<string> GetMatchingAssets(bool force_increase = false)
    {
        if (_cachedFileNames.Count == 0)
        {
            // 首次使用时全量初始化缓存
            FullInitCache();
        }
        else
        {
            if (force_increase)
            {
                C4lConsistencyUtil.IncreaseCheck();
            }
        }
        C4lConsistencyUtil.PrintCheckResult();
        return new List<string>(_cachedFileNames); // 返回副本防止外部修改
    }

    // 全量初始化缓存（首次启动或强制刷新时调用）
    [MenuItem("Tools/强制刷新UI缓存")]
    public static void FullInitCache()
    {
        _cachedFileNames.Clear();
        // 全量搜索目标路径下的预制体
        string[] assetGuids = AssetDatabase.FindAssets("t:Prefab", new[] { TargetPath });
        foreach (var guid in assetGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string fileName = Path.GetFileNameWithoutExtension(path);
            if (AssetRegex.IsMatch(fileName))
            {
                _cachedFileNames.Add(fileName);
                C4lConsistencyUtil.CheckC4lConsistency(fileName);
            }
        }
        Debug.Log($"完成全量初始化，共 {_cachedFileNames.Count} 个匹配资源");
    }

    // 处理资源变化，通过文件名直接更新缓存
    internal static void UpdateCacheByFileNameChanges(
        string[] importedAssets,   // 导入/修改的资源路径
        string[] deletedAssets,    // 删除的资源路径
        string[] movedAssets,      // 移动后的路径
        string[] movedFromAssets)  // 移动前的旧路径
    {
        // 1. 处理导入/修改的资源，添加新的文件名或更新现有文件名
        foreach (var newPath in importedAssets)
        {
            HandleNewOrModifiedAsset(newPath);
        }

        // 2. 处理删除的资源
        foreach (var deletedPath in deletedAssets)
        {
            HandleDeletedAsset(deletedPath);
        }

        // 3. 处理移动/重命名的资源，旧路径 -> 新路径
        for (int i = 0; i < movedAssets.Length; i++)
        {
            string oldPath = movedFromAssets[i];
            string newPath = movedAssets[i];
            HandleMovedAsset(oldPath, newPath);
        }
    }

    // 处理新增或修改的资源，判断文件名是否符合规则
    private static void HandleNewOrModifiedAsset(string newPath)
    {
        // 过滤：仅处理目标路径下的预制体
        if (!IsInTargetScope(newPath)) return;

        string newFileName = Path.GetFileNameWithoutExtension(newPath);

        // 如果文件名符合规则且不在缓存中，添加
        if (AssetRegex.IsMatch(newFileName) && !_cachedFileNames.Contains(newFileName))
        {
            Debug.Log("新增文件名不在缓存中，添加到缓存：" + newFileName);
            _cachedFileNames.Add(newFileName);
            C4lConsistencyUtil.UpdateCacheResultByAction(ActionType.Add, newFileName, "");
        }
        // 如果文件名不符合规则但在缓存中，移除
        else if (!AssetRegex.IsMatch(newFileName) && _cachedFileNames.Contains(newFileName))
        {
            Debug.Log("文件在缓存中，但不再匹配，移除");
            _cachedFileNames.Remove(newFileName);
            C4lConsistencyUtil.UpdateCacheResultByAction(ActionType.Delete, newFileName, "");
        }
        else
        {
            C4lConsistencyUtil.UpdateCacheResultByAction(ActionType.Modify, newFileName, "");
            Debug.Log("修改文件:" + newFileName);
        }
    }

    // 处理删除的资源，移除缓存中对应的文件名
    private static void HandleDeletedAsset(string deletedPath)
    {
        if (!IsInTargetScope(deletedPath)) return;

        string deletedFileName = Path.GetFileNameWithoutExtension(deletedPath);
        if (_cachedFileNames.Contains(deletedFileName))
        {
            Debug.Log("删除文件:" + deletedFileName);
            _cachedFileNames.Remove(deletedFileName);
            C4lConsistencyUtil.UpdateCacheResultByAction(ActionType.Delete, deletedFileName, "");
        }
    }

    // 处理移动/重命名的资源，移除旧文件名并添加新文件名
    private static void HandleMovedAsset(string oldPath, string newPath)
    {
        // 先检查旧路径下的文件名，如果在目标范围内，移除缓存中的文件名
        if (IsInTargetScope(oldPath))
        {
            string oldFileName = Path.GetFileNameWithoutExtension(oldPath);
            Debug.Log("需要移除文件:" + oldFileName);
            _cachedFileNames.Remove(oldFileName);
            C4lConsistencyUtil.UpdateCacheResultByAction(ActionType.Delete, oldFileName, "");
        }

        // 再检查新路径下的文件名，如果在目标范围内，判断是否需要添加
        if (IsInTargetScope(newPath))
        {
            string newFileName = Path.GetFileNameWithoutExtension(newPath);
            if (AssetRegex.IsMatch(newFileName) && !_cachedFileNames.Contains(newFileName))
            {
                Debug.Log("添加新文件名" + newFileName);
                _cachedFileNames.Add(newFileName);
                C4lConsistencyUtil.UpdateCacheResultByAction(ActionType.Add, "", newFileName);
            }
        }
    }

    // 判断资源是否在目标范围内，路径+类型+名称
    private static bool IsInTargetScope(string assetPath)
    {
        // 基本判断，是否在目标路径下且为预制体
        if (!assetPath.StartsWith(TargetPath, StringComparison.OrdinalIgnoreCase) ||
            !Path.GetExtension(assetPath).Equals(".prefab", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // 获取文件名并验证规则
        string fileName = Path.GetFileNameWithoutExtension(assetPath);
        return AssetRegex.IsMatch(fileName);
    }

    // 菜单选项：显示当前匹配项
    [MenuItem("Tools/显示UI匹配资源")]
    public static void ShowMatchingAssets()
    {
        var results = GetMatchingAssets(true);
        if (results.Count == 0)
        {
            Debug.Log("未找到匹配的资源。");
        }
        else
        {
            Debug.Log($"找到 {results.Count} 个匹配的资源。");
            results.ForEach(name => Debug.Log(name));
        }
    }
}

// 资源变化监听器，自动更新匹配的文件名缓存
public class AssetChangeListener : AssetPostprocessor
{
    static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        // 过滤出符合规则的变化文件
        var filteredImported = FilterValidAssets(importedAssets);
        var filteredDeleted = FilterValidAssets(deletedAssets);

        // 处理移动和重命名，需要特殊过滤
        var filteredMoved = new List<string>();
        var filteredMovedFrom = new List<string>();
        for (int i = 0; i < movedAssets.Length; i++)
        {
            bool oldIsValid = IsValidUIPanelPath(movedFromAssetPaths[i]);
            bool newIsValid = IsValidUIPanelPath(movedAssets[i]);

            if (oldIsValid || newIsValid)
            {
                filteredMovedFrom.Add(movedFromAssetPaths[i]);
                filteredMoved.Add(movedAssets[i]);
            }
        }

        // 检测到有效变化时更新缓存
        if (filteredImported.Count > 0 || filteredDeleted.Count > 0 || filteredMoved.Count > 0)
        {
            Debug.Log($"检测到 {filteredImported.Count + filteredDeleted.Count + filteredMoved.Count} 个资源变化，更新缓存");

            // 仅在已经初始化时更新
            if (RegexAssetSearcher.GetMatchingAssets().Count > 0)
            {
                RegexAssetSearcher.UpdateCacheByFileNameChanges(
                    filteredImported.ToArray(),
                    filteredDeleted.ToArray(),
                    filteredMoved.ToArray(),
                    filteredMovedFrom.ToArray());
            }
        }
    }

    // 过滤出符合规则的资源路径
    private static List<string> FilterValidAssets(string[] assetPaths)
    {
        var result = new List<string>();
        foreach (var path in assetPaths)
        {
            if (IsValidUIPanelPath(path))
            {
                result.Add(path);
            }
        }
        return result;
    }

    // 判断给定路径是否为有效的UI面板预制体
    private static bool IsValidUIPanelPath(string assetPath)
    {
        // 基本判断，是否在目标路径下且为预制体
        if (!assetPath.StartsWith(RegexAssetSearcher.TargetPath, StringComparison.OrdinalIgnoreCase) ||
            !Path.GetExtension(assetPath).Equals(".prefab", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // 获取文件名并验证规则
        string fileName = Path.GetFileNameWithoutExtension(assetPath);
        return RegexAssetSearcher.AssetRegex.IsMatch(fileName);
    }
}

// 自定义编辑器窗口类
public class RegexAssetSearcherWindow : EditorWindow
{
    private string targetPath;
    private string regexPattern;

    [MenuItem("Tools/Regex Asset Searcher")]
    public static void ShowWindow()
    {
        RegexAssetSearcherWindow window = GetWindow<RegexAssetSearcherWindow>("Regex Asset Searcher");
        window.targetPath = RegexAssetSearcher.TargetPath;
        window.regexPattern = RegexAssetSearcher.RegexPattern;
        window.Show();
    }

    private void OnGUI()
    {
        targetPath = EditorGUILayout.TextField("Target Path", targetPath);
        regexPattern = EditorGUILayout.TextField("Regex Pattern", regexPattern);

        if (GUILayout.Button("查找"))
        {
            RegexAssetSearcher.TargetPath = targetPath;
            RegexAssetSearcher.RegexPattern = regexPattern;
            RegexAssetSearcher.AssetRegex = new Regex(regexPattern, RegexOptions.IgnoreCase);
            RegexAssetSearcher.ShowMatchingAssets();
        }
    }
}
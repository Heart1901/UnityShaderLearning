using System;
using System.Collections.Generic;

using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

/*using System.Diagnostics;*/
/*using System.Diagnostics;*/
public enum ActionType { 
    Delete,
    Add,
    Modify,
}
public class C4lConsistencyUtil
{
    // 缓存：仅存储符合规则的文件名（假设文件名唯一）
    public static Dictionary<string, bool> cachedFileResults = new Dictionary<string, bool>();
    public static List<string> addFileList = new List<string>();
    public static List<string> removeFileList = new List<string>();
    public static List<string> modifyFileList = new List<string>();
    public static bool CheckC4lConsistency(string file_panel_name) {
    
        GameObject go = EditorResourceFinder.FindAssetByFileName<GameObject>(file_panel_name);
        
        if (go != null)
        {
            cachedFileResults[file_panel_name] = true;
            Debug.Log("事件 检测文件：" + file_panel_name);
            return true;
        }
        else {
            
        }

            
       /* if (go != null)
        { 
            Debug.Log($"找到资源: {AssetDatabase.GetAssetPath(go)}");
            UIOrientation comp = go.GetComponent<UIOrientation>();
            if (comp != null)
            {
                GameObject landscape_go = comp.GetLanscapeNewGo();
                GameObject portrait_go = comp.GetPortraitNewGo();
                if (landscape_go == null && portrait_go)
                {
                    Debug.Log("横板工程未引用，请检查工程");
                    return false;
                }
                if (portrait_go == null && landscape_go) {
                    Debug.Log("竖版工程未引用，请检查工程");
                    return false;
                }
                if (portrait_go == null && landscape_go == null) {
                    Debug.Log("横竖屏工程未引用，请检查工程");
                }
      
                Component4Lua landscape_c4l = landscape_go.GetComponent<Component4Lua>();
                Component4Lua portrait_c4l = portrait_go.GetComponent<Component4Lua>();
                int go_len1 = landscape_c4l.GetGosLength();
                int go_len2 = portrait_c4l.GetGosLength();
                if (go_len1 != go_len2)
                {
                    Debug.Log("横屏竖屏对象引用长度不一致，请检查工程");
                    return false;
                }
                else {
                    for (int i = 0; i < go_len1; i++) {
                        GameObject go1 = landscape_c4l.GetGo(i);
                        GameObject go2 = portrait_c4l.GetGo(i);
                        if (go1 == null && go2 != null) {
                            Debug.Log("横竖屏对象引用不一致，请检查横屏工程");
                            return false;
                        }
                        if (go2 == null && go1 != null) {
                            Debug.Log("横竖屏对象引用不一致，请检查竖屏工程");
                            return false;
                        }
                        if(go1 != null && go2 != null)
                        {
                            bool res = CompareComponentTypes(go1, go2, i);
                            if (!res)
                            {
                                Debug.Log("横竖屏对象组件不一致，请检查横竖屏工程");
                                return false;
                            }
                            
                        }
                    }
                }
                int comp_len1 = landscape_c4l.GetComponentsLength();
                int comp_len2 = portrait_c4l.GetComponentsLength();
                if (comp_len1 != comp_len2)
                {
                    Debug.Log("横竖屏组件引用长度不一致，请检查工程"+ comp_len1.ToString() + " " + comp_len2.ToString());
                    return false;
                }
                else {
                    for (int i = 0; i < comp_len1; i++) {
                        UnityEngine.Component comp1 = landscape_c4l.Get(i);
                        UnityEngine.Component comp2 = portrait_c4l.Get(i);
                        if (comp1 == null && comp2 != null)
                        {
                            Debug.Log("横竖屏组件引用不一致，请检查横屏工程");
                            return false;
                        }
                        else if (comp2 == null && comp1 != null)
                        {
                            Debug.Log("横竖屏组件引用不一致，请检查竖屏工程");
                            return false;
                        }
                        else {
                            bool res = CompareComponentType(comp1, comp2);
                            if (!res)
                            {
                                Debug.Log("横竖屏组件类型不一致，请检查横竖屏工程");
                                return false;
                            }
                        }
                    }
                }

            }
            else
            {
                Debug.Log("界面不需要横竖屏，仅仅需要横屏，无需检查");
                return true;
            }
        }
        Debug.Log("检测通过，横竖屏一致性");*/
        return true;
    }

    // 比较组件类型（不比较组件属性值）
    private static bool CompareComponentTypes(GameObject a, GameObject b, int index)
    {
        var componentsA = a.GetComponents<UnityEngine.Component>();
        var componentsB = b.GetComponents<UnityEngine.Component>();

        if (componentsA.Length != componentsB.Length) return false;

        // 检查组件类型是否完全一致（忽略顺序）
        foreach (var component in componentsA)
        {
            bool found = false;
            foreach (var otherComponent in componentsB)
            {
                if (component.GetType() == otherComponent.GetType())
                {
                    found = true;
                    break;
                }
            }
            if (!found) return false;
        }

        return true;
    }
    public static void UpdateCacheResultByAction(ActionType action, string file_name="", string new_file_name="") {
        if (action == ActionType.Add) {
            bool exists = addFileList.Contains(file_name); // true
            if (!exists) {
                Debug.Log("事件 添加文件："+ file_name);
                addFileList.Add(file_name);
            }
            
        }
        if (action == ActionType.Delete) {
            bool exists = removeFileList.Contains(file_name); // true
            if (!exists) {

                Debug.Log("事件 删除文件：" + file_name);
                removeFileList.Add(file_name);
            }
                
        }
        if (action == ActionType.Modify) {
            bool exists = modifyFileList.Contains(file_name); // true
            if (!exists) {
                Debug.Log("事件 修改文件：" + file_name);
                modifyFileList.Add(file_name);
            }
                
        }
    }

    public static void IncreaseCheck()
    {
        foreach (string fileName in addFileList)
        {
            CheckC4lConsistency(fileName);
        }
        foreach (string fileName in removeFileList) {
            cachedFileResults.Remove(fileName);
        }
        foreach (string fileName in modifyFileList) {
            CheckC4lConsistency(fileName);
        }
        Debug.Log("增加列表："+addFileList.Count.ToString());
        Debug.Log("删除列表：" + removeFileList.Count.ToString());
        Debug.Log("修改列表：" + modifyFileList.Count.ToString());
        ResetActionList();
    }
    private static void ResetActionList() {
        addFileList.Clear();
        removeFileList.Clear();
        modifyFileList.Clear();
    }
    public static void PrintCheckResult() {
        Debug.Log("打印事件检测结果："+cachedFileResults.Count);
        foreach (string name in cachedFileResults.Keys)
        {
            Debug.Log($"事件文件名：{name}");
        }
        // 打开结果展示窗口
        C4lResultWindow.ShowWindow(cachedFileResults.Keys);
    }

    //比较组件类型
    private static bool CompareComponentType(UnityEngine.Component a, UnityEngine.Component b) {
        return a.GetType() == b.GetType();
    }

}

// 自定义编辑器窗口类，用于展示检查结果
public class C4lResultWindow : EditorWindow
{
    private List<string> fileNames;
    private Vector2 scrollPosition;

    public static void ShowWindow(IEnumerable<string> names)
    {
        C4lResultWindow window = GetWindow<C4lResultWindow>("C4l 检查结果");
        window.fileNames = new List<string>(names);
        window.Show();
    }

    private void OnGUI()
    {
        if (fileNames != null && fileNames.Count > 0)
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            foreach (string name in fileNames)
            {
                GUILayout.Label($"记录文件名: {name}");
            }
            GUILayout.EndScrollView();
        }
        else
        {
            GUILayout.Label("没有记录的文件名。");
        }
    }
}
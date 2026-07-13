using System.Collections.Generic;
using UnityEngine;

public class DynamicLandBuild : MonoBehaviour
{
    [Header("空地参数")]
    public List<Vector3> landBoundary = new List<Vector3>(); // 空地多边形边界（XZ 平面，Y=0）
    public int landSizeX = 8; // 备用参数（可删除，优先用 landBoundary）
    public int landSizeZ = 5;

    [Header("装饰配置")]
    public List<GameObject> lawnPrefabs; // 草坪预制件
    public List<GameObject> fencePrefabs; // 围栏预制件

    // 可扩展：自动生成 landBoundary（如根据 landSizeX/Z 生成矩形）
    void Awake()
    {
        if (landBoundary.Count == 0)
        {
            // 示例：根据 landSizeX/Z 生成矩形边界
            landBoundary.Add(new Vector3(-landSizeX / 2, 0, -landSizeZ / 2));
            landBoundary.Add(new Vector3(landSizeX / 2, 0, -landSizeZ / 2));
            landBoundary.Add(new Vector3(landSizeX / 2, 0, landSizeZ / 2));
            landBoundary.Add(new Vector3(-landSizeX / 2, 0, landSizeZ / 2));
        }
    }
}
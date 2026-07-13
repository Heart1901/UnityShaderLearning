using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BuildingPlacer : MonoBehaviour
{
    [Header("空地配置")]
    public DynamicLandBuild land; // 挂载 DynamicLandBuild 的空地对象
    public bool useLargestRect = true; // 用最大内接矩形适配（否则用自定义形状缩放）

    [Header("建筑配置")]
    public List<Vector3> buildingPrototype; // 建筑原型地基（XZ 平面，Y=0）
    public float buildingHeight = 5f; // 建筑高度
    public GameObject buildingPrefab; // 建筑预制件（可选，优先实例化预制件）

    void Start()
    {
        if (land == null || land.landBoundary.Count < 3)
        {
            Debug.LogError("空地边界无效！");
            return;
        }
        PlaceBuilding();
    }

    void PlaceBuilding()
    {
        List<Vector3> landBoundary = land.landBoundary; // 空地多边形（XZ 平面）
        List<Vector3> adaptedFoundation = new List<Vector3>();

        // 步骤1：适配建筑地基到空地
        if (useLargestRect)
        {
            // 方式1：最大内接矩形（适合规则建筑）
            adaptedFoundation = FindLargestInscribedRectangle(landBoundary);
        }
        else
        {
            // 方式2：缩放自定义形状（适合异形建筑）
            adaptedFoundation = GeometryUtils.ScaleToFitRegion(buildingPrototype, landBoundary);
        }

        // 步骤2：生成/实例化建筑
        if (buildingPrefab != null)
        {
            InstantiatePrefabBuilding(adaptedFoundation);
        }
        else
        {
            GenerateRuntimeBuilding(adaptedFoundation);
        }
    }

    // 找空地的最大内接矩形（适配规则建筑）
    List<Vector3> FindLargestInscribedRectangle(List<Vector3> region)
    {
        List<Vector2> region2D = region.Select(p => new Vector2(p.x, p.z)).ToList();
        List<Vector2> convexHull = GeometryUtils.ConvexHull(region2D);
        List<Vector2> rect2D = GeometryUtils.RotatingCalipersLargestRect(convexHull);
        return rect2D.Select(v => new Vector3(v.x, 0, v.y)).ToList();
    }

    // 实例化预制件建筑（需预制件包含地基 MeshFilter）
    void InstantiatePrefabBuilding(List<Vector3> foundation)
    {
        Bounds landBounds = GeometryUtils.GetBounds(land.landBoundary);
        Bounds foundationBounds = GeometryUtils.GetBounds(foundation);

        // 计算建筑位置（中心对齐空地）
        Vector3 position = landBounds.center - (foundationBounds.center - foundation[0]);
        position.y = 0; // 固定在平面上

        GameObject building = Instantiate(buildingPrefab, position, Quaternion.identity);
        building.transform.SetParent(land.transform);

        // 替换预制件的地基 Mesh（如果需要）
        MeshFilter mf = building.GetComponentInChildren<MeshFilter>();
        if (mf != null)
        {
            mf.mesh = CreatePolygonMesh(foundation);
        }
    }

    // 运行时生成建筑（地基 + 拉伸主体）
    void GenerateRuntimeBuilding(List<Vector3> foundation)
    {
        // 生成地基 Mesh
        Mesh foundationMesh = CreatePolygonMesh(foundation);

        // 地基对象
        GameObject foundationObj = new GameObject("BuildingFoundation");
        foundationObj.transform.SetParent(land.transform);
        foundationObj.transform.localPosition = Vector3.zero;
        MeshFilter mf = foundationObj.AddComponent<MeshFilter>();
        MeshRenderer mr = foundationObj.AddComponent<MeshRenderer>();
        mf.mesh = foundationMesh;
        mr.material = new Material(Shader.Find("Standard"));
        mr.material.color = Color.gray;

        // 拉伸生成建筑主体
        GameObject buildingObj = new GameObject("Building");
        buildingObj.transform.SetParent(foundationObj.transform);
        buildingObj.transform.localPosition = new Vector3(0, buildingHeight / 2, 0);

        Mesh buildingMesh = ExtrudeMesh(foundationMesh, buildingHeight);
        MeshFilter buildingMF = buildingObj.AddComponent<MeshFilter>();
        MeshRenderer buildingMR = buildingObj.AddComponent<MeshRenderer>();
        buildingMF.mesh = buildingMesh;
        buildingMR.material = new Material(Shader.Find("Standard"));
        buildingMR.material.color = Color.white;
    }

    // 生成多边形地基 Mesh
    Mesh CreatePolygonMesh(List<Vector3> poly3D)
    {
        List<Vector2> poly2D = poly3D.Select(p => new Vector2(p.x, p.z)).ToList();
        List<int> triangles = GeometryUtils.Triangulate(poly2D);

        Mesh mesh = new Mesh();
        mesh.vertices = poly3D.Select(p => new Vector3(p.x, 0, p.z)).ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        return mesh;
    }

    // 拉伸 Mesh 生成建筑主体
    Mesh ExtrudeMesh(Mesh baseMesh, float height)
    {
        Vector3[] baseVertices = baseMesh.vertices;
        int vertexCount = baseVertices.Length;
        int triangleCount = baseMesh.triangles.Length;

        // 顶部顶点（Y 方向拉伸）
        Vector3[] topVertices = new Vector3[vertexCount];
        for (int i = 0; i < vertexCount; i++)
        {
            topVertices[i] = baseVertices[i] + new Vector3(0, height, 0);
        }

        // 合并顶点
        List<Vector3> vertices = new List<Vector3>(baseVertices);
        vertices.AddRange(topVertices);

        // 底部三角形
        List<int> triangles = new List<int>(baseMesh.triangles);

        // 顶部三角形（反转法线方向）
        int[] topTriangles = new int[triangleCount];
        for (int i = 0; i < triangleCount; i++)
        {
            topTriangles[i] = baseMesh.triangles[triangleCount - 1 - i] + vertexCount;
        }
        triangles.AddRange(topTriangles);

        // 侧面三角形（四边形拆分为两个三角形）
        for (int i = 0; i < vertexCount; i++)
        {
            int j = (i + 1) % vertexCount;
            triangles.Add(i);
            triangles.Add(j + vertexCount);
            triangles.Add(j);
            triangles.Add(i);
            triangles.Add(i + vertexCount);
            triangles.Add(j + vertexCount);
        }

        Mesh extrudedMesh = new Mesh();
        extrudedMesh.vertices = vertices.ToArray();
        extrudedMesh.triangles = triangles.ToArray();
        extrudedMesh.RecalculateNormals();
        return extrudedMesh;
    }
}
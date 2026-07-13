using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class GeometryUtils
{
    // 凸包计算（Graham 扫描法，提取区域最大外轮廓）
    public static List<Vector2> ConvexHull(List<Vector2> points)
    {
        if (points.Count <= 1) return points;
        points = points.Distinct().ToList();
        points.Sort((a, b) => a.x.CompareTo(b.x) == 0 ? a.y.CompareTo(b.y) : a.x.CompareTo(b.x));

        List<Vector2> lower = new List<Vector2>();
        foreach (Vector2 p in points)
        {
            while (lower.Count >= 2 && Cross(lower[lower.Count - 2], lower[lower.Count - 1], p) <= 0)
                lower.RemoveAt(lower.Count - 1);
            lower.Add(p);
        }

        List<Vector2> upper = new List<Vector2>();
        for (int i = points.Count - 1; i >= 0; i--)
        {
            Vector2 p = points[i];
            while (upper.Count >= 2 && Cross(upper[upper.Count - 2], upper[upper.Count - 1], p) <= 0)
                upper.RemoveAt(upper.Count - 1);
            upper.Add(p);
        }

        lower.RemoveAt(lower.Count - 1);
        upper.RemoveAt(upper.Count - 1);
        lower.AddRange(upper);
        return lower;
    }

    // 旋转卡壳法：找凸多边形最大内接矩形（适配规则建筑）
    public static List<Vector2> RotatingCalipersLargestRect(List<Vector2> convexHull)
    {
        int n = convexHull.Count;
        if (n < 3) return new List<Vector2>();

        int[] next = new int[n];
        for (int i = 0; i < n; i++) next[i] = (i + 1) % n;

        float maxArea = 0;
        List<Vector2> bestRect = new List<Vector2>();

        for (int i = 0; i < n; i++)
        {
            int j = (i + 1) % n;
            int k = (j + 1) % n;
            int l = (k + 1) % n;

            while (true)
            {
                while (Cross(convexHull[j] - convexHull[i], convexHull[k] - convexHull[j]) < 0)
                {
                    k = next[k];
                    l = next[l];
                }

                float area = RectangleArea(convexHull[i], convexHull[j], convexHull[k]);
                if (area > maxArea)
                {
                    maxArea = area;
                    bestRect = GetRectangle(convexHull[i], convexHull[j], convexHull[k]);
                }

                if (Cross(convexHull[k] - convexHull[j], convexHull[l] - convexHull[k]) >= 0) break;
                l = next[l];
            }
            next[i] = j;
        }

        return bestRect;
    }

    // 多边形缩放（适配自定义形状建筑）
    public static List<Vector3> ScaleToFitRegion(List<Vector3> buildingPoly, List<Vector3> regionPoly)
    {
        Bounds regionBounds = GetBounds(regionPoly);
        Bounds buildingBounds = GetBounds(buildingPoly);

        float scaleX = regionBounds.size.x / buildingBounds.size.x;
        float scaleZ = regionBounds.size.z / buildingBounds.size.z;
        float scale = Mathf.Min(scaleX, scaleZ);

        Vector3 center = regionBounds.center;
        List<Vector3> scaledPoly = new List<Vector3>();
        foreach (Vector3 p in buildingPoly)
        {
            Vector3 local = p - buildingBounds.center;
            local.x *= scale;
            local.z *= scale;
            scaledPoly.Add(center + local);
        }
        return scaledPoly;
    }

    // 获取 XZ 平面边界（忽略 Y 轴）
    public static Bounds GetBounds(List<Vector3> poly)
    {
        float minX = float.MaxValue, maxX = float.MinValue;
        float minZ = float.MaxValue, maxZ = float.MinValue;
        foreach (Vector3 p in poly)
        {
            minX = Mathf.Min(minX, p.x);
            maxX = Mathf.Max(maxX, p.x);
            minZ = Mathf.Min(minZ, p.z);
            maxZ = Mathf.Max(maxZ, p.z);
        }
        return new Bounds(new Vector3((minX + maxX) / 2, 0, (minZ + maxZ) / 2), new Vector3(maxX - minX, 0, maxZ - minZ));
    }

    // 三角剖分（生成 Mesh 用）
    public static List<int> Triangulate(List<Vector2> poly)
    {
        List<int> triangles = new List<int>();
        int n = poly.Count;
        if (n < 3) return triangles;

        List<int> vs = Enumerable.Range(0, n).ToList();
        int[] prev = new int[n], next = new int[n];
        for (int j = 0; j < n; j++)
        {
            prev[j] = j == 0 ? n - 1 : j - 1;
            next[j] = j == n - 1 ? 0 : j + 1;
        }

        int count = 0;
        int i = 0;
        while (n > 3)
        {
            if (IsEar(i, prev, next, poly))
            {
                triangles.Add(prev[i]);
                triangles.Add(i);
                triangles.Add(next[i]);
                next[prev[i]] = next[i];
                prev[next[i]] = prev[i];
                n--;
                count = 0;
            }
            else
            {
                i = next[i];
                count++;
                if (count > n) break;
            }
        }
        triangles.Add(prev[0]);
        triangles.Add(0);
        triangles.Add(next[0]);
        return triangles;
    }

    // 辅助方法（叉积、面积计算等）
    private static float Cross(Vector2 a, Vector2 b) => a.x * b.y - a.y * b.x;
    private static float Cross(Vector2 a, Vector2 b, Vector2 c) => Cross(b - a, c - a);
    private static float RectangleArea(Vector2 a, Vector2 b, Vector2 c)
    {
        Vector2 ab = b - a;
        Vector2 bc = c - b;
        return Mathf.Abs(Cross(ab, bc)) * ab.magnitude;
    }
    private static List<Vector2> GetRectangle(Vector2 a, Vector2 b, Vector2 c)
    {
        Vector2 ab = b - a;
        Vector2 bc = c - b;
        Vector2 normal = new Vector2(-bc.y, bc.x).normalized;
        float dist = Cross(normal, a - b);
        Vector2 d = a + ab + normal * dist;
        return new List<Vector2> { a, b, c, d };
    }
    private static bool IsEar(int i, int[] prev, int[] next, List<Vector2> poly)
    {
        Vector2 a = poly[prev[i]], b = poly[i], c = poly[next[i]];
        if (Cross(b - a, c - b) < 0) return false;
        for (int j = next[next[i]]; j != prev[i]; j = next[j])
        {
            if (IsPointInTriangle(poly[j], a, b, c)) return false;
        }
        return true;
    }
    private static bool IsPointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        float c1 = Cross(b - a, p - a);
        float c2 = Cross(c - b, p - b);
        float c3 = Cross(a - c, p - c);
        return (c1 >= 0 && c2 >= 0 && c3 >= 0) || (c1 <= 0 && c2 <= 0 && c3 <= 0);
    }
}
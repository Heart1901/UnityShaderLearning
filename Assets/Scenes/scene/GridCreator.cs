using System.Diagnostics;
using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    [Header("棋盘参数")]
    public int rowCount = 5; // 行数（n）
    public int colCount = 5; // 列数（m）
    public Material cellMaterial; // 格子材质（可选）

    private void Start()
    {
        GenerateGrid();
    }

    /// <summary>生成完整棋盘</summary>
    private void GenerateGrid()
    {
        // 输入验证
        if (rowCount <= 0 || colCount <= 0)
        {
            //Debug.LogError("行数和列数必须为正整数！");
            return;
        }

        Vector2Int gridSize = new Vector2Int(rowCount, colCount);
        float cellSize = 1f;
        Vector3 basePosition = transform.position; // 棋盘整体基础位置（父物体坐标）

        // 循环生成每个格子
        for (int gridIndex = 0; gridIndex < rowCount * colCount; gridIndex++)
        {
            CreateCell(gridIndex, gridSize, cellSize, basePosition);
        }
    }

    /// <summary>创建单个格子</summary>
    private void CreateCell(int gridIndex, Vector2Int gridSize, float cellSize, Vector3 basePosition)
    {
        // 1. 计算行列坐标（基于GridVertexCalculator的逻辑）
        int row = gridIndex / gridSize.y; // 行号（0 ~ rowCount-1）
        int col = gridIndex % gridSize.y; // 列号（0 ~ colCount-1）

        // 2. 计算格子中心位置（因格子尺寸1×1，中心偏移0.5）
        float xPos = col * cellSize + cellSize / 2f; // 列方向（X轴）
        float zPos = row * cellSize + cellSize / 2f; // 行方向（Z轴）
        Vector3 cellCenter = basePosition + new Vector3(xPos, 0, zPos);

        // 3. 实例化格子（使用Plane，默认10×10，缩放到1×1）
        GameObject cell = GameObject.CreatePrimitive(PrimitiveType.Plane);
        cell.name = $"Cell_{gridIndex}"; // 命名格式：Cell_索引
        cell.transform.parent = transform; // 归属于棋盘父物体
        cell.transform.position = cellCenter;
        cell.transform.localScale = new Vector3(0.1f, 1f, 0.1f); // 10×10 → 1×1（缩放0.1倍）

        // 4. 材质与外观（可选）
        if (cellMaterial != null)
        {
            cell.GetComponent<MeshRenderer>().material = cellMaterial;
        }
        // 可选：设置交替颜色（如奇偶索引区分）
        if (gridIndex % 2 == 0)
        {
            cell.GetComponent<MeshRenderer>().material.color = new Color(0.9f, 0.9f, 0.9f);
        }
    }
}
using UnityEngine;
using UnityEngine.UI;

public class CanvasArcPointCalculator : MonoBehaviour
{
    [Header("位置参数")]
    [SerializeField] private RectTransform startPoint;
    [SerializeField] private RectTransform endPoint;
    [SerializeField] private RectTransform rootPoint;

    [Header("点数配置")]
    [Range(2, 20)] // 限制点数范围
    [SerializeField] private int pointCount = 5;

    [Header("可视化参数")]
    [SerializeField] private Image pointImagePrefab;
    [SerializeField] private float pointSize = 15f;
    [SerializeField] private bool autoUpdate = true; // 是否自动更新

    private GameObject[] pointObjects;
    private Canvas canvas;
    private bool isDirty = true; // 是否需要重新计算

    // 外部可调用的设置方法
    public void SetStartPoint(RectTransform point) => startPoint = point;
    public void SetEndPoint(RectTransform point) => endPoint = point;
    public void SetRootPoint(RectTransform point) => rootPoint = point;
    public void SetPointCount(int count) => pointCount = Mathf.Max(2, count);

    // 触发重新计算
    public void Refresh() => isDirty = true;

    void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("脚本必须放在Canvas下的对象上!");
            return;
        }
    }

    void Update()
    {
        if (autoUpdate && isDirty)
        {
            RecalculatePoints();
            isDirty = false;
        }
    }

    void RecalculatePoints()
    {
        // 清除旧的点
        ClearPoints();

        if (startPoint == null || endPoint == null || rootPoint == null)
        {
            Debug.LogError("请设置起点、终点和圆心的RectTransform!");
            return;
        }

        // 获取Canvas坐标(世界坐标)
        Vector2 start = startPoint.position;
        Vector2 end = endPoint.position;
        Vector2 root = rootPoint.position;

        // 计算向量
        Vector2 v = start - root;
        Vector2 w = end - root;

        // 计算半径
        float radius = v.magnitude;

        // 计算总角度
        float dotProduct = Vector2.Dot(v.normalized, w.normalized);
        dotProduct = Mathf.Clamp(dotProduct, -1f, 1f); // 防止浮点数误差
        float theta = Mathf.Acos(dotProduct);

        // 确定旋转方向
        float crossZ = v.x * w.y - v.y * w.x;
        if (crossZ < 0) theta = -theta;

        // 计算角度增量
        float deltaTheta = theta / (pointCount - 1);

        // 创建点数组
        pointObjects = new GameObject[pointCount];

        // 计算并设置每个点的位置
        for (int i = 0; i < pointCount; i++)
        {
            // 计算当前角度
            float currentAngle = Mathf.Atan2(v.y, v.x) + i * deltaTheta;

            // 计算点坐标
            float x = root.x + radius * Mathf.Cos(currentAngle);
            float y = root.y + radius * Mathf.Sin(currentAngle);
            Vector3 pointPosition = new Vector3(x, y, 0);

            // 创建可视化点
            CreatePointMarker(pointPosition, i);
        }
    }

    void CreatePointMarker(Vector3 worldPosition, int index)
    {
        if (pointImagePrefab == null)
        {
            Debug.LogError("请设置点的Image预制体!");
            return;
        }

        // 确保startPoint有父节点
        if (startPoint.parent == null)
        {
            Debug.LogError("startPoint没有父节点!");
            return;
        }

        RectTransform startParent = startPoint.parent as RectTransform;

        // 实例化Image
        GameObject pointObj = Instantiate(pointImagePrefab.gameObject, worldPosition, Quaternion.identity);
        pointObj.transform.SetParent(startParent, false); // 设置父级为startPoint的父节点

        // 配置RectTransform
        RectTransform rect = pointObj.GetComponent<RectTransform>();
        if (rect != null)
        {
            // 设置锚点和轴心点为中心
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            
            // 设置大小
            rect.sizeDelta = new Vector2(pointSize, pointSize);
            
            // 转换为父节点的本地坐标（调整部分）
            Vector2 localPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                startParent, // 改为使用startPoint的父节点
                worldPosition, 
                canvas.worldCamera, 
                out localPosition
            );
            
            rect.anchoredPosition = localPosition;
        }

        pointObj.name = "Point_" + index;
        pointObjects[index] = pointObj;
    }

    void ClearPoints()
    {
        if (pointObjects != null)
        {
            foreach (var obj in pointObjects)
            {
                if (obj != null)
                    Destroy(obj);
            }
            pointObjects = null;
        }
    }

    void OnDrawGizmos()
    {
        // 在编辑模式下显示起点、终点和圆心
        if (startPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(startPoint.position, 5f);
        }

        if (endPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(endPoint.position, 5f);
        }

        if (rootPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(rootPoint.position, 7f);
        }
    }

    // 当Inspector中的值改变时调用
    void OnValidate()
    {
        isDirty = true;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;
using System.Collections;

public class SphereArcVisualizer : MonoBehaviour
{
    [Header("球体设置")]
    public Transform sphereCenter;  // 球心Transform
    public float sphereRadius = 50f; // 球体半径
    [SerializeField]
    private Transform start;
    [SerializeField]
    private Transform end;
    [Header("弧线端点")]
    public Vector3 startPoint;      // 起点坐标
    public Vector3 endPoint;        // 终点坐标
    
    [Header("可视化设置")]
    public bool drawArc = true;              // 是否绘制弧线
    public int segmentCount = 50;            // 弧线分段数
    public Color arcColor = Color.red;       // 弧线颜色
    public float lineWidth = 0.5f;           // 线宽
    public bool drawSphere = true;           // 是否绘制球体
    
    private LineRenderer lineRenderer;       // 线渲染器
    private GameObject sphereObject;         // 球体对象

    void Start()
    {
        startPoint = start.position;
        endPoint = end.position;
        // 确保点在球面上
        startPoint = (startPoint - sphereCenter.position).normalized * sphereRadius + sphereCenter.position;
        endPoint = (endPoint - sphereCenter.position).normalized * sphereRadius + sphereCenter.position;
        
        // 创建可视化对象
        if (drawSphere)
        {
            CreateSphere();
        }
        
        if (drawArc)
        {
            CreateArcRenderer();
            DrawSmoothArc();
        }
    }
    
    void OnDrawGizmos()
    {
        // 绘制球心和端点（编辑模式下）
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(sphereCenter.position, 1f);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(startPoint, 2f);
        Gizmos.DrawSphere(endPoint, 2f);
    }
    
    /// <summary>
    /// 创建球体对象
    /// </summary>
    private void CreateSphere()
    {
        sphereObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphereObject.transform.position = sphereCenter.position;
        sphereObject.transform.localScale = Vector3.one * sphereRadius * 2;
        sphereObject.GetComponent<Renderer>().material = new Material(Shader.Find("Standard"));
        sphereObject.GetComponent<Renderer>().material.color = new Color(0.2f, 0.2f, 0.8f, 0.3f); // 半透明蓝色
    }
    
    /// <summary>
    /// 创建弧线渲染器
    /// </summary>
    private void CreateArcRenderer()
    {
        GameObject arcObject = new GameObject("Smooth Arc");
        arcObject.transform.parent = sphereCenter;
        lineRenderer = arcObject.AddComponent<LineRenderer>();
        
        // 配置线渲染器
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.material.color = arcColor;
        lineRenderer.sortingOrder = 10;
    }
    
    /// <summary>
    /// 绘制球面上的平滑弧线
    /// </summary>
    private void DrawSmoothArc()
    {
        if (lineRenderer == null)
        {
            CreateArcRenderer();
        }
        
        // 获取插值函数
        var interpolator = SphereArcInterpolator.GetSphereArcInterpolator(
            sphereCenter.position, 
            sphereRadius, 
            startPoint, 
            endPoint
        );
        
        // 设置线渲染器点数量
        lineRenderer.positionCount = segmentCount + 1;
        
        // 生成弧线上的点
        for (int i = 0; i <= segmentCount; i++)
        {
            float t = (float)i / segmentCount;
            Vector3 arcPoint = interpolator(t);
            lineRenderer.SetPosition(i, arcPoint);
        }
    }
    
    /// <summary>
    /// 更新弧线（当端点变化时）
    /// </summary>
    public void UpdateArc()
    {
        // 确保点在球面上
        startPoint = (startPoint - sphereCenter.position).normalized * sphereRadius + sphereCenter.position;
        endPoint = (endPoint - sphereCenter.position).normalized * sphereRadius + sphereCenter.position;
        
        if (drawArc)
        {
            DrawSmoothArc();
        }
    }
}


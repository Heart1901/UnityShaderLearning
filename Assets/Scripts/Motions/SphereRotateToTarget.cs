using UnityEngine;
using System;

public class SphereRotateToTarget : MonoBehaviour
{
    [Header("目标角度")]
    public Vector3 targetPos;
    [Header("参考点")]
    public Transform cankao;          // 球体对象
    [Header("目标点")]
    public Transform target;          // 球体对象
    [Header("核心参数")]
    public Transform sphere;          // 球体对象
    public Transform mainCamera;      // 主摄像机
    public float rotateSpeed = 100f;  // 旋转速度（度/秒）
    public bool autoRotate = true;    // 自动开始旋转

    private bool isRotating = false;   // 旋转状态
    private Vector3 targetPoint;       // 球面上目标点的坐标
    Vector3 oa;
    Vector3 ob;
    bool isRunning = false;

    [Header("旋转参数")]
    public float totalAngle = 360f;     // 总旋转角度（逆时针）
    public float totalDuration = 2f;    // 总持续时间（秒）
    public int segmentCount = 2;        // 分段数量
    
    [Header("分段设置")]
    [Tooltip("每段的持续时间，总和必须等于totalDuration")]
    public float[] segmentDurations;    // 每段持续时间
    
    private float startTime;            // 开始时间
    private Quaternion startRotation;   // 初始旋转
    private Quaternion targetRotation;  // 目标旋转;

    public float cur_angle = 0;         // 当前已旋转角度
    private float angleThisFrame;       // 每帧旋转角度
    private float elapsedTime = 0f;     // 已用时间
    private float lastElapsedTime = 0f; // 上一帧已用时间
    private int currentSegment = 0;     // 当前段索引
    private float segmentStartTime;     // 当前段开始时间
    private float segmentAngle;         // 当前段应旋转角度

    // 运动完成回调事件
    public event Action MotionCompleted;
    public event Action<int> SegmentCompleted; // 段完成回调

    void Start()
    {
        // 确保球体中心正确
        if (sphere == null)
            sphere = transform;

        ob = VectorMath.CalculateNormal(cankao.position, sphere.position);
        oa = VectorMath.CalculateNormal(target.position, sphere.position);
        totalAngle = VectorMath.CalculateYAxisRotationAngle0To360(oa, ob);
        
        // 初始化分段设置
        InitializeSegments();
        
        Debug.Log($"计算的总旋转角度: {totalAngle}度, 持续时间: {totalDuration}秒, 分段数: {segmentCount}");

        if (autoRotate)
        {
            StartMotion(target.position);
        }
    }

    void Update()
    {
        if (isRunning)
        {
            // 计算当前段已用时间
            elapsedTime = Time.time - segmentStartTime;
            
            // 计算当前段进度 (0-1)
            float segmentProgress = Mathf.Clamp01(elapsedTime / segmentDurations[currentSegment]);
            
            // 计算上一帧到当前帧的进度增量
            float deltaProgress = segmentProgress - (lastElapsedTime / segmentDurations[currentSegment]);
            lastElapsedTime = elapsedTime;
            
            // 动态计算当前帧应旋转的角度（基于当前段进度增量）
            angleThisFrame = segmentAngle * deltaProgress;
            
            // 执行逆时针旋转
            transform.Rotate(Vector3.up, -angleThisFrame);
            
            // 更新当前已旋转角度
            cur_angle += angleThisFrame;
            
            // 检查当前段是否完成
            if (segmentProgress >= 1f)
            {
                Debug.Log($"完成第 {currentSegment+1}/{segmentCount} 段旋转, 旋转角度: {segmentAngle} 度");
                
                // 触发段完成回调
                SegmentCompleted?.Invoke(currentSegment);
                
                // 进入下一段
                currentSegment++;
                
                // 检查是否所有段都完成
                if (currentSegment >= segmentCount)
                {
                    // 确保最终角度精确
                    cur_angle = totalAngle;
                    transform.rotation = targetRotation;
                    
                    Debug.Log($"旋转完成: 目标角度 {totalAngle} 度, 实际旋转 {cur_angle} 度");
                    isRotating = false;
                    isRunning = false;
                    
                    MotionCompleted?.Invoke();
                    return;
                }
                
                // 开始新的段
                segmentStartTime = Time.time;
                lastElapsedTime = 0f;
                Debug.Log($"开始第 {currentSegment+1}/{segmentCount} 段旋转, 持续时间: {segmentDurations[currentSegment]} 秒");
            }
        }
    }

    // 开始运动，接收 targetPoint 作为参数
    public void StartMotion(Vector3 newTargetPoint)
    {
        targetPoint = newTargetPoint;
        isRotating = true;
        isRunning = true;
        startTime = Time.time;
        segmentStartTime = startTime;
        startRotation = transform.rotation;
        cur_angle = 0;
        elapsedTime = 0f;
        lastElapsedTime = 0f;
        currentSegment = 0;
        
        // 计算每段角度
        segmentAngle = totalAngle / segmentCount;
        
        // 计算目标旋转
        targetRotation = startRotation * Quaternion.Euler(0, -totalAngle, 0);

        Debug.Log($"开始旋转: 总角度 {totalAngle} 度, 持续时间 {totalDuration} 秒, 分为 {segmentCount} 段");
    }

    // 停止运动
    public void StopMotion()
    {
        isRotating = false;
        isRunning = false;
    }

    // 初始化分段设置
    private void InitializeSegments()
    {
        // 确保分段数有效
        segmentCount = Mathf.Max(1, segmentCount);
        
        // 初始化分段持续时间
        if (segmentDurations == null || segmentDurations.Length != segmentCount)
        {
            segmentDurations = new float[segmentCount];
            
            // 默认均分持续时间
            float defaultDuration = totalDuration / segmentCount;
            for (int i = 0; i < segmentCount; i++)
            {
                segmentDurations[i] = defaultDuration;
            }
        }
        
        // 验证总持续时间
        float sum = 0;
        foreach (float duration in segmentDurations)
        {
            sum += duration;
        }
        
        // 调整最后一段持续时间以确保总和等于总持续时间
        if (Mathf.Abs(sum - totalDuration) > 0.001f)
        {
            Debug.LogWarning($"分段持续时间总和 {sum} 不等于总持续时间 {totalDuration}，自动调整最后一段");
            segmentDurations[segmentCount - 1] += (totalDuration - sum);
        }
    }

    // 调试辅助：在场景中绘制方向线
    void OnDrawGizmosSelected()
    {
        if (sphere == null || mainCamera == null)
            return;

        Vector3 sphereCenter = sphere.position;

        // 绘制球心
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(sphereCenter, 0.1f);

        // 绘制目标点方向
        Gizmos.color = Color.red;
        Gizmos.DrawLine(sphereCenter, targetPoint);

        // 绘制摄像机方向
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(sphereCenter, mainCamera.position);

        // 绘制目标旋转方向
        if (isRotating)
        {
            Gizmos.color = Color.cyan;
            Vector3 targetDir = targetRotation * (targetPoint - sphereCenter).normalized;
            Gizmos.DrawLine(sphereCenter, sphereCenter + targetDir * 2f);
        }
    }
}



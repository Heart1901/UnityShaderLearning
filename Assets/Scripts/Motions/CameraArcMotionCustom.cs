using UnityEngine;
using System;

public class CameraArcMotionCustom : MonoBehaviour
{
    [Header("球体参数")]
    public Transform sphereCenter; // 球体中心 transform
    public float sphereRadius = 50f;
     [SerializeField]
    private Transform cankao;
    [SerializeField]
    private Transform start;
    [SerializeField]
    private Transform end;

    [Header("起点和终点")]
    public Vector3 startPoint = new Vector3(25f, 25f, 35.36f);
    // 若未手动设置，自动计算为 startPoint 关于球心的对称点
    public Vector3 endPoint; 

    [Header("运动参数")]
    public float duration = 5f;
    public float lookSmoothing = 5f; // 注视缓动系数，值越大转向越快
    public bool loop = false;

    [Header("高度曲线（控制相机距弧线点高度）")]
    public AnimationCurve heightCurve = AnimationCurve.Linear(0, 1, 1, 1);
    public float maxHeight = 10f; // 最大高度值

    [Header("偏移曲线（xyz 分量分别控制位置偏移）")]
    public AnimationCurve offsetXCurve = AnimationCurve.Linear(0, 0, 1, 0);
    public AnimationCurve offsetYCurve = AnimationCurve.Linear(0, 0, 1, 0);
    public AnimationCurve offsetZCurve = AnimationCurve.Linear(0, 0, 1, 0);
    public float maxOffset = 5f; // 最大偏移量

    private Func<float, Vector3> arcInterpolator;
    private float currentTime = 0f;
    private bool isPlaying = false;
    private Vector3 lastLookAtPoint; // 记录要注视的弧线点

    // 运动完成回调事件
    public event Action MotionCompleted;

    Vector3 direction;

    void Start()
    {
        direction = (cankao.transform.position - sphereCenter.position).normalized;
        startPoint = start.position;
        endPoint = end.position;
        // 自动计算对称终点逻辑（可选，若不需要可直接手动赋值 endPoint）
        if (endPoint == Vector3.zero)
        {
            Vector3 relativeStart = startPoint - sphereCenter.position;
            endPoint = sphereCenter.position - relativeStart;
        }

        // 初始化球面弧线插值器
        arcInterpolator = SphereArcInterpolator.GetSphereArcInterpolator(
            sphereCenter.position,
            sphereRadius,
            startPoint,
            endPoint
        );
        StartMotion(startPoint, endPoint);

        // 默认不自动开始，可通过 StartMotion 方法启动
        // isPlaying = true; 
    }

    void Update()
    {
        if (isPlaying)
        {
            currentTime += Time.deltaTime;
            float t = Mathf.Clamp01(currentTime / duration);

            UpdateCameraPositionAndRotation(t);

            // 循环逻辑
            if (currentTime >= duration)
            {
                if (loop)
                {
                    currentTime = 0f;
                }
                else
                {
                    isPlaying = false;
                    // 触发运动完成回调
                    MotionCompleted?.Invoke();
                }
            }
        }
    }

        // 计算与XZ平面平行的切线方向
    Vector3 CalculateTangentInXZPlane(Vector3 pointOnSphere, Vector3 sphereCenter)
    {
        // 将点投影到XZ平面（忽略Y轴）
        Vector3 projectionOnXZ = new Vector3(
            pointOnSphere.x - sphereCenter.x,
            0,
            pointOnSphere.z - sphereCenter.z
        ).normalized;
        
        // 特殊情况处理：若点在球心正上方/正下方（投影为零向量）
        if (projectionOnXZ == Vector3.zero)
            return Vector3.right; // 默认使用X轴方向
        
        // 返回与投影向量垂直的方向（在XZ平面内）
        return new Vector3(-projectionOnXZ.z, 0, projectionOnXZ.x);
    }

    public Vector3 RotateAroundYAxis(Vector3 originalVector, float angleDegrees, bool isClockwise)
    {
        // 处理顺时针：角度取反
        if (isClockwise)
            angleDegrees = -angleDegrees;

        // 构造绕Y轴旋转的四元数（角度制直接使用）
        Quaternion rotation = Quaternion.Euler(0, angleDegrees, 0);

        // 应用旋转
        return rotation * originalVector;
    }

    void UpdateCameraPositionAndRotation(float t)
    {



        // 步骤 1：根据高度曲线和方向（圆心到弧线点向量）确定基础位置
        Vector3 arcPoint = arcInterpolator(t);
        lastLookAtPoint = arcPoint; // 记录用于后续注视

        // 计算与XZ平面平行的切线方向（关键逻辑）
        Vector3 tangentInXZPlane = CalculateTangentInXZPlane(arcPoint, sphereCenter.position);

        // 计算与该切线垂直的法线（位于YZ平面或XY平面内）
        Vector3 normal = Vector3.Cross(tangentInXZPlane, Vector3.up).normalized;


        // 确保法线方向朝外（与径向方向一致）
        Vector3 radialDirection = (arcPoint - sphereCenter.position).normalized;
        if (Vector3.Dot(normal, radialDirection) < 0)
            normal = -normal;

        // 可视化法线
        Debug.DrawRay(arcPoint, normal * 500, Color.black);
        Vector3 newNormal = RotateAroundYAxis(normal, -10, true);

        Debug.DrawRay(arcPoint, newNormal * 500, Color.red);

        // 步骤 2：根据偏移曲线（xyz 分量）调整相机位置
        float offsetX = offsetXCurve.Evaluate(t) * maxOffset;
        float offsetY = offsetYCurve.Evaluate(t) * maxOffset;
        float offsetZ = offsetZCurve.Evaluate(t) * maxOffset;
        Vector3 dir = SphereGeometry.CalculateVerticalNormal(arcPoint, sphereCenter.position, 0);
        // 圆心到弧线点的单位方向向量
        Vector3 centerToArcDir = dir.normalized;//(arcPoint - sphereCenter.position).normalized; 
        float height = heightCurve.Evaluate(t) * maxHeight;

        Vector3 basePosition = arcPoint + newNormal * height;


        Vector3 finalPosition = basePosition + new Vector3(0, 30, 0);

        // 步骤 3：让相机注视弧线上的点
        Vector3 lookDirection = (lastLookAtPoint - finalPosition).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(lookDirection);

        // 平滑过渡旋转（可选，若要瞬间转向可直接赋值 transform.rotation = targetRotation）
        if (lookSmoothing > 0)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime * lookSmoothing
            );
        }
        else
        {
            transform.rotation = targetRotation;
        }

        // 更新相机位置
        transform.position = finalPosition;
    
    }

    // 开始运动，接收 startPoint 和 endPoint 作为参数
    public void StartMotion(Vector3 newStartPoint, Vector3 newEndPoint)
    {
        startPoint = newStartPoint;
        endPoint = newEndPoint;

        // 重新初始化球面弧线插值器
        arcInterpolator = SphereArcInterpolator.GetSphereArcInterpolator(
            sphereCenter.position, 
            sphereRadius, 
            startPoint, 
            endPoint
        );

        currentTime = 0f;
        isPlaying = true;
    }

    // 停止运动
    public void StopMotion()
    {
        isPlaying = false;
    }

    public void PauseMotion()
    {
        isPlaying = false;
    }

    public void ResetMotion()
    {
        currentTime = 0f;
        UpdateCameraPositionAndRotation(0f);
    }
}



public static class SphereGeometry
{
    /// <summary>
    /// 计算与xz平面平行的切线方向，并返回垂直于该切线且指向球外的法向量
    /// </summary>
    /// <param name="pointOnSphere">球面上的点坐标</param>
    /// <param name="sphereCenter">球心坐标</param>
    /// <param name="radius">球体半径</param>
    /// <returns>垂直于xz平面切线且指向球外的法向量</returns>
    public static Vector3 CalculateVerticalNormal(Vector3 pointOnSphere, Vector3 sphereCenter, float radius)
    {
        // 计算从球心指向球面点的方向向量（未归一化）
        Vector3 radialVector = pointOnSphere - sphereCenter;

        // 确保点确实在球面上（避免浮点误差）
        radialVector = radialVector.normalized * radius;

        // 计算在xz平面上的切线方向（与xz平面平行）
        // 方法：将径向向量投影到xz平面，然后取其垂直向量
        Vector3 tangentInXZPlane = new Vector3(-radialVector.z, 0, radialVector.x).normalized;

        // 计算垂直于切线且指向球外的法向量
        // 方法：使用叉乘，将切线方向与y轴叉乘得到法向量
        Vector3 normal = Vector3.Cross(tangentInXZPlane, Vector3.up).normalized;

        // 确保法向量方向朝外（与径向方向一致）
        if (Vector3.Dot(normal, radialVector) < 0)
            normal = -normal;

        return normal;
    }
}

public static class SphereArcInterpolator
{
    /// <summary>
    /// 获取球面两点间平滑（Y分量变化最小）的顺时针弧线插值函数
    /// </summary>
    public static Func<float, Vector3> GetSphereArcInterpolator(
        Vector3 center, float radius, Vector3 start, Vector3 end)
    {
        // 归一化到球面
        Vector3 normalizedStart = (start - center).normalized * radius + center;
        Vector3 normalizedEnd = (end - center).normalized * radius + center;

        // 计算球心指向两点的向量
        Vector3 vecStart = normalizedStart - center;
        Vector3 vecEnd = normalizedEnd - center;

        // 计算使Y分量变化最小的旋转轴
        Vector3 rotationAxis = CalculateOptimalRotationAxis(vecStart, vecEnd);

        // 计算顺时针旋转角度
        float angle = CalculateClockwiseAngle(vecStart, vecEnd, rotationAxis);

        return (t) =>
        {
            t = Mathf.Clamp01(t);
            float currentAngle = angle * t;

            // 绕轴旋转生成点
            Quaternion rotation = Quaternion.AngleAxis(currentAngle, rotationAxis);
            Vector3 rotatedVector = rotation * vecStart;

            return center + rotatedVector.normalized * radius;
        };
    }

    /// <summary>
    /// 计算使Y分量变化最小的旋转轴
    /// </summary>
    private static Vector3 CalculateOptimalRotationAxis(Vector3 start, Vector3 end)
    {
        // 默认旋转轴（两点叉乘）
        Vector3 defaultAxis = Vector3.Cross(start, end).normalized;

        // 计算Y轴变化量（使用默认轴）
        float defaultYVariation = CalculatePotentialYVariation(start, end, defaultAxis);

        // 尝试寻找更好的轴：基于XZ平面投影
        Vector3 xzAxis = Vector3.Cross(
            new Vector3(start.x, 0, start.z).normalized,
            new Vector3(end.x, 0, end.z).normalized
        ).normalized;

        float xzYVariation = CalculatePotentialYVariation(start, end, xzAxis);

        // 尝试寻找更好的轴：基于XY平面投影
        Vector3 xyAxis = Vector3.Cross(
            new Vector3(start.x, start.y, 0).normalized,
            new Vector3(end.x, end.y, 0).normalized
        ).normalized;

        float xyYVariation = CalculatePotentialYVariation(start, end, xyAxis);

        // 选择Y分量变化最小的轴
        if (xzYVariation < defaultYVariation && xzYVariation < xyYVariation)
            return xzAxis;
        else if (xyYVariation < defaultYVariation)
            return xyAxis;
        else
            return defaultAxis;
    }

    /// <summary>
    /// 计算使用特定轴旋转时的潜在Y分量变化
    /// </summary>
    private static float CalculatePotentialYVariation(Vector3 start, Vector3 end, Vector3 axis)
    {
        // 计算从起点到终点的中间点（t=0.5）的Y值变化
        Quaternion rotation = Quaternion.AngleAxis(Vector3.Angle(start, end) * 0.5f, axis);
        Vector3 midPoint = rotation * start;

        // 计算Y分量的绝对变化
        return Mathf.Abs(midPoint.y - start.y) + Mathf.Abs(midPoint.y - end.y);
    }

    /// <summary>
    /// 计算顺时针旋转角度（0-360度）
    /// </summary>
    private static float CalculateClockwiseAngle(Vector3 start, Vector3 end, Vector3 axis)
    {
        // 计算两向量之间的角度
        float angle = Vector3.Angle(start, end);

        // 确定旋转方向是否为顺时针（使用叉积和轴的点积）
        Vector3 cross = Vector3.Cross(start, end);
        if (Vector3.Dot(cross, axis) < 0)
        {
            angle = 360f - angle; // 转为顺时针角度
        }

        return angle;
    }
}
//球体：自传转到终点
//球体：终点转起点，镜头逐渐拉近
//相机：沿着最短弧线运动，地球不转
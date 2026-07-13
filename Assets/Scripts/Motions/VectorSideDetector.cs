using UnityEngine;

public class SphereRotationController : MonoBehaviour
{
    // 判断点在向量哪一侧（原有逻辑，保持不变）
    public static int GetSide(Transform center, Transform start, Transform target)
    {
        Vector3 oa = start.position - center.position;
        Vector3 ot = target.position - center.position;

        if (oa.magnitude < 0.0001f || ot.magnitude < 0.0001f)
            return 0;

        oa = oa.normalized;
        ot = ot.normalized;

        Vector3 crossProduct = Vector3.Cross(oa, ot);
        Vector3 referenceDirection = Vector3.up;

        float dotResult = Vector3.Dot(crossProduct, referenceDirection);

        if (Mathf.Approximately(dotResult, 0f))
            return 0;
        else if (dotResult < 0)
            return 1; 
        else
            return -1; 
    }

    // 计算 0~360° 角度（原有逻辑，保持不变）
    public static float GetAngleAndSide(Transform center, Transform start, Transform target)
    {
        Vector3 oa = start.position - center.position;
        Vector3 ot = target.position - center.position;

        float angle = Vector3.Angle(oa, ot);

        int side = GetSide(center, start, target);
        if (side == -1)
            angle = 360f - angle; 

        return angle;
    }

    [Header("旋转参数")]
    public Transform center;       // 球心（要旋转的球体）
    public Transform start;        // 起点
    public Transform target;       // 目标点
    public float rotationSpeed = 90f; // 旋转速度（度/秒）
    
    public System.Action onRotationComplete; // 旋转完成回调

    private Quaternion startRotation;
    private Quaternion targetRotation;
    private bool isRotating = false;
    private float elapsedTime = 0f;
    private float totalRotationAngle = 0f; // 记录需要旋转的总角度（0~360）
    public AnimationCurve angleCurve; // 在Inspector拖拽曲线
public float totalDuration = 2f;  // 总持续时间（秒）


    void Start()
    {
        isRotating = true;
        StartRotation(); // 自动开始旋转
    }

    /// <summary>
    /// 开始旋转：计算角度 → 初始化参数
    /// </summary>
    float start_angle = 0f;
    public void StartRotation()
    {
        if (center == null || start == null || target == null)
        {
            Debug.LogError("请确保 center、start、target 已赋值！");
            return;
        }

        // 1. 计算需要旋转的总角度（0~360度）
        float angle = GetAngleAndSide(center, start, target);
        totalRotationAngle = 360f - angle; 

        // 2. 计算目标数学角度（-totalRotationAngle242°）
        float targetMathAngle = totalRotationAngle; 
        Debug.Log($"需要逆时针旋转: {totalRotationAngle}° → 目标数学角度: {targetMathAngle}°");

        // 3. 初始化起始和目标旋转
        startRotation = center.rotation;
        targetRotation = Quaternion.Euler(
            startRotation.eulerAngles.x, 
            targetMathAngle, // 直接设置目标数学角度
            startRotation.eulerAngles.z
        );

        // 4. 重置插值参数
        elapsedTime = 0f;
        isRotating = true;
    }

    void Update()
    {
        if (isRotating)
        {
            elapsedTime += Time.deltaTime;

            float curveT = elapsedTime / totalDuration; 
        // 获取曲线当前值（0~1映射到曲线的Y值）
        float curveValue = angleCurve.Evaluate(curveT); 

        float a = 10.5f;
        float b = 10.3f;
        float tolerance = 0.5f; // 允许的最大差值
            float end_angle = -totalRotationAngle;
        bool areClose = Mathf.Abs(start_angle - end_angle) < tolerance;
            if (areClose)
            {
                Debug.Log($"a 和 b 在 {tolerance} 的范围内");
                return;
        }
            else
            {
                Debug.Log($"a 和 b 超出 {tolerance} 的范围");
            }

            float t = curveValue ;
            t = Time.deltaTime * curveValue;
            start_angle -= t;
     
       
            // if (start_angle == -totalRotationAngle)
            // {
            //     return;
            // }
            
            center.rotation = Quaternion.Euler(0, start_angle, 0);
            // // 计算旋转时长（总角度 / 速度）
            // float duration = Mathf.Abs(totalRotationAngle) / rotationSpeed; 
            // elapsedTime += Time.deltaTime;

            // // 计算插值进度（0~1）
            // float t = Mathf.Clamp01(elapsedTime / duration);

            // // 球面插值（确保旋转方向正确）
            // center.rotation = Quaternion.Slerp(startRotation, targetRotation, t);

            // // 旋转完成判断
            // if (t >= 1f)
            // {
            //     isRotating = false;
            //     // 手动转换为数学角度显示
            //     float finalY = NormalizeAngle(center.rotation.eulerAngles.y);
            //     Debug.Log($"旋转完成，最终数学角度: {finalY}° → Unity 显示角度: {center.rotation.eulerAngles.y}°");
            //     onRotationComplete?.Invoke();
            // }
        }
    }

    /// <summary>
    /// 将 Unity 显示的欧拉角转换为数学角度（-180~180）
    /// </summary>
    private float NormalizeAngle(float angle)
    {
        while (angle > 180) angle -= 360;
        while (angle <= -180) angle += 360;
        return angle;
    }

    /// <summary>
    /// 可视化调试：绘制旋转方向
    /// </summary>
    void OnDrawGizmos()
    {
        if (center == null) return;

        // 绘制旋转轴（Y轴）
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(center.position, center.position + Vector3.up * 2);

        // 绘制旋转方向（逆时针）
        if (isRotating)
        {
            Vector3 axis = Vector3.up;
            Vector3 startPoint = center.position + center.right;
            Gizmos.color = Color.blue;

            for (int i = 0; i < 12; i++)
            {
                float angle1 = i * 30f;
                float angle2 = (i + 1) * 30f;
                Vector3 p1 = center.position + Quaternion.AngleAxis(angle1, axis) * center.right;
                Vector3 p2 = center.position + Quaternion.AngleAxis(angle2, axis) * center.right;
                Gizmos.DrawLine(p1, p2);
            }
        }
    }
}
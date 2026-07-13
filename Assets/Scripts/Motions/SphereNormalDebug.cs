using UnityEngine;

public class TangentNormalVisualizer : MonoBehaviour
{
    public Transform sphere;          // 球体（球心位置）
    public Transform pointOnSphere;   // 球面上的点
    public float normalLength = 1f;   // 法线显示长度
    public Color normalColor = Color.black; // 法线颜色
    
    void Update()
    {
        if (sphere == null || pointOnSphere == null) return;
        
        // 计算与XZ平面平行的切线方向（关键逻辑）
        Vector3 tangentInXZPlane = CalculateTangentInXZPlane(pointOnSphere.position, sphere.position);
        
        // 计算与该切线垂直的法线（位于YZ平面或XY平面内）
        Vector3 normal = Vector3.Cross(tangentInXZPlane, Vector3.up).normalized;
        
        // 确保法线方向朝外（与径向方向一致）
        Vector3 radialDirection = (pointOnSphere.position - sphere.position).normalized;
        if (Vector3.Dot(normal, radialDirection) < 0)
            normal = -normal;
        
        // 可视化法线
        Debug.DrawRay(pointOnSphere.position, normal * normalLength, normalColor);
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
}
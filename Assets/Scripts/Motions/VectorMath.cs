using UnityEngine;

public static class VectorMath
{
    /// <summary>
    /// 计算向量OA绕Y轴旋转到向量OB的0-360度角度（考虑左右位置）
    /// </summary>
    /// <param name="oa">起始向量</param>
    /// <param name="ob">目标向量</param>
    /// <returns>旋转角度（0-360度），当OA在OB右侧时返回360-角度差</returns>
    public static float CalculateYAxisRotationAngle0To360(Vector3 oa, Vector3 ob)
    {
        // 1. 提取XZ分量并归一化
        Vector2 oaXZ = new Vector2(oa.x, oa.z).normalized;
        Vector2 obXZ = new Vector2(ob.x, ob.z).normalized;

        // 2. 计算无向角度（0-180度）
        float unsignedAngle = Vector2.Angle(oaXZ, obXZ);

        // 3. 确定旋转方向（叉积判断顺时针/逆时针）
        //    叉积为负表示OA在OB的顺时针方向（右侧）
        float crossProduct = oaXZ.x * obXZ.y - oaXZ.y * obXZ.x;
        bool isClockwise = crossProduct < 0;

        // 4. 计算带符号的角度（-180到180度）
        float signedAngle = isClockwise ? -unsignedAngle : unsignedAngle;

        // 5. 转换为0-360度范围
        float angle0To360 = signedAngle + 360f;
        angle0To360 %= 360f;

        // 6. 处理右侧情况：如果是顺时针旋转，返回360-角度
        //    （此处逻辑根据用户描述"右侧返回360-t"实现）
        if (isClockwise)
        {
            angle0To360 = 360f - unsignedAngle;
        }

        return angle0To360;
    }

    /// <summary>
    /// 计算向量OA绕Y轴旋转到向量OB的最小正角度（0-360度）
    /// </summary>
    public static float CalculateMinPositiveAngle(Vector3 oa, Vector3 ob)
    {
        float signedAngle = CalculateYAxisRotationAngle(oa, ob);
        return signedAngle >= 0 ? signedAngle : 360f + signedAngle;
    }

    /// <summary>
    /// 计算带符号的旋转角度（-180到180度）
    /// </summary>
    public static float CalculateYAxisRotationAngle(Vector3 oa, Vector3 ob)
    {
        Vector2 oaXZ = new Vector2(oa.x, oa.z).normalized;
        Vector2 obXZ = new Vector2(ob.x, ob.z).normalized;

        float angle = Vector2.Angle(oaXZ, obXZ);
        float cross = oaXZ.x * obXZ.y - oaXZ.y * obXZ.x;
        return cross > 0 ? angle : -angle;
    }

    // 计算与XZ平面平行的切线方向
    public static Vector3 CalculateTangentInXZPlane(Vector3 pointOnSphere, Vector3 sphereCenter)
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

    public static Vector3 CalculateNormal(Vector3 arcPoint, Vector3 sphereCenter)
    { 
        // 计算与XZ平面平行的切线方向（关键逻辑）
        Vector3 tangentInXZPlane = CalculateTangentInXZPlane(arcPoint, sphereCenter);
        Vector3 normal = Vector3.Cross(tangentInXZPlane, Vector3.up).normalized;

        // 确保法线方向朝外（与径向方向一致）
        Vector3 radialDirection = (arcPoint - sphereCenter).normalized;
        if (Vector3.Dot(normal, radialDirection) < 0)
            normal = -normal;
        return normal;
    }
}
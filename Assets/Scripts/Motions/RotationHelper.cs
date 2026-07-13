using UnityEngine;
using System.Collections;

public static class RotationHelper
{
    /// <summary>
    /// 绕Y轴逆时针旋转指定角度，按速度曲线控制旋转速度
    /// </summary>
    /// <param name="target">旋转目标对象</param>
    /// <param name="totalAngle">总旋转角度（逆时针为正）</param>
    /// <param name="totalDuration">总持续时间（秒）</param>
    /// <param name="speedCurve">速度曲线（0-1对应0-总时长，值对应速度比例）</param>
    /// <returns>协程引用</returns>
    public static IEnumerator RotateAroundYWithCurve(
        Transform target, 
        float totalAngle, 
        float totalDuration, 
        AnimationCurve speedCurve)
    {
        if (target == null || totalDuration <= 0f || speedCurve == null)
            yield break;
            
        float currentTime = 0f;
        float remainingAngle = totalAngle;
        
        while (currentTime < totalDuration)
        {
            // 计算当前时间比例 (0-1)
            float timeRatio = currentTime / totalDuration;
            
            // 获取当前速度比例（通过速度曲线）
            float speedRatio = speedCurve.Evaluate(timeRatio);
            
            // 计算当前帧的旋转量（考虑速度曲线和时间增量）
            float deltaTime = Time.deltaTime;
            float angleThisFrame = totalAngle * speedRatio * (deltaTime / totalDuration);
            
            // 执行旋转（注意：rotate(-angle) 实现逆时针旋转）
            target.Rotate(Vector3.up, -angleThisFrame);
            
            // 更新剩余角度和时间
            remainingAngle -= angleThisFrame;
            currentTime += deltaTime;
            
            // 处理浮点数误差
            if (Mathf.Abs(remainingAngle) < 0.1f)
            {
                target.Rotate(Vector3.up, -remainingAngle);
                break;
            }
            
            yield return null;
        }
        
        // 确保最终角度准确
        if (Mathf.Abs(remainingAngle) > 0.1f)
        {
            target.Rotate(Vector3.up, -remainingAngle);
        }
    }
    
    /// <summary>
    /// 绕Y轴逆时针旋转指定角度，使用线性速度（匀速旋转）
    /// </summary>
    public static IEnumerator RotateAroundYLinearly(
        Transform target, 
        float totalAngle, 
        float totalDuration)
    {
        return RotateAroundYWithCurve(
            target, 
            totalAngle, 
            totalDuration, 
            AnimationCurve.Linear(0, 1, 1, 1)
        );
    }
}
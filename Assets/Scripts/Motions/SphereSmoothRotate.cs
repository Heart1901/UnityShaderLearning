using UnityEngine;
using System.Collections;

public class SphereSmoothRotate : MonoBehaviour
{
    // 球体的 Transform（在 Inspector 拖入场景中的 Sphere）
    public Transform sphereTransform; 

    // 目标旋转角度（Y 轴），可在 Inspector 调整
    public float targetYRotation = -242f; 

    // 旋转速度（度/秒），可在 Inspector 调整
    public float rotationSpeed = 60f; 

    // 标记：是否正在旋转
    private bool isRotating = false; 

    void Start()
    {
        // 启动协程，开始平滑旋转
        StartCoroutine(RotateSphereSmoothly()); 
    }

    /// <summary>
    /// 协程：平滑旋转球体（仅绕 Y 轴逆时针插值）
    /// </summary>
    private IEnumerator RotateSphereSmoothly()
    {
        isRotating = true;

        // 记录初始旋转（四元数）
        Quaternion startRotation = sphereTransform.rotation; 

        // 计算目标旋转（仅修改 Y 轴，保持 X、Z 轴不变）
        Quaternion targetRotation = Quaternion.Euler(
            sphereTransform.rotation.eulerAngles.x, 
            targetYRotation, 
            sphereTransform.rotation.eulerAngles.z
        ); 

        // 计算总旋转角度（用于控制插值进度）
        float totalAngle = Mathf.DeltaAngle(
            startRotation.eulerAngles.y, 
            targetRotation.eulerAngles.y
        ); 
        // 若总角度为负，说明是逆时针（Unity 中角度计算特性，可直接用 Mathf.Abs 处理）
        totalAngle = Mathf.Abs(totalAngle); 

        // 记录开始时间
        float elapsedTime = 0f; 

        while (elapsedTime < totalAngle)
        {
            // 计算插值进度（0 ~ 1）
            float t = elapsedTime / totalAngle; 

            // 球面插值（平滑过渡）
            sphereTransform.rotation = Quaternion.Slerp(
                startRotation, 
                targetRotation, 
                t
            ); 

            // 累加旋转角度（按速度推进）
            elapsedTime += rotationSpeed * Time.deltaTime; 

            yield return null; // 下一帧继续
        }

        // 确保最终角度精准到位
        sphereTransform.rotation = targetRotation; 

        isRotating = false;
        Debug.Log("旋转完成！最终 Y 轴角度：" + sphereTransform.rotation.eulerAngles.y);
    }

    // 可选：暴露给外部调用的“开始旋转”方法
    public void StartRotation()
    {
        if (!isRotating)
        {
            StartCoroutine(RotateSphereSmoothly());
        }
    }
}
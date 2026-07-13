using UnityEngine;
using System;

public class RotateToMultipleOf360 : MonoBehaviour
{
    [Tooltip("旋转到目标角度的持续时间（秒）")]
    public float duration = 2f;

    [Tooltip("角度容差，用于判断是否到达目标角度")]
    [Range(0.01f, 1f)]
    public float angleTolerance = 0.1f;

    // 标记运动是否正在进行
    private bool isMoving = false;
    // 运动完成事件
    public event Action MotionCompleted;

    // 插值相关变量
    private float startAngleY;  // 运动开始时的 Y 角度
    private float targetAngleY; // 目标 Y 角度（360 的倍数）
    private float elapsedTime;  // 已用时间


    void Start()
    {
        // 默认启动运动，可在外部调用 startMotion() 灵活控制
        startMotion();
    }


    // 开始旋转运动
    public void startMotion()
    {
        isMoving = true;
        elapsedTime = 0f;
        startAngleY = transform.rotation.eulerAngles.y;

        // 计算最近的 360 倍数目标角度
        targetAngleY = Mathf.Round(startAngleY / 360f) * 360f;

        // 处理特殊情况：如果初始角度已经接近目标，直接完成
        if (Mathf.Abs(Mathf.DeltaAngle(startAngleY, targetAngleY)) < angleTolerance)
        {
            stopMotion();
            MotionCompleted?.Invoke();
        }
    }


    // 停止旋转运动
    public void stopMotion()
    {
        isMoving = false;
        elapsedTime = 0f;
    }


    void Update()
    {
        if (!isMoving) return;

        // 更新已用时间
        elapsedTime += Time.deltaTime;
        float progress = Mathf.Clamp01(elapsedTime / duration);

        // 使用平滑插值旋转到目标角度
        float currentAngleY = Mathf.LerpAngle(startAngleY, targetAngleY, progress);
        transform.rotation = Quaternion.Euler(0, currentAngleY, 0);

        // 检查是否到达目标角度（容差范围内）
        if (progress >= 1f || Mathf.Abs(Mathf.DeltaAngle(currentAngleY, targetAngleY)) < angleTolerance)
        {
            stopMotion();
            MotionCompleted?.Invoke();
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBreathing : MonoBehaviour
{
    public float amplitude = 0.05f;  // 抖动的振幅
    public float frequency = 0.5f;   // 呼吸的频率

    private Vector3 originalPosition;  // 记录初始位置

    void Start()
    {
        originalPosition = transform.localPosition;  // 保存初始本地位置
    }

    void Update()
    {
        // 计算新的Y位置
        float newY = originalPosition.y + Mathf.Sin(Time.time * Mathf.PI * frequency) * amplitude;

        // 更新摄像机的位置
        transform.localPosition = new Vector3(originalPosition.x, newY, originalPosition.z);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovable : MonoBehaviour
{
    public float mouseSensitivity = 100.0f;  // 鼠标灵敏度
    public float clampAngle = 80.0f;         // 最大垂直旋转角度（上下查看限制）

    private float verticalRotation = 0.0f;   // 垂直方向旋转
    private float horizontalRotation = 0.0f; // 水平方向旋转

    void Start()
    {
        Vector3 rot = transform.localRotation.eulerAngles;
        horizontalRotation = rot.y;
        verticalRotation = rot.x;
    }

    void Update()
    {
        // 获取鼠标输入
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // 累计旋转量
        horizontalRotation += mouseX;
        verticalRotation -= mouseY;

        // 限制垂直旋转角度
        verticalRotation = Mathf.Clamp(verticalRotation, -clampAngle, clampAngle);

        // 创建旋转
        Quaternion localRotation = Quaternion.Euler(verticalRotation, horizontalRotation, 0.0f);
        transform.rotation = localRotation;
    }
}

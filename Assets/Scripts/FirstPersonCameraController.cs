using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 第一人称相机控制器 - 控制相机的跟随和视角
/// </summary>
public class FirstPersonCameraController : MonoBehaviour
{
    [Header("目标设置")]
    [Tooltip("跟随目标")]
    public Transform target;
    
    [Tooltip("头部骨骼引用")]
    public Transform headBone;
    
    [Header("相机设置")]
    [Tooltip("相机引用")]
    public Camera mainCamera;
    
    [Tooltip("相机灵敏度")]
    public float sensitivity = 2.0f;
    
    [Tooltip("视角平滑度")]
    public float smoothing = 2.0f;
    
    [Tooltip("相机偏移量（从头部骨骼）")]
    public Vector3 headOffset = new Vector3(0, 0.1f, 0); // 从头部骨骼的小偏移，微调位置
    
    [Tooltip("最小视角（负值表示可以看向脚下）")]
    public float minimumY = -60f;
    
    [Tooltip("最大视角")]
    public float maximumY = 60f;
    
    [Header("状态")]
    [Tooltip("是否锁定到讲台视角")]
    public bool lockToPodiumView = false;
    
    // 视角旋转
    public Vector2 rotation = Vector2.zero;
    
    // 当前视角平滑值
    public Vector2 currentRotation = Vector2.zero;
    
    // 旋转速度
    private Vector2 rotationVelocity = Vector2.zero;
    
    // 讲台模式下的固定视角
    private Vector3 podiumViewEuler = new Vector3(5, 0, 0); // 稍微向下看，面向前方
    
    // 原始相机位置
    public Vector3 originalPosition;
    
    // 原始相机旋转
    public Quaternion originalRotation;
    
    // 讲演者控制器
    private PresenterController presenterController;
    
    private void Awake()
    {
        // 如果没有指定相机，获取Main Camera
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("找不到主相机！请手动指定相机引用。");
                enabled = false;
                return;
            }
        }
        
        // 初始化相机设置
        originalPosition = mainCamera.transform.position;
        originalRotation = mainCamera.transform.rotation;
        
        // 初始化旋转值
        // rotation.y = mainCamera.transform.eulerAngles.x;
        // rotation.x = mainCamera.transform.eulerAngles.y;
        currentRotation = rotation;
        
        // 将讲台视角修改为面向前方（Z轴正方向）
        podiumViewEuler = new Vector3(0,0,-5); // 稍微向下看，面向前方
    }
    
    private void Start()
    {
        // 设置鼠标隐藏和锁定
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // 如果没有在Awake中设置，则保存初始相机位置和旋转
        if (originalPosition == Vector3.zero)
        {
            originalPosition = mainCamera.transform.position;
            
            // 初始化相机朝向为向前（Z轴正方向）
            mainCamera.transform.rotation = Quaternion.Euler(0, 0, 0);
            originalRotation = mainCamera.transform.rotation;
            
            Debug.Log("已将相机初始朝向调整为向前");
        }
        
        // 初始化旋转值为初始向前方向
        // rotation.y = mainCamera.transform.eulerAngles.x; // 垂直方向
        // rotation.x = mainCamera.transform.eulerAngles.y; // 水平方向
        currentRotation = rotation;
        
        // 相机控制器已挂在演讲者上，直接获取同一对象上的PresenterController组件
        presenterController = GetComponent<PresenterController>();
        if (presenterController != null)
        {
            Debug.Log("获取到挂载的PresenterController组件");
            
            // 如果没有设置目标，可以使用当前对象作为目标
            if (target == null)
            {
                // 尝试使用cameraTarget如果有设置
                target = presenterController.cameraTarget != null ? 
                    presenterController.cameraTarget : transform;
                
                Debug.Log($"设置相机目标为: {target.name}");
            }
            
            // 查找头部骨骼
            if (headBone == null)
            {
                FindHeadBone();
            }
        }
        else
        {
            Debug.LogWarning("未找到PresenterController组件，某些功能可能无法正常工作");
        }
    }
    
    /// <summary>
    /// 尝试查找角色头部骨骼
    /// </summary>
    private void FindHeadBone()
    {
        Animator animator = GetComponentInChildren<Animator>();
        if (animator != null && animator.isHuman)
        {
            // 从标准人形骨骼中获取头部骨骼
            headBone = animator.GetBoneTransform(HumanBodyBones.Head);
            if (headBone != null)
            {
                Debug.Log($"自动找到头部骨骼: {headBone.name}");
            }
            else
            {
                Debug.LogWarning("未找到头部骨骼，将使用固定偏移量");
            }
        }
        else
        {
            // 尝试通过名称查找
            Transform[] allChildren = GetComponentsInChildren<Transform>();
            foreach (Transform child in allChildren)
            {
                if (child.name.ToLower().Contains("head"))
                {
                    headBone = child;
                    Debug.Log($"通过名称找到头部骨骼: {headBone.name}");
                    break;
                }
            }
            
            if (headBone == null)
            {
                Debug.LogWarning("未找到头部骨骼，将使用固定偏移量");
            }
        }
    }
    
    private void LateUpdate()
    {
        // 如果找不到相机，不执行更新
        if (mainCamera == null) return;
        
        // 根据相机模式选择不同的更新方法
        if (presenterController != null && presenterController.startedPresentation)
        {
            lockToPodiumView = true;
        }
        
        // 根据模式选择不同的更新方法
        if (lockToPodiumView)
        {
            UpdatePodiumCameraView();
        }
        else
        {
            UpdateFirstPersonCameraView();
        }
    }
    
    /// <summary>
    /// 更新第一人称相机视角
    /// </summary>
    private void UpdateFirstPersonCameraView()
    {
        // 获取鼠标输入
        Vector2 input = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        
        // 按灵敏度缩放输入
        input *= sensitivity;
        
        // 平滑处理
        input = Vector2.Scale(input, new Vector2(1f, 1f));
        
        // 计算旋转值
        rotation.x += input.x;
        rotation.y += -input.y; // 垂直轴是反向的
        
        // 限制垂直视角
        rotation.y = Mathf.Clamp(rotation.y, minimumY, maximumY);
        
        // 应用平滑
        currentRotation = Vector2.SmoothDamp(currentRotation, rotation, ref rotationVelocity, 1f / smoothing);
        
        // 应用旋转到相机
        mainCamera.transform.rotation = Quaternion.Euler(currentRotation.y, currentRotation.x, 0);
        
        // 如果有presenterController，同步水平旋转到角色（只有当角色不在移动时）
        if (presenterController != null && presenterController.stoodUp)
        {
            // 获取当前输入状态
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            
            // 只有当角色不移动时才同步相机旋转
            if (Mathf.Abs(horizontal) < 0.1f && Mathf.Abs(vertical) < 0.1f)
            {
                // 只旋转Y轴（水平旋转）
                Quaternion targetRotation = Quaternion.Euler(0, currentRotation.x, 0);
                
                // 平滑应用旋转
                presenterController.transform.rotation = Quaternion.Slerp(
                    presenterController.transform.rotation, 
                    targetRotation, 
                    Time.deltaTime * 5f // 调整这个值可以改变旋转速度
                );
            }
        }
        
        // 设置相机位置，优先使用头部骨骼
        if (headBone != null)
        {
            // 使用头部骨骼位置+小偏移
            mainCamera.transform.position = headBone.position + headOffset;
        }
        else if (target != null)
        {
            // 退回到使用目标位置+固定偏移量
            Vector3 defaultHeadOffset = new Vector3(0, 1.7f, 0); // 默认头部高度
            mainCamera.transform.position = target.position + defaultHeadOffset;
        }
        else if (presenterController != null)
        {
            // 如果没有目标但有演讲者控制器，使用演讲者位置
            Vector3 presenterPosition = presenterController.transform.position;
            // 应用固定的头部高度偏移
            presenterPosition.y += 1.7f; // 固定头部高度
            mainCamera.transform.position = presenterPosition;
        }
    }
    
    /// <summary>
    /// 更新讲台相机视角
    /// </summary>
    private void UpdatePodiumCameraView()
    {
        // 确定目标位置
        Vector3 targetPosition;
        if (headBone != null)
        {
            // 使用头部骨骼位置
            targetPosition = headBone.position + headOffset;
        }
        else if (target != null)
        {
            // 退回到目标位置+固定偏移量
            Vector3 defaultHeadOffset = new Vector3(0, 1.7f, 0); // 默认头部高度
            targetPosition = target.position + defaultHeadOffset;
        }
        else if (presenterController != null)
        {
            // 如果没有目标但有演讲者控制器，使用演讲者位置
            targetPosition = presenterController.transform.position;
            targetPosition.y += 1.7f; // 固定头部高度
        }
        else
        {
            // 无目标也无演讲者控制器，使用当前位置（保持不变）
            targetPosition = mainCamera.transform.position;
        }
        
        // 平滑过渡到目标视角 - 注意这里podiumViewEuler是讲台模式的固定朝向
        mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetPosition, Time.deltaTime * 5);
        // mainCamera.transform.rotation = Quaternion.Slerp(mainCamera.transform.rotation, Quaternion.Euler(podiumViewEuler), Time.deltaTime * 5);
    }
    
    /// <summary>
    /// 切换到讲台视角模式
    /// </summary>
    public void SwitchToPodiumView()
    {
        lockToPodiumView = true;
        
        // 解锁鼠标，允许使用UI
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    /// <summary>
    /// 切换到第一人称模式
    /// </summary>
    public void SwitchToFirstPersonView()
    {
        lockToPodiumView = false;
        
        // 锁定鼠标，用于视角控制
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    /// <summary>
    /// 重置相机
    /// </summary>
    public void ResetCamera()
    {
        // 恢复原始位置和旋转
        mainCamera.transform.position = originalPosition;
        mainCamera.transform.rotation = originalRotation;
        
        // 重置旋转值
        rotation = new Vector2(mainCamera.transform.eulerAngles.y, mainCamera.transform.eulerAngles.x);
        currentRotation = rotation;
        
        // 重置锁定状态
        lockToPodiumView = false;
    }
    
    /// <summary>
    /// 设置相机的跟随目标
    /// </summary>
    public void SetCameraTarget(Transform newTarget)
    {
        target = newTarget;
        
        // 更新讲演者控制器引用
        if (target != null)
        {
            presenterController = target.GetComponent<PresenterController>();
            
            // 尝试查找头部骨骼
            if (headBone == null)
            {
                FindHeadBone();
            }
        }
    }
    
    /// <summary>
    /// 手动设置头部骨骼引用
    /// </summary>
    public void SetHeadBone(Transform bone)
    {
        headBone = bone;
    }
} 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 讲演者控制器 - 控制讲演者的移动、动画和状态
/// </summary>
public class PresenterController : MonoBehaviour
{
    [Header("移动设置")]
    [Tooltip("行走速度")]
    public float walkSpeed = 2.0f;
    
    [Tooltip("旋转速度")]
    public float rotationSpeed = 100.0f;
    
    [Tooltip("重力")]
    public float gravity = 9.8f;
    
    [Tooltip("跳跃高度")]
    public float jumpHeight = 1.0f;
    
    [Header("相机设置")]
    [Tooltip("摄像机目标")]
    public Transform cameraTarget;
    
    [Tooltip("相机跟随平滑度")]
    public float cameraSmoothness = 5.0f;
    
    [Header("讲台设置")]
    [Tooltip("讲台位置")]
    public Transform podiumPosition;
    
    [Header("状态")]
    [Tooltip("是否已到达讲台")]
    public bool reachedPodium = false;
    
    [Tooltip("是否已开始演讲")]
    public bool startedPresentation = false;
    
    [Header("兼容性设置")]
    [Tooltip("是否禁用CameraMovable组件")]
    public bool disableCameraMovable = true;
    
    [Tooltip("座位位置（初始位置）")]
    public Transform seatPosition;
    
    [Tooltip("始终使用指定的初始位置")]
    public bool useCustomInitialPosition = false;
    
    // 角色控制器组件
    private CharacterController characterController;
    
    // 动画控制器组件
    private Animator animator;
    
    // 移动速度
    private Vector3 moveVelocity;
    
    // 垂直速度（用于重力和跳跃）
    private float verticalVelocity;
    
    // 是否正在讲台上
    private bool isOnPodium = false;
    
    // 相机对象
    private Camera mainCamera;
    
    // 是否已经站起来
    public bool stoodUp = false;
    
    // 初始位置
    private Vector3 initialPosition;
    
    // 初始旋转
    private Quaternion initialRotation;
    
    private void Awake()
    {
        // 获取组件
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        
        // 如果没有角色控制器，添加一个
        if (characterController == null)
        {
            characterController = gameObject.AddComponent<CharacterController>();
            characterController.height = 1.8f;
            characterController.radius = 0.3f;
            characterController.center = new Vector3(0, 0.9f, 0);
            characterController.stepOffset = 0.3f; // 允许爬上台阶
        }
        
        // 禁用角色控制器，直到站起来
        if (characterController.enabled)
        {
            characterController.enabled = false;
        }
        
        // 获取主相机
        mainCamera = Camera.main;
        
        // 如果没有指定摄像机目标，使用头部位置
        if (cameraTarget == null)
        {
            // 尝试找到头部骨骼
            Transform headBone = FindHeadBone(transform);
            if (headBone != null)
            {
                cameraTarget = headBone;
            }
            else
            {
                // 创建一个目标点在头部位置
                GameObject target = new GameObject("CameraTarget");
                target.transform.parent = transform;
                target.transform.localPosition = new Vector3(0, 1.6f, 0); // 大致头部高度
                cameraTarget = target.transform;
            }
        }
        
        // 禁用可能冲突的CameraMovable组件
     
    }
    
    private void Start()
    {
        // 设置初始位置（如果指定了座位位置）
        if (useCustomInitialPosition && seatPosition != null)
        {
            transform.position = seatPosition.position;
            transform.rotation = seatPosition.rotation;
            Debug.Log($"已将演讲者移动到座位位置: {seatPosition.position}");
        }
        else
        {
            // 确保演讲者初始朝向向前（默认朝向Z轴正方向）
            transform.rotation = Quaternion.Euler(0, 0, 0);
            Debug.Log("已将演讲者初始朝向调整为向前");
        }
        
        // 保存初始位置和旋转（在设置完初始位置后）
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        
        // 获取动画控制器组件
        PresenterAnimatorController animController = GetComponent<PresenterAnimatorController>();
        
        // 检查是否需要从站立状态开始
        bool startWithStandUp = false; // 默认不从站立状态开始
        
        // 如果有参数指定要从站立状态开始，则设置
        #if UNITY_EDITOR
        // 检查是否有命令行参数或PlayerPrefs设置
        startWithStandUp = UnityEditor.EditorPrefs.GetBool("PresenterStartWithStandUp", false);
        #endif
        
        // 根据设置初始化动画状态
        if (startWithStandUp)
        {
            // 直接设置为闲置状态
            if (animController != null)
            {
                animController.SetIdle(true);
                stoodUp = true;
                
                // 启用角色控制器
                if (characterController != null && !characterController.enabled)
                {
                    characterController.enabled = true;
                }
            }
            else if (animator != null)
            {
                // 兼容旧代码，直接设置animator的参数
                animator.SetBool("isWalking", false);
                animator.SetBool("isIdle", true);
                stoodUp = true;
                
                // 启用角色控制器
                if (characterController != null && !characterController.enabled)
                {
                    characterController.enabled = true;
                }
            }
            
            Debug.Log("初始化为站立状态");
        }
        else
        {
            // 初始化为不可控制状态（默认）
            if (animController != null)
            {
                // 动画控制器组件会在自己的Awake中初始化为闲置状态
            }
            else if (animator != null)
            {
                // 兼容旧代码，直接设置animator的参数
                animator.SetBool("isWalking", false);
                animator.SetBool("isIdle", false);
            }
            
            // 禁用角色控制器，直到按下W键
            if (characterController != null)
            {
                characterController.enabled = false;
            }
            
            // 确保尚未开始演讲
            stoodUp = false;
            Debug.Log("初始化为不可控制状态");
        }
        
        reachedPodium = false;
        startedPresentation = false;
    }
    
    /// <summary>
    /// 设置讲演者是否从站立状态开始
    /// </summary>
    public static void SetStartWithStandUp(bool value)
    {
        #if UNITY_EDITOR
        UnityEditor.EditorPrefs.SetBool("PresenterStartWithStandUp", value);
        Debug.Log($"设置讲演者从站立状态开始: {value}");
        #endif
    }
    
    private void Update()
    {
        // 处理移动输入（即使未站起，也需要检测W键）
        HandleMovementInput();
        
        // 如果尚未站起来，不执行后续操作
        if (!stoodUp) return;
        
        // 检查是否到达讲台
        if (!reachedPodium && !startedPresentation)
        {
            CheckIfReachedPodium();
        }
        
        // 更新相机位置
        UpdateCameraPosition();
    }
    
    /// <summary>
    /// 处理WASD移动输入
    /// </summary>
    private void HandleMovementInput()
    {
        float vertical = Input.GetAxis("Vertical");

        if (characterController == null) return;
        
        // 检查CharacterController是否激活，如果未激活则不执行移动操作
        if (!characterController.enabled)
        {
            // 获取输入，只检测是否需要站起来，不执行移动
            
            // 检查是否从未站起状态开始移动（按W键）
            if (!stoodUp && vertical > 0.1f)
            {
                StartCoroutine(SwitchToIdleRoutine());
            }
            
            return; // 控制器未激活，不执行后续移动操作
        }
        
        // 获取输入
        float horizontal = Input.GetAxis("Horizontal");
        
        // 检查是否从未站起状态开始移动（按W键）
        if (!stoodUp && vertical > 0.1f)
        {
            StartCoroutine(SwitchToIdleRoutine());
            return;
        }
        
        // 计算移动方向，基于相机朝向
        Vector3 cameraForward = mainCamera.transform.forward;
        Vector3 cameraRight = mainCamera.transform.right;
        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();
        
        Vector3 moveDirection = (cameraForward * vertical + cameraRight * horizontal).normalized;
        
        // 获取动画控制器组件
        PresenterAnimatorController animController = GetComponent<PresenterAnimatorController>();
        
        // 设置移动速度
        if (moveDirection.magnitude > 0.1f)
        {
            // 如果已经开始演讲，移动速度减半
            float currentSpeed = startedPresentation ? walkSpeed * 0.5f : walkSpeed;
            moveVelocity = moveDirection * currentSpeed;
            
            // 根据移动方向旋转角色
            if (!startedPresentation)
            {
                // 如果尚未开始演讲，正常旋转
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
            else
            {
                // 如果已开始演讲，保持基本朝向，但允许小范围旋转（±45度）
                Quaternion baseRotation = Quaternion.Euler(0, 0, 0); // 面向观众的基准方向
                Quaternion desiredRotation = Quaternion.LookRotation(moveDirection);
                
                // 计算当前旋转与基准方向的夹角
                float angle = Quaternion.Angle(baseRotation, desiredRotation);
                
                // 如果旋转角度小于45度，允许旋转
                if (angle < 45f)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSpeed * 0.5f * Time.deltaTime);
                }
                else
                {
                    // 超过限制，使用限制后的旋转
                    Quaternion limitedRotation = Quaternion.RotateTowards(baseRotation, desiredRotation, 45f);
                    transform.rotation = Quaternion.Slerp(transform.rotation, limitedRotation, rotationSpeed * 0.5f * Time.deltaTime);
                }
            }
            
            // 设置行走动画
            if (animController != null)
            {
                if (startedPresentation)
                {
                    // 已开始演讲，保持说话动画，但可以稍微移动
                    animController.SetTalking(true);
                }
                else
                {
                    animController.SetWalking(true);
                }
            }
            else if (animator != null && !startedPresentation)
            {
                // 兼容旧代码，只在未开始演讲时设置行走动画
                animator.SetBool("isWalking", true);
                animator.SetBool("isIdle", false);
            }
        }
        else
        {
            moveVelocity = Vector3.zero;
            
            // 设置站立动画
            if (animController != null)
            {
                if (startedPresentation)
                {
                    // 已开始演讲，设置说话动画
                    animController.SetTalking(true);
                }
                else
                {
                    animController.SetWalking(false);
                    animController.SetIdle(true);
                }
            }
            else if (animator != null && !startedPresentation)
            {
                // 兼容旧代码，只在未开始演讲时设置闲置动画
                animator.SetBool("isWalking", false);
                animator.SetBool("isIdle", true);
            }
        }
        
        // 处理重力和跳跃
        if (characterController.isGrounded)
        {
            verticalVelocity = -0.5f; // 轻微的向下力，确保与地面接触
            
            // 跳跃，只在未开始演讲时允许
            if (Input.GetButtonDown("Jump") && !startedPresentation)
            {
                verticalVelocity = Mathf.Sqrt(2 * jumpHeight * gravity);
            }
        }
        else
        {
            verticalVelocity -= gravity * Time.deltaTime;
        }
        
        // 应用移动
        Vector3 movement = moveVelocity + new Vector3(0, verticalVelocity, 0);
        characterController.Move(movement * Time.deltaTime);
    }
    
    /// <summary>
    /// 切换到闲置状态
    /// </summary>
    private IEnumerator SwitchToIdleRoutine()
    {
        if (stoodUp) yield break; // 如果已经站起来了，不执行
        
        Debug.Log("开始从不可控制状态切换到闲置状态");
        
        // 获取动画控制器组件
        PresenterAnimatorController animController = GetComponent<PresenterAnimatorController>();
        
        // 设置闲置状态动画
        if (animController != null)
        {
            animController.SetIdle(true);
        }
        else if (animator != null)
        {
            // 兼容旧代码
            animator.SetBool("isWalking", false);
            animator.SetBool("isIdle", true);
        }
        
        // 等待一小段时间让动画过渡
        yield return new WaitForSeconds(0.1f);
        
        // 完成站起来
        stoodUp = true;
        
        // 启用角色控制器
        if (characterController != null && !characterController.enabled)
        {
            characterController.enabled = true;
        }
        
        Debug.Log("已切换到闲置状态，现在可以移动");
    }
    
    /// <summary>
    /// 检查是否到达讲台
    /// </summary>
    private void CheckIfReachedPodium()
    {
        if (podiumPosition == null || reachedPodium) return;
        
        // 计算到讲台的水平距离
        Vector3 playerPos = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 podiumPos = new Vector3(podiumPosition.position.x, 0, podiumPosition.position.z);
        float distance = Vector3.Distance(playerPos, podiumPos);
        
        // 如果足够接近讲台
        if (distance < 1.0f)
        {
            reachedPodium = true;
            
            // 开始演讲
            StartCoroutine(StartPresentationRoutine());
        }
    }
    
    /// <summary>
    /// 开始演讲的协程
    /// </summary>
    private IEnumerator StartPresentationRoutine()
    {
        // 获取动画控制器组件
        PresenterAnimatorController animController = GetComponent<PresenterAnimatorController>();
        
        // 设置站立/闲置动画
        if (animController != null)
        {
            animController.SetWalking(false);
            animController.SetIdle(true);
        }
        else if (animator != null)
        {
            // 兼容旧代码
            animator.SetBool("isWalking", false);
            animator.SetBool("isIdle", true);
        }
        
        // 平滑旋转面向观众
        // Quaternion targetRotation = Quaternion.Euler(0, 180, 0); // 假设观众在反方向
        //
        // float rotationTime = 0;
        // Quaternion startRotation = transform.rotation;
        //
        // while (rotationTime < 1.0f)
        // {
        //     rotationTime += Time.deltaTime;
        //     transform.rotation = Quaternion.Slerp(startRotation, targetRotation, rotationTime);
        //     yield return null;
        // }
        
        // 标记为已开始演讲
        startedPresentation = true;
        
        // 切换到讲话状态
        if (animController != null)
        {
            animController.SetTalking(true);
        }
        
        // 通知AcademicReportUI已到达讲台
        AcademicReportUI reportUI = FindObjectOfType<AcademicReportUI>();
        if (reportUI != null)
        {
            reportUI.OnReachedPodium();
        }
        
        // 触发演讲事件
        AcademicPresentationManager presentationManager = FindObjectOfType<AcademicPresentationManager>();
        if (presentationManager != null)
        {
            presentationManager.StartPresentation();
        }
        yield return null;
    }
    
    /// <summary>
    /// 更新相机位置
    /// </summary>
    private void UpdateCameraPosition()
    {
        if (mainCamera == null || cameraTarget == null) return;
        
        // 设置相机位置
        mainCamera.transform.position = cameraTarget.position;
        
        // 如果已开始演讲，相机可以自由旋转
        if (startedPresentation)
        {
            // 获取鼠标输入
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            
            // 如果按下右键，允许调整视角
            if (Input.GetMouseButton(1))
            {
                // 计算旋转
                float xRotation = mainCamera.transform.eulerAngles.x - mouseY * rotationSpeed * 0.5f;
                float yRotation = mainCamera.transform.eulerAngles.y + mouseX * rotationSpeed * 0.5f;
                
                // 限制垂直旋转角度
                if (xRotation > 180f) xRotation -= 360f;
                if (xRotation < -180f) xRotation += 360f;
                xRotation = Mathf.Clamp(xRotation, -30f, 30f);
                
                // 应用旋转
                mainCamera.transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
            }
        }
    }
    
    /// <summary>
    /// 查找头部骨骼
    /// </summary>
    private Transform FindHeadBone(Transform parent)
    {
        // 尝试查找常见的头部骨骼名称
        string[] headBoneNames = { "head", "Head", "mixamorig:Head", "Bip001 Head", "Armature/Hips/Spine/Chest/Neck/Head" };
        
        foreach (string boneName in headBoneNames)
        {
            Transform bone = parent.Find(boneName);
            if (bone != null) return bone;
        }
        
        // 递归查找子骨骼
        foreach (Transform child in parent)
        {
            Transform bone = FindHeadBone(child);
            if (bone != null) return bone;
        }
        
        return null;
    }
    
    /// <summary>
    /// 重置讲演者
    /// </summary>
    public void ResetPresenter()
    {
        // 重置状态
        stoodUp = false;
        reachedPodium = false;
        startedPresentation = false;
        
        // 获取动画控制器组件
        PresenterAnimatorController animController = GetComponent<PresenterAnimatorController>();
        
        // 重置动画
        if (animController != null)
        {
            // 重置所有参数，默认回到初始状态
            animController.ResetAllParameters();
        }
        else if (animator != null)
        {
            // 兼容旧代码
            animator.SetBool("isWalking", false);
            animator.SetBool("isIdle", false);
        }
        
        // 禁用角色控制器
        if (characterController != null)
        {
            characterController.enabled = false;
        }
        
        // 恢复初始位置和旋转
        transform.position = initialPosition;
        transform.rotation = initialRotation;
        
        // 通知AcademicReportUI重置状态
        AcademicReportUI reportUI = FindObjectOfType<AcademicReportUI>();
        if (reportUI != null)
        {
            reportUI.SetReachedPodiumStatus(false);
        }
    }
} 
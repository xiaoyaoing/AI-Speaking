using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 演讲场景初始化 - 在场景启动时正确设置讲演者和相机
/// </summary>
public class PresentationSceneSetup : MonoBehaviour
{
    [Header("讲演者设置")]
    [Tooltip("讲演者预制体")]
    public GameObject presenterPrefab;
    
    [Tooltip("座位位置")]
    public Transform seatPosition;
    
    [Tooltip("讲台位置")]
    public Transform podiumPosition;
    
    [Header("相机设置")]
    [Tooltip("相机初始位置")]
    public Transform cameraInitialPosition;
    
    [Header("配置选项")]
    [Tooltip("是否自动设置")]
    public bool autoSetup = true;
    
    private FirstPersonCameraController fpController;
    private PresenterController presenterController;
    
    private void Awake()
    {
        // 查找相机控制器
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            fpController = mainCamera.GetComponent<FirstPersonCameraController>();
        }
        
        // 查找讲演者控制器
        presenterController = FindObjectOfType<PresenterController>();
    }
    
    private void Start()
    {
        if (autoSetup)
        {
            SetupScene();
        }
    }
    
    /// <summary>
    /// 设置场景
    /// </summary>
    public void SetupScene()
    {
        // 设置讲演者
        SetupPresenter();
        
        // 设置相机
        SetupCamera();
    }
    
    /// <summary>
    /// 设置讲演者
    /// </summary>
    private void SetupPresenter()
    {
        // 如果场景中没有讲演者，且有预制体，就实例化一个
        if (presenterController == null && presenterPrefab != null)
        {
            Vector3 spawnPosition = seatPosition != null ? seatPosition.position : Vector3.zero;
            Quaternion spawnRotation = seatPosition != null ? seatPosition.rotation : Quaternion.identity;
            
            GameObject presenterObj = Instantiate(presenterPrefab, spawnPosition, spawnRotation);
            presenterController = presenterObj.GetComponent<PresenterController>();
            
            Debug.Log($"已创建讲演者: {presenterObj.name}");
        }
        
        // 如果找到讲演者控制器，设置其属性
        if (presenterController != null)
        {
            if (seatPosition != null)
            {
                presenterController.seatPosition = seatPosition;
                presenterController.useCustomInitialPosition = true;
                
                // 直接设置位置，确保正确放置
                presenterController.transform.position = seatPosition.position;
                presenterController.transform.rotation = seatPosition.rotation;
            }
            
            if (podiumPosition != null)
            {
                presenterController.podiumPosition = podiumPosition;
            }
            
            // 确保讲演者初始未站起
            presenterController.stoodUp = false;
            presenterController.reachedPodium = false;
            presenterController.startedPresentation = false;
            
            Debug.Log("已设置讲演者属性");
        }
        else
        {
            Debug.LogWarning("未找到讲演者控制器！");
        }
    }
    
    /// <summary>
    /// 设置相机
    /// </summary>
    private void SetupCamera()
    {
        // 如果没有找到相机控制器，尝试添加一个
        Camera mainCamera = Camera.main;
        if (mainCamera != null && fpController == null)
        {
            fpController = mainCamera.gameObject.AddComponent<FirstPersonCameraController>();
            Debug.Log("已添加FirstPersonCameraController组件");
        }
        
        if (fpController != null)
        {
            // 设置相机目标为讲演者
            if (presenterController != null)
            {
                fpController.target = presenterController.cameraTarget != null ? 
                    presenterController.cameraTarget : presenterController.transform;
                
                Debug.Log($"已设置相机目标: {fpController.target.name}");
            }
            
            // 设置相机初始位置
            if (cameraInitialPosition != null)
            {
                mainCamera.transform.position = cameraInitialPosition.position;
                mainCamera.transform.rotation = cameraInitialPosition.rotation;
                fpController.originalPosition = cameraInitialPosition.position;
                fpController.originalRotation = cameraInitialPosition.rotation;
                
                // 更新旋转值
                fpController.rotation.y = cameraInitialPosition.rotation.eulerAngles.x;
                fpController.rotation.x = cameraInitialPosition.rotation.eulerAngles.y;
                fpController.currentRotation = fpController.rotation;
                
                Debug.Log($"已设置相机初始位置和旋转");
            }
            
            Debug.Log("已设置相机属性");
        }
        else
        {
            Debug.LogWarning("未找到FirstPersonCameraController组件！");
        }
    }
    
    /// <summary>
    /// 手动调用重置场景
    /// </summary>
    public void ResetScene()
    {
        if (presenterController != null)
        {
            presenterController.ResetPresenter();
        }
        
        if (fpController != null)
        {
            fpController.ResetCamera();
        }
        
        // 重新设置场景
        SetupScene();
        
        Debug.Log("已重置场景");
    }
} 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 讲演者动画控制器 - 处理讲演者的动画过渡
/// </summary>
public class PresenterAnimatorController : MonoBehaviour
{
    [Header("动画参数")]
    [Tooltip("行走动画过渡时间")]
    public float walkTransitionTime = 0.25f;
    
    [Tooltip("闲置动画过渡时间")]
    public float idleTransitionTime = 0.25f;
    
    [Header("动画状态名称")]
    [Tooltip("闲置状态名称")]
    public string idleStateName = "idle";
    
    [Tooltip("行走状态名称")]
    public string walkingStateName = "walk";
    
    [Tooltip("讲话状态名称")]
    public string talkingStateName = "talk1";
    
    // 动画控制器组件
    private Animator animator;
    
    // 当前参数状态
    private bool isWalking = false;
    private bool isIdle = false;
    private bool isTalking = false;
    
    private void Awake()
    {
        // 获取动画控制器组件
        animator = GetComponent<Animator>();
        
        // 如果没有动画控制器，则报错
        if (animator == null)
        {
            Debug.LogError("PresenterAnimatorController需要Animator组件！");
            enabled = false; // 禁用这个脚本
            return;
        }
        
        // 初始化动画参数 - 默认为闲置状态
        InitializeIdleState();
    }
    
    private void Update()
    {
        // 从PresenterController获取状态，更新动画
        UpdateAnimationFromController();
        
        // 打印当前参数状态用于调试
        if (Time.frameCount % 300 == 0) // 降低日志频率
        {
            Debug.Log($"当前动画状态: 行走: {isWalking}, 闲置: {isIdle}, 讲话: {isTalking}");
        }
    }
    
    /// <summary>
    /// 初始化为闲置状态
    /// </summary>
    private void InitializeIdleState()
    {
        // 设置状态
        isIdle = true;
        isWalking = false;
        isTalking = false;
        
        // 直接播放闲置动画
        PlayAnimationWithRandomTime(idleStateName, 0.25f);
        
        Debug.Log("初始化为闲置状态");
    }
    
    /// <summary>
    /// 根据PresenterController状态更新动画参数
    /// </summary>
    private void UpdateAnimationFromController()
    {
        // 获取PresenterController组件
        PresenterController controller = GetComponent<PresenterController>();
        if (controller == null) return;
        
        // 这里可以添加根据controller状态的其他动画逻辑
    }
    
    /// <summary>
    /// 重置所有动画参数
    /// </summary>
    private void ResetAnimationParameters()
    {
        isWalking = false;
        isIdle = false;
        isTalking = false;
    }
    
    /// <summary>
    /// 重置所有动画参数 - 公共方法
    /// </summary>
    public void ResetAllParameters()
    {
        ResetAnimationParameters();
        
        // 恢复到闲置状态
        PlayAnimationWithRandomTime(idleStateName, 0.25f);
        isIdle = true;
        
        Debug.Log("重置所有动画参数");
    }
    
    /// <summary>
    /// 播放动画并使用随机起始时间
    /// </summary>
    private void PlayAnimationWithRandomTime(string stateName, float transitionTime)
    {
        if (animator == null) return;
        
        // 启用Animator
        animator.enabled = true;
        
        // 随机起始时间
        float normalizedTime = Random.Range(0f, 1f);
        
        // 随机动画速度变化
        animator.speed = Random.Range(0.9f, 1.1f);
        
        // 使用CrossFade播放动画
        animator.CrossFade(stateName, transitionTime, 0, normalizedTime);
        
        // 确保动画立即更新
        animator.Update(0);
        
        Debug.Log($"播放动画: {stateName}, 过渡时间: {transitionTime}, 起始位置: {normalizedTime}");
    }
    
    /// <summary>
    /// 设置行走状态
    /// </summary>
    public void SetWalking(bool value)
    {
        // 如果状态没有变化，不做任何操作
        if (isWalking == value) return;
        
        isWalking = value;
        
        if (value)
        {
            // 设置为行走状态
            isIdle = false;
            isTalking = false;
            
            // 直接播放行走动画
            PlayAnimationWithRandomTime(walkingStateName, walkTransitionTime);
            
            Debug.Log("设置为行走状态");
        }
        else
        {
            // 从行走切换到闲置
            isIdle = true;
            isTalking = false;
            
            // 直接播放闲置动画
            PlayAnimationWithRandomTime(idleStateName, idleTransitionTime);
            
            Debug.Log("从行走切换到闲置状态");
        }
    }
    
    /// <summary>
    /// 设置闲置状态
    /// </summary>
    public void SetIdle(bool value)
    {
        // 如果状态没有变化，不做任何操作
        if (isIdle == value) return;
        
        isIdle = value;
        
        if (value)
        {
            // 设置为闲置状态
            isWalking = false;
            isTalking = false;
            
            // 直接播放闲置动画
            PlayAnimationWithRandomTime(idleStateName, idleTransitionTime);
            
            Debug.Log("设置为闲置状态");
        }
        else
        {
            Debug.Log("关闭闲置状态");
        }
    }
    
    /// <summary>
    /// 设置讲话状态
    /// </summary>
    public void SetTalking(bool value)
    {
        // 如果状态没有变化，不做任何操作
        if (isTalking == value) return;
        
        isTalking = value;
        
        if (value)
        {
            // 设置为讲话状态
            isWalking = false;
            isIdle = false;
            
            // 直接播放讲话动画
            PlayAnimationWithRandomTime(talkingStateName, 0.25f);
            
            Debug.Log("设置为讲话状态");
        }
        else
        {
            // 从讲话切换到闲置
            isIdle = true;
            
            // 直接播放闲置动画
            PlayAnimationWithRandomTime(idleStateName, idleTransitionTime);
            
            Debug.Log("从讲话切换到闲置状态");
        }
    }
} 
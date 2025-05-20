using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 简单的按钮组件，用于直接播放单个角色的所有动画
/// </summary>
public class SingleCharacterAnimationButton : MonoBehaviour
{
    [Header("按钮设置")]
    [SerializeField] private Button actionButton;
    [SerializeField] private Text buttonText;
    
    [Header("动画设置")]
    [SerializeField] private CharacterAnimationPlayer animationPlayer;
    [SerializeField] private float animationDuration = 3f;
    [SerializeField] private bool autoRotate = true;
    
    private bool isPlaying = false;
    
    private void Start()
    {
        // 如果没有设置按钮，尝试获取当前对象上的按钮
        if (actionButton == null)
        {
            actionButton = GetComponent<Button>();
        }
        
        // 设置按钮事件
        if (actionButton != null)
        {
            actionButton.onClick.AddListener(OnButtonClicked);
            UpdateButtonText();
        }
        
        // 如果没有设置动画播放器，查找场景中的动画播放器
        if (animationPlayer == null)
        {
            animationPlayer = FindObjectOfType<CharacterAnimationPlayer>();
        }
    }
    
    private void OnButtonClicked()
    {
        if (animationPlayer == null)
        {
            Debug.LogWarning("未找到动画播放器组件");
            return;
        }
        
        if (isPlaying)
        {
            // 如果正在播放，则停止动画
            animationPlayer.StopAnimations();
            isPlaying = false;
        }
        else
        {
            // 设置动画播放参数
            animationPlayer.SetAnimationDuration(animationDuration);
            animationPlayer.SetAutoRotateCharacters(autoRotate);
            
            // 播放最近角色的所有动画
            animationPlayer.PlayAllAnimationsForNearestCharacter();
            isPlaying = true;
        }
        
        // 更新按钮文本
        UpdateButtonText();
    }
    
    private void UpdateButtonText()
    {
        if (buttonText != null)
        {
            buttonText.text = isPlaying ? "停止动画" : "播放角色动画";
        }
    }
} 
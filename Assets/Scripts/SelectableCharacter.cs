using UnityEngine;

/// <summary>
/// 使角色可被选择，并播放动画
/// </summary>
public class SelectableCharacter : MonoBehaviour
{
    [Header("动画播放器引用")]
    [SerializeField] private CharacterAnimationPlayer animationPlayer;
    
    [Header("动画设置")]
    [SerializeField] private float animationDuration = 3f;
    [SerializeField] private bool autoRotate = true;
    
    private bool isPlaying = false;
    private Color originalColor;
    private Renderer characterRenderer;
    
    private void Start()
    {
        // 查找场景中的动画播放器
        if (animationPlayer == null)
        {
            animationPlayer = FindObjectOfType<CharacterAnimationPlayer>();
        }
        
        // 获取角色的渲染器
        characterRenderer = GetComponentInChildren<Renderer>();
        if (characterRenderer != null)
        {
            originalColor = characterRenderer.material.color;
        }
    }
    
    private void OnMouseDown()
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
            
            // 恢复原始颜色
            if (characterRenderer != null)
            {
                characterRenderer.material.color = originalColor;
            }
        }
        else
        {
            // 设置动画播放参数
            animationPlayer.SetAnimationDuration(animationDuration);
            animationPlayer.SetAutoRotateCharacters(autoRotate);
            
            // 播放当前角色的所有动画
            animationPlayer.PlayAllAnimationsForSingleCharacter(gameObject);
            isPlaying = true;
            
            // 高亮显示当前角色
            if (characterRenderer != null)
            {
                characterRenderer.material.color = Color.yellow;
            }
        }
    }
    
    private void OnDestroy()
    {
        // 如果销毁时正在播放，停止动画
        if (isPlaying && animationPlayer != null)
        {
            animationPlayer.StopAnimations();
        }
    }
} 
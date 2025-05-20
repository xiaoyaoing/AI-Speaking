using UnityEngine;

public class CharacterDefaultAnimation : MonoBehaviour
{
    private Animator animator;
    private string defaultAnimation = "listen";

    private void Start()
    {
        // 获取Animator组件
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError($"角色 {gameObject.name} 缺少Animator组件！");
            return;
        }

        // 设置默认动画
        SetDefaultAnimation();
    }

    private void SetDefaultAnimation()
    {
        // 根据角色类型设置正确的动画名称
        string animationName = GetAnimationName();
        
        // 播放动画
        if (!string.IsNullOrEmpty(animationName))
        {
            animator.Play(animationName);
        }
    }

    private string GetAnimationName()
    {
        // 获取角色名称
        string characterName = gameObject.name.ToLower();

        // 根据角色类型返回对应的动画名称
        if (characterName.Contains("man"))
        {
            return "m_" + defaultAnimation;
        }
        else if (characterName.Contains("girlwithheel"))
        {
            return "f_heel_" + defaultAnimation;
        }
        else if (characterName.Contains("girlnoheel"))
        {
            return "f_noheel_" + defaultAnimation;
        }

        Debug.LogWarning($"无法识别角色类型: {characterName}");
        return null;
    }
} 
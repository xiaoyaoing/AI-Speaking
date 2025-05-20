using UnityEngine;

public class CharacterAnimationManager : MonoBehaviour
{
    public enum AnimationType
    {
        Listen,
        Talk,
        Clap
    }

    private Animator animator;
    private string characterType;

    private void Start()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError($"角色 {gameObject.name} 缺少Animator组件！");
            return;
        }

        // 确定角色类型
        DetermineCharacterType();
        
        // 设置初始动画
        PlayAnimation(AnimationType.Listen);
    }

    private void DetermineCharacterType()
    {
        string name = gameObject.name.ToLower();
        if (name.Contains("man"))
        {
            characterType = "m_";
        }
        else if (name.Contains("girlwithheel"))
        {
            characterType = "f_heel_";
        }
        else if (name.Contains("girlnoheel"))
        {
            characterType = "f_noheel_";
        }
        else
        {
            Debug.LogWarning($"无法识别角色类型: {name}");
        }
    }

    public void PlayAnimation(AnimationType type)
    {
        if (animator == null || string.IsNullOrEmpty(characterType)) return;

        string animationName = characterType + type.ToString().ToLower();
        animator.Play(animationName);
    }

    // 公共方法，供其他脚本调用
    public void PlayListen() => PlayAnimation(AnimationType.Listen);
    public void PlayTalk() => PlayAnimation(AnimationType.Talk);
    public void PlayClap() => PlayAnimation(AnimationType.Clap);
} 
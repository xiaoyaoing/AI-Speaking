using UnityEngine;

public class AnimationTest : MonoBehaviour
{
    [Header("动画设置")]
    [Tooltip("要播放的动画")]
    public AnimationClip animationClip;
    [Tooltip("是否循环播放")]
    public bool loop = true;
    [Tooltip("播放速度")]
    public float speed = 1.0f;

    private Animator animator;
    private RuntimeAnimatorController originalController;
    private AnimatorOverrideController overrideController;

    private void Start()
    {
        // 获取Animator组件
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("对象上没有Animator组件");
            return;
        }

        // 保存原始控制器
        originalController = animator.runtimeAnimatorController;

        // 创建覆盖控制器
        overrideController = new AnimatorOverrideController(originalController);
        animator.runtimeAnimatorController = overrideController;

        // 播放动画
        PlayAnimation();
    }

    private void PlayAnimation()
    {
        if (animationClip == null)
        {
            Debug.LogError("未设置要播放的动画");
            return;
        }

        // 设置动画
        overrideController["idle1"] = animationClip; // 使用idle1作为默认状态
        animator.speed = speed;

        // 播放动画
        animator.Play("idle1", 0, 0);
    }

    private void OnDestroy()
    {
        // 恢复原始控制器
        if (animator != null)
        {
            animator.runtimeAnimatorController = originalController;
        }
    }

    // 在编辑器中测试动画
    [ContextMenu("测试播放动画")]
    private void TestPlayAnimation()
    {
        if (Application.isPlaying)
        {
            PlayAnimation();
        }
        else
        {
            Debug.LogWarning("请在播放模式下测试动画");
        }
    }
} 
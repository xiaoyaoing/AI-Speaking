using UnityEngine;
using System.Collections.Generic;

public class AudienceAnimationController : MonoBehaviour
{
    [System.Serializable]
    public class AnimationSet
    {
        public string listen1Animation = "m_listen1";  // 听讲状态1
        public string listen2Animation = "m_listen2";  // 听讲状态2
        public string talkAnimation = "m_talk";        // 说话状态
        public string clapAnimation = "m_clap";        // 鼓掌状态
    }

    [Header("动画设置")]
    public AnimationSet maleAnimations = new AnimationSet();
    public AnimationSet femaleAnimations = new AnimationSet();

    [Header("状态设置")]
    [Range(0, 1)]
    public float listen1Probability = 0.5f;  // 听讲状态1的概率
    public float stateChangeInterval = 5f;    // 状态切换间隔
    public float minStateDuration = 3f;       // 最小状态持续时间
    public float maxStateDuration = 8f;       // 最大状态持续时间

    private Animator animator;
    private bool isMale;
    private float nextStateChangeTime;
    private string currentState;
    private bool isTalking = false;
    private bool isClapping = false;

    private void Start()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("找不到Animator组件！");
            return;
        }

        // 根据角色名称判断性别
        isMale = gameObject.name.ToLower().Contains("male");
        
        // 设置初始状态
        SetInitialState();
    }

    private void Update()
    {
        if (animator == null) return;

        // 检查是否需要切换状态
        if (Time.time >= nextStateChangeTime && !isTalking && !isClapping)
        {
            ChangeState();
        }
    }

    private void SetInitialState()
    {
        // 随机选择听讲状态1或2
        if (Random.value < listen1Probability)
        {
            PlayListen1();
        }
        else
        {
            PlayListen2();
        }
    }

    private void ChangeState()
    {
        // 随机选择下一个状态
        float random = Random.value;
        if (random < 0.5f)
        {
            PlayListen1();
        }
        else
        {
            PlayListen2();
        }

        // 设置下一次状态切换时间
        nextStateChangeTime = Time.time + Random.Range(minStateDuration, maxStateDuration);
    }

    public void StartTalking()
    {
        if (isTalking) return;
        
        isTalking = true;
        PlayTalk();
    }

    public void StopTalking()
    {
        if (!isTalking) return;
        
        isTalking = false;
        SetInitialState();
    }

    public void StartClapping()
    {
        if (isClapping) return;
        
        isClapping = true;
        PlayClap();
    }

    public void StopClapping()
    {
        if (!isClapping) return;
        
        isClapping = false;
        SetInitialState();
    }

    private void PlayListen1()
    {
        currentState = isMale ? maleAnimations.listen1Animation : femaleAnimations.listen1Animation;
        animator.Play(currentState);
    }

    private void PlayListen2()
    {
        currentState = isMale ? maleAnimations.listen2Animation : femaleAnimations.listen2Animation;
        animator.Play(currentState);
    }

    private void PlayTalk()
    {
        currentState = isMale ? maleAnimations.talkAnimation : femaleAnimations.talkAnimation;
        animator.Play(currentState);
    }

    private void PlayClap()
    {
        currentState = isMale ? maleAnimations.clapAnimation : femaleAnimations.clapAnimation;
        animator.Play(currentState);
    }
} 
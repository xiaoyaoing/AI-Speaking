using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;




public class RuntimeCharacterManager : MonoBehaviour
{
    
    public enum PresentationState 
    {
        NotStarted,     // 演讲尚未开始
        Introduction,   // 播放介绍音频
        Walking,        // 演讲者走向讲台
        Presenting,     // 正在演讲
        QuestionTime,   // 问答环节
        Applause,       // 鼓掌阶段
        Completed       // 整个演讲结束
    }
    [Tooltip("当前演讲状态")]
    public PresentationState currentState = PresentationState.NotStarted;
    public static RuntimeCharacterManager Instance { get; private set; }
    

    [Header("角色动画设置")]
    [Tooltip("是否启用自动状态切换")]
    public bool enableAutoStateChange = true;
    
    [Tooltip("动画切换最小间隔(秒)")]
    public float minStateChangeInterval = 3f;
    [Tooltip("动画切换最大间隔(秒)")]
    public float maxStateChangeInterval = 8f;

    [Tooltip("动画速度最小范围")]
    public float minAnimationSpeed = 0.7f;
    [Tooltip("动画速度最大范围")]
    public float maxAnimationSpeed = 1.3f;

    [Header("动画控制器资源")]
    [Tooltip("男性角色动画控制器")]
    public RuntimeAnimatorController maleAnimatorController;
    
    [Tooltip("女性角色动画控制器")]
    public RuntimeAnimatorController femaleAnimatorController;
    
    [Tooltip("默认动画控制器")]
    public RuntimeAnimatorController defaultAnimatorController;
    
    // 演讲状态枚举
  
    // 当前演讲状态
    
    // 当前提问者
    private GameObject currentQuestioner = null;
    
    // 演讲者对象引用
    public GameObject presenterObject = null;

    // 角色注册信息
    private Dictionary<GameObject, Animator> characterAnimators = new Dictionary<GameObject, Animator>();
    private Dictionary<GameObject, string> characterTypes = new Dictionary<GameObject, string>();
    
    // 角色当前状态
    private Dictionary<GameObject, AnimationType> currentCharacterStates = new Dictionary<GameObject, AnimationType>();
    
    // 下一次动画更新时间
    private Dictionary<GameObject, float> nextAnimationChangeTime = new Dictionary<GameObject, float>();

    // 存储角色原始颜色
    private Dictionary<GameObject, Color> originalColors = new Dictionary<GameObject, Color>();

    // 存储角色原始材质
    private Dictionary<GameObject, Material[]> originalMaterials = new Dictionary<GameObject, Material[]>();
    
    // 高亮材质
    private Material highlightMaterial;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 自动注册场景中的所有角色
        // FindAndRegisterAllCharacters();
        
        // 直接启用自动状态切换，确保角色始终有动画
        enableAutoStateChange = true;
        
        // 初始化状态
        currentState = PresentationState.NotStarted;
        
        // 创建高亮材质
        CreateHighlightMaterial();
    }

    private void Update()
    {
        if (!enableAutoStateChange) return;
        
        // 更新所有角色的动画状态
        float currentTime = Time.time;
        
        foreach (GameObject character in characterAnimators.Keys.ToList())
        {
            if (character == null) continue;
            
            // 检查是否是演讲者
            bool isPresenter = IsPresenter(character);
            
            // 如果这是提问者，并且正在问答环节，强制保持Talk状态
            if (character == currentQuestioner && currentState == PresentationState.QuestionTime)
            {
                // 只有当当前状态不是Talk时才更新
                if (!currentCharacterStates.ContainsKey(character) || currentCharacterStates[character] != AnimationType.Talk)
                {
                    PlayAnimation(character, AnimationType.Talk);
                }
                continue;
            }
            
            // 如果是演讲者，根据演讲状态决定动画
            if (isPresenter)
            {
                if (currentState == PresentationState.Presenting || 
                    currentState == PresentationState.QuestionTime)
                {
                    // 演讲中或问答环节，演讲者保持Talk状态
                    if (!currentCharacterStates.ContainsKey(character) || currentCharacterStates[character] != AnimationType.Talk)
                    {
                        PlayAnimation(character, AnimationType.Talk);
                    }
                    continue;
                }
            }
            
            // 如果在问答环节，非提问观众应该在Listen和Idle之间随机切换
            if (currentState == PresentationState.QuestionTime && !isPresenter && character != currentQuestioner)
            {
                // 检查是否需要更新动画
                if (!nextAnimationChangeTime.ContainsKey(character) || currentTime >= nextAnimationChangeTime[character])
                {
                    // 90%概率播放Listen，10%概率播放Idle
                    float random = Random.value;
                    if (random < 0.9f)
                    {
                        // 播放Listen动画，使用随机偏移使其看起来更自然
                        PlayAnimationWithRandomOffset(character, AnimationType.Listen);
                    }
                    else
                    {
                        // 播放Idle动画
                        PlayAnimation(character, AnimationType.Idle);
                    }
                    
                    // 设置下次更新时间（3-8秒后）
                    SetNextUpdateTime(character);
                }
                continue;
            }
            
            // 如果在鼓掌阶段，所有非演讲者应该保持Clap状态
            if (currentState == PresentationState.Applause && !isPresenter)
            {
                if (!currentCharacterStates.ContainsKey(character) || currentCharacterStates[character] != AnimationType.Clap)
                {
                    PlayAnimation(character, AnimationType.Clap);
                }
                continue;
            }
            
            // 其他情况，使用自动状态切换
            // 检查是否需要更新动画
            if (!nextAnimationChangeTime.ContainsKey(character))
            {
                // 第一次设置动画和下次更新时间
                SetRandomAnimation(character);
                SetNextUpdateTime(character);
            }
            else if (currentTime >= nextAnimationChangeTime[character])
            {
                // 时间到了，更新动画
                SetRandomAnimation(character);
                SetNextUpdateTime(character);
            }
        }
    }

    // 设置下一次动画更新时间
    private void SetNextUpdateTime(GameObject character)
    {
        if (character == null) return;
        
        float interval = Random.Range(minStateChangeInterval, maxStateChangeInterval);
        nextAnimationChangeTime[character] = Time.time + interval;
    }

    // 根据当前模式设置随机动画
    private void SetRandomAnimation(GameObject character)
    {
        if (character == null) return;
        
        // 检查是否是演讲者
        bool isPresenter = IsPresenter(character);
        
        // 演讲者始终使用Talk动画（如果演讲已开始）
        if (isPresenter && (currentState == PresentationState.Presenting || 
                            currentState == PresentationState.QuestionTime))
        {
            PlayAnimation(character, AnimationType.Talk);
            return;
        }
        
        // 如果是提问者并且在问答环节
        if (character == currentQuestioner && currentState == PresentationState.QuestionTime)
        {
            PlayAnimation(character, AnimationType.Talk);
            return;
        }
        
        // 如果是在鼓掌阶段，所有观众都应该鼓掌
        if (currentState == PresentationState.Applause && !isPresenter)
        {
            PlayAnimation(character, AnimationType.Clap);
            return;
        }
        
        float random = Random.value;
        
        // 根据演讲状态，选择不同的动画集
        switch (currentState)
        {
            case PresentationState.NotStarted:
                // 演讲前：60% Idle, 40% Talk
                if (random < 0.6f)
                    PlayAnimation(character, AnimationType.Idle);
                else
                    PlayAnimation(character, AnimationType.Talk);
                break;
                
            case PresentationState.Introduction:
                // 介绍阶段：80% Listen, 20% Idle
                if (random < 0.8f)
                    PlayAnimation(character, AnimationType.Listen);
                else
                    PlayAnimation(character, AnimationType.Idle);
                break;
                
            case PresentationState.Walking:
                // 行走阶段：70% Listen, 30% Idle
                if (random < 0.7f)
                    PlayAnimation(character, AnimationType.Listen);
                else
                    PlayAnimation(character, AnimationType.Idle);
                break;
                
            case PresentationState.Presenting:
                // 演讲中：80% Listen, 15% Idle, 5% Talk
                if (random < 0.8f)
                    PlayAnimation(character, AnimationType.Listen);
                else if (random < 0.95f)
                    PlayAnimation(character, AnimationType.Idle);
                else
                    PlayAnimation(character, AnimationType.Talk);
                break;
                
            case PresentationState.Applause:
                // 鼓掌阶段：100% Clap
                PlayAnimation(character, AnimationType.Clap);
                break;
                
            case PresentationState.QuestionTime:
                // 问答环节：90% Listen, 10% Idle (除了提问者)
                if (character != currentQuestioner)
                {
                    if (random < 0.9f)
                        PlayAnimation(character, AnimationType.Listen);
                    else
                        PlayAnimation(character, AnimationType.Idle);
                }
                break;
                
            case PresentationState.Completed:
                // 已结束：50% Idle, 30% Talk, 20% Clap
                if (random < 0.5f)
                    PlayAnimation(character, AnimationType.Idle);
                else if (random < 0.8f)
                    PlayAnimation(character, AnimationType.Talk);
                else
                    PlayAnimation(character, AnimationType.Clap);
                break;
        }
    }

    /// <summary>
    /// 播放带有随机偏移的动画，使观众行为更自然
    /// </summary>
    public void PlayAnimationWithRandomOffset(GameObject character, AnimationType type)
    {
        if (character == null) return;
        
        Animator animator = character.GetComponent<Animator>();
        if (animator == null) return;
        
        // 确保Animator启用
        animator.enabled = true;
        
        // 使用更随机的动画速度
        animator.speed = Random.Range(minAnimationSpeed, maxAnimationSpeed);
        
        // 存储当前动画状态
        currentCharacterStates[character] = type;
        
        // 随机起始时间，使不同角色的动画不完全同步
        float normalizedTime = Random.value;
        float transitionDuration = Random.Range(0.2f, 0.4f); // 随机过渡时间
        
        // 根据动画类型播放相应动画
        switch (type)
        {
            case AnimationType.Listen:
                animator.CrossFade("listen", transitionDuration, 0, normalizedTime);
                break;
            case AnimationType.Idle:
                // 随机选择idle1或idle2
                int idleVariant = Random.Range(1, 3);
                string idleState = idleVariant == 1 ? "idle1" : "idle2";
                animator.CrossFade(idleState, transitionDuration, 0, normalizedTime);
                break;
            default:
                // 对于其他类型的动画，使用常规的PlayAnimation方法
                PlayAnimation(character, type);
                break;
        }
        
        // 立即更新动画器
        animator.Update(0);
    }

    private void FindAndRegisterAllCharacters()
    {
        // 查找场景中的所有角色
        GameObject[] allCharacters = GameObject.FindObjectsOfType<GameObject>();
        
        foreach (GameObject character in allCharacters)
        {
            // 检查名称是否包含 "man" 或 "girl"，粗略判断是否为角色
            string name = character.name.ToLower();
            if (name.Contains("man") || name.Contains("girl") || name.Contains("female") || name.Contains("male"))
            {
                RegisterCharacter(character);
            }
        }
    }

    public void RegisterCharacter(GameObject character)
    {
        if (character == null) return;

        // 获取Animator组件
        Animator animator = character.GetComponent<Animator>();
        if (animator == null)
        {
            // 如果没有Animator组件，添加一个
            animator = character.AddComponent<Animator>();
            Debug.Log($"为角色 {character.name} 添加了Animator组件");
        }

        // 检测角色性别并分配相应的动画控制器
        string characterName = character.name.ToLower();
        bool isMale = characterName.Contains("man") || characterName.Contains("male");
        bool isFemale = characterName.Contains("girl") || characterName.Contains("female") || characterName.Contains("woman");
        
        // 根据性别设置动画控制器
        if (isMale && maleAnimatorController != null)
        {
            animator.runtimeAnimatorController = maleAnimatorController;
        }
        else if (isFemale && femaleAnimatorController != null)
        {
            animator.runtimeAnimatorController = femaleAnimatorController;
        }
        else if (defaultAnimatorController != null)
        {
            // 如果无法确定性别或没有相应的控制器，使用默认控制器
            animator.runtimeAnimatorController = defaultAnimatorController;
        }
        
        // 确保有动画控制器
        if (animator.runtimeAnimatorController == null)
        {
            Debug.LogWarning($"角色 {character.name} 的Animator组件缺少动画控制器！");
            return;
        }

        // 从角色名称确定类型（m_ 或 f_）
        string characterType = DetermineCharacterType(character.name);
        
        // 注册角色
        characterAnimators[character] = animator;
        characterTypes[character] = characterType;
        
        // 确保动画速度正常
        animator.speed = Random.Range(0.8f, 1.2f);
        
        // 检查是否是演讲者
        bool isPresenter = IsPresenter(character);
        if (isPresenter)
        {
            // 记录演讲者对象
            presenterObject = character;
            
            // 根据当前状态决定演讲者动画
            if (currentState == PresentationState.Presenting || currentState == PresentationState.QuestionTime)
            {
                PlayAnimation(character, AnimationType.Talk);
            }
            else
            {
                PlayAnimation(character, AnimationType.Idle);
            }
        }
        else
        {
            // 设置随机动画
            SetRandomAnimation(character);
            SetNextUpdateTime(character);
        }
    }

    public void UnregisterCharacter(GameObject character)
    {
        if (character == null) return;

        // 如果是当前提问者，重置提问者
        if (character == currentQuestioner)
        {
            currentQuestioner = null;
        }
        
        // 如果是演讲者，重置演讲者
        if (character == presenterObject)
        {
            presenterObject = null;
        }

        characterAnimators.Remove(character);
        characterTypes.Remove(character);
        currentCharacterStates.Remove(character);
        nextAnimationChangeTime.Remove(character);
    }

    private string DetermineCharacterType(string characterName)
    {
        string name = characterName.ToLower();
        
        if (name.Contains("man") || name.Contains("male"))
            return "m_";
        else if (name.Contains("girl") || name.Contains("female") || name.Contains("woman"))
            return "f_";
        
        // 默认为男性
        return "m_";
    }

    // 播放指定动画
    public void PlayAnimation(GameObject character, AnimationType type)
    {
        if (character == null) return;
        
        Animator animator = character.GetComponent<Animator>();
        if (animator == null) return;
        
        // 确保Animator启用
        animator.enabled = true;
        // 使用更随机的动画速度
        animator.speed = Random.Range(minAnimationSpeed, maxAnimationSpeed);
        
        // 存储当前动画状态
        currentCharacterStates[character] = type;
        
        // 随机起始时间
        float normalizedTime = Random.value;
        float transitionDuration = Random.Range(0.2f, 0.4f); // 随机过渡时间
        
        // 根据动画类型播放相应动画
        switch (type)
        {
            case AnimationType.Idle:
                // 随机选择idle1或idle2
                int idleVariant = Random.Range(1, 3);
                string idleState = idleVariant == 1 ? "idle1" : "idle2";
                animator.CrossFade(idleState, transitionDuration, 0, normalizedTime);
                break;
                
            case AnimationType.Walk:
                animator.CrossFade("walk", transitionDuration, 0, normalizedTime);
                break;
                
            case AnimationType.Talk:
                // 随机选择talk1或talk2
                int talkVariant = Random.Range(1, 3);
                string talkState = talkVariant == 1 ? "talk1" : "talk2";
                animator.CrossFade(talkState, transitionDuration, 0, normalizedTime);
                break;
                
            case AnimationType.Listen:
                // 演讲者不应该听讲
                bool isPresenter = IsPresenter(character);
                if (isPresenter)
                {
                    // 演讲者用Talk代替Listen
                    int presenterTalkVariant = Random.Range(1, 3);
                    string presenterTalkState = presenterTalkVariant == 1 ? "talk1" : "talk2";
                    animator.CrossFade(presenterTalkState, transitionDuration, 0, normalizedTime);
                }
                else
                {
                    animator.CrossFade("listen", transitionDuration, 0, normalizedTime);
                }
                break;
                
            case AnimationType.Clap:
                animator.CrossFade("claphands", transitionDuration, 0, normalizedTime);
                break;
        }
        
        // 立即更新动画器
        animator.Update(0);
    }

    // 检查是否是演讲者
    private bool IsPresenter(GameObject character)
    {
        if (character == null) return false;
        
        // 如果我们有明确的演讲者引用，使用它
        if (presenterObject != null)
        {
            return character == presenterObject;
        }
        
        // 否则，使用名称检查
        string name = character.name.ToLower();
        return name.Contains("present") || name.Contains("speak") || name.Contains("lecturer");
    }

    // 设置当前提问者
    public void SetCurrentQuestioner(GameObject questioner)
    {
        // 如果有先前的提问者，重置其状态和材质
        if (currentQuestioner != null && currentQuestioner != questioner)
        {
            PlayAnimation(currentQuestioner, AnimationType.Listen);
            RestoreQuestionerColor(currentQuestioner);
        }
        
        // 设置新的提问者
        currentQuestioner = questioner;
        
        // 为新提问者播放Talk动画并设置高亮
        if (currentQuestioner != null)
        {
            PlayAnimation(currentQuestioner, AnimationType.Talk);
            HighlightQuestioner(currentQuestioner);
            Debug.Log($"已设置当前提问者: {currentQuestioner.name}");
        }
    }

    // 获取当前提问者
    public GameObject GetCurrentQuestioner()
    {
        return currentQuestioner;
    }

    // 重置当前提问者
    public void ResetCurrentQuestioner()
    {
        // 如果有提问者，让其回到Listen状态并恢复材质
        if (currentQuestioner != null)
        {
            PlayAnimation(currentQuestioner, AnimationType.Listen);
            RestoreQuestionerColor(currentQuestioner);
            currentQuestioner = null;
            Debug.Log("已重置当前提问者");
        }
    }

    // 设置演讲者
    public void SetPresenter(GameObject presenter)
    {
        presenterObject = presenter;
        Debug.Log($"已设置演讲者: {(presenter != null ? presenter.name : "null")}");
    }

    // 获取演讲者
    public GameObject GetPresenter()
    {
        return presenterObject;
    }

    // 播放所有角色特定动画的便捷方法
    public void PlayAllListen()
    {
        foreach (var character in characterAnimators.Keys.ToList())
        {
            if (character != null)
            {
                // 使用随机偏移播放Listen动画，使观众行为更自然
                PlayAnimationWithRandomOffset(character, AnimationType.Listen);
            }
        }
    }

    public void PlayAllIdle()
    {
        foreach (var character in characterAnimators.Keys.ToList())
        {
            if (character != null)
                PlayAnimation(character, AnimationType.Idle);
        }
    }

    public void PlayAllWalk()
    {
        foreach (var character in characterAnimators.Keys.ToList())
        {
            if (character != null)
                PlayAnimation(character, AnimationType.Walk);
        }
    }

    public void PlayAllTalk()
    {
        foreach (var character in characterAnimators.Keys.ToList())
        {
            if (character != null)
                PlayAnimation(character, AnimationType.Talk);
        }
    }

    public void PlayAllClap()
    {
        foreach (var character in characterAnimators.Keys.ToList())
        {
            if (character != null)
                PlayAnimation(character, AnimationType.Clap);
        }
    }

    // 处理演讲开始
    public void HandlePresentationStart()
    {
        // 更新状态
        currentState = PresentationState.Presenting;
        
        // 激活所有角色的动画
        foreach (var animator in characterAnimators.Values)
        {
            if (animator != null)
                animator.speed = Random.Range(0.8f, 1.2f);
        }
        
        // 清空下一次动画更新时间，让所有角色立即更新动画
        nextAnimationChangeTime.Clear();
        
        Debug.Log("演讲已开始，进入Presenting状态");
    }
    
    // 处理演讲结束，进入鼓掌阶段
    public void HandlePresentationEnd()
    {
        // 更新状态
        currentState = PresentationState.Applause;
        
        // 所有观众播放鼓掌动画
        PlayAllClap();
        
        Debug.Log("演讲已结束，进入Applause状态");
    }
    
    // 处理问答环节开始
    public void HandleQuestionPhaseStart()
    {
        // 更新状态
        currentState = PresentationState.QuestionTime;
        
        // 确保所有观众处于Listen状态
        foreach (var character in characterAnimators.Keys.ToList())
        {
            if (character != null && character != presenterObject && character != currentQuestioner)
            {
                PlayAnimation(character, AnimationType.Listen);
            }
        }
        
        // 确保演讲者处于Talk状态
        if (presenterObject != null)
        {
            PlayAnimation(presenterObject, AnimationType.Talk);
        }
        
        Debug.Log("问答环节已开始，进入QuestionTime状态");
    }
    
    // 处理演讲完成
    public void HandlePresentationComplete()
    {
        // 更新状态
        currentState = PresentationState.Completed;
        
        // 重置提问者
        ResetCurrentQuestioner();
        
        Debug.Log("演讲已完成，进入Completed状态");
    }

    // 处理演讲者动画序列
    public void StartPresenterSequence()
    {
        // 查找演讲者
        if (presenterObject == null)
        {
            presenterObject = characterAnimators.Keys.FirstOrDefault(c => 
                c != null && (c.name.ToLower().Contains("present") || c.name.ToLower().Contains("speak")));
        }
            
        if (presenterObject != null)
        {
            // 播放走路动画
            PlayAnimation(presenterObject, AnimationType.Walk);
            
            // 稍后切换到讲话动画
            StartCoroutine(DelayedAction(3f, () => {
                // 先播放短暂的Idle
                PlayAnimation(presenterObject, AnimationType.Idle);
                
                // 然后开始Talk
                StartCoroutine(DelayedAction(1f, () => {
                    PlayAnimation(presenterObject, AnimationType.Talk);
                    currentState = PresentationState.Presenting;
                }));
            }));
        }
    }
    
    // 辅助方法：延迟执行动作
    private System.Collections.IEnumerator DelayedAction(float delay, System.Action action)
    {
        yield return new WaitForSeconds(delay);
        action?.Invoke();
    }

    // 重置演讲状态
    public void ResetPresentationState()
    {
        // 重置状态
        currentState = PresentationState.NotStarted;
        
        // 重置提问者
        ResetCurrentQuestioner();
        
        // 暂停所有角色的动画
        foreach (Animator animator in characterAnimators.Values)
        {
            if (animator != null)
                animator.speed = 0;
        }
        
        // 清空状态
        nextAnimationChangeTime.Clear();
        
        Debug.Log("已重置演讲状态为NotStarted");
    }

    // 停止所有随机听讲协程
    public void StopAllRandomListenCoroutines()
    {
        // 清空下一次动画更新时间
        nextAnimationChangeTime.Clear();
        
        // 可能还需要停止特定的协程，如果有的话
        StopAllCoroutines();
    }

    // 启动观众随机动画
    public void StartRandomAudienceAnimations()
    {
        // 启用自动状态切换
        enableAutoStateChange = true;
        
        // 清空下一次动画更新时间，让所有角色立即更新动画
        nextAnimationChangeTime.Clear();
        
        // 为所有非演讲者角色设置动画
        foreach (GameObject character in characterAnimators.Keys.ToList())
        {
            if (character == null) continue;
            
            // 检查是否是演讲者
            bool isPresenter = IsPresenter(character);
            
            // 非演讲者且非当前提问者才设置随机动画
            if (!isPresenter && character != currentQuestioner)
            {
                // 激活动画 - 确保动画速度不为0
                Animator animator = characterAnimators[character];
                if (animator != null)
                {
                    animator.speed = Random.Range(0.8f, 1.2f);
                }
                
                // 设置随机动画和下次更新时间
                SetRandomAnimation(character);
                SetNextUpdateTime(character);
            }
        }
    }

    // 动画类型枚举
    public enum AnimationType
    {
        Idle,
        Walk,
        Listen,
        Talk,
        Clap
    }

    // 获取所有已注册的角色
    public List<GameObject> GetAllCharacters()
    {
        return characterAnimators.Keys.Where(c => c != null).ToList();
    }

    /// <summary>
    /// 让所有观众转向演讲者
    /// </summary>
    public IEnumerator RotateAllAudienceTowardsPresenter(GameObject presenter)
    {
        if (presenter == null) yield break;
        
        // 获取所有观众
        List<GameObject> allAudience = new List<GameObject>();
        
        foreach (var character in GetAllCharacters())
        {
            // 跳过演讲者本身
            if (character != presenter)
            {
                allAudience.Add(character);
            }
        }
        
        // 如果找不到观众，直接返回
        if (allAudience.Count == 0) yield break;
        
        // 同时旋转所有观众，但给每个人一个随机的小延迟，使其更自然
        foreach (GameObject audience in allAudience)
        {
            if (audience == null) continue;
            
            // 随机延迟，让旋转看起来更自然
            float delay = UnityEngine.Random.Range(0.0f, 0.5f);
            StartCoroutine(RotateAudienceWithDelay(audience, presenter, delay));
        }
        
        Debug.Log($"所有观众({allAudience.Count}人)开始转向演讲者");
        
        // 等待最长的旋转时间
        yield return new WaitForSeconds(1.5f);
    }

    /// <summary>
    /// 延迟后旋转单个观众
    /// </summary>
    private IEnumerator RotateAudienceWithDelay(GameObject audience, GameObject presenter, float delay)
    {
        // 等待随机延迟
        yield return new WaitForSeconds(delay);
        
        // 计算朝向演讲者的方向
        Vector3 directionToPresenter = presenter.transform.position - audience.transform.position;
        directionToPresenter.y = 0; // 保持水平旋转
        
        if (directionToPresenter != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPresenter);
            Quaternion startRotation = audience.transform.rotation;
            
            // 添加随机偏移，使观众不会完全对齐
            float randomYOffset = UnityEngine.Random.Range(-5f, 5f);
            targetRotation = Quaternion.Euler(0, targetRotation.eulerAngles.y + randomYOffset, 0);
            
            float rotationTime = 0;
            float rotationDuration = UnityEngine.Random.Range(0.5f, 1.0f); // 随机旋转持续时间
            
            while (rotationTime < rotationDuration)
            {
                rotationTime += Time.deltaTime;
                float t = rotationTime / rotationDuration;
                
                // 使用平滑的旋转插值
                audience.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
                
                yield return null;
            }
            
            // 确保最终旋转正确
            audience.transform.rotation = targetRotation;
        }
    }

    /// <summary>
    /// 平滑旋转演讲者面向提问者
    /// </summary>
    public IEnumerator RotatePresenterTowardsQuestioner(GameObject presenter, GameObject questioner)
    {
        if (presenter == null || questioner == null) yield break;
        
        // 计算朝向提问者的方向
        Vector3 directionToQuestioner = questioner.transform.position - presenter.transform.position;
        directionToQuestioner.y = 0; // 保持水平旋转
        
        if (directionToQuestioner != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToQuestioner);
            Quaternion startRotation = presenter.transform.rotation;
            
            float rotationTime = 0;
            float rotationDuration = 1.0f; // 旋转持续时间
            
            while (rotationTime < rotationDuration)
            {
                rotationTime += Time.deltaTime;
                float t = rotationTime / rotationDuration;
                
                // 使用平滑的旋转插值
                presenter.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
                
                yield return null;
            }
            
            // 确保最终旋转正确
            presenter.transform.rotation = targetRotation;
            
            Debug.Log($"演讲者已转向提问者 {questioner.name}");
        }
    }

    /// <summary>
    /// 平滑旋转提问者面向演讲者
    /// </summary>
    public IEnumerator RotateQuestionerTowardsPresenter(GameObject questioner, GameObject presenter)
    {
        if (questioner == null || presenter == null) yield break;
        
        // 计算朝向演讲者的方向
        Vector3 directionToPresenter = presenter.transform.position - questioner.transform.position;
        directionToPresenter.y = 0; // 保持水平旋转
        
        if (directionToPresenter != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPresenter);
            Quaternion startRotation = questioner.transform.rotation;
            
            float rotationTime = 0;
            float rotationDuration = 0.8f; // 稍微快一点的旋转
            
            while (rotationTime < rotationDuration)
            {
                rotationTime += Time.deltaTime;
                float t = rotationTime / rotationDuration;
                
                // 使用平滑的旋转插值
                questioner.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
                
                yield return null;
            }
            
            // 确保最终旋转正确
            questioner.transform.rotation = targetRotation;
            
            Debug.Log($"提问者 {questioner.name} 已转向演讲者");
        }
    }

    /// <summary>
    /// 重置演讲者旋转到默认朝向
    /// </summary>
    public IEnumerator ResetPresenterRotation(GameObject presenter)
    {
        if (presenter == null) yield break;
        
        // 默认朝向（面向前方/观众）
        Quaternion defaultRotation = Quaternion.Euler(0, 0, 0);
        Quaternion startRotation = presenter.transform.rotation;
        
        float rotationTime = 0;
        float rotationDuration = 1.0f;
        
        while (rotationTime < rotationDuration)
        {
            rotationTime += Time.deltaTime;
            float t = rotationTime / rotationDuration;
            
            presenter.transform.rotation = Quaternion.Slerp(startRotation, defaultRotation, t);
            
            yield return null;
        }
        
        // 确保最终旋转正确
        presenter.transform.rotation = defaultRotation;
        
        Debug.Log("演讲者已重置朝向");
    }

    /// <summary>
    /// 创建高亮材质
    /// </summary>
    private void CreateHighlightMaterial()
    {
        // 创建一个新的材质
        highlightMaterial = new Material(Shader.Find("Standard"));
        highlightMaterial.color = Color.white;
        highlightMaterial.SetFloat("_Metallic", 0.3f);
        highlightMaterial.SetFloat("_Glossiness", 0.8f);
        highlightMaterial.EnableKeyword("_EMISSION");
        highlightMaterial.SetColor("_EmissionColor", new Color(0.2f, 0.2f, 0.2f));
    }

    /// <summary>
    /// 设置提问者高亮
    /// </summary>
    public void HighlightQuestioner(GameObject questioner)
    {
        if (questioner == null) return;
        
        // 获取所有渲染器
        Renderer[] renderers = questioner.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            // 保存原始材质
            if (!originalMaterials.ContainsKey(questioner))
            {
                originalMaterials[questioner] = renderer.materials;
            }
            
            // 创建新的材质数组
            Material[] newMaterials = new Material[renderer.materials.Length];
            for (int i = 0; i < newMaterials.Length; i++)
            {
                newMaterials[i] = highlightMaterial;
            }
            
            // 应用新材质
            renderer.materials = newMaterials;
        }
    }
    
    /// <summary>
    /// 恢复提问者原始材质
    /// </summary>
    public void RestoreQuestionerColor(GameObject questioner)
    {
        if (questioner == null) return;
        
        // 获取所有渲染器
        Renderer[] renderers = questioner.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            // 恢复原始材质
            if (originalMaterials.ContainsKey(questioner))
            {
                renderer.materials = originalMaterials[questioner];
            }
        }
    }
} 


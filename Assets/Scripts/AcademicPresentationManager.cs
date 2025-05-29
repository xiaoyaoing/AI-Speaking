using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.Events;

/// <summary>
/// 学术汇报管理器 - 控制整个学术汇报流程
/// </summary>
public class AcademicPresentationManager : MonoBehaviour
{
    [Header("引用")]
    [Tooltip("UI控制器")]
    public AcademicReportUI uiController;
    
    [Tooltip("观众管理器")]
    public AudienceManager audienceManager;
    
    [Tooltip("问题音频播放器")]
    public AudioSource questionAudioSource;
    
    [Tooltip("问题音频文件列表")]
    public AudioClip[] questionAudioClips;
    
    [Tooltip("默认音频路径")]
    public string defaultAudioPath = "Audio/Questions/default";
    
    [Tooltip("当前问题内容描述")]
    public string currentQuestionContent = "";
    
    [Tooltip("音频播放速度")]
    [Range(0.5f, 2.0f)]
    public float audioPlaybackSpeed = 1.0f;
    
    [Header("汇报设置")]
    [Tooltip("主题")]
    public string presentationTopic = "默认学术主题";
    
    [Tooltip("汇报人姓名")]
    public string presenterName = "汇报人";
    
    [Tooltip("指导老师")]
    public string supervisor = "指导老师";
    
    [Tooltip("汇报时间(分钟)")]
    [Range(1, 30)]
    public float presentationTime = 10f;
    
    [Tooltip("问答时间(分钟)")]
    [Range(1, 20)]
    public float questionTime = 5f;
    
    [Header("状态")]
    [Tooltip("保存当前剩余时间(秒)")]
    public float currentTimeRemaining;

    [Tooltip("是否优先从后端加载音频")]
    public bool loadAudioFromBackendFirst = true;

[   Tooltip("音频加载超时时间(秒)")]
    public float audioLoadTimeout = 20f;
    // 计时器协程
    private Coroutine timerCoroutine;
    
    // 事件
    public UnityEvent onPresentationStart = new UnityEvent();
    public UnityEvent onPresentationEnd = new UnityEvent();
    public UnityEvent onQuestionPhaseStart = new UnityEvent();
    public UnityEvent onQuestionPhaseEnd = new UnityEvent();
    public UnityEvent<float> onTimerUpdate = new UnityEvent<float>();
    public UnityEvent onPresentationComplete = new UnityEvent();
    
    [Header("语速设置")]
    [Tooltip("默认语速")]
    public float defaultSpeechRate = 1.0f;
    
    [Tooltip("语速最小范围")]
    public float minSpeechRate = 0.8f;
    
    [Tooltip("语速最大范围")]
    public float maxSpeechRate = 1.2f;
    
    // 当前语速
    private float currentSpeechRate = 1.0f;
    
    [Header("音频设置")]
    [Tooltip("是否将音频源移动到说话者位置")]
    public bool moveAudioToSpeaker = true;
    
    [Tooltip("音频源距离说话者的高度偏移")]
    public float audioSourceHeightOffset = 1.5f;
    
    // 当前正在说话的角色
    private GameObject currentSpeaker = null;
    
    // 音频源的原始父对象和位置
    private Transform originalAudioParent;
    private Vector3 originalAudioPosition;
    
    [Header("场景介绍音频")]
    [Tooltip("场景介绍音频")]
    public AudioSource introductionAudioSource;

    [Tooltip("场景介绍音频剪辑")]
    public AudioClip introductionAudioClip;

    [Tooltip("是否正在播放介绍音频")]
    public bool isPlayingIntroduction = false;

    [Tooltip("演讲者标签")]
    public string presenterTag = "Presenter";

    [Tooltip("是否允许演讲者移动")]
    private bool presenterCanMove = false;

    [Header("掌声音效")]
    [Tooltip("掌声音效音频源")]
    public AudioSource applauseAudioSource;

    [Tooltip("掌声音效音频剪辑")]
    public AudioClip applauseAudioClip;
    
    [Header("幻灯片设置")]
    [Tooltip("幻灯片幕布对象")]
    public GameObject screenObject;

    [Tooltip("幻灯片图片路径")]
    public string slidesPath = "ppts";

    [Tooltip("幻灯片切换时间(秒)")]
    public float slideDuration = 5f;

    [Tooltip("幻灯片切换过渡时间(秒)")]
    public float transitionDuration = 1f;

    [Tooltip("上一页按键")]
    public KeyCode previousSlideKey = KeyCode.X;

    [Tooltip("下一页按键")]
    public KeyCode nextSlideKey = KeyCode.C;

    // public string apiUrl = "http://localhost:5001/gen_hello";

    // 幻灯片播放器引用
    private SlidePlayer slidePlayer;
    
    /// <summary>
    /// 在开始时加载默认音频
    /// </summary>
    private void LoadDefaultAudio()
    {
        if (string.IsNullOrEmpty(defaultAudioPath))
        {
            Debug.LogWarning("默认音频路径未设置！");
            return;
        }
        
        AudioClip defaultClip = Resources.Load<AudioClip>(defaultAudioPath);
        if (defaultClip != null)
        {
            // 如果已有音频列表，添加到列表中
            if (questionAudioClips != null && questionAudioClips.Length > 0)
            {
                AudioClip[] newClips = new AudioClip[questionAudioClips.Length + 1];
                questionAudioClips.CopyTo(newClips, 0);
                newClips[questionAudioClips.Length] = defaultClip;
                questionAudioClips = newClips;
            }
            else
            {
                // 创建新列表
                questionAudioClips = new AudioClip[] { defaultClip };
            }
            
            Debug.Log($"已加载默认音频: {defaultAudioPath}");
        }
        else
        {
            Debug.LogWarning($"无法加载默认音频: {defaultAudioPath}");
        }
    }
    // /// <summary>
    // /// 在开始时加载音频
    // /// </summary>  
    // private void PlayIntroductionAudio()
    // {
        
    //     AudioClip defaultClip = Resources.Load<AudioClip>(defaultAudioPath);
    //     if (defaultClip != null)
    //     {
    //         // 如果已有音频列表，添加到列表中
    //         if (questionAudioClips != null && questionAudioClips.Length > 0)
    //         {
    //             AudioClip[] newClips = new AudioClip[questionAudioClips.Length + 1];
    //             questionAudioClips.CopyTo(newClips, 0);
    //             newClips[questionAudioClips.Length] = defaultClip;
    //             questionAudioClips = newClips;
    //         }
    //         else
    //         {
    //             // 创建新列表
    //             questionAudioClips = new AudioClip[] { defaultClip };
    //         }
            
    //         Debug.Log($"已加载音频");
    //     }
    //     else
    //     {
    //         Debug.LogWarning($"无法加载音频");
    //     }
    // }
    private void Start()
    {
        // 初始化
        if (uiController == null)
        {
            uiController = FindObjectOfType<AcademicReportUI>();
        }
        
        if (audienceManager == null)
        {
            audienceManager = FindObjectOfType<AudienceManager>();
        }
        
        // 设置初始UI状态
        UpdateUISettings();

        // // 播放场景介绍音频
        // PlayIntroductionAudio();       
        // 加载默认音频
        LoadDefaultAudio();

        // 设置初始语速
        currentSpeechRate = defaultSpeechRate;
        
        // 注册事件的默认处理函数
        RegisterEventHandlers();
        
        // 隐藏音频源图标
        HideAudioSourceGizmo();
        
        // 保存音频源的原始信息
        if (questionAudioSource != null)
        {
            originalAudioParent = questionAudioSource.transform.parent;
            originalAudioPosition = questionAudioSource.transform.localPosition;
        }
        
        // 初始化幻灯片播放器
        InitializeSlidePlayer();
    }

    /// <summary>
    /// 注册事件的默认处理函数
    /// </summary>
    private void RegisterEventHandlers()
    {
        // 演讲开始事件
        onPresentationStart.AddListener(() => {
            Debug.Log("Presentation started!");
            
            // 让所有观众看向讲台
            if (audienceManager != null)
            {
                audienceManager.PlayAllAudienceListen();
            }
            
            // 获取角色管理器实例
            RuntimeCharacterManager characterManager = RuntimeCharacterManager.Instance;
            if (characterManager != null)
            {
                // 设置演讲开始状态
                characterManager.HandlePresentationStart();
                
                // 让所有观众看向讲台 - 播放聆听动画
                characterManager.PlayAllListen();
            }
        });
        
        // 演讲结束事件
        onPresentationEnd.AddListener(() => {
            Debug.Log("Presentation ended!");
            
            // 获取角色管理器实例进入鼓掌阶段
            RuntimeCharacterManager characterManager = RuntimeCharacterManager.Instance;
            if (characterManager != null)
            {
                // 设置演讲结束状态（鼓掌阶段）
                characterManager.HandlePresentationEnd();
            }
            
            // 让所有观众鼓掌
            TriggerApplause();
            
            // 停止观众随机听讲动画
            if (characterManager != null)
            {
                characterManager.StopAllRandomListenCoroutines();
            }
        });
        
        // 问答阶段开始事件
        onQuestionPhaseStart.AddListener(() => {
            Debug.Log("Question phase started!");
            
            
            
            // 更新角色管理器的状态为问答环节
            RuntimeCharacterManager characterManager = RuntimeCharacterManager.Instance;
            if (characterManager != null)
            {
                characterManager.HandleQuestionPhaseStart();
            }
            
            // 1.5秒后自动触发第一个问题
            StartCoroutine(TriggerFirstQuestionAfterDelay(1.5f));
        });
        
        // 问答阶段结束事件
        onQuestionPhaseEnd.AddListener(() => {
            Debug.Log("Question phase ended!");
            
            // 重置观众状态
            if (audienceManager != null)
            {
                audienceManager.EndQuestionPhase();
            }
            
            // 重置提问者状态
            RuntimeCharacterManager characterManager = RuntimeCharacterManager.Instance;
            if (characterManager != null)
            {
                characterManager.ResetCurrentQuestioner();
            }
        });
        
        // 整个演讲完成事件
        onPresentationComplete.AddListener(() => {
            Debug.Log("Presentation completed!");
            
            // 设置为完成状态
            RuntimeCharacterManager characterManager = RuntimeCharacterManager.Instance;
            if (characterManager != null)
            {
                characterManager.HandlePresentationComplete();
            }
            
            // 重置所有状态
            ResetPresentation();
        });
    }
    
    /// <summary>
    /// 延迟触发第一个问题的协程
    /// </summary>
    private IEnumerator TriggerFirstQuestionAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        AskRandomQuestion();
    }
    
    /// <summary>
    /// 开始汇报
    /// </summary>
    public void StartPresentation()
    {
        // 获取RuntimeCharacterManager实例
        RuntimeCharacterManager characterManager = RuntimeCharacterManager.Instance;
        
        // 检查当前状态，如果已经在演讲或问答阶段，直接返回
        if (characterManager != null && 
            (characterManager.currentState == RuntimeCharacterManager.PresentationState.Presenting ||
             characterManager.currentState == RuntimeCharacterManager.PresentationState.QuestionTime)) 
        {
            return;
        }
        
        // 设置初始时间
        currentTimeRemaining = presentationTime * 60f; // 转换为秒
        
        // 启动计时器
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
        }
        timerCoroutine = StartCoroutine(TimerCoroutine());
        
        // 触发事件
        onPresentationStart.Invoke();
        
        // 获取角色管理器实例
        if (characterManager != null)
        {
            // 设置演讲开始状态
            characterManager.HandlePresentationStart();
            
            // 让所有观众看向讲台 - 播放聆听动画
            characterManager.PlayAllListen();
            
            // 获取演讲者，让所有人面向演讲者
            GameObject presenter = characterManager.GetPresenter();
            if (presenter != null)
            {
                // 使用RuntimeCharacterManager的方法让所有观众转向演讲者
                StartCoroutine(characterManager.RotateAllAudienceTowardsPresenter(presenter));
            }
        }
        
        // 通过观众管理器另外确保所有观众看向讲台
        if (audienceManager != null)
        {
            audienceManager.PlayAllAudienceListen();
        }

        // 更新UI
        if (uiController != null)
        {
            uiController.UpdateTimerDisplay(currentTimeRemaining);
        }
        
        Debug.Log($"开始学术汇报: {presentationTopic}，汇报人: {presenterName}，时间: {presentationTime}分钟");
    }
    
    /// <summary>
    /// 结束汇报，开始问答阶段
    /// </summary>
    public void StartQuestionPhase()
    {
        // 获取RuntimeCharacterManager实例
        RuntimeCharacterManager characterManager = RuntimeCharacterManager.Instance;
        
        // 检查当前状态，只有在演讲阶段才能进入问答阶段
        if (characterManager == null || characterManager.currentState != RuntimeCharacterManager.PresentationState.Presenting)
        {
            return;
        }
        
        // 先触发汇报结束事件，让观众鼓掌
        onPresentationEnd.Invoke();
        
        // 设置为鼓掌阶段
        characterManager.HandlePresentationEnd();
        
        // 等待鼓掌结束再进入问答阶段
        StartCoroutine(StartQuestionPhaseAfterApplause(3.0f));
        
        AskRandomQuestion();
    }
    
    /// <summary>
    /// 等待鼓掌结束后进入问答阶段
    /// </summary>
    private IEnumerator StartQuestionPhaseAfterApplause(float applauseDuration)
    {
        Debug.Log("观众正在鼓掌...");
        
        // 等待鼓掌持续时间
        yield return new WaitForSeconds(applauseDuration);
        
        // 停止之前的计时器
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
        }
        
        // 停止观众随机听讲动画
        RuntimeCharacterManager characterManager = RuntimeCharacterManager.Instance;
        if (characterManager != null)
        {
            characterManager.StopAllRandomListenCoroutines();
        }
        
        // 设置问答时间
        currentTimeRemaining = questionTime * 60f; // 转换为秒
        
        // 启动问答阶段计时器
        timerCoroutine = StartCoroutine(TimerCoroutine());
        
        // 触发问答阶段开始事件
        onQuestionPhaseStart.Invoke();
        
        // 更新UI
        if (uiController != null)
        {
            uiController.UpdateTimerDisplay(currentTimeRemaining);
        }
        
        Debug.Log("鼓掌结束，开始问答阶段");
    }
    
    /// <summary>
    /// 让随机观众提问
    /// </summary>
    public void AskRandomQuestion()
    {
        // 获取RuntimeCharacterManager实例
        RuntimeCharacterManager characterManager = RuntimeCharacterManager.Instance;
        
        // 检查是否在问答阶段
        if (characterManager == null || characterManager.currentState != RuntimeCharacterManager.PresentationState.QuestionTime) 
        {
            return;
        }
        
        if (audienceManager != null)
        {
            // 使用AudienceManager的GetRandomQuestioner方法
            GameObject questioner = audienceManager.GetRandomQuestioner();
            
            if (questioner != null)
            {
                Debug.Log($"观众 {questioner.name} 正在提问...");
                
                // 设置当前提问者
                if (characterManager != null)
                {
                    // 设置当前提问者
                    characterManager.SetCurrentQuestioner(questioner);
                    
                    // 获取演讲者对象
                    GameObject presenter = characterManager.GetPresenter();
                    if (presenter != null)
                    {
                        // 使用RuntimeCharacterManager的方法让演讲者和提问者互相看向对方
                        StartCoroutine(characterManager.RotatePresenterTowardsQuestioner(presenter, questioner));
                        StartCoroutine(characterManager.RotateQuestionerTowardsPresenter(questioner, presenter));
                    }
                }
                
                // 设置当前说话者
                SetCurrentSpeaker(questioner);
                
                // 使用新的方法播放随机问题音频
                PlayQuestionAudio(-1); // -1表示随机选择
            }
        }
    }
    
    /// <summary>
    /// 设置当前说话者并移动音频源
    /// </summary>
    /// <param name="speaker">说话者游戏对象</param>
    public void SetCurrentSpeaker(GameObject speaker)
    {
        if (speaker == null) return;
        
        currentSpeaker = speaker;
        
        
        
        // 如果启用了音频源移动功能，将音频源移动到说话者位置
        if (moveAudioToSpeaker && questionAudioSource != null && currentSpeaker != null)
        {
            // 将音频源移动到说话者头部位置
            MoveAudioSourceToSpeaker();
        }
    }
    
    /// <summary>
    /// 将音频源移动到当前说话者位置
    /// </summary>
    private void MoveAudioSourceToSpeaker()
    {
        if (questionAudioSource == null || currentSpeaker == null) return;
        
        // 获取说话者的头部位置（假设Y轴向上）
        Vector3 headPosition = currentSpeaker.transform.position + new Vector3(0, audioSourceHeightOffset, 0);
        
        // 将音频源父对象设置为说话者
        questionAudioSource.transform.SetParent(currentSpeaker.transform, true);
        
        // 设置音频源位置
        questionAudioSource.transform.position = headPosition;
        
        Debug.Log($"音频源已移动到说话者 {currentSpeaker.name} 的位置");
    }
    
    /// <summary>
    /// 重置音频源到原始位置
    /// </summary>
    private void ResetAudioSourcePosition()
    {
        if (questionAudioSource == null) return;
        
        // 恢复音频源的原始父对象和位置
        if (originalAudioParent != null)
        {
            questionAudioSource.transform.SetParent(originalAudioParent, false);
            questionAudioSource.transform.localPosition = originalAudioPosition;
        }
        
        currentSpeaker = null;
        
        Debug.Log("音频源已重置到原始位置");
    }
    
    /// <summary>
    /// 完成整个汇报过程
    /// </summary>
    public void CompletePresentation()
    {
        // 获取RuntimeCharacterManager实例
        RuntimeCharacterManager characterManager = RuntimeCharacterManager.Instance;
        
        // 停止所有计时器
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }
        
        // 如果在问答阶段，触发问答阶段结束事件
        if (characterManager != null && characterManager.currentState == RuntimeCharacterManager.PresentationState.QuestionTime)
        {
            onQuestionPhaseEnd.Invoke();
            
            // 重置观众状态
            if (audienceManager != null)
            {
                audienceManager.EndQuestionPhase();
            }
        }
        // 如果在演讲阶段，触发汇报结束事件
        else if (characterManager != null && characterManager.currentState == RuntimeCharacterManager.PresentationState.Presenting)
        {
            onPresentationEnd.Invoke();
        }
        
        // 触发汇报完成事件
        onPresentationComplete.Invoke();
        
        Debug.Log("学术汇报完成");
    }
    
    /// <summary>
    /// 重置汇报
    /// </summary>
    public void ResetPresentation()
    {
        // 停止所有计时器
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }
        
        // 重置观众
        if (audienceManager != null)
        {
            audienceManager.ResetAllAudience();
        }
        
        // 重置角色动画管理器的演讲状态
        RuntimeCharacterManager characterManager = RuntimeCharacterManager.Instance;
        if (characterManager != null)
        {
            characterManager.ResetPresentationState();
        }
        
        // 重置音频源位置
        ResetAudioSourcePosition();
        
        Debug.Log("重置学术汇报");
    }
    
    /// <summary>
    /// 计时器协程
    /// </summary>
    private IEnumerator TimerCoroutine()
    {
        // 获取RuntimeCharacterManager实例
        RuntimeCharacterManager characterManager = RuntimeCharacterManager.Instance;
        
        while (currentTimeRemaining > 0)
        {
            yield return new WaitForSeconds(1f);
            
            currentTimeRemaining -= 1f;
            
            // 触发计时器更新事件
            onTimerUpdate.Invoke(currentTimeRemaining);
            
            // 更新UI显示
            if (uiController != null)
            {
                uiController.UpdateTimerDisplay(currentTimeRemaining);
            }
            
            // 检查时间是否结束
            if (currentTimeRemaining <= 0)
            {
                // 如果是汇报阶段结束，自动进入问答阶段
                if (characterManager != null && characterManager.currentState == RuntimeCharacterManager.PresentationState.Presenting)
                {
                    StartQuestionPhase();
                }
                // 如果是问答阶段结束，完成整个汇报
                else if (characterManager != null && characterManager.currentState == RuntimeCharacterManager.PresentationState.QuestionTime)
                {
                    CompletePresentation();
                }
                
                break;
            }
        }
    }
    
    /// <summary>
    /// 更新设置
    /// </summary>
    public void UpdateSettings(string topic, string presenter, string supervisorName, float presTime, float quesTime)
    {
        presentationTopic = topic;
        presenterName = presenter;
        supervisor = supervisorName;
        presentationTime = presTime;
        questionTime = quesTime;
        
        UpdateUISettings();
        
        Debug.Log($"更新设置 - 主题: {topic}, 汇报人: {presenter}, 指导老师: {supervisorName}, " +
                 $"汇报时间: {presTime}分钟, 问答时间: {quesTime}分钟");
    }
    
    /// <summary>
    /// 更新UI设置
    /// </summary>
    private void UpdateUISettings()
    {
        if (uiController != null)
        {
            // 只更新计时器显示
            uiController.UpdateTimerDisplay(presentationTime * 60f);
        }
    }
    
    /// <summary>
    /// 通过音频文件路径播放问题
    /// </summary>
    /// <param name="audioPath">音频文件路径</param>
    /// <param name="questionText">问题内容描述</param>
    public void PlayQuestionByPath(string audioPath, string questionText = "")
    {
        // 获取RuntimeCharacterManager实例
        RuntimeCharacterManager characterManager = RuntimeCharacterManager.Instance;
        
        // 检查是否在问答阶段
        if (characterManager == null || characterManager.currentState != RuntimeCharacterManager.PresentationState.QuestionTime) 
        {
            return;
        }
        
        // 确保有音频播放器
        if (questionAudioSource != null)
        {
            // 设置当前问题内容
            currentQuestionContent = !string.IsNullOrEmpty(questionText) ? questionText : $"问题音频: {audioPath}";
            
            // 加载音频资源
            AudioClip clip = Resources.Load<AudioClip>(audioPath);
            
            if (clip != null)
            {
                // 播放音频
                questionAudioSource.clip = clip;
                questionAudioSource.pitch = audioPlaybackSpeed; // 设置播放速度
                questionAudioSource.Play();
                
                Debug.Log($"正在播放问题音频: {audioPath}, 内容: {currentQuestionContent}, 速度: {audioPlaybackSpeed}");
            }
            else
            {
                Debug.LogWarning($"无法加载音频资源: {audioPath}");
            }
        }
        else
        {
            Debug.LogWarning("问题音频组件未设置，无法播放问题音频。");
        }
    }
    
    /// <summary>
    /// 从字节流加载并播放音频
    /// </summary>
    /// <param name="audioData">音频字节数据</param>
    /// <param name="questionText">问题内容描述</param>
    public void PlayQuestionFromBytes(byte[] audioData, string questionText)
    {
        // 获取RuntimeCharacterManager实例
        RuntimeCharacterManager characterManager = RuntimeCharacterManager.Instance;
        
        // 检查是否在问答阶段
        if (characterManager == null || characterManager.currentState != RuntimeCharacterManager.PresentationState.QuestionTime) 
        {
            return;
        }
        
        // 确保有音频播放器
        if (questionAudioSource != null && audioData != null && audioData.Length > 0)
        {
            // 设置当前问题内容
            currentQuestionContent = !string.IsNullOrEmpty(questionText) ? questionText : "问题音频(从字节流加载)";
            
            try
            {
                // 使用AudioUtility从WAV字节流创建AudioClip
                AudioClip clip = AudioUtility.CreateAudioClipFromWAV(audioData, "question_from_bytes");
                
                if (clip != null)
                {
                    // 获取音频时长
                    float duration = AudioUtility.GetWAVDuration(audioData);
                    
                    // 播放音频
                    questionAudioSource.clip = clip;
                    questionAudioSource.pitch = audioPlaybackSpeed; // 设置播放速度
                    questionAudioSource.Play();
                    
                    Debug.Log($"正在播放从字节流创建的问题音频，内容: {currentQuestionContent}, 时长: {duration:F1}秒, 速度: {audioPlaybackSpeed}");
                }
                else
                {
                    Debug.LogError("从字节流创建音频剪辑失败");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"从字节流创建音频剪辑时出错: {e.Message}");
            }
        }
        else
        {
            Debug.LogWarning("音频组件未设置或音频数据为空，无法播放问题音频。");
        }
    }
    
    /// <summary>
    /// 设置音频播放速度
    /// </summary>
    /// <param name="speed">播放速度（0.5-2.0）</param>
    public void SetAudioPlaybackSpeed(float speed)
    {
        audioPlaybackSpeed = Mathf.Clamp(speed, 0.5f, 2.0f);
        
        // 如果当前正在播放，实时调整速度
        if (questionAudioSource != null && questionAudioSource.isPlaying)
        {
            questionAudioSource.pitch = audioPlaybackSpeed;
        }
        
        Debug.Log($"音频播放速度已设置为: {audioPlaybackSpeed}");
    }
    
    /// <summary>
    /// 播放问题音频
    /// </summary>
    public void PlayQuestionAudio(int questionIndex = -1)
    {
        // 获取RuntimeCharacterManager实例
        RuntimeCharacterManager characterManager = RuntimeCharacterManager.Instance;
        
        // 检查是否在问答阶段
        if (characterManager == null || characterManager.currentState != RuntimeCharacterManager.PresentationState.QuestionTime) 
        {
            return;
        }
        
        // 确保有音频播放器和音频剪辑
        if (questionAudioSource != null && questionAudioClips != null && questionAudioClips.Length > 0)
        {
            // 如果没有当前说话者，尝试获取一个
            if (currentSpeaker == null && audienceManager != null)
            {
                GameObject questioner = audienceManager.GetRandomQuestioner();
                if (questioner != null)
                {
                    SetCurrentSpeaker(questioner);
                }
            }
            
            AudioClip clipToPlay;
            
            // 如果指定了索引并且索引有效，使用指定的音频
            if (questionIndex >= 0 && questionIndex < questionAudioClips.Length)
            {
                clipToPlay = questionAudioClips[questionIndex];
            }
            // 否则随机选择一个音频
            else
            {
                clipToPlay = questionAudioClips[UnityEngine.Random.Range(0, questionAudioClips.Length)];
            }
            
            // 设置当前问题内容
            currentQuestionContent = $"问题: {clipToPlay.name}";
            
            // 播放音频
            questionAudioSource.clip = clipToPlay;
            // 使用随机语速
            float randomSpeechRate = UnityEngine.Random.Range(minSpeechRate, maxSpeechRate);
            questionAudioSource.pitch = randomSpeechRate;
            questionAudioSource.Play();
            
            // 在音频完成后重置音频源位置
            StartCoroutine(ResetAudioSourceAfterPlaying(clipToPlay.length / randomSpeechRate));
            
            Debug.Log($"正在播放问题音频: {clipToPlay.name}, 速度: {randomSpeechRate}");
        }
        else
        {
            Debug.LogWarning("问题音频组件或音频剪辑未设置，无法播放问题音频。");
        }
    }
    
    /// <summary>
    /// 在音频播放完成后重置音频源位置
    /// </summary>
    private IEnumerator ResetAudioSourceAfterPlaying(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // 停止说话动画
        if (currentSpeaker != null)
        {
            RuntimeCharacterManager characterManager = RuntimeCharacterManager.Instance;
            if (characterManager != null)
            {
                // 将当前发言者重置为听讲状态
                characterManager.PlayAnimation(currentSpeaker, RuntimeCharacterManager.AnimationType.Listen);
                
                // 如果当前发言者是提问者，重置提问者状态和颜色
                if (characterManager.GetCurrentQuestioner() == currentSpeaker)
                {
                    characterManager.ResetCurrentQuestioner();
                    
                    // 获取演讲者并重置朝向
                    GameObject presenter = characterManager.GetPresenter();
                    if (presenter != null)
                    {
                        StartCoroutine(ResetPresenterRotation(presenter));
                    }
                }
            }
        }
        
        // 重置音频源位置
        ResetAudioSourcePosition();
    }
    
    /// <summary>
    /// 重置演讲者旋转到默认朝向
    /// </summary>
    private IEnumerator ResetPresenterRotation(GameObject presenter)
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
    /// 停止问题音频播放
    /// </summary>
    public void StopQuestionAudio()
    {
        if (questionAudioSource != null && questionAudioSource.isPlaying)
        {
            questionAudioSource.Stop();
            ResetAudioSourcePosition();
            Debug.Log("已停止播放问题音频");
        }
    }
    
    /// <summary>
    /// 设置语速
    /// </summary>
    public void SetSpeechRate(float rate)
    {
        if (rate <= 0) return;
        
        // 使用随机范围
        currentSpeechRate = Mathf.Clamp(rate, minSpeechRate, maxSpeechRate);
        
        // 如果当前正在播放，实时调整速度
        if (questionAudioSource != null && questionAudioSource.isPlaying)
        {
            questionAudioSource.pitch = currentSpeechRate;
        }
        
        Debug.Log($"设置语速为: {currentSpeechRate}");
    }
    
    /// <summary>
    /// 触发所有观众鼓掌
    /// </summary>
    public void TriggerApplause()
    {
        Debug.Log("触发所有观众鼓掌");
        
        // 播放掌声音效
        PlayApplauseSound();
        
        // 获取RuntimeCharacterManager实例并触发所有人鼓掌
        RuntimeCharacterManager characterManager = RuntimeCharacterManager.Instance;
        if (characterManager != null)
        {
            characterManager.PlayAllClap();
        }
        
        // AudienceManager是可选的，如果系统中没有这个组件也能正常工作
        if (audienceManager != null)
        {
            // 可选：通过AudienceManager实现更自然的鼓掌动画效果（有延迟和随机性）
            audienceManager.PlayAllAudienceAnimation(RuntimeCharacterManager.AnimationType.Clap);
        }
    }
    
    /// <summary>
    /// 播放掌声音效
    /// </summary>
    private void PlayApplauseSound()
    {
        if (applauseAudioSource != null && applauseAudioClip != null)
        {
            // 设置音频剪辑
            applauseAudioSource.clip = applauseAudioClip;
            
            // 播放音频
            applauseAudioSource.Play();
            
            Debug.Log("正在播放掌声音效");
        }
        else
        {
            Debug.LogWarning("掌声音效音频源或音频剪辑未设置，无法播放掌声音效");
        }
    }
    
    /// <summary>
    /// 隐藏音频源的可视化图标
    /// </summary>
    private void HideAudioSourceGizmo()
    {
        if (questionAudioSource != null)
        {
            // 禁用音频源的图标
#if UNITY_EDITOR
            // 设置音频源的hideFlags，使其在Scene视图中不可见
            questionAudioSource.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
            
            // 另一种方式：添加一个空组件使其不显示图标
            AudioSourceHider hider = questionAudioSource.gameObject.AddComponent<AudioSourceHider>();
            hider.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
#endif
        }
    }
    
    /// <summary>
    /// 自动查找演讲者对象
    /// </summary>
    /// <returns>演讲者游戏对象</returns>
    private GameObject FindPresenter()
    {
        // 方法1：通过标签查找
        GameObject presenter = GameObject.FindGameObjectWithTag(presenterTag);
        
        // 方法2：通过名称查找（如果方法1未找到）
        if (presenter == null)
        {
            GameObject[] possiblePresenters = GameObject.FindObjectsOfType<GameObject>();
            foreach (GameObject obj in possiblePresenters)
            {
                if (obj.name.ToLower().Contains("presenter") || 
                    obj.name.ToLower().Contains("speaker") || 
                    obj.name.ToLower().Contains("lecturer"))
                {
                    presenter = obj;
                    break;
                }
            }
        }
        
        // 方法3：通过位置查找（在讲台附近）
        if (presenter == null)
        {
            GameObject podium = GameObject.Find("Podium");
            if (podium != null)
            {
                Collider[] nearbyObjects = Physics.OverlapSphere(podium.transform.position, 3.0f);
                foreach (Collider col in nearbyObjects)
                {
                    if (col.gameObject.name.ToLower().Contains("character") || 
                        col.gameObject.name.ToLower().Contains("person"))
                    {
                        presenter = col.gameObject;
                        break;
                    }
                }
            }
        }
        
        if (presenter != null)
        {
            Debug.Log($"自动找到演讲者对象: {presenter.name}");
        }
        else
        {
            Debug.LogWarning("未能找到演讲者对象");
        }
        
        return presenter;
    }
    
    /// <summary>
    /// 播放场景介绍音频
    /// </summary>
    public void PlayIntroductionAudio()
    {
        if (introductionAudioSource != null && introductionAudioClip != null)
        {
            // 设置音频剪辑
            introductionAudioSource.clip = introductionAudioClip;
            
            // 播放音频
            introductionAudioSource.Play();
            
            // 设置状态
            isPlayingIntroduction = true;
            presenterCanMove = false;
            
            // 启动协程等待音频播放完成
            StartCoroutine(WaitForIntroductionToFinish());
            
            Debug.Log("正在播放场景介绍音频");
        }
        else
        {
            Debug.LogWarning("场景介绍音频源或音频剪辑未设置，无法播放介绍音频");
            // 如果没有介绍音频，直接允许演讲者移动
            presenterCanMove = true;
        }
    }
    
    /// <summary>
    /// 等待介绍音频播放完成
    /// </summary>
    private IEnumerator WaitForIntroductionToFinish()
    {
        // 等待音频播放完成
        while (introductionAudioSource != null && introductionAudioSource.isPlaying)
        {
            yield return null;
        }
        
        // 音频播放完成后更新状态
        isPlayingIntroduction = false;
        presenterCanMove = true;
        
        Debug.Log("介绍音频播放完成，演讲者现在可以移动");
        
        // 可以在这里触发事件或其他操作
        OnIntroductionFinished();
    }
    
    /// <summary>
    /// 介绍音频播放完成后的回调
    /// </summary>
    private void OnIntroductionFinished()
    {
        // 这里可以添加介绍音频播放完成后的操作
        // 例如显示UI提示、启用某些游戏功能等
    }
    
    /// <summary>
    /// 检查演讲者是否可以移动
    /// </summary>
    public bool CanPresenterMove()
    {
        return presenterCanMove;
    }
    
    /// <summary>
    /// 跳过介绍音频
    /// </summary>
    public void SkipIntroduction()
    {
        if (isPlayingIntroduction && introductionAudioSource != null)
        {
            // 停止音频播放
            introductionAudioSource.Stop();
            
            // 更新状态
            isPlayingIntroduction = false;
            presenterCanMove = true;
            
            Debug.Log("介绍音频已跳过，演讲者现在可以移动");
            
            // 调用完成回调
            OnIntroductionFinished();
        }
    }

    /// <summary>
    /// 初始化幻灯片播放器
    /// </summary>
    private void InitializeSlidePlayer()
    {
        if (screenObject == null)
        {
            Debug.LogWarning("未设置幻灯片幕布对象！");
            return;
        }

        // 添加SlidePlayer组件
        slidePlayer = screenObject.AddComponent<SlidePlayer>();
        
        // 设置基本参数
        slidePlayer.screenObject = screenObject;
        slidePlayer.slideDuration = slideDuration;
        slidePlayer.transitionDuration = transitionDuration;
        slidePlayer.autoPlay = false; // 不自动播放，等待用户控制
        
        // 加载幻灯片
        slidePlayer.LoadSlidesFromResources(slidesPath);
        
        // 显示第一页
        
        Debug.Log($"已初始化幻灯片播放器，从 {slidesPath} 加载幻灯片");
    }

    private void Update()
    {
        // 检查按键输入
        if (slidePlayer != null)
        {
            // 添加调试日志
            if (Input.GetKeyDown(previousSlideKey))
            {
                Debug.Log($"检测到按键 {previousSlideKey}，切换到上一页");
                slidePlayer.PlayPreviousSlide();
            }
            else if (Input.GetKeyDown(nextSlideKey))
            {
                Debug.Log($"检测到按键 {nextSlideKey}，切换到下一页");
                slidePlayer.PlayNextSlide();
            }
        }
        else
        {
            Debug.LogWarning("slidePlayer 为空，无法检测按键");
        }
    }

    /// <summary>
    /// 开始播放幻灯片
    /// </summary>
    public void StartSlideShow()
    {
        if (slidePlayer != null)
        {
            slidePlayer.StartPlaying();
        }
    }

    /// <summary>
    /// 停止播放幻灯片
    /// </summary>
    public void StopSlideShow()
    {
        if (slidePlayer != null)
        {
            slidePlayer.StopPlaying();
        }
    }

    /// <summary>
    /// 跳转到指定幻灯片
    /// </summary>
    public void JumpToSlide(int index)
    {
        if (slidePlayer != null)
        {
            slidePlayer.JumpToSlide(index);
        }
    }
}

/// <summary>
/// 用于隐藏AudioSource图标的辅助组件
/// </summary>
public class AudioSourceHider : MonoBehaviour
{
    // 这个组件不需要任何功能，仅用于修改hideFlags
    // 空组件
}

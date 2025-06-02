using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityRandom = UnityEngine.Random;
using TMPro;
using System;
using System.IO;
using UnityEngine.Events;
using UnityEngine.Networking;
// using NAudio.Wave;
using System.Linq;
// using System.Collections;

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
    public static FileDataManager dataManager;

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
    
    [Header("评价音频状态")]
    [Tooltip("评价音频请求状态")]
    public bool isJudgeAudioRequesting = false;
    
    [Tooltip("评价音频是否已准备好")]
    public bool isJudgeAudioReady = false;
    
    [Tooltip("准备好的评价音频数据")]
    private string preparedJudgeAudioData = "";
    
    [Tooltip("评价音频请求协程")]
    private Coroutine judgeAudioRequestCoroutine = null;
    
        private void Awake()
    {

        dataManager = FindObjectOfType<FileDataManager>();
        if (dataManager == null)
        {
            Debug.LogError("FileDataManager 未找到！");
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

       // 播放场景介绍音频
       StartCoroutine(PlayIntroductionAudio());       

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
        onQuestionPhaseStart.AddListener(() =>
        {
            Debug.Log("Question phase started!");

            // 更新角色管理器的状态为问答环节
            RuntimeCharacterManager characterManager = RuntimeCharacterManager.Instance;
            if (characterManager != null)
            {
                Debug.Log("问答环节状态");
                characterManager.HandleQuestionPhaseStart();
                
                // 重置所有观众状态，准备参与问答
                characterManager.ResetCurrentQuestioner();
                
                // 让所有观众看向讲台，准备提问
                characterManager.PlayAllListen();
            }
            
            // 重置音频源位置
            ResetAudioSourcePosition();

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
            
            // 重置提问者状态和所有视觉效果
            RuntimeCharacterManager characterManager = RuntimeCharacterManager.Instance;
            if (characterManager != null)
            {
                characterManager.ResetCurrentQuestioner();
                
                // 让所有观众回到默认状态
                characterManager.PlayAllIdle();
            }
            
            // 重置音频源位置
            ResetAudioSourcePosition();
            
            // 清除当前说话者
            currentSpeaker = null;
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
        Debug.Log("已经进入问题环节");
        
        // 播放第一个问题（包含所有视觉交互）
        yield return StartCoroutine(PlayQuestionWithInteraction(1));
        
        // 等待一段时间
        yield return new WaitForSeconds(3f);
        
        // 播放第二个问题（包含所有视觉交互）
        yield return StartCoroutine(PlayQuestionWithInteraction(2));
        
        // 两个问题播放完成后，等待一段时间再播放评价
        yield return new WaitForSeconds(5f);
        
        // 生成并播放评价音频
        yield return StartCoroutine(GenerateAndPlayJudgeAudio());
    }

    /// <summary>
    /// 播放指定的问题音频，包含所有视觉交互效果
    /// </summary>
    private IEnumerator PlayQuestionWithInteraction(int questionNumber)
    {
        Debug.Log($"正在播放第{questionNumber}个问题（包含视觉交互）");
        
        // 获取RuntimeCharacterManager实例
        RuntimeCharacterManager characterManager = RuntimeCharacterManager.Instance;
        if (characterManager == null || characterManager.currentState != RuntimeCharacterManager.PresentationState.QuestionTime)
        {
            yield break;
        }

        // 选择随机提问者
        GameObject questioner = null;
        if (audienceManager != null)
        {
            questioner = audienceManager.GetRandomQuestioner();
            
            if (questioner != null)
            {
                Debug.Log($"观众 {questioner.name} 正在提问第{questionNumber}个问题...");
                
                // 设置当前提问者
                characterManager.SetCurrentQuestioner(questioner);
                
                // 获取演讲者对象
                GameObject presenter = characterManager.GetPresenter();
                if (presenter != null)
                {
                    // 让演讲者转向提问者
                    yield return StartCoroutine(characterManager.RotatePresenterTowardsQuestioner(presenter, questioner));
                    // 让提问者转向演讲者
                    yield return StartCoroutine(characterManager.RotateQuestionerTowardsPresenter(questioner, presenter));
                }
                
                // 设置当前说话者（移动音频源到提问者位置）
                SetCurrentSpeaker(questioner);
            }
        }

        // 播放问题音频
        yield return StartCoroutine(PlayQuestionAudio(questionNumber));
        
        // 问题播放完成后，重置提问者状态
        if (characterManager != null && questioner != null)
        {
            // 可以在这里添加问题结束后的状态重置
            // 比如取消高亮、让观众回到默认状态等
            yield return new WaitForSeconds(1f);
        }
    }

    /// <summary>
    /// 播放指定的问题音频（纯音频播放部分）
    /// </summary>
    private IEnumerator PlayQuestionAudio(int questionNumber)
    {
        Debug.Log($"正在播放第{questionNumber}个问题音频");

        // 构建完整的文件路径
        string audioResourcesPath = Path.Combine(Application.dataPath, "Resources", "GeneratedAudios");
        string audioFilePath = Path.Combine(audioResourcesPath, $"question_audio_{questionNumber}.wav");
        
        Debug.Log($"尝试加载音频文件: {audioFilePath}");
        
        // 检查文件是否存在
        if (!File.Exists(audioFilePath))
        {
            Debug.LogWarning($"音频文件不存在: {audioFilePath}");
            yield return new WaitForSeconds(3f);
            yield break;
        }

        // 使用UnityWebRequest加载音频文件
        string fileUrl = "file://" + audioFilePath;
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(fileUrl, AudioType.WAV))
        {
            yield return www.SendWebRequest();
            
            if (www.result == UnityWebRequest.Result.Success)
            {
                AudioClip questionClip = DownloadHandlerAudioClip.GetContent(www);
                
                if (questionClip != null && questionAudioSource != null)
                {
                    // 播放音频
                    questionAudioSource.clip = questionClip;
                    questionAudioSource.pitch = audioPlaybackSpeed;
                    questionAudioSource.Play();
                    
                    Debug.Log($"正在播放第{questionNumber}个问题音频，时长: {questionClip.length}秒");
                    
                    // 等待音频播放完成
                    yield return new WaitForSeconds(questionClip.length / audioPlaybackSpeed);
                }
                else
                {
                    Debug.LogWarning($"无法创建AudioClip或AudioSource为空");
                    yield return new WaitForSeconds(3f);
                }
            }
            else
            {
                Debug.LogError($"加载音频文件失败: {www.error}");
                yield return new WaitForSeconds(3f);
            }
        }
    }

    /// <summary>
    /// 生成并播放评价音频
    /// </summary>
    private IEnumerator GenerateAndPlayJudgeAudio()
    {
        Debug.Log("开始播放评价音频");
        
        // 获取RuntimeCharacterManager实例
        RuntimeCharacterManager characterManager = RuntimeCharacterManager.Instance;
        if (characterManager == null || characterManager.currentState != RuntimeCharacterManager.PresentationState.QuestionTime) 
        {
            yield break;
        }

        // 选择一个评委来进行评价（可以是之前的提问者或其他观众）
        GameObject judge = null;
        if (audienceManager != null)
        {
            judge = audienceManager.GetRandomQuestioner();
            
            if (judge != null)
            {
                Debug.Log($"观众 {judge.name} 正在进行评价...");
                
                // 设置当前评委
                characterManager.SetCurrentQuestioner(judge);
                
                // 获取演讲者对象
                GameObject presenter = characterManager.GetPresenter();
                if (presenter != null)
                {
                    // 让演讲者转向评委
                    yield return StartCoroutine(characterManager.RotatePresenterTowardsQuestioner(presenter, judge));
                    // 让评委转向演讲者
                    yield return StartCoroutine(characterManager.RotateQuestionerTowardsPresenter(judge, presenter));
                }
                
                // 设置当前说话者（移动音频源到评委位置）
                SetCurrentSpeaker(judge);
            }
        }

        bool useBackupAudio = false;
        
        // 检查评价音频是否已经准备好
        if (isJudgeAudioReady && !string.IsNullOrEmpty(preparedJudgeAudioData))
        {
            Debug.Log("使用预先准备好的评价音频");
            yield return StartCoroutine(PlayBase64Audio(preparedJudgeAudioData));
        }
        else if (isJudgeAudioRequesting)
        {
            Debug.Log("评价音频还在请求中，等待最多10秒...");
            
            // 等待最多10秒让请求完成
            float waitTime = 0f;
            float maxWaitTime = 10f;
            
            while (waitTime < maxWaitTime && isJudgeAudioRequesting && !isJudgeAudioReady)
            {
                yield return new WaitForSeconds(0.5f);
                waitTime += 0.5f;
                Debug.Log($"等待评价音频请求完成... {waitTime:F1}s / {maxWaitTime}s");
            }
            
            // 检查是否在等待期间完成了
            if (isJudgeAudioReady && !string.IsNullOrEmpty(preparedJudgeAudioData))
            {
                Debug.Log("等待期间评价音频准备完成，开始播放");
                yield return StartCoroutine(PlayBase64Audio(preparedJudgeAudioData));
            }
            else
            {
                Debug.LogWarning("等待超时或请求失败，使用备用音频");
                useBackupAudio = true;
            }
        }
        else
        {
            Debug.LogWarning("没有发起评价音频请求，直接使用备用音频");
            useBackupAudio = true;
        }

        // 如果需要使用备用音频
        if (useBackupAudio)
        {
            yield return StartCoroutine(PlayBackupJudgeAudio());
        }

        // 评价完成后，重置说话者状态
        if (characterManager != null && judge != null)
        {
            Debug.Log("评价完成，重置观众状态");
            // 重置当前提问者
            characterManager.ResetCurrentQuestioner();
            // 重置音频源位置
            ResetAudioSourcePosition();
            
            yield return new WaitForSeconds(1f);
        }

        // 清理录音文件
        string audioFolderPath = @"Assets/Recordings";
        foreach (var file in Directory.GetFiles(audioFolderPath))
        {
            File.Delete(file);
        }

        // 重置评价音频状态
        isJudgeAudioReady = false;
        preparedJudgeAudioData = "";
    }

    /// <summary>
    /// 播放备用的预制评价音频
    /// </summary>
    private IEnumerator PlayBackupJudgeAudio()
    {
        Debug.Log("开始播放备用评价音频");
        
        // 构建备用评价音频文件路径
        string audioResourcesPath = Path.Combine(Application.dataPath, "Resources", "GeneratedAudios");
        string backupAudioFilePath = Path.Combine(audioResourcesPath, "judge_temp.wav");
        
        Debug.Log($"尝试加载备用评价音频文件: {backupAudioFilePath}");
        
        // 检查文件是否存在
        if (!File.Exists(backupAudioFilePath))
        {
            Debug.LogWarning($"备用评价音频文件不存在: {backupAudioFilePath}");
            yield return new WaitForSeconds(3f);
            yield break;
        }

        // 使用UnityWebRequest加载音频文件
        string fileUrl = "file://" + backupAudioFilePath;
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(fileUrl, AudioType.WAV))
        {
            yield return www.SendWebRequest();
            
            if (www.result == UnityWebRequest.Result.Success)
            {
                AudioClip judgeClip = DownloadHandlerAudioClip.GetContent(www);
                
                if (judgeClip != null && questionAudioSource != null)
                {
                    // 播放音频
                    questionAudioSource.clip = judgeClip;
                    questionAudioSource.pitch = audioPlaybackSpeed;
                    questionAudioSource.Play();
                    
                    Debug.Log($"正在播放备用评价音频，时长: {judgeClip.length}秒");
                    
                    // 等待音频播放完成
                    yield return new WaitForSeconds(judgeClip.length / audioPlaybackSpeed);
                    
                    Debug.Log("备用评价音频播放完成");
                }
                else
                {
                    Debug.LogWarning("无法创建AudioClip或AudioSource为空");
                    yield return new WaitForSeconds(3f);
                }
            }
            else
            {
                Debug.LogError($"加载备用评价音频文件失败: {www.error}");
                yield return new WaitForSeconds(3f);
            }
        }
    }

    // 添加评价音频相关的数据类
    [System.Serializable]
    public class JudgeRequestData
    {
        public string speech_text;
        public string speaker_audio;
    }

    [System.Serializable]
    private class JudgeResponse
    {
        public string audio;
        public string text;
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
    private Coroutine questionRoutine;
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

        // 立即开始发送评价音频请求（在后台进行）
        Debug.Log("演讲结束，开始在后台准备评价音频");
        StartJudgeAudioRequest();

        // 先触发汇报结束事件，让观众鼓掌
        onPresentationEnd.Invoke();

        // 设置为鼓掌阶段
        characterManager.HandlePresentationEnd();

        // 等待鼓掌结束再进入问答阶段
        StartCoroutine(StartQuestionPhaseAfterApplause(3.0f));
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
        
        // 停止评价音频请求协程
        if (judgeAudioRequestCoroutine != null)
        {
            StopCoroutine(judgeAudioRequestCoroutine);
            judgeAudioRequestCoroutine = null;
        }
        
        // 重置评价音频状态
        isJudgeAudioRequesting = false;
        isJudgeAudioReady = false;
        preparedJudgeAudioData = "";
        
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
    [System.Serializable]  // 必须添加此特性
    public class QuestionRequestData
    {
        public string speech_text;
        public int n;
    }
    private IEnumerator AskQuestionsRepeatedly()
    {
        Debug.Log("进入了AskQuestionsRepeatedly");
        // 获取RuntimeCharacterManager实例
        RuntimeCharacterManager characterManager = RuntimeCharacterManager.Instance;
        if (characterManager == null || characterManager.currentState != RuntimeCharacterManager.PresentationState.QuestionTime)
        {
            yield break;
        }

        int questionCount = 0;
        const int maxQuestions = 3;
        const float interval = 25f; // 30秒间隔
        while (questionCount < maxQuestions)
        {
            // 确保有音频播放器和音频剪辑
            // if (questionAudioSource != null && questionAudioClips != null && questionAudioClips.Length > 0)
            // {
            // 如果没有当前说话者，尝试获取一个
            if (currentSpeaker == null && audienceManager != null)
            {
                GameObject questioner = audienceManager.GetRandomQuestioner();
                if (questioner != null)
                {
                    SetCurrentSpeaker(questioner);
                }
            }

            // 调用后端API获取问题
            string apiUrl = "http://127.0.0.1:5001/gen_question"; // 你的Flask后端地址
            if (dataManager == null || !dataManager)
            {
                dataManager = new FileDataManager();
                dataManager.SetFileData( "Assets/Resources/ppts/temp.ppt","Assets/Resources/演讲稿.txt");
                // yield break;
            }

            Debug.Log("dataManager.GetFileData().txtPath)" + dataManager.GetFileData().txtPath);
            string text;
            if (File.Exists(dataManager.GetFileData().txtPath))
            {
                text = File.ReadAllText(dataManager.GetFileData().txtPath);
                Debug.Log(text);
            }
            else
            {
                text = "";
            }
            Debug.Log("text:" + text);

            // 准备请求数据
            var requestData = new QuestionRequestData
            {
                speech_text = text, // 替换为实际演讲文本
                n = 1 // 想要生成的问题数量
            };

            TextAsset textAsset = Resources.Load<TextAsset>("演讲稿");
            var testData = new QuestionRequestData
            {
                speech_text = textAsset.text, // 替换为实际演讲文本
                n = 1 // 想要生成的问题数量
            };

            string jsonData = JsonUtility.ToJson(requestData);
            Debug.Log("正在发送请求");
            Debug.Log("即将发送的JSON: " + jsonData);
            // 创建并发送POST请求
            using (UnityWebRequest webRequest = new UnityWebRequest(apiUrl, "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
                webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("Content-Type", "application/json");
                Debug.Log("Webrequest：" + webRequest);
                // 发送请求
                yield return webRequest.SendWebRequest();
                Debug.Log("webRequest.result = " + webRequest.result);
                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log("Received response: " + webRequest.downloadHandler.text);

                    // 解析响应
                    QuestionResponse response = JsonUtility.FromJson<QuestionResponse>(webRequest.downloadHandler.text);
                    Debug.Log("QuestionResponse : " + response);
                    Debug.Log("related audio : " + response.audio);
                    // 处理问题和音频
                    if (!string.IsNullOrEmpty(response.audio))
                    {
                        StartCoroutine(PlayBase64Audio(response.audio));
                    }
                    questionCount++;
                    if (questionCount < maxQuestions)
                    {
                        Debug.Log($"Waiting {interval} seconds before next question...");
                        yield return new WaitForSeconds(interval);
                    }

                }
                else
                {
                    Debug.LogError("Error: " + webRequest.error);
                    yield break;
                }
            }
        }
        Debug.Log("Finished asking all questions");
               yield return new WaitForSeconds(20f);
              }
        [System.Serializable]
        private class QuestionResponse
        {
            public string audio;
            public string text;
        }
    IEnumerator PlayBase64Audio(string base64Data)
    {
        byte[] audioBytes = Convert.FromBase64String(base64Data);

        // 创建临时文件
        string tempPath = Path.Combine(Application.temporaryCachePath, "tempAudio.wav");
        File.WriteAllBytes(tempPath, audioBytes);
        Debug.Log("已经写入临时文件 : "+ tempPath);
        // 加载音频
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + tempPath, AudioType.WAV))
        {
            yield return www.SendWebRequest();
            Debug.Log("www.result " + www.result);
            if (www.result == UnityWebRequest.Result.Success)
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position);
            }
            else
            {
                Debug.LogError("Audio load error: " + www.error);
            }
        }

        // 删除临时文件
        File.Delete(tempPath);
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
    /// 创建静音WAV文件
    /// </summary>
    void CreateSilentWavFile(string filePath, int durationInSeconds)
    {
        // WAV格式参数
        const int sampleRate = 44100;
        const short bitsPerSample = 16;
        const short channels = 1;
        const int byteRate = sampleRate * channels * bitsPerSample / 8;
        int dataSize = sampleRate * durationInSeconds * channels * bitsPerSample / 8;

        using (var fs = new FileStream(filePath, FileMode.Create))
        using (var writer = new BinaryWriter(fs))
        {
            // RIFF头
            writer.Write(new[] { 'R', 'I', 'F', 'F' });
            writer.Write(36 + dataSize); // 文件总大小
            writer.Write(new[] { 'W', 'A', 'V', 'E' });

            // fmt子块
            writer.Write(new[] { 'f', 'm', 't', ' ' });
            writer.Write(16); // PCM块大小
            writer.Write((short)1); // 格式类型（PCM）
            writer.Write(channels);
            writer.Write(sampleRate);
            writer.Write(byteRate);
            writer.Write((short)(channels * bitsPerSample / 8)); // 块对齐
            writer.Write(bitsPerSample);

            // data子块
            writer.Write(new[] { 'd', 'a', 't', 'a' });
            writer.Write(dataSize);

            // 写入静音数据（全0）
            writer.Write(new byte[dataSize]);
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
        [System.Serializable]  // 必须添加此特性[System.Serializable] // 确保可被 Unity 的 JsonUtility 序列化
    public class IntroductionRequestData
    {
        public string speaker_name; // 演讲文本
        public string speech_title; // Base64 编码的 WAV 音频数据
    }
    /// <summary>
    /// 播放场景介绍音频
    /// </summary>
    public IEnumerator PlayIntroductionAudio()
    {
        Debug.Log("开始播放欢迎音频");
        
        // 构建欢迎音频文件路径
        string audioResourcesPath = Path.Combine(Application.dataPath, "Resources", "GeneratedAudios");
        string introAudioFilePath = Path.Combine(audioResourcesPath, "introduction_audio.wav");
        
        Debug.Log($"尝试加载欢迎音频文件: {introAudioFilePath}");
        
        // 检查文件是否存在
        if (!File.Exists(introAudioFilePath))
        {
            Debug.LogWarning($"欢迎音频文件不存在: {introAudioFilePath}");
            yield break;
        }

        // 使用UnityWebRequest加载音频文件
        string fileUrl = "file://" + introAudioFilePath;
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(fileUrl, AudioType.WAV))
        {
            yield return www.SendWebRequest();
            
            if (www.result == UnityWebRequest.Result.Success)
            {
                AudioClip introClip = DownloadHandlerAudioClip.GetContent(www);
                
                if (introClip != null && introductionAudioSource != null)
                {
                    // 设置并播放音频
                    introductionAudioSource.clip = introClip;
                    introductionAudioSource.Play();
                    
                    // 设置播放状态
                    isPlayingIntroduction = true;
                    presenterCanMove = false;
                    
                    Debug.Log($"正在播放欢迎音频，时长: {introClip.length}秒");
                    
                    // 等待音频播放完成
                    yield return new WaitForSeconds(introClip.length);
                    
                    // 播放完成后更新状态
                    isPlayingIntroduction = false;
                    presenterCanMove = true;
                    
                    Debug.Log("欢迎音频播放完成");
                }
                else if (introClip != null)
                {
                    // 如果没有专用的introductionAudioSource，使用PlayBase64Audio的方式播放
                    Debug.Log("使用AudioSource.PlayClipAtPoint播放欢迎音频");
                    AudioSource.PlayClipAtPoint(introClip, Camera.main.transform.position);
                    
                    // 等待音频播放完成
                    yield return new WaitForSeconds(introClip.length);
                    Debug.Log("欢迎音频播放完成");
                }
                else
                {
                    Debug.LogWarning("无法创建AudioClip");
                }
            }
            else
            {
                Debug.LogError($"加载欢迎音频文件失败: {www.error}");
            }
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

    /// <summary>
    /// 开始发送评价音频请求（异步后台处理）
    /// </summary>
    public void StartJudgeAudioRequest()
    {
        // 如果已经在请求中，不重复发送
        if (isJudgeAudioRequesting)
        {
            Debug.Log("评价音频请求已在进行中，跳过重复请求");
            return;
        }

        // 开始异步请求
        judgeAudioRequestCoroutine = StartCoroutine(RequestJudgeAudioInBackground());
    }

    /// <summary>
    /// 在后台异步请求评价音频
    /// </summary>
    private IEnumerator RequestJudgeAudioInBackground()
    {
        isJudgeAudioRequesting = true;
        isJudgeAudioReady = false;
        preparedJudgeAudioData = "";

        Debug.Log("后台开始请求评价音频");

        // 调用后端API获取评价音频
        string apiUrl = "http://127.0.0.1:5001/judge";
        
        if (dataManager == null || !dataManager)
        {
            dataManager = new FileDataManager();
            dataManager.SetFileData("Assets/Resources/ppts/temp.ppt", "Assets/Resources/script.txt");
        }

        string text = "";
        string txtPath = dataManager.GetFileData().txtPath;
        if (File.Exists(txtPath))
        {
            text = File.ReadAllText(txtPath);
        }

        // 获取录音文件
        string audioFolderPath = @"Assets/Recordings";
        string tempOutputPath = Path.Combine(Path.GetTempPath(), "output.wav");

        var audioFiles = Directory.GetFiles(audioFolderPath, "*.wav")
                         .OrderBy(f => f)
                         .ToArray();

        if (audioFiles.Length == 0)
        {
            CreateSilentWavFile(tempOutputPath, 5);
            Debug.Log("没有找到WAV文件，已创建空白音频");
        }
        else
        {
            string lastAudioFile = audioFiles.Last();
            File.Copy(lastAudioFile, tempOutputPath, overwrite: true);
        }

        byte[] combinedAudioBytes = File.ReadAllBytes(tempOutputPath);
        string base64Audio = Convert.ToBase64String(combinedAudioBytes);

        var requestData = new JudgeRequestData
        {
            speech_text = text,
            speaker_audio = base64Audio
        };

        string jsonData = JsonUtility.ToJson(requestData);
        Debug.Log("正在后台发送评价音频请求");

        using (UnityWebRequest webRequest = new UnityWebRequest(apiUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("后台评价音频请求成功: " + webRequest.downloadHandler.text);

                JudgeResponse response = JsonUtility.FromJson<JudgeResponse>(webRequest.downloadHandler.text);
                
                if (!string.IsNullOrEmpty(response.audio))
                {
                    preparedJudgeAudioData = response.audio;
                    isJudgeAudioReady = true;
                    Debug.Log("评价音频已准备就绪，等待播放");
                }
                else
                {
                    Debug.LogWarning("后台请求返回的评价音频为空");
                }
            }
            else
            {
                Debug.LogError("后台评价音频请求失败: " + webRequest.error);
            }
        }

        // 清理临时文件
        if (File.Exists(tempOutputPath))
        {
            File.Delete(tempOutputPath);
        }

        isJudgeAudioRequesting = false;
        Debug.Log("后台评价音频请求完成");
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

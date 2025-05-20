using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 学术汇报UI控制器 - 管理汇报界面元素和交互，自动创建必要的UI组件
/// </summary>
public class AcademicReportUI : MonoBehaviour
{
    [Header("计时器显示")]
    [Tooltip("计时器文本")]
    public TextMeshProUGUI timerText;
    
    [Tooltip("计时器警告颜色")]
    public Color warningColor = Color.yellow;
    
    [Tooltip("计时器危险颜色")]
    public Color dangerColor = Color.red;
    
    [Tooltip("计时器警告阈值(秒)")]
    public float warningThreshold = 120f; // 2分钟
    
    [Tooltip("计时器危险阈值(秒)")]
    public float dangerThreshold = 30f; // 30秒
    
    [Header("键盘控制")]
    [Tooltip("结束演讲的按键")]
    public KeyCode endPresentationKey = KeyCode.Z;
    
    [Header("语速显示")]
    [Tooltip("语速显示文本")]
    public TextMeshProUGUI speechRateText;
    
    [Header("控制提示")]
    [Tooltip("按键提示文本")]
    public TextMeshProUGUI keyPromptText;
    
    [Header("音效")]
    [Tooltip("鼓掌音效")]
    public AudioClip applauseSound;
    
    [Tooltip("按钮点击音效")]
    public AudioClip buttonClickSound;
    
    [Header("UI生成设置")]
    [Tooltip("是否在启动时自动生成UI")]
    public bool generateUIOnStart = true;
    
    [Tooltip("UI画布的参考大小")]
    public Vector2 canvasSize = new Vector2(1920, 1080);
    
    [Tooltip("UI字体大小")]
    public int fontSize = 36;
    
    [Tooltip("按钮颜色")]
    public Color buttonColor = new Color(0.2f, 0.6f, 1f);
    
    [Tooltip("文本颜色")]
    public Color textColor = Color.white;
    
    // 汇报管理器引用
    private AcademicPresentationManager presentationManager;
    
    // 音频源
    private AudioSource audioSource;
    
    // 正常计时器颜色
    private Color normalTimerColor;
    
    // 当前语速
    private float currentSpeechRate = 1.0f;
    
    // 是否在演讲阶段
    private bool isPresenting = false;
    
    // 是否在问答阶段
    private bool isQuestionPhase = false;
    
    // 是否已到达讲台
    private bool hasReachedPodium = false;
    
    // Canvas引用
    private Canvas canvas;
    
    private void Awake()
    {
        // 如果设置了自动生成UI，则执行生成
        if (generateUIOnStart)
        {
            GenerateUI();
        }
    }
    
    private void Start()
    {
        // 获取汇报管理器引用
        presentationManager = FindObjectOfType<AcademicPresentationManager>();
        
        if (presentationManager == null)
        {
            Debug.LogError("AcademicPresentationManager not found!");
            return;
        }
        
        // 添加音频源
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        
        // 保存计时器的正常颜色
        if (timerText != null)
        {
            normalTimerColor = timerText.color;
        }
        
        // 初始化UI状态
        InitializeUI();
        
        // 订阅事件
        SubscribeToEvents();
        
        // 随机生成语速用于展示
        StartCoroutine(RandomSpeechRateDisplay());
        
        // 初始状态设置为未到达讲台
        SetReachedPodiumStatus(false);
    }
    
    /// <summary>
    /// 设置是否已到达讲台的状态
    /// </summary>
    public void SetReachedPodiumStatus(bool reached)
    {
        hasReachedPodium = reached;
        
        // 更新UI状态
        UpdateUIBasedOnPodiumStatus();
    }
    
    /// <summary>
    /// 根据是否到达讲台更新UI状态
    /// </summary>
    private void UpdateUIBasedOnPodiumStatus()
    {
        Debug.LogError($"Updating UI state: reachedPodium={hasReachedPodium}, isPresenting={isPresenting}, isQuestionPhase={isQuestionPhase}");
        
        // 更新按键提示文本
        UpdateKeyPromptText();
        
        if (hasReachedPodium)
        {
            // 已到达讲台：显示计时器和语速，隐藏提示
            if (timerText != null && timerText.transform.parent != null)
            {
                timerText.transform.parent.gameObject.SetActive(true);
            }
            
            // 如果正在演讲，显示语速
            if (isPresenting && !isQuestionPhase && speechRateText != null && speechRateText.transform.parent != null)
            {
                speechRateText.transform.parent.gameObject.SetActive(true);
            }
            
            // 自动开始演讲
            if (!isPresenting && !isQuestionPhase && presentationManager != null)
            {
                Debug.Log("Auto starting presentation...");
                isPresenting = true; // 先设置状态，防止presentationManager.StartPresentation()中的事件回调产生冲突
                presentationManager.StartPresentation();
            }
        }
        else
        {
            // 未到达讲台：显示提示，隐藏计时器和语速
            if (timerText != null && timerText.transform.parent != null)
            {
                timerText.transform.parent.gameObject.SetActive(false);
            }
            
            // 显示提示信息
            if (speechRateText != null)
            {
                speechRateText.text = "Please reach the podium";
                if (speechRateText.transform.parent != null)
                {
                    speechRateText.transform.parent.gameObject.SetActive(true);
                }
            }
        }
    }
    
    /// <summary>
    /// 自动生成UI组件
    /// </summary>
    private void GenerateUI()
    {
        // 检查是否已存在Canvas
        canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            // 创建Canvas
            GameObject canvasObj = new GameObject("UI Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            // 添加CanvasScaler
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = canvasSize;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            
            // 添加GraphicRaycaster
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        // 检查是否已存在EventSystem
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            // 创建EventSystem
            GameObject eventSystemObj = new GameObject("Event System");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
        
        // 如果没有计时器文本，创建计时器
        if (timerText == null)
        {
            CreateTimerDisplay();
        }
        
        // 如果没有语速显示，创建语速显示
        if (speechRateText == null)
        {
            CreateSpeechRateDisplay();
        }
        
        // 如果没有按键提示文本，创建按键提示
        if (keyPromptText == null)
        {
            CreateKeyPromptText();
        }
        
        Debug.Log("UI components generated");
    }
    
    /// <summary>
    /// 创建计时器显示
    /// </summary>
    private void CreateTimerDisplay()
    {
        // 创建计时器面板
        GameObject timerPanelObj = new GameObject("Timer Panel", typeof(RectTransform));
        timerPanelObj.transform.SetParent(canvas.transform, false);
        
        // 设置计时器面板位置和大小
        RectTransform timerPanelRect = timerPanelObj.GetComponent<RectTransform>();
        timerPanelRect.anchorMin = new Vector2(0.5f, 0.9f);
        timerPanelRect.anchorMax = new Vector2(0.5f, 0.98f);
        timerPanelRect.anchoredPosition = Vector2.zero;
        timerPanelRect.sizeDelta = new Vector2(300, 80);
        
        // 添加背景
        Image timerBgImage = timerPanelObj.AddComponent<Image>();
        timerBgImage.color = new Color(0, 0, 0, 0.7f);
        
        // 创建计时器文本
        GameObject timerTextObj = new GameObject("Timer Text", typeof(RectTransform));
        timerTextObj.transform.SetParent(timerPanelObj.transform, false);
        
        // 设置计时器文本位置和大小
        RectTransform timerTextRect = timerTextObj.GetComponent<RectTransform>();
        timerTextRect.anchorMin = Vector2.zero;
        timerTextRect.anchorMax = Vector2.one;
        timerTextRect.offsetMin = new Vector2(10, 10);
        timerTextRect.offsetMax = new Vector2(-10, -10);
        
        // 添加TMP文本组件
        timerText = timerTextObj.AddComponent<TextMeshProUGUI>();
        timerText.text = "00:00";
        timerText.fontSize = fontSize;
        timerText.color = textColor;
        timerText.alignment = TextAlignmentOptions.Center;
    }
    
    /// <summary>
    /// 创建语速显示
    /// </summary>
    private void CreateSpeechRateDisplay()
    {
        // 创建语速面板
        GameObject speechRatePanelObj = new GameObject("Speech Rate Panel", typeof(RectTransform));
        speechRatePanelObj.transform.SetParent(canvas.transform, false);
        
        // 设置语速面板位置和大小
        RectTransform speechRatePanelRect = speechRatePanelObj.GetComponent<RectTransform>();
        speechRatePanelRect.anchorMin = new Vector2(0.02f, 0.9f);
        speechRatePanelRect.anchorMax = new Vector2(0.3f, 0.98f);
        speechRatePanelRect.anchoredPosition = Vector2.zero;
        speechRatePanelRect.sizeDelta = Vector2.zero;
        
        // 添加背景
        Image speechRateBgImage = speechRatePanelObj.AddComponent<Image>();
        speechRateBgImage.color = new Color(0, 0, 0, 0.7f);
        
        // 创建语速文本
        GameObject speechRateTextObj = new GameObject("Speech Rate Text", typeof(RectTransform));
        speechRateTextObj.transform.SetParent(speechRatePanelObj.transform, false);
        
        // 设置语速文本位置和大小
        RectTransform speechRateTextRect = speechRateTextObj.GetComponent<RectTransform>();
        speechRateTextRect.anchorMin = Vector2.zero;
        speechRateTextRect.anchorMax = Vector2.one;
        speechRateTextRect.offsetMin = new Vector2(10, 5);
        speechRateTextRect.offsetMax = new Vector2(-10, -5);
        
        // 添加TMP文本组件
        speechRateText = speechRateTextObj.AddComponent<TextMeshProUGUI>();
        speechRateText.text = "Please reach the podium";
        speechRateText.fontSize = fontSize - 4;
        speechRateText.color = textColor;
        speechRateText.alignment = TextAlignmentOptions.Center;
    }
    
    /// <summary>
    /// 创建按键提示文本
    /// </summary>
    private void CreateKeyPromptText()
    {
        // 创建按键提示面板
        GameObject promptPanelObj = new GameObject("Key Prompt Panel", typeof(RectTransform));
        promptPanelObj.transform.SetParent(canvas.transform, false);
        
        // 设置提示面板位置和大小（右上角）
        RectTransform promptPanelRect = promptPanelObj.GetComponent<RectTransform>();
        promptPanelRect.anchorMin = new Vector2(0.7f, 0.85f);
        promptPanelRect.anchorMax = new Vector2(0.98f, 0.95f);
        promptPanelRect.anchoredPosition = Vector2.zero;
        promptPanelRect.sizeDelta = Vector2.zero;
        
        // 添加背景
        Image promptBgImage = promptPanelObj.AddComponent<Image>();
        promptBgImage.color = new Color(0, 0, 0, 0.7f);
        
        // 创建提示文本
        GameObject promptTextObj = new GameObject("Prompt Text", typeof(RectTransform));
        promptTextObj.transform.SetParent(promptPanelObj.transform, false);
        
        // 设置文本位置和大小
        RectTransform promptTextRect = promptTextObj.GetComponent<RectTransform>();
        promptTextRect.anchorMin = Vector2.zero;
        promptTextRect.anchorMax = Vector2.one;
        promptTextRect.offsetMin = new Vector2(10, 5);
        promptTextRect.offsetMax = new Vector2(-10, -5);
        
        // 添加TMP文本组件
        keyPromptText = promptTextObj.AddComponent<TextMeshProUGUI>();
        keyPromptText.text = "Press Z to end presentation";
        keyPromptText.fontSize = fontSize - 4;
        keyPromptText.color = textColor;
        keyPromptText.alignment = TextAlignmentOptions.Center;
        
        // 初始隐藏提示
        promptPanelObj.SetActive(false);
    }
    
    /// <summary>
    /// 初始化UI
    /// </summary>
    private void InitializeUI()
    {
        // 初始化语速显示
        UpdateSpeechRateText(1.0f);
        
        // 更新按键提示
        UpdateKeyPromptText();
        
        // 根据讲台状态更新UI
        UpdateUIBasedOnPodiumStatus();
    }
    
    /// <summary>
    /// 更新按键提示文本
    /// </summary>
    private void UpdateKeyPromptText()
    {
        if (keyPromptText != null && keyPromptText.transform.parent != null)
        {
            // 只在到达讲台且正在演讲时显示提示
            bool shouldShowPrompt = hasReachedPodium && isPresenting && !isQuestionPhase;
            keyPromptText.transform.parent.gameObject.SetActive(shouldShowPrompt);
            
            if (shouldShowPrompt)
            {
                keyPromptText.text = $"Press {endPresentationKey} to end presentation";
            }
        }
    }
    
    /// <summary>
    /// 订阅事件
    /// </summary>
    private void SubscribeToEvents()
    {
        if (presentationManager != null)
        {
            presentationManager.onPresentationStart.AddListener(() => {
                isPresenting = true;
                isQuestionPhase = false;
                UpdateKeyPromptText();
            });
            
            presentationManager.onPresentationEnd.AddListener(() => {
                isPresenting = false;
                UpdateKeyPromptText();
            });
            
            presentationManager.onQuestionPhaseStart.AddListener(() => {
                isQuestionPhase = true;
                UpdateKeyPromptText();
            });
            
            presentationManager.onQuestionPhaseEnd.AddListener(() => {
                isQuestionPhase = false;
                UpdateKeyPromptText();
            });
            
            presentationManager.onTimerUpdate.AddListener(UpdateTimerDisplay);
        }
    }
    
    /// <summary>
    /// 播放按钮音效
    /// </summary>
    private void PlayButtonSound()
    {
        if (audioSource != null && buttonClickSound != null)
        {
            audioSource.clip = buttonClickSound;
            audioSource.Play();
        }
    }
    
    /// <summary>
    /// 播放鼓掌音效
    /// </summary>
    private void PlayApplauseSound()
    {
        if (audioSource != null && applauseSound != null)
        {
            audioSource.clip = applauseSound;
            audioSource.Play();
        }
    }
    
    /// <summary>
    /// 演讲结束序列
    /// </summary>
    private IEnumerator EndPresentationSequence()
    {
        Debug.Log("Starting end presentation sequence");
        
        // 播放按钮音效
        PlayButtonSound();
        
        // 触发演讲结束事件
        if (presentationManager != null)
        {
            // 播放鼓掌音效
            // PlayApplauseSound();
            // Debug.Log("Playing applause sound");
            //
            // 触发演讲结束事件
            // presentationManager.onPresentationEnd.Invoke();
            Debug.Log("Presentation end event triggered");
            
            // // 让观众鼓掌
            // presentationManager.TriggerApplause();
            // Debug.Log("Audience applause triggered");
            //
            // // 等待鼓掌结束
            // Debug.Log("Waiting 3 seconds for applause to end...");
            // yield return new WaitForSeconds(3.0f);
            //
            // 开始问答环节
            presentationManager.StartQuestionPhase();
            Debug.Log("Question phase started");
            yield return null;

            // // 短暂延迟后自动触发第一个问题
            // Debug.Log("Waiting 1.5 seconds before first question...");
            // yield return new WaitForSeconds(1.5f);
            // presentationManager.AskRandomQuestion();
            // Debug.Log("First question triggered");
        }
        else
        {
            Debug.LogError("presentationManager is null, cannot execute end presentation sequence");
        }
        
        Debug.Log("End presentation sequence completed");
    }
    
    /// <summary>
    /// 更新计时器显示
    /// </summary>
    public void UpdateTimerDisplay(float timeRemaining)
    {
        if (timerText == null || !hasReachedPodium) return;
        
        // 计算分钟和秒数
        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);
        
        // 更新计时器文本
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        
        // 根据剩余时间更改颜色
        if (timeRemaining <= dangerThreshold)
        {
            timerText.color = dangerColor;
        }
        else if (timeRemaining <= warningThreshold)
        {
            timerText.color = warningColor;
        }
        else
        {
            timerText.color = normalTimerColor;
        }
    }
    
    /// <summary>
    /// 更新语速显示文本
    /// </summary>
    private void UpdateSpeechRateText(float rate)
    {
        if (speechRateText != null && hasReachedPodium)
        {
            // 将数值转换为文字描述
            string rateDescription;
            if (rate > 1.1f)
                rateDescription = "Speed: High";
            else if (rate < 0.9f)
                rateDescription = "Speed: Low";
            else
                rateDescription = "Speed: Medium";
            
            speechRateText.text = rateDescription;
        }
    }
    
    /// <summary>
    /// 随机展示语速 (用于演示)
    /// </summary>
    private IEnumerator RandomSpeechRateDisplay()
    {
        while (true)
        {
            // 只在已到达讲台、演讲中且不在问答阶段时显示随机语速
            if (hasReachedPodium && isPresenting && !isQuestionPhase)
            {
                // 生成随机语速 (0.8-1.2范围)
                float randomRate = Random.Range(0.8f, 1.2f);
                currentSpeechRate = randomRate;
                UpdateSpeechRateText(randomRate);
                
                // 确保语速显示可见
                if (speechRateText != null && speechRateText.transform.parent != null)
                {
                    speechRateText.transform.parent.gameObject.SetActive(true);
                }
            }
            else if (!hasReachedPodium)
            {
                // 未到达讲台时显示提示信息
                if (speechRateText != null)
                {
                    speechRateText.text = "Please reach the podium";
                    if (speechRateText.transform.parent != null)
                    {
                        speechRateText.transform.parent.gameObject.SetActive(true);
                    }
                }
            }
            else
            {
                // 其他情况隐藏语速显示
                if (speechRateText != null && speechRateText.transform.parent != null && hasReachedPodium)
                {
                    speechRateText.transform.parent.gameObject.SetActive(false);
                }
            }
            
            // 每2-4秒更新一次
            yield return new WaitForSeconds(Random.Range(2f, 4f));
        }
    }
    
    /// <summary>
    /// 检测键盘输入
    /// </summary>
    private void Update()
    {
        // 检测按键以结束演讲
        if (Input.GetKeyDown(endPresentationKey))
        {
            if (hasReachedPodium && isPresenting && !isQuestionPhase)
            {
                Debug.LogError($"Key {endPresentationKey} pressed - ending presentation");
                StartCoroutine(EndPresentationSequence());
                }
                else
                {
                Debug.Log($"Key {endPresentationKey} pressed, but conditions not met to end presentation");
            }
        }
    }
    
    /// <summary>
    /// 玩家到达讲台的回调方法
    /// </summary>
    public void OnReachedPodium()
    {
        SetReachedPodiumStatus(true);
    }
} 
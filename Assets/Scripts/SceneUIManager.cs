using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SceneUIManager : MonoBehaviour
{
    [Header("面板设置")]
    public GameObject mainPanel;
    public GameObject settingsPanel;
    public GameObject helpPanel;
    public GameObject statsPanel;
    
    [Header("状态显示")]
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI scoreText;
    
    [Header("进度条")]
    public Slider progressBar;
    public Image progressFill;
    public Gradient progressColorGradient;
    
    [Header("按钮")]
    public Button settingsButton;
    public Button helpButton;
    public Button statsButton;
    public Button closeButton;
    
    private GameObject currentActivePanel;
    
    void Start()
    {
        // 初始化设置
        InitializeUI();
        
        // 设置按钮事件监听
        SetupButtonListeners();
        
        // 默认隐藏所有面板，只显示主面板
        ShowOnlyMainPanel();
    }
    
    void Update()
    {
        // 更新时间显示
        UpdateTimeDisplay();
        
        // ESC键返回主面板
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ShowOnlyMainPanel();
        }
    }
    
    private void InitializeUI()
    {
        // 确保引用有效
        if (statusText == null || timeText == null || scoreText == null)
        {
            Debug.LogWarning("UI管理器中的文本引用未设置");
        }
        
        if (progressBar != null && progressFill != null)
        {
            progressBar.value = 0;
            UpdateProgressBarColor(0);
        }
        
        // 初始化状态文本
        if (statusText != null)
            statusText.text = "欢迎来到学术报告模拟器";
        
        if (scoreText != null)
            scoreText.text = "成绩: 0";
    }
    
    private void SetupButtonListeners()
    {
        // 设置按钮
        if (settingsButton != null)
            settingsButton.onClick.AddListener(() => ShowPanel(settingsPanel));
        
        // 帮助按钮
        if (helpButton != null)
            helpButton.onClick.AddListener(() => ShowPanel(helpPanel));
        
        // 统计按钮
        if (statsButton != null)
            statsButton.onClick.AddListener(() => ShowPanel(statsPanel));
        
        // 关闭按钮
        if (closeButton != null)
            closeButton.onClick.AddListener(ShowOnlyMainPanel);
    }
    
    // 只显示主面板
    public void ShowOnlyMainPanel()
    {
        HideAllPanels();
        if (mainPanel != null)
            mainPanel.SetActive(true);
        
        currentActivePanel = mainPanel;
    }
    
    // 隐藏所有面板
    private void HideAllPanels()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (helpPanel != null) helpPanel.SetActive(false);
        if (statsPanel != null) statsPanel.SetActive(false);
    }
    
    // 显示特定面板
    public void ShowPanel(GameObject panel)
    {
        if (panel == null) return;
        
        HideAllPanels();
        panel.SetActive(true);
        currentActivePanel = panel;
    }
    
    // 更新状态文本
    public void UpdateStatusText(string message)
    {
        if (statusText != null)
            statusText.text = message;
    }
    
    // 更新分数
    public void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = "成绩: " + score.ToString();
    }
    
    // 更新进度条
    public void UpdateProgress(float progress)
    {
        if (progressBar == null) return;
        
        // 确保进度值在0-1之间
        progress = Mathf.Clamp01(progress);
        progressBar.value = progress;
        
        // 更新颜色
        UpdateProgressBarColor(progress);
    }
    
    // 更新进度条颜色
    private void UpdateProgressBarColor(float progress)
    {
        if (progressFill != null && progressColorGradient != null)
        {
            progressFill.color = progressColorGradient.Evaluate(progress);
        }
    }
    
    // 更新时间显示
    private void UpdateTimeDisplay()
    {
        if (timeText != null)
        {
            float time = Time.time;
            int minutes = Mathf.FloorToInt(time / 60);
            int seconds = Mathf.FloorToInt(time % 60);
            timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }
    
    // 显示临时消息
    public void ShowTemporaryMessage(string message, float duration = 3f)
    {
        StartCoroutine(DisplayTemporaryMessage(message, duration));
    }
    
    // 临时信息协程
    private IEnumerator DisplayTemporaryMessage(string message, float duration)
    {
        string originalText = "";
        if (statusText != null)
        {
            originalText = statusText.text;
            statusText.text = message;
        }
        
        yield return new WaitForSeconds(duration);
        
        if (statusText != null)
            statusText.text = originalText;
    }
} 
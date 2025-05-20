using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class DemoScene : MonoBehaviour
{
    [Header("场景设置")]
    public string[] availableScenes; // 可用场景名称
    public int currentSceneIndex = 0;
    
    [Header("UI设置")]
    public Button nextSceneButton;
    public Button prevSceneButton;
    public TextMeshProUGUI sceneNameText;
    public TextMeshProUGUI sceneDescriptionText;
    public GameObject loadingPanel;
    
    [Header("角色设置")]
    public GameObject[] availableCharacters; // 可选角色
    public bool spawnCharactersAutomatically = true;
    public Transform[] spawnPoints; // 角色生成点
    
    // 场景描述
    private Dictionary<string, string> sceneDescriptions = new Dictionary<string, string>();
    
    private void Awake()
    {
        // 初始化场景描述
        InitializeSceneDescriptions();
    }
    
    void Start()
    {
        // 设置UI按钮
        if (nextSceneButton != null)
            nextSceneButton.onClick.AddListener(LoadNextScene);
        
        if (prevSceneButton != null)
            prevSceneButton.onClick.AddListener(LoadPreviousScene);
        
        // 如果需要，自动生成角色
        if (spawnCharactersAutomatically)
            SpawnCharacters();
        
        // 更新UI
        UpdateSceneUI();
    }
    
    // 初始化场景描述信息
    private void InitializeSceneDescriptions()
    {
        sceneDescriptions.Add("Classroom", "教室场景：测试学生角色和课堂环境中的互动功能");
        sceneDescriptions.Add("Library", "图书馆场景：测试安静环境中的角色行为和书籍互动");
        sceneDescriptions.Add("Campus", "校园场景：测试户外环境和多角色互动");
        sceneDescriptions.Add("Lab", "实验室场景：测试与实验设备的交互和特殊动画");
        sceneDescriptions.Add("Dorm", "宿舍场景：测试生活环境中的角色行为");
    }
    
    // 更新场景相关UI
    private void UpdateSceneUI()
    {
        if (availableScenes.Length == 0) return;
        
        string currentSceneName = availableScenes[currentSceneIndex];
        
        if (sceneNameText != null)
            sceneNameText.text = "当前场景: " + currentSceneName;
        
        if (sceneDescriptionText != null && sceneDescriptions.ContainsKey(currentSceneName))
            sceneDescriptionText.text = sceneDescriptions[currentSceneName];
        else if (sceneDescriptionText != null)
            sceneDescriptionText.text = "没有可用描述";
        
        // 更新按钮状态
        if (prevSceneButton != null)
            prevSceneButton.interactable = currentSceneIndex > 0;
        
        if (nextSceneButton != null)
            nextSceneButton.interactable = currentSceneIndex < availableScenes.Length - 1;
    }
    
    // 加载下一个场景
    public void LoadNextScene()
    {
        if (currentSceneIndex < availableScenes.Length - 1)
        {
            currentSceneIndex++;
            LoadScene(availableScenes[currentSceneIndex]);
        }
    }
    
    // 加载上一个场景
    public void LoadPreviousScene()
    {
        if (currentSceneIndex > 0)
        {
            currentSceneIndex--;
            LoadScene(availableScenes[currentSceneIndex]);
        }
    }
    
    // 加载指定场景
    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneAsync(sceneName));
    }
    
    // 异步加载场景
    private IEnumerator LoadSceneAsync(string sceneName)
    {
        // 显示加载界面
        if (loadingPanel != null)
            loadingPanel.SetActive(true);
        
        // 异步加载场景
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;
        
        // 等待加载完成
        while (asyncLoad.progress < 0.9f)
        {
            // 这里可以更新加载进度条
            yield return null;
        }
        
        // 激活场景
        asyncLoad.allowSceneActivation = true;
        
        // 等待场景完全加载
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        
        // 隐藏加载界面
        if (loadingPanel != null)
            loadingPanel.SetActive(false);
        
        // 场景加载完成后，生成角色
        SpawnCharacters();
        
        // 更新UI
        UpdateSceneUI();
    }
    
    // 在场景中生成角色
    private void SpawnCharacters()
    {
        if (availableCharacters == null || availableCharacters.Length == 0)
            return;
        
        // 确定要使用的生成点
        Transform[] points = spawnPoints.Length > 0 ? spawnPoints : new Transform[] { transform };
        
        // 生成角色
        for (int i = 0; i < availableCharacters.Length && i < points.Length; i++)
        {
            GameObject character = Instantiate(availableCharacters[i], points[i].position, points[i].rotation);
            character.name = "Character_" + i;
            
            // 如果场景中有Demo脚本，通知它角色已经生成
            Demo demoScript = FindObjectOfType<Demo>();
            if (demoScript != null)
            {
                // 这里可以通过某种方式通知Demo脚本角色已生成
                // 例如，如果Demo脚本有公共方法可以调用
            }
        }
    }
} 
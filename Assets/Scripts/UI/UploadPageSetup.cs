using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Events;
using System.IO;
using UnityEngine.SceneManagement;

public class UploadPageSetup : MonoBehaviour
{
    [MenuItem("Tools/Create Upload Page")]
    public static void CreateUploadPage()
    {
        // 创建新场景
        Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        
        // 创建主相机
        GameObject mainCamera = new GameObject("Main Camera");
        Camera camera = mainCamera.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.Skybox;
        camera.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 1f);
        camera.orthographic = false;
        camera.fieldOfView = 60f;
        camera.nearClipPlane = 0.3f;
        camera.farClipPlane = 1000f;
        mainCamera.tag = "MainCamera";
        
        // 创建天空盒材质
        Material skyboxMaterial = new Material(Shader.Find("Skybox/6 Sided"));
        RenderSettings.skybox = skyboxMaterial;
        
        // 创建EventSystem
        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        
        // 创建Canvas
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = camera;
        canvas.sortingOrder = 0; // 确保Canvas在最前面
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        
        canvasObj.AddComponent<GraphicRaycaster>();

        // 创建背景图片
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(canvasObj.transform, false);
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = new Color(1f, 1f, 1f, 1f);
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        bgRect.SetAsFirstSibling();

        // 创建背景面板
        GameObject panelObj = new GameObject("Panel");
        panelObj.transform.SetParent(canvasObj.transform, false);
        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;
        panelRect.SetAsLastSibling(); // 确保面板在最上层

        // 创建标题
        GameObject titleObj = CreateText("学术报告模拟器", new Vector2(0.5f, 0.8f), panelObj.transform);
        titleObj.GetComponent<Text>().fontSize = 48;
        titleObj.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 60);

        // 创建说明文本
        GameObject descObj = CreateText("请选择PPT文件和对应的文本文件", new Vector2(0.5f, 0.7f), panelObj.transform);
        descObj.GetComponent<Text>().fontSize = 24;
        descObj.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 40);

        // 创建文件选择区域
        GameObject fileSelectObj = new GameObject("FileSelect");
        fileSelectObj.transform.SetParent(panelObj.transform, false);
        RectTransform fileSelectRect = fileSelectObj.AddComponent<RectTransform>();
        fileSelectRect.anchorMin = new Vector2(0.5f, 0.4f);
        fileSelectRect.anchorMax = new Vector2(0.5f, 0.6f);
        fileSelectRect.sizeDelta = new Vector2(400, 200);
        fileSelectRect.anchoredPosition = Vector2.zero;

        // 创建PPT选择按钮
        GameObject pptButtonObj = CreateButton("选择PPT文件", new Vector2(0.5f, 0.7f), fileSelectObj.transform);
        Button pptButton = pptButtonObj.GetComponent<Button>();
        pptButton.transition = Selectable.Transition.ColorTint;
        ColorBlock pptColors = pptButton.colors;
        pptColors.normalColor = new Color(0.2f, 0.6f, 1f, 1f);
        pptColors.highlightedColor = new Color(0.3f, 0.7f, 1f, 1f);
        pptColors.pressedColor = new Color(0.1f, 0.5f, 0.9f, 1f);
        pptButton.colors = pptColors;
        
        // 创建PPT路径文本
        GameObject pptPathObj = CreateText("未选择文件", new Vector2(0.5f, 0.5f), fileSelectObj.transform);
        
        // 创建TXT选择按钮
        GameObject txtButtonObj = CreateButton("选择文本文件", new Vector2(0.5f, 0.3f), fileSelectObj.transform);
        Button txtButton = txtButtonObj.GetComponent<Button>();
        txtButton.transition = Selectable.Transition.ColorTint;
        ColorBlock txtColors = txtButton.colors;
        txtColors.normalColor = new Color(0.2f, 0.6f, 1f, 1f);
        txtColors.highlightedColor = new Color(0.3f, 0.7f, 1f, 1f);
        txtColors.pressedColor = new Color(0.1f, 0.5f, 0.9f, 1f);
        txtButton.colors = txtColors;
        
        // 创建TXT路径文本
        GameObject txtPathObj = CreateText("未选择文件", new Vector2(0.5f, 0.1f), fileSelectObj.transform);

        // 创建重新生成选项区域
        GameObject regenerateObj = new GameObject("RegenerateOption");
        regenerateObj.transform.SetParent(panelObj.transform, false);
        RectTransform regenerateRect = regenerateObj.AddComponent<RectTransform>();
        regenerateRect.anchorMin = new Vector2(0.5f, 0.3f);
        regenerateRect.anchorMax = new Vector2(0.5f, 0.3f);
        regenerateRect.sizeDelta = new Vector2(400, 50);
        regenerateRect.anchoredPosition = Vector2.zero;

        // 创建Toggle
        GameObject toggleObj = CreateToggle("重新生成音频和PPT", new Vector2(0.5f, 0.5f), regenerateObj.transform);
        Toggle regenerateToggle = toggleObj.GetComponent<Toggle>();
        regenerateToggle.isOn = true; // 默认选中

        // 获取Toggle的Label
        Text regenerateLabel = toggleObj.GetComponentInChildren<Text>();

        // 创建开始按钮
        GameObject startButtonObj = CreateButton("开始", new Vector2(0.5f, 0.2f), panelObj.transform);
        Button startButton = startButtonObj.GetComponent<Button>();
        startButton.transition = Selectable.Transition.ColorTint;
        ColorBlock startColors = startButton.colors;
        startColors.normalColor = new Color(0.2f, 0.8f, 0.2f, 1f);
        startColors.highlightedColor = new Color(0.3f, 0.9f, 0.3f, 1f);
        startColors.pressedColor = new Color(0.1f, 0.7f, 0.1f, 1f);
        startButton.colors = startColors;
        startButton.interactable = false;
        startButtonObj.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 60);
        startButtonObj.GetComponentInChildren<Text>().fontSize = 32;
        
        // 创建状态文本
        GameObject statusObj = CreateText("", new Vector2(0.5f, 0.1f), panelObj.transform);
        statusObj.GetComponent<Text>().color = new Color(0.8f, 0.8f, 0.8f, 1f);

        // 添加UploadPage组件
        UploadPage uploadPage = panelObj.AddComponent<UploadPage>();
        
        // 设置引用
        SerializedObject serializedObject = new SerializedObject(uploadPage);
        serializedObject.FindProperty("selectPptButton").objectReferenceValue = pptButton;
        serializedObject.FindProperty("selectTxtButton").objectReferenceValue = txtButton;
        serializedObject.FindProperty("startButton").objectReferenceValue = startButton;
        serializedObject.FindProperty("pptPathText").objectReferenceValue = pptPathObj.GetComponent<Text>();
        serializedObject.FindProperty("txtPathText").objectReferenceValue = txtPathObj.GetComponent<Text>();
        serializedObject.FindProperty("statusText").objectReferenceValue = statusObj.GetComponent<Text>();
        serializedObject.FindProperty("backgroundImage").objectReferenceValue = bgImage;
        serializedObject.FindProperty("regenerateToggle").objectReferenceValue = regenerateToggle;
        serializedObject.FindProperty("regenerateLabel").objectReferenceValue = regenerateLabel;
        
        // 使用持久化的UnityEvent绑定按钮事件
        UnityEditor.Events.UnityEventTools.AddPersistentListener(pptButton.onClick, uploadPage.SelectPptFile);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(txtButton.onClick, uploadPage.SelectTxtFile);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(startButton.onClick, uploadPage.StartProcess);
        
        serializedObject.ApplyModifiedProperties();

        // 确保Resources/Backgrounds文件夹存在
        string backgroundsPath = "Assets/Resources/Backgrounds";
        if (!Directory.Exists(backgroundsPath))
        {
            Directory.CreateDirectory(backgroundsPath);
        }

        // 确保Scenes文件夹存在
        string scenesPath = "Assets/Scenes";
        if (!Directory.Exists(scenesPath))
        {
            Directory.CreateDirectory(scenesPath);
        }

        // 保存新场景
        string scenePath = "Assets/Scenes/UploadScene.unity";
        EditorSceneManager.SaveScene(newScene, scenePath);
        
        // 提示用户
        Debug.Log($"上传页面已创建并保存到: {scenePath}");
        Debug.Log($"请将背景图片放在: {backgroundsPath}/upload_bg.exr 或 upload_bg.png");
        Debug.Log("支持的格式：EXR（推荐）、PNG、JPG等");
        Debug.Log("注意：EXR文件导入设置：");
        Debug.Log("1. Texture Type: Default");
        Debug.Log("2. Texture Shape: Cube");
        Debug.Log("3. Mapping: Latitude-Longitude Layout");
        Debug.Log("4. Generate Mip Maps: 关闭");
        Debug.Log("5. Read/Write Enabled: 开启");
        
        // 将场景添加到构建设置
        AddSceneToBuildSettings(scenePath);
    }

    private static void AddSceneToBuildSettings(string scenePath)
    {
        var scenes = EditorBuildSettings.scenes;
        var newScenes = new EditorBuildSettingsScene[scenes.Length + 1];
        scenes.CopyTo(newScenes, 0);
        newScenes[scenes.Length] = new EditorBuildSettingsScene(scenePath, true);
        EditorBuildSettings.scenes = newScenes;
    }

    private static GameObject CreateButton(string text, Vector2 anchor, Transform parent)
    {
        GameObject buttonObj = new GameObject(text);
        buttonObj.transform.SetParent(parent, false);
        
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.6f, 1f, 1f);
        
        Button button = buttonObj.AddComponent<Button>();
        button.targetGraphic = buttonImage;
        
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        
        Text buttonText = textObj.AddComponent<Text>();
        buttonText.text = text;
        buttonText.fontSize = 24;
        buttonText.alignment = TextAnchor.MiddleCenter;
        buttonText.color = Color.white;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        
        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.anchorMin = anchor;
        buttonRect.anchorMax = anchor;
        buttonRect.sizeDelta = new Vector2(200, 50);
        buttonRect.anchoredPosition = Vector2.zero;
        
        return buttonObj;
    }

    private static GameObject CreateText(string text, Vector2 anchor, Transform parent)
    {
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(parent, false);
        
        Text textComponent = textObj.AddComponent<Text>();
        textComponent.text = text;
        textComponent.fontSize = 20;
        textComponent.alignment = TextAnchor.MiddleCenter;
        textComponent.color = Color.white;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = anchor;
        textRect.anchorMax = anchor;
        textRect.sizeDelta = new Vector2(400, 30);
        textRect.anchoredPosition = Vector2.zero;
        
        return textObj;
    }

    private static GameObject CreateToggle(string text, Vector2 anchor, Transform parent)
    {
        GameObject toggleObj = new GameObject("Toggle");
        toggleObj.transform.SetParent(parent, false);
        
        // 添加Toggle组件
        Toggle toggle = toggleObj.AddComponent<Toggle>();
        
        // 创建背景
        GameObject backgroundObj = new GameObject("Background");
        backgroundObj.transform.SetParent(toggleObj.transform, false);
        Image backgroundImage = backgroundObj.AddComponent<Image>();
        backgroundImage.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
        
        RectTransform backgroundRect = backgroundObj.GetComponent<RectTransform>();
        backgroundRect.anchorMin = new Vector2(0f, 0f);
        backgroundRect.anchorMax = new Vector2(0f, 1f);
        backgroundRect.sizeDelta = new Vector2(30, 0);
        backgroundRect.anchoredPosition = new Vector2(15, 0);
        
        // 创建勾选标记
        GameObject checkmarkObj = new GameObject("Checkmark");
        checkmarkObj.transform.SetParent(backgroundObj.transform, false);
        Image checkmarkImage = checkmarkObj.AddComponent<Image>();
        checkmarkImage.color = new Color(0.2f, 0.8f, 0.2f, 1f);
        
        RectTransform checkmarkRect = checkmarkObj.GetComponent<RectTransform>();
        checkmarkRect.anchorMin = Vector2.zero;
        checkmarkRect.anchorMax = Vector2.one;
        checkmarkRect.sizeDelta = new Vector2(-6, -6);
        checkmarkRect.anchoredPosition = Vector2.zero;
        
        // 创建文本标签
        GameObject textObj = new GameObject("Label");
        textObj.transform.SetParent(toggleObj.transform, false);
        
        Text toggleText = textObj.AddComponent<Text>();
        toggleText.text = text;
        toggleText.fontSize = 20;
        toggleText.alignment = TextAnchor.MiddleLeft;
        toggleText.color = Color.white;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0f, 0f);
        textRect.anchorMax = new Vector2(1f, 1f);
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = new Vector2(20, 0);
        
        // 设置Toggle引用
        toggle.targetGraphic = backgroundImage;
        toggle.graphic = checkmarkImage;
        
        // 设置Toggle的RectTransform
        RectTransform toggleRect = toggleObj.GetComponent<RectTransform>();
        toggleRect.anchorMin = anchor;
        toggleRect.anchorMax = anchor;
        toggleRect.sizeDelta = new Vector2(350, 30);
        toggleRect.anchoredPosition = Vector2.zero;
        
        return toggleObj;
    }
} 
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 整合角色放置和动画播放系统
/// </summary>
public class CharacterPlacementSystem : MonoBehaviour
{
    [Header("组件引用")]
    [SerializeField] private CharacterPlacementManager placementManager;
    [SerializeField] private CharacterAnimationPlayer animationPlayer;
    [SerializeField] private CharacterPlacementUIPanel uiPanel;
    
    [Header("UI引用")]
    [SerializeField] private Canvas uiCanvas;
    [SerializeField] private Button openUIButton;
    
    private void Start()
    {
        // 确保各组件之间的引用关系正确
        SetupComponentReferences();
        
        // 设置打开UI按钮
        if (openUIButton != null)
        {
            openUIButton.onClick.AddListener(OpenUI);
        }
    }
    
    private void SetupComponentReferences()
    {
        // 如果没有设置组件，尝试自动查找
        if (placementManager == null)
        {
            placementManager = GetComponent<CharacterPlacementManager>();
            if (placementManager == null)
            {
                placementManager = gameObject.AddComponent<CharacterPlacementManager>();
                Debug.Log("自动添加了CharacterPlacementManager组件");
            }
        }
        
        if (animationPlayer == null)
        {
            animationPlayer = GetComponent<CharacterAnimationPlayer>();
            if (animationPlayer == null)
            {
                animationPlayer = gameObject.AddComponent<CharacterAnimationPlayer>();
                Debug.Log("自动添加了CharacterAnimationPlayer组件");
            }
        }
        
        if (uiPanel == null && uiCanvas != null)
        {
            uiPanel = uiCanvas.GetComponentInChildren<CharacterPlacementUIPanel>();
        }
        
#if UNITY_EDITOR
        SetupComponentReferencesInEditor();
#endif
    }
    
#if UNITY_EDITOR
    private void SetupComponentReferencesInEditor()
    {
        // 确保引用关系正确
        if (placementManager != null && animationPlayer != null)
        {
            // 设置Placement Manager中的动画播放器引用
            var serializedObject = new UnityEditor.SerializedObject(placementManager);
            var animationPlayerProp = serializedObject.FindProperty("animationPlayer");
            if (animationPlayerProp != null)
            {
                animationPlayerProp.objectReferenceValue = animationPlayer;
                serializedObject.ApplyModifiedProperties();
                Debug.Log("已将动画播放器引用设置到放置管理器中");
            }
        }
        
        if (uiPanel != null)
        {
            // 设置UI面板中的引用
            var serializedObject = new UnityEditor.SerializedObject(uiPanel);
            var placementManagerProp = serializedObject.FindProperty("placementManager");
            var animationPlayerProp = serializedObject.FindProperty("animationPlayer");
            
            if (placementManagerProp != null && placementManager != null)
            {
                placementManagerProp.objectReferenceValue = placementManager;
            }
            
            if (animationPlayerProp != null && animationPlayer != null)
            {
                animationPlayerProp.objectReferenceValue = animationPlayer;
            }
            
            serializedObject.ApplyModifiedProperties();
            Debug.Log("已将组件引用设置到UI面板中");
        }
    }
    
    /// <summary>
    /// 编辑器方法：用于创建和配置整个系统
    /// </summary>
    [UnityEditor.MenuItem("工具/创建角色放置系统")]
    public static void CreateCharacterPlacementSystem()
    {
        // 创建系统根GameObject
        GameObject systemRoot = new GameObject("CharacterPlacementSystem");
        systemRoot.AddComponent<CharacterPlacementSystem>();
        systemRoot.AddComponent<CharacterPlacementManager>();
        systemRoot.AddComponent<CharacterAnimationPlayer>();
        
        Debug.Log("已创建角色放置系统，请添加UI面板并配置引用");
        
        // 选中创建的对象
        UnityEditor.Selection.activeGameObject = systemRoot;
    }
#endif
    
    public void OpenUI()
    {
        if (uiCanvas != null)
        {
            uiCanvas.gameObject.SetActive(true);
        }
        else if (uiPanel != null)
        {
            uiPanel.Show();
        }
    }
} 
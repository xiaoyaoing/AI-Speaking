using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
#endif

/// <summary>
/// 用于自动生成和修改角色动画控制器的工具类
/// </summary>
public class AnimatorControllerGenerator : MonoBehaviour
{
#if UNITY_EDITOR
    [Header("动画控制器设置")]
    [Tooltip("控制器保存路径")]
    public string controllerSavePath = "Assets/AnimatorControllers/";
    [Tooltip("控制器名称前缀")]
    public string controllerPrefix = "Character_";
    
    [Header("动画状态配置")]
    [Tooltip("说话动画1")]
    public AnimationClip talk1Animation;
    [Tooltip("说话动画2")]
    public AnimationClip talk2Animation;
    [Tooltip("聆听动画")]
    public AnimationClip listenAnimation;
    [Tooltip("鼓掌动画")]
    public AnimationClip clapAnimation;
    
    [Header("转换参数")]
    [Tooltip("状态参数前缀")]
    public string paramPrefix = "Is";
    
    [Header("批量处理")]
    [Tooltip("处理的角色层级")]
    public Transform charactersRoot;
    
    // 在编辑器中添加按钮来创建控制器
    [ContextMenu("创建动画控制器")]
    public void CreateAnimatorController()
    {
        // 确保保存路径存在
        if (!Directory.Exists(controllerSavePath))
        {
            Directory.CreateDirectory(controllerSavePath);
        }
        
        // 生成控制器的路径和名称
        string controllerName = $"{controllerPrefix}{gameObject.name}";
        string controllerPath = Path.Combine(controllerSavePath, $"{controllerName}.controller");
        
        // 创建新的动画控制器
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        
        // 添加参数
        controller.AddParameter("IsTalking", AnimatorControllerParameterType.Bool);
        controller.AddParameter("IsListening", AnimatorControllerParameterType.Bool);
        controller.AddParameter("IsClapping", AnimatorControllerParameterType.Bool);
        controller.AddParameter("TalkVariant", AnimatorControllerParameterType.Int);
        
        // 获取根层
        AnimatorControllerLayer baseLayer = controller.layers[0];
        AnimatorStateMachine rootStateMachine = baseLayer.stateMachine;
        
        // 添加状态
        AnimatorState talk1State = rootStateMachine.AddState("talk1_NoLegs");
        AnimatorState talk2State = rootStateMachine.AddState("talk2_NoLegs");
        AnimatorState listenState = rootStateMachine.AddState("listen_Nolegs");
        AnimatorState clapState = rootStateMachine.AddState("claphands_NoLegs");
        
        // 分配动画剪辑
        if (talk1Animation != null) talk1State.motion = talk1Animation;
        if (talk2Animation != null) talk2State.motion = talk2Animation;
        if (listenAnimation != null) listenState.motion = listenAnimation;
        if (clapAnimation != null) clapState.motion = clapAnimation;
        
        // 设置默认状态
        rootStateMachine.defaultState = listenState;
        
        // 添加从Listen到Talk1的转换
        AnimatorStateTransition listenToTalk1 = listenState.AddTransition(talk1State);
        listenToTalk1.AddCondition(AnimatorConditionMode.If, 0, "IsTalking");
        listenToTalk1.AddCondition(AnimatorConditionMode.Equals, 1, "TalkVariant");
        listenToTalk1.hasExitTime = false;
        listenToTalk1.duration = 0.25f;
        
        // 添加从Listen到Talk2的转换
        AnimatorStateTransition listenToTalk2 = listenState.AddTransition(talk2State);
        listenToTalk2.AddCondition(AnimatorConditionMode.If, 0, "IsTalking");
        listenToTalk2.AddCondition(AnimatorConditionMode.Equals, 2, "TalkVariant");
        listenToTalk2.hasExitTime = false;
        listenToTalk2.duration = 0.25f;
        
        // 添加从Listen到Clap的转换
        AnimatorStateTransition listenToClap = listenState.AddTransition(clapState);
        listenToClap.AddCondition(AnimatorConditionMode.If, 0, "IsClapping");
        listenToClap.hasExitTime = false;
        listenToClap.duration = 0.25f;
        
        // 添加从Talk1到Listen的转换
        AnimatorStateTransition talk1ToListen = talk1State.AddTransition(listenState);
        talk1ToListen.AddCondition(AnimatorConditionMode.If, 0, "IsListening");
        talk1ToListen.hasExitTime = false;
        talk1ToListen.duration = 0.25f;
        
        // 添加从Talk2到Listen的转换
        AnimatorStateTransition talk2ToListen = talk2State.AddTransition(listenState);
        talk2ToListen.AddCondition(AnimatorConditionMode.If, 0, "IsListening");
        talk2ToListen.hasExitTime = false;
        talk2ToListen.duration = 0.25f;
        
        // 添加从Clap到Listen的转换
        AnimatorStateTransition clapToListen = clapState.AddTransition(listenState);
        clapToListen.AddCondition(AnimatorConditionMode.If, 0, "IsListening");
        clapToListen.hasExitTime = false;
        clapToListen.duration = 0.25f;
        
        // 应用更改
        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        // 将控制器分配给对象的Animator组件
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.runtimeAnimatorController = controller;
            EditorUtility.SetDirty(animator);
        }
        
        Debug.Log($"已创建动画控制器: {controllerPath}");
    }
    
    [ContextMenu("批量创建所有角色的动画控制器")]
    public void BatchCreateAnimatorControllers()
    {
        if (charactersRoot == null)
        {
            Debug.LogError("请先设置角色层级");
            return;
        }
        
        // 查找所有符合条件的角色
        foreach (Transform child in charactersRoot)
        {
            string name = child.name.ToLower();
            if (name.Contains("man") || name.Contains("girl") || name.Contains("female") || name.Contains("male"))
            {
                // 生成该角色的控制器
                CreateAnimatorControllerForCharacter(child.gameObject);
            }
        }
        
        Debug.Log("批量创建控制器完成");
    }
    
    private void CreateAnimatorControllerForCharacter(GameObject character)
    {
        if (character == null) return;
        
        // 确保保存路径存在
        if (!Directory.Exists(controllerSavePath))
        {
            Directory.CreateDirectory(controllerSavePath);
        }
        
        // 生成控制器的路径和名称
        string controllerName = $"{controllerPrefix}{character.name}";
        string controllerPath = Path.Combine(controllerSavePath, $"{controllerName}.controller");
        
        // 创建新的动画控制器
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        
        // 添加参数
        controller.AddParameter("IsTalking", AnimatorControllerParameterType.Bool);
        controller.AddParameter("IsListening", AnimatorControllerParameterType.Bool);
        controller.AddParameter("IsClapping", AnimatorControllerParameterType.Bool);
        controller.AddParameter("TalkVariant", AnimatorControllerParameterType.Int);
        
        // 获取根层
        AnimatorControllerLayer baseLayer = controller.layers[0];
        AnimatorStateMachine rootStateMachine = baseLayer.stateMachine;
        
        // 添加状态
        AnimatorState talk1State = rootStateMachine.AddState("talk1_NoLegs");
        AnimatorState talk2State = rootStateMachine.AddState("talk2_NoLegs");
        AnimatorState listenState = rootStateMachine.AddState("listen_Nolegs");
        AnimatorState clapState = rootStateMachine.AddState("claphands_NoLegs");
        
        // 自动查找和分配动画剪辑
        string characterType = DetermineCharacterType(character.name);
        string animationsPath = "Assets/Animations/";
        
        // 查找并分配Talk1动画
        AnimationClip talk1Clip = FindAnimationClip($"{characterType}talk1", animationsPath);
        if (talk1Clip != null) talk1State.motion = talk1Clip;
        
        // 查找并分配Talk2动画
        AnimationClip talk2Clip = FindAnimationClip($"{characterType}talk2", animationsPath);
        if (talk2Clip != null) talk2State.motion = talk2Clip;
        
        // 查找并分配Listen动画
        AnimationClip listenClip = FindAnimationClip($"{characterType}listen", animationsPath);
        if (listenClip != null) listenState.motion = listenClip;
        
        // 查找并分配Clap动画
        AnimationClip clapClip = FindAnimationClip($"{characterType}clap", animationsPath);
        if (clapClip != null) clapState.motion = clapClip;
        
        // 设置默认状态
        rootStateMachine.defaultState = listenState;
        
        // 添加各种状态转换，与上面相同...
        // 添加从Listen到Talk1的转换
        AnimatorStateTransition listenToTalk1 = listenState.AddTransition(talk1State);
        listenToTalk1.AddCondition(AnimatorConditionMode.If, 0, "IsTalking");
        listenToTalk1.AddCondition(AnimatorConditionMode.Equals, 1, "TalkVariant");
        listenToTalk1.hasExitTime = false;
        listenToTalk1.duration = 0.25f;
        
        // 添加从Listen到Talk2的转换
        AnimatorStateTransition listenToTalk2 = listenState.AddTransition(talk2State);
        listenToTalk2.AddCondition(AnimatorConditionMode.If, 0, "IsTalking");
        listenToTalk2.AddCondition(AnimatorConditionMode.Equals, 2, "TalkVariant");
        listenToTalk2.hasExitTime = false;
        listenToTalk2.duration = 0.25f;
        
        // 添加从Listen到Clap的转换
        AnimatorStateTransition listenToClap = listenState.AddTransition(clapState);
        listenToClap.AddCondition(AnimatorConditionMode.If, 0, "IsClapping");
        listenToClap.hasExitTime = false;
        listenToClap.duration = 0.25f;
        
        // 添加从Talk1到Listen的转换
        AnimatorStateTransition talk1ToListen = talk1State.AddTransition(listenState);
        talk1ToListen.AddCondition(AnimatorConditionMode.If, 0, "IsListening");
        talk1ToListen.hasExitTime = false;
        talk1ToListen.duration = 0.25f;
        
        // 添加从Talk2到Listen的转换
        AnimatorStateTransition talk2ToListen = talk2State.AddTransition(listenState);
        talk2ToListen.AddCondition(AnimatorConditionMode.If, 0, "IsListening");
        talk2ToListen.hasExitTime = false;
        talk2ToListen.duration = 0.25f;
        
        // 添加从Clap到Listen的转换
        AnimatorStateTransition clapToListen = clapState.AddTransition(listenState);
        clapToListen.AddCondition(AnimatorConditionMode.If, 0, "IsListening");
        clapToListen.hasExitTime = false;
        clapToListen.duration = 0.25f;
        
        // 应用更改
        EditorUtility.SetDirty(controller);
        
        // 将控制器分配给对象的Animator组件
        Animator animator = character.GetComponent<Animator>();
        if (animator == null)
        {
            animator = character.AddComponent<Animator>();
        }
        animator.runtimeAnimatorController = controller;
        EditorUtility.SetDirty(animator);
        
        Debug.Log($"已为角色 {character.name} 创建动画控制器: {controllerPath}");
    }
    
    // 查找匹配的动画剪辑
    private AnimationClip FindAnimationClip(string clipNamePattern, string searchPath)
    {
        string[] guids = AssetDatabase.FindAssets("t:AnimationClip", new[] { searchPath });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            
            if (clip != null && clip.name.ToLower().Contains(clipNamePattern.ToLower()))
            {
                return clip;
            }
        }
        return null;
    }
    
    // 确定角色类型前缀
    private string DetermineCharacterType(string characterName)
    {
        string name = characterName.ToLower();
        if (name.Contains("man") || name.Contains("male"))
        {
            return "m_";
        }
        else if (name.Contains("girl") || name.Contains("female"))
        {
            return "f_";
        }
        return "";
    }
    
    [MenuItem("工具/角色工具/为选中对象创建动画控制器")]
    private static void CreateControllerForSelected()
    {
        GameObject selectedObject = Selection.activeGameObject;
        if (selectedObject == null)
        {
            EditorUtility.DisplayDialog("错误", "请先选择一个角色对象", "确定");
            return;
        }
        
        AnimatorControllerGenerator generator = selectedObject.GetComponent<AnimatorControllerGenerator>();
        if (generator == null)
        {
            generator = selectedObject.AddComponent<AnimatorControllerGenerator>();
        }
        
        generator.CreateAnimatorController();
    }
    
    [MenuItem("工具/角色工具/批量为所有角色创建动画控制器")]
    private static void BatchCreateControllers()
    {
        // 查找场景中的所有角色
        GameObject[] allGameObjects = GameObject.FindObjectsOfType<GameObject>();
        int count = 0;
        
        foreach (GameObject obj in allGameObjects)
        {
            string name = obj.name.ToLower();
            if (name.Contains("man") || name.Contains("girl") || name.Contains("female") || name.Contains("male"))
            {
                AnimatorControllerGenerator generator = obj.GetComponent<AnimatorControllerGenerator>();
                if (generator == null)
                {
                    generator = obj.AddComponent<AnimatorControllerGenerator>();
                }
                
                generator.CreateAnimatorController();
                count++;
            }
        }
        
        EditorUtility.DisplayDialog("完成", $"已为 {count} 个角色创建动画控制器", "确定");
    }
    
    // 同步已有控制器中的参数和状态
    [MenuItem("工具/角色工具/同步现有控制器")]
    private static void SyncExistingControllers()
    {
        string[] guids = AssetDatabase.FindAssets("t:AnimatorController");
        int count = 0;
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            
            if (controller != null)
            {
                bool modified = false;
                
                // 检查并添加必要的参数
                if (controller.parameters.FirstOrDefault(p => p.name == "IsTalking") == null)
                {
                    controller.AddParameter("IsTalking", AnimatorControllerParameterType.Bool);
                    modified = true;
                }
                
                if (controller.parameters.FirstOrDefault(p => p.name == "IsListening") == null)
                {
                    controller.AddParameter("IsListening", AnimatorControllerParameterType.Bool);
                    modified = true;
                }
                
                if (controller.parameters.FirstOrDefault(p => p.name == "IsClapping") == null)
                {
                    controller.AddParameter("IsClapping", AnimatorControllerParameterType.Bool);
                    modified = true;
                }
                
                if (controller.parameters.FirstOrDefault(p => p.name == "TalkVariant") == null)
                {
                    controller.AddParameter("TalkVariant", AnimatorControllerParameterType.Int);
                    modified = true;
                }
                
                // 检查根状态机
                AnimatorControllerLayer baseLayer = controller.layers[0];
                AnimatorStateMachine rootStateMachine = baseLayer.stateMachine;
                
                // 检查并添加必要的状态
                AnimatorState talk1State = rootStateMachine.states.FirstOrDefault(s => s.state.name == "talk1_NoLegs").state;
                AnimatorState talk2State = rootStateMachine.states.FirstOrDefault(s => s.state.name == "talk2_NoLegs").state;
                AnimatorState listenState = rootStateMachine.states.FirstOrDefault(s => s.state.name == "listen_Nolegs").state;
                AnimatorState clapState = rootStateMachine.states.FirstOrDefault(s => s.state.name == "claphands_NoLegs").state;
                
                if (talk1State == null)
                {
                    talk1State = rootStateMachine.AddState("talk1_NoLegs");
                    modified = true;
                }
                
                if (talk2State == null)
                {
                    talk2State = rootStateMachine.AddState("talk2_NoLegs");
                    modified = true;
                }
                
                if (listenState == null)
                {
                    listenState = rootStateMachine.AddState("listen_Nolegs");
                    rootStateMachine.defaultState = listenState;
                    modified = true;
                }
                
                if (clapState == null)
                {
                    clapState = rootStateMachine.AddState("claphands_NoLegs");
                    modified = true;
                }
                
                // TODO: 检查并添加必要的转换
                // 实现类似于上面创建控制器中的转换添加代码
                
                if (modified)
                {
                    EditorUtility.SetDirty(controller);
                    count++;
                }
            }
        }
        
        if (count > 0)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("完成", $"已同步 {count} 个动画控制器", "确定");
        }
        else
        {
            EditorUtility.DisplayDialog("完成", "没有找到需要同步的控制器", "确定");
        }
    }
#endif
} 
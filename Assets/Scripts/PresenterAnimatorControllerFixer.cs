using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;

/// <summary>
/// 用于修复Animator Controller转换条件的编辑器工具
/// </summary>
public static class PresenterAnimatorControllerFixer
{
    [MenuItem("工具/讲演者动画/修复Animator Controller转换条件")]
    public static void ShowWindow()
    {
        PresenterAnimatorControllerFixerWindow window = EditorWindow.GetWindow<PresenterAnimatorControllerFixerWindow>("修复动画控制器");
        window.Show();
    }
    
    [MenuItem("工具/讲演者动画/设置讲演者从站立状态开始")]
    public static void SetStartWithStandUp()
    {
        bool currentValue = EditorPrefs.GetBool("PresenterStartWithStandUp", false);
        bool newValue = !currentValue; // 切换值
        
        // 保存新值
        EditorPrefs.SetBool("PresenterStartWithStandUp", newValue);
        
        // 显示当前设置
        EditorUtility.DisplayDialog(
            "设置讲演者初始状态", 
            $"讲演者将{(newValue ? "从站立状态开始" : "从坐着状态开始")}\n\n" +
            "注意：此设置将在场景重新加载后生效。",
            "确定");
        
        // 尝试找到场景中的PresenterController并通知它
        PresenterController[] controllers = Object.FindObjectsOfType<PresenterController>();
        if (controllers.Length > 0)
        {
            PresenterController.SetStartWithStandUp(newValue);
        }
    }
    
    [MenuItem("工具/讲演者动画/显示当前设置")]
    public static void ShowCurrentSettings()
    {
        bool startWithStandUp = EditorPrefs.GetBool("PresenterStartWithStandUp", false);
        
        EditorUtility.DisplayDialog(
            "当前讲演者设置", 
            $"讲演者初始状态: {(startWithStandUp ? "站立" : "坐着")}\n\n" +
            "此设置将在场景重新加载后生效。",
            "确定");
    }
}

/// <summary>
/// 修复Animator Controller的编辑器窗口
/// </summary>
public class PresenterAnimatorControllerFixerWindow : EditorWindow
{
    private string controllerPath = "Assets/Animations/PresenterController.controller";
    private string idleStateName = "Idle";
    private string walkStateName = "Walk";
    
    private bool showAdvanced = false;
    private float transitionTime = 0.25f;
    private bool startWithStandUp = false;
    
    private void OnEnable()
    {
        // 加载当前设置
        startWithStandUp = EditorPrefs.GetBool("PresenterStartWithStandUp", false);
    }
    
    private void OnGUI()
    {
        GUILayout.Label("讲演者动画控制器修复工具", EditorStyles.boldLabel);
        EditorGUILayout.Space(10);
        
        // 控制器路径
        EditorGUILayout.BeginHorizontal();
        controllerPath = EditorGUILayout.TextField("控制器路径:", controllerPath);
        if (GUILayout.Button("浏览", GUILayout.Width(60)))
        {
            string path = EditorUtility.OpenFilePanel("选择Animator Controller", "Assets", "controller");
            if (!string.IsNullOrEmpty(path))
            {
                // 将完整路径转换为相对于Assets的路径
                if (path.StartsWith(Application.dataPath))
                {
                    controllerPath = "Assets" + path.Substring(Application.dataPath.Length);
                }
            }
        }
        EditorGUILayout.EndHorizontal();
        
        // 动画状态名称
        EditorGUILayout.LabelField("动画状态名称", EditorStyles.boldLabel);
        idleStateName = EditorGUILayout.TextField("闲置状态名称:", idleStateName);
        walkStateName = EditorGUILayout.TextField("行走状态名称:", walkStateName);
        
        // 高级选项
        EditorGUILayout.Space(10);
        showAdvanced = EditorGUILayout.Foldout(showAdvanced, "高级选项");
        if (showAdvanced)
        {
            transitionTime = EditorGUILayout.Slider("过渡时间:", transitionTime, 0.01f, 1f);
            
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("初始状态设置", EditorStyles.boldLabel);
            bool newStartWithStandUp = EditorGUILayout.Toggle("从站立状态开始:", startWithStandUp);
            if (newStartWithStandUp != startWithStandUp)
            {
                startWithStandUp = newStartWithStandUp;
                EditorPrefs.SetBool("PresenterStartWithStandUp", startWithStandUp);
                
                // 通知讲演者控制器
                PresenterController.SetStartWithStandUp(startWithStandUp);
            }
        }
        
        // 应用按钮
        EditorGUILayout.Space(20);
        if (GUILayout.Button("应用修复", GUILayout.Height(40)))
        {
            FixAnimatorController();
        }
        
        // 提示
        EditorGUILayout.Space(10);
        EditorGUILayout.HelpBox(
            "使用说明：\n" +
            "1. 设置现有Animator Controller的路径\n" +
            "2. 确认两个状态的名称是否正确\n" +
            "3. 点击'应用修复'按钮修复转换条件\n" +
            "4. 在'高级选项'中可以设置讲演者的初始状态\n" +
            "注意：此操作会清除现有的所有转换条件，请确保已备份", 
            MessageType.Warning);
    }
    
    /// <summary>
    /// 修复Animator Controller的转换条件
    /// </summary>
    private void FixAnimatorController()
    {
        // 加载现有的controller
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
        if (controller == null)
        {
            EditorUtility.DisplayDialog("错误", $"无法加载Animator Controller: {controllerPath}", "确定");
            return;
        }
        
        // 获取根状态机
        AnimatorStateMachine rootStateMachine = controller.layers[0].stateMachine;
        
        // 确保参数存在
        EnsureParameterExists(controller, "isWalking", AnimatorControllerParameterType.Bool);
        EnsureParameterExists(controller, "isIdle", AnimatorControllerParameterType.Bool);
        
        // 查找状态
        AnimatorState idleState = FindState(rootStateMachine, idleStateName);
        AnimatorState walkState = FindState(rootStateMachine, walkStateName);
        
        if (idleState == null || walkState == null)
        {
            EditorUtility.DisplayDialog("错误", "无法找到所有必要的动画状态，请确保状态名称正确", "确定");
            return;
        }
        
        // 清除现有转换
        ClearExistingTransitions(idleState);
        ClearExistingTransitions(walkState);
        
        // 设置状态循环
        SetStateToLoop(idleState);
        SetStateToLoop(walkState);
        
        // 设置默认状态为idle
        rootStateMachine.defaultState = idleState;
        Debug.Log($"设置默认状态为: {idleStateName}");
        
        // 设置新的转换条件
        // 1. 闲置 -> 行走 (当isWalking = true)
        AnimatorStateTransition idleToWalk = idleState.AddTransition(walkState);
        idleToWalk.AddCondition(AnimatorConditionMode.If, 0, "isWalking");
        idleToWalk.hasExitTime = false;
        idleToWalk.duration = transitionTime;
        
        // 2. 行走 -> 闲置 (当isWalking = false 且 isIdle = true)
        AnimatorStateTransition walkToIdle = walkState.AddTransition(idleState);
        walkToIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "isWalking");
        walkToIdle.AddCondition(AnimatorConditionMode.If, 0, "isIdle");
        walkToIdle.hasExitTime = false;
        walkToIdle.duration = transitionTime;
        
        // 保存修改
        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        EditorUtility.DisplayDialog("成功", $"已成功修复Animator Controller: {controllerPath}", "确定");
    }
    
    /// <summary>
    /// 确保参数存在
    /// </summary>
    private void EnsureParameterExists(AnimatorController controller, string paramName, AnimatorControllerParameterType paramType)
    {
        // 检查参数是否已存在
        bool paramExists = false;
        foreach (AnimatorControllerParameter param in controller.parameters)
        {
            if (param.name == paramName)
            {
                paramExists = true;
                break;
            }
        }
        
        // 如果不存在，添加参数
        if (!paramExists)
        {
            controller.AddParameter(paramName, paramType);
            Debug.Log($"添加参数: {paramName}");
        }
    }
    
    /// <summary>
    /// 查找状态
    /// </summary>
    private AnimatorState FindState(AnimatorStateMachine stateMachine, string stateName)
    {
        foreach (ChildAnimatorState childState in stateMachine.states)
        {
            if (childState.state.name == stateName)
            {
                return childState.state;
            }
        }
        
        Debug.LogWarning($"找不到状态: {stateName}");
        return null;
    }
    
    /// <summary>
    /// 清除已有的转换
    /// </summary>
    private void ClearExistingTransitions(AnimatorState state)
    {
        if (state == null) return;
        
        // 删除所有传出转换
        while (state.transitions.Length > 0)
        {
            state.RemoveTransition(state.transitions[0]);
        }
        
        Debug.Log($"已清除状态 '{state.name}' 的所有转换");
    }
    
    /// <summary>
    /// 设置状态循环播放
    /// </summary>
    private void SetStateToLoop(AnimatorState state)
    {
        if (state == null) return;
        
        // 状态的Motion应该是AnimationClip
        AnimationClip clip = state.motion as AnimationClip;
        if (clip != null)
        {
            // 检查是否可编辑
            if (!clip.isLooping)
            {
                // 注意：如果是导入的动画（不是在Unity中创建的），可能无法直接修改循环设置
                // 在这种情况下，需要创建一个动画覆盖（Animation Override）
                Debug.Log($"设置状态 '{state.name}' 的动画为循环播放");
                
                try
                {
                    SerializedObject serializedClip = new SerializedObject(clip);
                    SerializedProperty loopTimeProperty = serializedClip.FindProperty("m_LoopTime");
                    if (loopTimeProperty != null)
                    {
                        loopTimeProperty.boolValue = true;
                        serializedClip.ApplyModifiedProperties();
                        EditorUtility.SetDirty(clip);
                    }
                    else
                    {
                        Debug.LogWarning($"无法设置动画 '{clip.name}' 循环播放 - 找不到m_LoopTime属性");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"无法设置动画 '{clip.name}' 循环播放 - {e.Message}");
                }
            }
        }
        else
        {
            Debug.LogWarning($"状态 '{state.name}' 没有有效的AnimationClip");
        }
        
        // 设置循环时间和循环姿势参数
        state.cycleOffset = 0;  // 循环偏移设为0
        state.cycleOffsetParameterActive = false;  // 不使用参数控制循环偏移
        state.timeParameterActive = false;  // 不使用参数控制时间
        state.mirrorParameterActive = false;  // 不使用参数控制镜像
        state.speedParameterActive = false;  // 不使用参数控制速度
    }
} 
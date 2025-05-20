using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
#endif

/// <summary>
/// 用于修改现有动画控制器的工具类
/// </summary>
public class AnimatorControllerModifier : MonoBehaviour
{
#if UNITY_EDITOR
    [Header("控制器输出路径")]
    [Tooltip("男性角色控制器输出路径")]
    public string manControllerOutputPath = "Assets/Citizens PRO/Animations/man_controller.controller";
    [Tooltip("女性角色控制器输出路径")]
    public string womanControllerOutputPath = "Assets/Citizens PRO/Animations/woman_controller.controller";
    [Tooltip("演讲者角色控制器输出路径")]
    public string presenterControllerOutputPath = "Assets/Citizens PRO/Animations/presenter_controller.controller";
    
    [Header("动画剪辑路径")]
    [Tooltip("男性角色动画文件夹（坐着）")]
    public string manAnimationFolderPath = "Assets/Citizens PRO/Animations_NoLegs/Man";
    [Tooltip("女性角色动画文件夹（坐着）")]
    public string womanAnimationFolderPath = "Assets/Citizens PRO/Animations_NoLegs/Girl no Heel";
    [Tooltip("演讲者动画文件夹（站立）")]
    public string presenterAnimationFolderPath = "Assets/Citizens PRO/Animations/Man";
    
    [Header("状态名称设置")]
    [Tooltip("走路动画状态名称")]
    public string walkStateName = "walk";
    [Tooltip("待机动画1状态名称")]
    public string idle1StateName = "idle1";
    [Tooltip("待机动画2状态名称")]
    public string idle2StateName = "idle2";
    [Tooltip("聆听动画状态名称")]
    public string listenStateName = "listen";
    [Tooltip("说话动画1状态名称")]
    public string talk1StateName = "talk1";
    [Tooltip("说话动画2状态名称")]
    public string talk2StateName = "talk2";
    [Tooltip("鼓掌动画状态名称")]
    public string clapStateName = "claphands";
    
    [Header("参数名称设置")]
    [Tooltip("是否正在走路")]
    public string isWalkingParam = "IsWalking";
    [Tooltip("是否正在待机")]
    public string isIdleParam = "IsIdle";
    [Tooltip("是否正在说话")]
    public string isTalkingParam = "IsTalking";
    [Tooltip("是否正在聆听")]
    public string isListeningParam = "IsListening";
    [Tooltip("是否正在鼓掌")]
    public string isClappingParam = "IsClapping";
    [Tooltip("说话变体选择(1或2)")]
    public string talkVariantParam = "TalkVariant";
    [Tooltip("待机变体选择(1或2)")]
    public string idleVariantParam = "IdleVariant";
    
    [ContextMenu("生成所有控制器")]
    public void GenerateAllControllers()
    {
        GenerateController(manAnimationFolderPath, manControllerOutputPath, "男性角色");
        GenerateController(womanAnimationFolderPath, womanControllerOutputPath, "女性角色");
        GenerateController(presenterAnimationFolderPath, presenterControllerOutputPath, "演讲者角色");
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log("所有控制器生成完成!");
    }
    
    private void GenerateController(string animationFolderPath, string outputPath, string controllerDesc)
    {
        if (string.IsNullOrEmpty(animationFolderPath) || string.IsNullOrEmpty(outputPath))
        {
            Debug.LogWarning($"未指定{controllerDesc}动画文件夹路径或输出路径");
            return;
        }
        
        // 确保输出目录存在
        string directory = Path.GetDirectoryName(outputPath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        
        // 创建新的动画控制器
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(outputPath);
        if (controller == null)
        {
            Debug.LogError($"无法创建{controllerDesc}控制器: {outputPath}");
            return;
        }
        
        Debug.Log($"开始生成{controllerDesc}控制器: {outputPath}");
        
        // 从文件夹加载所有动画剪辑
        Dictionary<string, AnimationClip> animationClips = LoadAnimationClipsFromFolder(animationFolderPath);
        
        if (animationClips.Count == 0)
        {
            Debug.LogError($"在文件夹 {animationFolderPath} 中未找到任何动画剪辑");
            return;
        }
        
        Debug.Log($"从文件夹加载了 {animationClips.Count} 个动画剪辑");
        
        // 仍然添加参数，以便将来可能的扩展或调试
        AddParameter(controller, idleVariantParam, AnimatorControllerParameterType.Int);
        AddParameter(controller, talkVariantParam, AnimatorControllerParameterType.Int);
        
        // 获取根层状态机
        AnimatorControllerLayer baseLayer = controller.layers[0];
        AnimatorStateMachine rootStateMachine = baseLayer.stateMachine;
        
        // 创建所有状态
        AnimatorState walkState = CreateStateWithClip(rootStateMachine, walkStateName, animationClips);
        AnimatorState idle1State = CreateStateWithClip(rootStateMachine, idle1StateName, animationClips);
        AnimatorState idle2State = CreateStateWithClip(rootStateMachine, idle2StateName, animationClips);
        AnimatorState listenState = CreateStateWithClip(rootStateMachine, listenStateName, animationClips);
        AnimatorState talk1State = CreateStateWithClip(rootStateMachine, talk1StateName, animationClips);
        AnimatorState talk2State = CreateStateWithClip(rootStateMachine, talk2StateName, animationClips);
        AnimatorState clapState = CreateStateWithClip(rootStateMachine, clapStateName, animationClips);
        
        // 设置默认状态（为了在编辑器中有一个初始状态）
        rootStateMachine.defaultState = idle1State;
        
        // 不再添加状态之间的转换，因为我们使用CrossFade直接控制状态
        
        EditorUtility.SetDirty(controller);
        Debug.Log($"成功生成{controllerDesc}控制器 - 仅包含状态，不包含转换关系");
    }
    
    private Dictionary<string, AnimationClip> LoadAnimationClipsFromFolder(string folderPath)
    {
        Dictionary<string, AnimationClip> clips = new Dictionary<string, AnimationClip>();

        string pattern = "*.anim";
        if (!folderPath.Contains("Legs"))
            pattern = "*.fbx";
        // 获取指定文件夹中的所有FBX文件
        string[] fbxPaths = Directory.GetFiles(folderPath, pattern, SearchOption.TopDirectoryOnly);
        Debug.Log($"在 {folderPath} 中找到 {fbxPaths.Length} 个FBX文件");
        
        foreach (string fbxPath in fbxPaths)
        {
            string assetPath = fbxPath.Replace('\\', '/');
            if (!assetPath.StartsWith("Assets/"))
            {
                // 将物理路径转换为Unity资源路径
                string projectPath = Application.dataPath;
                string relativePath = assetPath.Replace(projectPath.Substring(0, projectPath.Length - 7), "");
                assetPath = relativePath;
            }
            
            // 从FBX文件获取动画剪辑
            AnimationClip[] fbxClips = AssetDatabase.LoadAllAssetsAtPath(assetPath)
                .OfType<AnimationClip>()
                .ToArray();
            
            if (fbxClips.Length > 0)
            {
                // 获取文件名(不含扩展名)作为关键字
                string fileName = Path.GetFileNameWithoutExtension(fbxPath).ToLower();
                
                // 保存动画剪辑到字典，同时处理f_前缀问题
                foreach (AnimationClip clip in fbxClips)
                {
                    // 尝试添加原始文件名
                    if (!clips.ContainsKey(fileName))
                    {
                        clips.Add(fileName, clip);
                        Debug.Log($"已加载动画剪辑: {fileName} 从文件 {assetPath}");
                    }
                    
                    string f_fileName = "f_" + fileName;
                    if (!clips.ContainsKey(f_fileName))
                    {
                        clips.Add(f_fileName, clip);
                        Debug.Log($"已为 {fileName} 添加女性变体: {f_fileName}");
                    }
                }
            }
        }
        
        return clips;
    }
    
    private void AddParameter(AnimatorController controller, string paramName, AnimatorControllerParameterType paramType)
    {
        // 检查参数是否已存在
        if (controller.parameters.Any(p => p.name == paramName))
        {
            Debug.Log($"参数 {paramName} 已存在");
            return;
        }
        
        // 添加参数
        controller.AddParameter(paramName, paramType);
        Debug.Log($"添加参数: {paramName}");
    }
    
    private AnimatorState CreateStateWithClip(AnimatorStateMachine stateMachine, string stateName, Dictionary<string, AnimationClip> clips)
    {
        // 创建新状态
        AnimatorState newState = stateMachine.AddState(stateName);
        
        // 尝试查找匹配的动画剪辑
        AnimationClip matchingClip = null;
        if (clips.ContainsKey(stateName.ToLower()))
        {
            matchingClip = clips[stateName.ToLower()];
        }
        
        // 如果找到匹配的剪辑，设置为状态的动画
        if (matchingClip != null)
        {
            newState.motion = matchingClip;
            Debug.Log($"为状态 {stateName} 设置动画剪辑: {matchingClip.name}");
        }
        else
        {
            Debug.LogWarning($"创建状态 {stateName} 但未找到匹配的动画剪辑");
        }
        
        return newState;
    }
    
    // 辅助类，用于存储转换条件
    private class TransitionCondition
    {
        public string paramName;
        public AnimatorConditionMode mode;
        public float threshold;
        
        public TransitionCondition(string paramName, AnimatorConditionMode mode, float threshold)
        {
            this.paramName = paramName;
            this.mode = mode;
            this.threshold = threshold;
        }
    }
    
    private void CreateTransition(AnimatorState sourceState, AnimatorState destinationState, TransitionCondition[] conditions)
    {
        AnimatorStateTransition newTransition = sourceState.AddTransition(destinationState);
        newTransition.hasExitTime = false;
        newTransition.duration = 0.25f;
        
        // 添加条件
        foreach (TransitionCondition condition in conditions)
        {
            newTransition.AddCondition(condition.mode, condition.threshold, condition.paramName);
        }
        
        Debug.Log($"创建从 {sourceState.name} 到 {destinationState.name} 的转换");
    }
    
    [MenuItem("工具/角色工具/生成角色动画控制器")]
    private static void GenerateAnimationControllers()
    {
        GameObject tempObject = new GameObject("TempAnimatorControllerModifier");
        AnimatorControllerModifier modifier = tempObject.AddComponent<AnimatorControllerModifier>();
        
        modifier.GenerateAllControllers();
        
        DestroyImmediate(tempObject);
        
        EditorUtility.DisplayDialog("完成", "已生成所有动画控制器", "确定");
    }
#endif
} 
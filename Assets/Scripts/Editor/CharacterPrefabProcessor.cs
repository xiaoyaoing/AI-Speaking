using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using UnityEditor.Animations;

public class CharacterPrefabProcessor : EditorWindow
{
    private string prefabPath = "Assets/Prefabs";
    private Vector2 scrollPosition;

    [MenuItem("工具/角色预制体处理")]
    public static void ShowWindow()
    {
        GetWindow<CharacterPrefabProcessor>("角色预制体处理");
    }

    private void OnGUI()
    {
        GUILayout.Label("角色预制体处理工具", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        prefabPath = EditorGUILayout.TextField("预制体路径", prefabPath);

        EditorGUILayout.Space();
        if (GUILayout.Button("处理所有预制体"))
        {
            ProcessAllPrefabs();
        }
    }

    private void ProcessAllPrefabs()
    {
        // 获取所有预制体
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { prefabPath });
        int total = prefabGuids.Length;
        int processed = 0;

        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab != null && IsCharacterPrefab(prefab))
            {
                // 显示进度
                float progress = (float)processed / total;
                if (EditorUtility.DisplayCancelableProgressBar("处理预制体", 
                    $"正在处理: {prefab.name} ({processed + 1}/{total})", progress))
                {
                    break;
                }

                ProcessPrefab(prefab, path);
                processed++;
            }
        }

        EditorUtility.ClearProgressBar();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("完成", $"已处理 {processed} 个预制体", "确定");
    }

    private bool IsCharacterPrefab(GameObject prefab)
    {
        string name = prefab.name.ToLower();
        return name.Contains("man") || name.Contains("girlwithheel") || name.Contains("girlnoheel");
    }

    private void ProcessPrefab(GameObject prefab, string path)
    {
        // 创建预制体实例
        GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        
        // 添加或获取Animator组件
        Animator animator = instance.GetComponent<Animator>();
        if (animator == null)
        {
            animator = instance.AddComponent<Animator>();
        }

        // 添加角色注册组件
        CharacterRegister register = instance.GetComponent<CharacterRegister>();
        if (register == null)
        {
            register = instance.AddComponent<CharacterRegister>();
        }

        // 创建或获取动画控制器
        string controllerPath = Path.Combine(Path.GetDirectoryName(path), 
            $"{prefab.name}_Animator.controller");
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);

        if (controller == null)
        {
            // 创建新的动画控制器
            controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            
            // 添加动画状态
            AddAnimationStates(controller, prefab.name);
        }

        // 设置动画控制器
        animator.runtimeAnimatorController = controller;

        // 保存预制体
        PrefabUtility.SaveAsPrefabAsset(instance, path);
        DestroyImmediate(instance);
    }

    private void AddAnimationStates(AnimatorController controller, string prefabName)
    {
        // 获取动画剪辑
        string[] animationGuids = AssetDatabase.FindAssets("t:AnimationClip", new[] { "Assets/Animations" });
        string prefix = GetAnimationPrefix(prefabName);

        // 创建默认状态
        var rootStateMachine = controller.layers[0].stateMachine;
        var defaultState = rootStateMachine.AddState("Default");
        defaultState.writeDefaultValues = false;

        foreach (string guid in animationGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            
            if (clip.name.StartsWith(prefix))
            {
                // 添加动画状态
                var state = rootStateMachine.AddState(clip.name);
                state.motion = clip;

                // 如果是listen动画，设置为默认状态
                if (clip.name.EndsWith("listen"))
                {
                    rootStateMachine.defaultState = state;
                }
            }
        }
    }

    private string GetAnimationPrefix(string prefabName)
    {
        string name = prefabName.ToLower();
        if (name.Contains("man")) return "m_";
        if (name.Contains("girlwithheel")) return "f_heel_";
        if (name.Contains("girlnoheel")) return "f_noheel_";
        return "";
    }
} 
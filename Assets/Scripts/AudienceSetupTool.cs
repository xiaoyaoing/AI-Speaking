using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class AudienceSetupTool : EditorWindow
{
    [MenuItem("工具/听众设置/自动设置听众")]
    public static void ShowWindow()
    {
        GetWindow<AudienceSetupTool>("听众设置工具");
    }

    private void OnGUI()
    {
        GUILayout.Label("听众设置工具", EditorStyles.boldLabel);

        EditorGUILayout.Space();
        if (GUILayout.Button("为所有角色添加听众控制器"))
        {
            SetupAllCharacters();
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("移除所有听众控制器"))
        {
            RemoveAllControllers();
        }
    }

    private void SetupAllCharacters()
    {
        // 获取场景中的所有对象
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
        int processedCount = 0;
        int totalCharacters = 0;

        // 先计算总角色数
        foreach (GameObject obj in allObjects)
        {
            if (IsCharacter(obj))
            {
                totalCharacters++;
            }
        }

        // 处理每个角色
        foreach (GameObject obj in allObjects)
        {
            if (IsCharacter(obj))
            {
                // 显示进度
                float progress = (float)processedCount / totalCharacters;
                if (EditorUtility.DisplayCancelableProgressBar("设置听众", 
                    $"正在处理: {obj.name} ({processedCount + 1}/{totalCharacters})", progress))
                {
                    break;
                }

                SetupCharacter(obj);
                processedCount++;
            }
        }

        // 清除进度条
        EditorUtility.ClearProgressBar();
        EditorUtility.DisplayDialog("完成", $"已处理 {processedCount} 个角色", "确定");
    }

    private void RemoveAllControllers()
    {
        // 获取场景中的所有对象
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
        int removedCount = 0;

        foreach (GameObject obj in allObjects)
        {
            AudienceAnimationController controller = obj.GetComponent<AudienceAnimationController>();
            if (controller != null)
            {
                DestroyImmediate(controller);
                removedCount++;
            }
        }

        EditorUtility.DisplayDialog("完成", $"已移除 {removedCount} 个听众控制器", "确定");
    }

    private bool IsCharacter(GameObject obj)
    {
        // 检查是否是角色（通过名称或标签判断）
        string lowerName = obj.name.ToLower();
        return (lowerName.Contains("male") || lowerName.Contains("female") || 
                lowerName.Contains("character") || lowerName.Contains("person")) &&
               obj.GetComponent<Animator>() != null;
    }

    private void SetupCharacter(GameObject character)
    {
        // 检查是否已经有控制器
        AudienceAnimationController controller = character.GetComponent<AudienceAnimationController>();
        if (controller == null)
        {
            controller = character.AddComponent<AudienceAnimationController>();
        }

        // 设置动画名称
        bool isMale = character.name.ToLower().Contains("male");
        AudienceAnimationController.AnimationSet animations = isMale ? controller.maleAnimations : controller.femaleAnimations;

        // 设置动画名称
        string prefix = isMale ? "m_" : "f_";
        animations.listen1Animation = prefix + "listen1";
        animations.listen2Animation = prefix + "listen2";
        animations.talkAnimation = prefix + "talk";
        animations.clapAnimation = prefix + "clap";

        // 设置其他参数
        controller.listen1Probability = 0.5f;
        controller.stateChangeInterval = 5f;
        controller.minStateDuration = 3f;
        controller.maxStateDuration = 8f;

        // 标记为已修改
        EditorUtility.SetDirty(character);
    }
} 
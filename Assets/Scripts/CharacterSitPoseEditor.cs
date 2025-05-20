using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class CharacterSitPoseEditor : EditorWindow
{
    [Header("参考设置")]
    [Tooltip("参考人物预制体（已调整好姿势的）")]
    public GameObject referencePrefab;
    [Tooltip("保存姿势的路径")]
    public string savePath = "Assets/Animations/Poses";

    private Dictionary<string, (Vector3 position, Quaternion rotation)> referencePose = new Dictionary<string, (Vector3, Quaternion)>();

    [MenuItem("工具/人物处理/复制坐下姿势")]
    public static void ShowWindow()
    {
        GetWindow<CharacterSitPoseEditor>("复制坐下姿势");
    }

    private void OnGUI()
    {
        GUILayout.Label("复制人物坐下姿势", EditorStyles.boldLabel);

        EditorGUILayout.Space();
        referencePrefab = (GameObject)EditorGUILayout.ObjectField("参考人物预制体", referencePrefab, typeof(GameObject), false);
        
        EditorGUILayout.Space();
        savePath = EditorGUILayout.TextField("保存路径", savePath);

        EditorGUILayout.Space();
        if (GUILayout.Button("保存参考姿势"))
        {
            SaveReferencePose();
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("应用姿势到所有人物"))
        {
            ApplyPoseToAllCharacters();
        }
    }

    private void SaveReferencePose()
    {
        if (referencePrefab == null)
        {
            EditorUtility.DisplayDialog("错误", "请设置参考人物预制体", "确定");
            return;
        }

        // 创建参考预制体实例
        GameObject instance = PrefabUtility.InstantiatePrefab(referencePrefab) as GameObject;
        
        // 查找骨骼
        Transform bip01 = FindTransformRecursive(instance.transform, "Bip01");
        if (bip01 == null)
        {
            Debug.LogWarning($"参考预制体 {referencePrefab.name} 没有找到Bip01骨骼");
            DestroyImmediate(instance);
            return;
        }

        // 保存所有骨骼的姿势
        SaveBonePoses(bip01);

        // 销毁实例
        DestroyImmediate(instance);

        EditorUtility.DisplayDialog("完成", "已保存参考姿势", "确定");
    }

    private void SaveBonePoses(Transform bone)
    {
        // 保存当前骨骼的姿势
        referencePose[bone.name] = (bone.localPosition, bone.localRotation);

        // 递归保存所有子骨骼
        foreach (Transform child in bone)
        {
            SaveBonePoses(child);
        }
    }

    private void ApplyPoseToAllCharacters()
    {
        if (referencePose.Count == 0)
        {
            EditorUtility.DisplayDialog("错误", "请先保存参考姿势", "确定");
            return;
        }

        // 获取所有预制体
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
        int processedCount = 0;

        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            // 检查是否是人物预制体（排除参考预制体）
            if (IsCharacterPrefab(prefab) && prefab != referencePrefab)
            {
                // 显示进度
                float progress = (float)processedCount / prefabGuids.Length;
                if (EditorUtility.DisplayCancelableProgressBar("处理预制体", 
                    $"正在处理: {prefab.name} ({processedCount + 1}/{prefabGuids.Length})", progress))
                {
                    break;
                }

                // 处理预制体
                ApplyPoseToPrefab(prefab, path);
                processedCount++;
            }
        }

        // 清除进度条
        EditorUtility.ClearProgressBar();
        EditorUtility.DisplayDialog("完成", $"已处理 {processedCount} 个人物预制体", "确定");
    }

    private bool IsCharacterPrefab(GameObject prefab)
    {
        if (prefab == null) return false;

        // 检查是否包含人物相关的组件或标签
        return prefab.GetComponent<Animator>() != null ||
               prefab.name.ToLower().Contains("male") ||
               prefab.name.ToLower().Contains("female") ||
               prefab.name.ToLower().Contains("character") ||
               prefab.name.ToLower().Contains("person");
    }

    private void ApplyPoseToPrefab(GameObject prefab, string path)
    {
        // 创建预制体实例
        GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        
        // 查找骨骼
        Transform bip01 = FindTransformRecursive(instance.transform, "Bip01");
        if (bip01 == null)
        {
            Debug.LogWarning($"预制体 {prefab.name} 没有找到Bip01骨骼");
            DestroyImmediate(instance);
            return;
        }

        // 应用保存的姿势
        ApplySavedPose(bip01);

        // 保存预制体
        PrefabUtility.SaveAsPrefabAsset(instance, path);
        DestroyImmediate(instance);
    }

    private void ApplySavedPose(Transform bone)
    {
        // 如果保存了这个骨骼的姿势，就应用它
        if (referencePose.TryGetValue(bone.name, out var pose))
        {
            bone.localPosition = pose.position;
            bone.localRotation = pose.rotation;
        }

        // 递归应用到所有子骨骼
        foreach (Transform child in bone)
        {
            ApplySavedPose(child);
        }
    }

    private Transform FindTransformRecursive(Transform parent, string name)
    {
        if (parent.name == name)
            return parent;

        foreach (Transform child in parent)
        {
            Transform result = FindTransformRecursive(child, name);
            if (result != null)
                return result;
        }

        return null;
    }
} 
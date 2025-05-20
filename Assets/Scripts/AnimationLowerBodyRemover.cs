using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using System.Linq;
using System.Text.RegularExpressions;

public class AnimationLowerBodyRemover : EditorWindow
{
    private string sourcePath = "Assets/Citizens PRO/Animations";
    private string targetPath = "Assets/Citizens PRO/Animations_NoLegs";
    private List<string> lowerBodyBones = new List<string>
    {
        "Bip01 L Thigh",
        "Bip01 R Thigh",
        "Bip01 L Calf",
        "Bip01 R Calf",
        "Bip01 L Foot",
        "Bip01 R Foot",
        "Bip01 L Toe0",
        "Bip01 R Toe0"
    };

    private List<string> targetAnimations = new List<string>
    {
        "talk",
        "clap",
        "listen"
    };

    [MenuItem("工具/动画处理/去除腿部动画")]
    public static void ShowWindow()
    {
        GetWindow<AnimationLowerBodyRemover>("去除腿部动画");
    }

    private void OnGUI()
    {
        GUILayout.Label("动画处理设置", EditorStyles.boldLabel);

        EditorGUILayout.Space();
        sourcePath = EditorGUILayout.TextField("源动画目录", sourcePath);
        targetPath = EditorGUILayout.TextField("目标保存目录", targetPath);

        EditorGUILayout.Space();
        if (GUILayout.Button("处理所有动画"))
        {
            ProcessAllAnimations();
        }
    }

    private void ProcessAllAnimations()
    {
        // 确保目标目录存在
        if (!Directory.Exists(targetPath))
        {
            Directory.CreateDirectory(targetPath);
            AssetDatabase.Refresh();
        }

        // 获取所有FBX文件（包括子目录）
        List<string> allFbxFiles = new List<string>();
        GetAllFbxFiles(sourcePath, allFbxFiles);

        int totalFiles = allFbxFiles.Count;
        int processedFiles = 0;

        foreach (string fbxFile in allFbxFiles)
        {
            // 显示进度
            float progress = (float)processedFiles / totalFiles;
            if (EditorUtility.DisplayCancelableProgressBar("处理动画", 
                $"正在处理: {Path.GetFileName(fbxFile)} ({processedFiles + 1}/{totalFiles})", progress))
            {
                break;
            }

            // 处理FBX文件中的所有动画
            ProcessFbxFile(fbxFile);
            processedFiles++;
        }

        // 清除进度条
        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("完成", $"已处理 {processedFiles} 个FBX文件", "确定");
    }

    private void GetAllFbxFiles(string directory, List<string> fbxFiles)
    {
        // 获取当前目录下的所有FBX文件
        string[] files = Directory.GetFiles(directory, "*.fbx");
        fbxFiles.AddRange(files);

        // 递归处理所有子目录
        string[] subdirectories = Directory.GetDirectories(directory);
        foreach (string subdir in subdirectories)
        {
            GetAllFbxFiles(subdir, fbxFiles);
        }
    }

    private void ProcessFbxFile(string fbxPath)
    {
        // 加载FBX文件
        GameObject fbxObject = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
        if (fbxObject == null)
        {
            Debug.LogWarning($"无法加载FBX文件: {fbxPath}");
            return;
        }

        // 获取FBX中的所有动画剪辑
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(fbxPath);
        foreach (Object asset in assets)
        {
            if (asset is AnimationClip clip && !clip.name.Contains("__preview__"))
            {
                // 检查是否是目标动画
                string clipName = clip.name.ToLower();
                bool isTargetAnimation = targetAnimations.Any(target => clipName.Contains(target));

                isTargetAnimation = true;
                
                if (isTargetAnimation)
                {
                    // 创建目标文件路径
                    string relativePath = Path.GetDirectoryName(fbxPath).Substring(sourcePath.Length);
                    string targetDir = Path.Combine(targetPath, relativePath.TrimStart('\\', '/'));
                    
                    // 确保目标目录存在
                    if (!Directory.Exists(targetDir))
                    {
                        Directory.CreateDirectory(targetDir);
                    }

                    // 处理动画剪辑
                    ProcessAnimationClip(clip, targetDir);
                }
            }
        }
    }

    private void ProcessAnimationClip(AnimationClip sourceClip, string targetDir)
    {
        try
        {
            // 创建新的动画剪辑
            AnimationClip newClip = new AnimationClip();
            newClip.name = sourceClip.name;
            
            if(newClip.name.Contains("f_"))
                newClip.name = newClip.name.Replace("f_", "");
            if(newClip.name.Contains("_f"))
                newClip.name = newClip.name.Replace("_f", "");

            // 复制所有曲线
            EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(sourceClip);
            foreach (EditorCurveBinding binding in bindings)
            {
                // 检查是否是腿部骨骼的动画
                bool isLowerBody = false;
                foreach (string boneName in lowerBodyBones)
                {
                    if (binding.path.Contains(boneName))
                    {
                        isLowerBody = true;
                        break;
                    }
                }

                // 检查是否是Bip01根节点的rotation或position
                bool isBip01Root = binding.path == "Bip01" && 
                    (binding.propertyName.Contains("Rotation") || binding.propertyName.Contains("Position"));

                // 如果不是腿部骨骼的动画且不是Bip01根节点的rotation/position，则复制曲线
                if (!isLowerBody && !isBip01Root)
                {
                    AnimationCurve curve = AnimationUtility.GetEditorCurve(sourceClip, binding);
                    AnimationUtility.SetEditorCurve(newClip, binding, curve);
                }
            }

            // 处理文件名中的非法字符
            string safeFileName = Regex.Replace(newClip.name, @"[^\w\.-]", "_");
            
            // 确保路径以Assets开头
            string targetPath = targetDir;
            if (!targetPath.StartsWith("Assets"))
            {
                targetPath = Path.Combine("Assets", targetPath.TrimStart('\\', '/'));
            }
            targetPath = Path.Combine(targetPath, $"{safeFileName}.anim");

            // 确保目标目录存在
            string targetDirPath = Path.GetDirectoryName(targetPath);
            if (!Directory.Exists(targetDirPath))
            {
                Directory.CreateDirectory(targetDirPath);
            }

            // 保存新的动画剪辑
            AssetDatabase.CreateAsset(newClip, targetPath);
            Debug.Log($"成功保存动画: {targetPath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"处理动画 {sourceClip.name} 时出错: {e.Message}");
        }
    }
} 
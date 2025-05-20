using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

[CustomEditor(typeof(CharacterPlacementManager))]
public class CharacterPlacementManagerEditor : Editor
{
    // 预制体路径
    private const string MaleSummerPath = "Assets/Citizens PRO/People Prefabs/Male/Summer";
    private const string MaleAutumnPath = "Assets/Citizens PRO/People Prefabs/Male/Winter";
    private const string FemaleSummerPath = "Assets/Citizens PRO/People Prefabs/Female/Summer";
    private const string FemaleAutumnPath = "Assets/Citizens PRO/People Prefabs/Female/Winter";
    
    public override void OnInspectorGUI()
    {
        CharacterPlacementManager manager = (CharacterPlacementManager)target;
        
        // 绘制默认检查器
        DrawDefaultInspector();
        
        EditorGUILayout.Space(10);
        
        // 绘制自动填充按钮
        if (GUILayout.Button("自动填充所有人物预制体"))
        {
            List<GameObject> maleSummer = LoadPrefabsFromFolder(MaleSummerPath);
            List<GameObject> maleAutumn = LoadPrefabsFromFolder(MaleAutumnPath);
            List<GameObject> femaleSummer = LoadPrefabsFromFolder(FemaleSummerPath);
            List<GameObject> femaleAutumn = LoadPrefabsFromFolder(FemaleAutumnPath);
            
            // 使用反射设置私有字段
            manager.SetPresets(maleSummer, maleAutumn, femaleSummer, femaleAutumn);
            
            // 标记为脏，确保Unity保存更改
            EditorUtility.SetDirty(manager);
            
            Debug.Log("已自动填充所有人物预制体引用");
        }
        
        EditorGUILayout.Space(5);
        
        // 分别填充不同类型的预制体
        if (GUILayout.Button("仅填充男性夏装预制体"))
        {
            List<GameObject> prefabs = LoadPrefabsFromFolder(MaleSummerPath);
            manager.SetPresets(prefabs, null, null, null);
            EditorUtility.SetDirty(manager);
        }
        
        if (GUILayout.Button("仅填充男性秋装预制体"))
        {
            List<GameObject> prefabs = LoadPrefabsFromFolder(MaleAutumnPath);
            manager.SetPresets(null, prefabs, null, null);
            EditorUtility.SetDirty(manager);
        }
        
        if (GUILayout.Button("仅填充女性夏装预制体"))
        {
            List<GameObject> prefabs = LoadPrefabsFromFolder(FemaleSummerPath);
            manager.SetPresets(null, null, prefabs, null);
            EditorUtility.SetDirty(manager);
        }
        
        if (GUILayout.Button("仅填充女性秋装预制体"))
        {
            List<GameObject> prefabs = LoadPrefabsFromFolder(FemaleAutumnPath);
            manager.SetPresets(null, null, null, prefabs);
            EditorUtility.SetDirty(manager);
        }
    }
    
    private List<GameObject> LoadPrefabsFromFolder(string folderPath)
    {
        List<GameObject> prefabs = new List<GameObject>();
        
        if (!Directory.Exists(folderPath))
        {
            Debug.LogWarning($"路径不存在: {folderPath}");
            return prefabs;
        }
        
        string[] prefabFiles = Directory.GetFiles(folderPath, "*.prefab");
        foreach (string prefabFile in prefabFiles)
        {
            string assetPath = prefabFile.Replace('\\', '/').Replace(Application.dataPath, "Assets");
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab != null)
            {
                prefabs.Add(prefab);
            }
        }
        
        return prefabs;
    }
} 
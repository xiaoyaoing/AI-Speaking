using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// 编辑器窗口，用于在场景中的椅子上随机放置人物
/// </summary>
public class CharacterPlacementEditor : EditorWindow
{
    // 人物覆盖率（0-1之间）
    private float coverageRate = 0.5f;
    
    // 人物装扮选择
    private int clothingTypeIndex = 0;
    private readonly string[] clothingTypes = new string[] { "夏装", "秋装" };
    
    // 人物性别比例
    private float maleRatio = 0.5f;
    
    // 椅子和人物预制体
    private List<GameObject> chairPrefabs = new List<GameObject>();
    private List<GameObject> maleSummerPrefabs = new List<GameObject>();
    private List<GameObject> maleAutumnPrefabs = new List<GameObject>();
    private List<GameObject> femaleSummerPrefabs = new List<GameObject>();
    private List<GameObject> femaleAutumnPrefabs = new List<GameObject>();
    
    // 已放置的人物
    private List<GameObject> placedCharacters = new List<GameObject>();
    
    // 角色管理器
    private GameObject characterManagerObject;
    
    [MenuItem("工具/人物椅子放置工具")]
    public static void ShowWindow()
    {
        GetWindow<CharacterPlacementEditor>("人物椅子放置工具");
    }
    
    private void OnEnable()
    {
        // 加载椅子预制体
        LoadChairPrefabs();
        
        // 加载人物预制体
        LoadCharacterPrefabs();
        
        // 查找或创建角色管理器
        FindOrCreateCharacterManager();
    }
    
    private void FindOrCreateCharacterManager()
    {
        // 查找场景中是否已有角色管理器
        RuntimeCharacterManager existingManager = GameObject.FindObjectOfType<RuntimeCharacterManager>();
        if (existingManager != null)
        {
            characterManagerObject = existingManager.gameObject;
        }
    }
    
    private void LoadChairPrefabs()
    {
        chairPrefabs.Clear();
        
        // 加载主要的椅子预制体
        GameObject chairPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Conference Room/Prefabs/Chair.prefab");
        if (chairPrefab != null)
        {
            chairPrefabs.Add(chairPrefab);
        }
        
        // 加载Chairs 1-4预制体
        for (int i = 1; i <= 4; i++)
        {
            GameObject chairsGroupPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Conference Room/Prefabs/Chairs {i}.prefab");
            if (chairsGroupPrefab != null)
            {
                chairPrefabs.Add(chairsGroupPrefab);
            }
        }
    }
    
    private void LoadCharacterPrefabs()
    {
        maleSummerPrefabs.Clear();
        maleAutumnPrefabs.Clear();
        femaleSummerPrefabs.Clear();
        femaleAutumnPrefabs.Clear();
        
        // 加载夏装男性预制体
        string maleSummerPath = "Assets/Citizens PRO/People Prefabs/Male/Summer";
        LoadPrefabsFromFolder(maleSummerPath, maleSummerPrefabs);
        
        // 加载秋装(Winter)男性预制体
        string maleAutumnPath = "Assets/Citizens PRO/People Prefabs/Male/Winter";
        LoadPrefabsFromFolder(maleAutumnPath, maleAutumnPrefabs);
        
        // 加载夏装女性预制体
        string femaleSummerPath = "Assets/Citizens PRO/People Prefabs/Female/Summer";
        LoadPrefabsFromFolder(femaleSummerPath, femaleSummerPrefabs);
        
        // 加载秋装(Winter)女性预制体
        string femaleAutumnPath = "Assets/Citizens PRO/People Prefabs/Female/Winter";
        LoadPrefabsFromFolder(femaleAutumnPath, femaleAutumnPrefabs);
    }
    
    private void LoadPrefabsFromFolder(string folderPath, List<GameObject> prefabsList)
    {
        if (!Directory.Exists(folderPath))
        {
            Debug.LogWarning($"路径不存在: {folderPath}");
            return;
        }
        
        string[] prefabFiles = Directory.GetFiles(folderPath, "*.prefab");
        foreach (string prefabFile in prefabFiles)
        {
            string assetPath = prefabFile.Replace('\\', '/').Replace(Application.dataPath, "Assets");
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab != null)
            {
                prefabsList.Add(prefab);
            }
        }
    }
    
    private void OnGUI()
    {
        GUILayout.Label("人物椅子放置设置", EditorStyles.boldLabel);
        
        // 人物覆盖率滑动条
        coverageRate = EditorGUILayout.Slider("人物覆盖率", coverageRate, 0f, 1f);
        
        // 人物装扮选择
        clothingTypeIndex = EditorGUILayout.Popup("人物装扮", clothingTypeIndex, clothingTypes);
        
        // 性别比例滑动条
        maleRatio = EditorGUILayout.Slider("男性比例", maleRatio, 0f, 1f);
        
        // 预制体状态信息
        DisplayPrefabsInfo();
        
        EditorGUILayout.Space(10);
        
        // 操作按钮
        GUILayout.BeginHorizontal();
        
        if (GUILayout.Button("放置人物"))
        {
            PlaceCharactersOnChairs();
        }
        
        if (GUILayout.Button("清除已放置人物"))
        {
            ClearPlacedCharacters();
        }
        
        GUILayout.EndHorizontal();
        
        // 显示角色管理器状态
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("角色管理器状态：", EditorStyles.boldLabel);
        
        if (characterManagerObject != null)
        {
            EditorGUILayout.LabelField($"已找到角色管理器: {characterManagerObject.name}");
        }
        else
        {
            EditorGUILayout.HelpBox("场景中未找到角色管理器，将在放置人物时自动创建", MessageType.Info);
        }
    }
    
    private void DisplayPrefabsInfo()
    {
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("预制体状态：", EditorStyles.boldLabel);
        
        EditorGUILayout.LabelField($"椅子预制体: {chairPrefabs.Count}个");
        EditorGUILayout.LabelField($"男性夏装预制体: {maleSummerPrefabs.Count}个");
        EditorGUILayout.LabelField($"男性秋装预制体: {maleAutumnPrefabs.Count}个");
        EditorGUILayout.LabelField($"女性夏装预制体: {femaleSummerPrefabs.Count}个");
        EditorGUILayout.LabelField($"女性秋装预制体: {femaleAutumnPrefabs.Count}个");
        EditorGUILayout.LabelField($"已放置人物: {placedCharacters.Count}个");
    }
    
    private void PlaceCharactersOnChairs()
    {
        // 确保有角色管理器
        if (characterManagerObject == null)
        {
            CreateCharacterManager();
        }
        
        // 清除之前放置的人物
        ClearPlacedCharacters();
        
        // 获取场景中的所有椅子对象
        List<GameObject> sceneChairs = FindChairsInScene();
        
        if (sceneChairs.Count == 0)
        {
            EditorUtility.DisplayDialog("错误", "场景中没有找到椅子对象", "确定");
            return;
        }
        
        // 计算要放置人物的椅子数量
        int chairsToFill = Mathf.RoundToInt(sceneChairs.Count * coverageRate);
        
        // 随机打乱椅子列表
        System.Random random = new System.Random();
        sceneChairs = sceneChairs.OrderBy(x => random.Next()).ToList();
        
        // 确定要使用的人物预制体列表
        List<GameObject> malePrefabs = clothingTypeIndex == 0 ? maleSummerPrefabs : maleAutumnPrefabs;
        List<GameObject> femalePrefabs = clothingTypeIndex == 0 ? femaleSummerPrefabs : femaleAutumnPrefabs;
        
        if (malePrefabs.Count == 0 || femalePrefabs.Count == 0)
        {
            EditorUtility.DisplayDialog("错误", "未加载到人物预制体", "确定");
            return;
        }
        
        // 记录Undo操作
        Undo.RegisterFullObjectHierarchyUndo(this, "Place Characters");
        
        // 放置人物到椅子上
        for (int i = 0; i < chairsToFill && i < sceneChairs.Count; i++)
        {
            GameObject chair = sceneChairs[i];
            
            // 根据性别比例决定放置男性还是女性
            float randomValue = Random.value;
            bool isMale = randomValue < maleRatio;
            
            // 获取人物预制体
            List<GameObject> prefabsToUse = isMale ? malePrefabs : femalePrefabs;
            GameObject characterPrefab = prefabsToUse[Random.Range(0, prefabsToUse.Count)];
            
            // 实例化人物到椅子上方
            GameObject character = PrefabUtility.InstantiatePrefab(characterPrefab) as GameObject;
            character.transform.position = chair.transform.position + new Vector3(0, -0.2f, 0); // 稍微向上偏移防止穿模
            character.transform.rotation = chair.transform.rotation;
            
            // 添加到已放置列表
            placedCharacters.Add(character);
            
            // 记录用于撤销
            Undo.RegisterCreatedObjectUndo(character, "Place Character");
        }
        
        // 设置所有放置的角色的动画
        RegisterAllPlacedCharacters();
        
        Debug.Log($"已在椅子上放置 {placedCharacters.Count} 个人物并设置动画状态");
    }
    
    private void RegisterAllPlacedCharacters()
    {
        // 确保有角色管理器
        if (characterManagerObject == null)
        {
            Debug.LogError("找不到角色管理器，无法设置动画状态！");
            return;
        }
        
        // 获取RuntimeCharacterManager组件
        RuntimeCharacterManager manager = characterManagerObject.GetComponent<RuntimeCharacterManager>();
        if (manager == null)
        {
            Debug.LogError("角色管理器对象缺少RuntimeCharacterManager组件！");
            return;
        }
        
        // 注册所有放置的角色
        foreach (GameObject character in placedCharacters)
        {
            if (character != null)
            {
                // 注册角色并设置动画
                manager.RegisterCharacter(character);
            }
        }
    }
    
    private void CreateCharacterManager()
    {
        GameObject managerObj = new GameObject("CharacterManager");
        managerObj.AddComponent<RuntimeCharacterManager>();
        Undo.RegisterCreatedObjectUndo(managerObj, "Create Character Manager");
        characterManagerObject = managerObj;
        
        Debug.Log("已创建角色管理器对象");
    }
    
    private List<GameObject> FindChairsInScene()
    {
        List<GameObject> result = new List<GameObject>();
        
        // 查找名称为"Chair"的游戏对象
        GameObject[] chairs = GameObject.FindGameObjectsWithTag("Untagged");
        foreach (GameObject obj in chairs)
        {
            if (obj.name.Contains("Chair"))
            {
                result.Add(obj);
            }
        }
        
        // 如果没有找到椅子，查找所有mesh包含椅子模型的对象
        if (result.Count == 0)
        {
            foreach (MeshFilter meshFilter in FindObjectsOfType<MeshFilter>())
            {
                if (meshFilter.sharedMesh != null && meshFilter.sharedMesh.name.Contains("Chair"))
                {
                    result.Add(meshFilter.gameObject);
                }
            }
        }
        
        return result;
    }
    
    private void ClearPlacedCharacters()
    {
        // 记录Undo操作
        Undo.RecordObjects(placedCharacters.ToArray(), "Clear Characters");
        
        // 删除所有已放置的人物
        foreach (GameObject character in placedCharacters)
        {
            if (character != null)
            {
                Undo.DestroyObjectImmediate(character);
            }
        }
        
        placedCharacters.Clear();
        Debug.Log("已清除所有放置的人物");
    }
} 
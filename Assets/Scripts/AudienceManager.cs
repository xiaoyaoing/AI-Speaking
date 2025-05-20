using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 观众管理器 - 控制学术汇报中的观众行为
/// </summary>
public class AudienceManager : MonoBehaviour
{
    [Header("观众设置")]
    [Tooltip("观众预制体")]
    public GameObject audiencePrefab;
    
    [Tooltip("观众生成区域")]
    public Transform audienceArea;
    
    [Tooltip("观众数量")]
    [Range(10, 100)]
    public int audienceCount = 30;
    
    [Tooltip("观众行列数量")]
    public Vector2Int audienceGrid = new Vector2Int(5, 6);
    
    [Tooltip("观众标签，用于识别场景中已有的观众")]
    public string audienceTag = "Audience";
    
    [Tooltip("观众父对象名称，用于查找场景中已有的观众")]
    public string audienceParentName = "ListenerList";
    
    [Header("椅子放置设置")]
    [Tooltip("是否自动查找椅子并放置观众")]
    public bool autoPlaceOnChairs = true;
    
    [Tooltip("椅子名称关键词")]
    public string chairNameKeyword = "Chair";
    public string chairNameKeyword1 = "chair";
    
    [Tooltip("椅子的标签")]
    public string chairTag = "Untagged";
    
    [Tooltip("人物占椅子比例")]
    [Range(0.1f, 1.0f)]
    public float chairCoverageRate = 0.5f;
    
    [Tooltip("人物垂直位置偏移")]
    public float verticalOffset = -1.2f;
    
    [Header("人物设置")]
    [Tooltip("男性比例")]
    [Range(0, 1)]
    public float maleRatio = 0.5f;
    
    [Tooltip("人物预制体路径 - 男性夏装")]
    [SerializeField]
    private string maleSummerPath = "Assets/Citizens PRO/People Prefabs/Male/Summer";
    
    [Tooltip("人物预制体路径 - 男性冬装")]
    [SerializeField]
    private string maleWinterPath = "Assets/Citizens PRO/People Prefabs/Male/Winter";
    
    [Tooltip("人物预制体路径 - 女性夏装")]
    [SerializeField]
    private string femaleSummerPath = "Assets/Citizens PRO/People Prefabs/Female/Summer";
    
    [Tooltip("人物预制体路径 - 女性冬装")]
    [SerializeField]
    private string femaleWinterPath = "Assets/Citizens PRO/People Prefabs/Female/Winter";
    
    [Tooltip("是否使用冬装（否则使用夏装）")]
    public bool useWinterClothing = true;
    
    [Header("提问设置")]
    [Tooltip("可能的问题列表")]
    [TextArea(2, 5)]
    public string[] possibleQuestions;
    
    [Header("观众行为")]
    [Tooltip("观众动画速度范围")]
    public Vector2 animationSpeedRange = new Vector2(0.8f, 1.2f);
    
    [Tooltip("观众反应延迟范围(秒)")]
    public Vector2 reactionDelayRange = new Vector2(0.5f, 2.5f);
    
    [Header("事件")]
    public UnityEvent<string> onQuestionAsked;
    
    [Header("角色动画控制器")]
    [Tooltip("男性角色动画控制器")]
    public RuntimeAnimatorController maleAnimatorController;
    
    [Tooltip("女性角色动画控制器")]
    public RuntimeAnimatorController femaleAnimatorController;
    
    [Tooltip("默认动画控制器")]
    public RuntimeAnimatorController defaultAnimatorController;
    
    // 生成的观众列表
    private List<GameObject> audienceMembers = new List<GameObject>();
    
    // 当前提问的观众
    private GameObject currentQuestioner;
    
    // 表示是否在问答阶段
    private bool isInQuestionPhase = false;
    
    // 角色管理器引用
    private RuntimeCharacterManager characterManager;
    
    // 私有字段存储自动加载的预制体
    private List<GameObject> malePrefabs = new List<GameObject>();
    private List<GameObject> femalePrefabs = new List<GameObject>();
    
    private void Start()
    {
        // 查找角色管理器
        characterManager = FindObjectOfType<RuntimeCharacterManager>();
        
        if (characterManager == null)
        {
            Debug.LogWarning("未找到RuntimeCharacterManager组件，观众动画将无法正常工作！");
        }
        else
        {
            // 设置动画控制器
            if (maleAnimatorController != null)
                characterManager.maleAnimatorController = maleAnimatorController;
                
            if (femaleAnimatorController != null)
                characterManager.femaleAnimatorController = femaleAnimatorController;
                
            if (defaultAnimatorController != null)
                characterManager.defaultAnimatorController = defaultAnimatorController;
        }
        
        // 自动加载人物预制体
        LoadCharacterPrefabs();
        
        // 初始化观众
        if (audienceMembers.Count == 0)
        {
            // 首先尝试查找场景中已有的观众
            FindExistingAudience();
            
            // 如果启用了自动椅子放置且没有找到足够的观众
            if (audienceMembers.Count == 0 && autoPlaceOnChairs)
            {
                PlaceCharactersOnChairs();
            }
            // 如果没有找到足够的观众且有生成区域，则使用网格方式生成新的观众
            else if (audienceMembers.Count == 0 && audienceArea != null && (malePrefabs.Count > 0 || femalePrefabs.Count > 0 || audiencePrefab != null))
            {
                InitializeAudience();
            }
        }
    }
    
    /// <summary>
    /// 自动加载人物预制体
    /// </summary>
    private void LoadCharacterPrefabs()
    {
        // 清空已加载的预制体
        malePrefabs.Clear();
        femalePrefabs.Clear();
        
        // 选择适当的季节路径
        string maleClothingPath = useWinterClothing ? maleWinterPath : maleSummerPath;
        string femaleClothingPath = useWinterClothing ? femaleWinterPath : femaleSummerPath;
        
        // 加载男性预制体
        LoadPrefabsFromPath(maleClothingPath, malePrefabs);
        
        // 加载女性预制体
        LoadPrefabsFromPath(femaleClothingPath, femalePrefabs);
        
        // 输出加载预制体的数量
        Debug.Log($"已加载 {malePrefabs.Count} 个男性预制体和 {femalePrefabs.Count} 个女性预制体");
        
        // 如果没有加载到预制体，尝试使用audiencePrefab作为备选
        if (malePrefabs.Count == 0 && femalePrefabs.Count == 0 && audiencePrefab != null)
        {
            malePrefabs.Add(audiencePrefab);
            femalePrefabs.Add(audiencePrefab);
            Debug.Log("使用默认观众预制体作为备选");
        }
    }
    
    /// <summary>
    /// 从指定路径加载预制体
    /// </summary>
    private void LoadPrefabsFromPath(string path, List<GameObject> targetList)
    {
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogWarning($"预制体路径为空");
            return;
        }

#if UNITY_EDITOR
        // 在编辑器模式下使用AssetDatabase
        string[] prefabGuids = UnityEditor.AssetDatabase.FindAssets("t:GameObject", new[] { path });
        foreach (string guid in prefabGuids)
        {
            string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            if (assetPath.EndsWith(".prefab"))
            {
                GameObject prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (prefab != null)
                {
                    targetList.Add(prefab);
                }
            }
        }
#else
        // 运行时使用Resources加载
        // 注意：这要求预制体放在Resources文件夹中，或者使用AssetBundle
        string resourcePath = path.Replace("Assets/", "").Replace(".prefab", "");
        GameObject[] prefabs = Resources.LoadAll<GameObject>(resourcePath);
        if (prefabs != null && prefabs.Length > 0)
        {
            targetList.AddRange(prefabs);
        }
        else
        {
            Debug.LogWarning($"在Resources目录中未找到预制体：{resourcePath}");
        }
#endif

        if (targetList.Count == 0)
        {
            Debug.LogWarning($"在路径 {path} 中未找到预制体");
        }
    }
    
    /// <summary>
    /// 查找场景中已有的观众
    /// </summary>
    private void FindExistingAudience()
    {
        // 清空现有观众列表
        audienceMembers.Clear();
        
   
        
        // 方法2：通过父对象名称查找
        if (audienceMembers.Count == 0 && !string.IsNullOrEmpty(audienceParentName))
        {
            GameObject parent = GameObject.Find(audienceParentName);
            if (parent != null)
            {
                // 获取所有子对象
                for (int i = 0; i < parent.transform.childCount; i++)
                {
                    audienceMembers.Add(parent.transform.GetChild(i).gameObject);
                }
                Debug.Log($"在'{audienceParentName}'下找到{audienceMembers.Count}个观众");
            }
        }
        
        // 注册所有找到的观众到RuntimeCharacterManager
        if (characterManager != null)
        {
            foreach (GameObject audience in audienceMembers)
            {
                characterManager.RegisterCharacter(audience);
            }
        }
    }
    
    /// <summary>
    /// 初始化并生成观众
    /// </summary>
    private void InitializeAudience()
    {
        // 清空现有观众列表
        ClearAudience();
        
        // 确保我们有预制体可用
        if (malePrefabs.Count == 0 && femalePrefabs.Count == 0 && audiencePrefab == null)
        {
            Debug.LogWarning("无法生成观众：没有可用的人物预制体");
            return;
        }
        
        // 如果没有观众区域，则返回
        if (audienceArea == null)
        {
            Debug.LogWarning("无法生成观众：缺少观众区域");
            return;
        }
        
        // 计算生成位置
        float rowSpacing = audienceArea.localScale.x / (audienceGrid.x + 1);
        float colSpacing = audienceArea.localScale.z / (audienceGrid.y + 1);
        
        Vector3 startPos = audienceArea.position - new Vector3(audienceArea.localScale.x / 2, 0, audienceArea.localScale.z / 2);
        
        // 控制生成的观众数量不超过网格大小
        int actualCount = Mathf.Min(audienceCount, audienceGrid.x * audienceGrid.y);
        
        // 查找或创建观众父对象
        Transform audienceParent = null;
        if (!string.IsNullOrEmpty(audienceParentName))
        {
            GameObject parent = GameObject.Find(audienceParentName);
            if (parent == null)
            {
                parent = new GameObject(audienceParentName);
            }
            audienceParent = parent.transform;
        }
        
        // 用于记录已使用的位置
        HashSet<Vector2> usedPositions = new HashSet<Vector2>();
        
        // 生成观众
        for (int i = 0; i < actualCount; i++)
        {
            int row = i % audienceGrid.x;
            int col = i / audienceGrid.x;
            
            // 计算位置
            Vector3 position = startPos + new Vector3(rowSpacing * (row + 1), 0, colSpacing * (col + 1));
            
            // 检查是否在第一排（根据实际位置判断，z值最大的是第一排）
            float maxZ = startPos.z + colSpacing * (audienceGrid.y + 1);
            if (position.z > maxZ - colSpacing * 1.5f) // 如果距离第一排太近，跳过
            {
                continue;
            }
            
            // 添加一些随机位置变化
            Vector3 randomOffset = new Vector3(Random.Range(-0.3f, 0.3f), 0, Random.Range(-0.3f, 0.3f));
            position += randomOffset;
            
            // 检查这个位置是否已经被使用（使用2D坐标来检查）
            Vector2 position2D = new Vector2(position.x, position.z);
            if (usedPositions.Contains(position2D))
            {
                continue; // 如果位置已被使用，跳过
            }
            
            // 记录这个位置已被使用
            usedPositions.Add(position2D);
            
            // 随机旋转，使观众在Y轴方向有轻微变化，面向中心点
            Quaternion rotation = Quaternion.Euler(0, Random.Range(-20f, 20f), 0);
            
            // 选择预制体
            GameObject prefabToUse = null;
            
            // 根据性别比例决定使用男性还是女性预制体
            bool isMale = Random.value < maleRatio;
            
            if (isMale && malePrefabs.Count > 0)
            {
                prefabToUse = malePrefabs[Random.Range(0, malePrefabs.Count)];
            }
            else if (!isMale && femalePrefabs.Count > 0)
            {
                prefabToUse = femalePrefabs[Random.Range(0, femalePrefabs.Count)];
            }
            else if (malePrefabs.Count > 0)
            {
                // 如果没有可用的性别预制体，使用另一个性别的
                prefabToUse = malePrefabs[Random.Range(0, malePrefabs.Count)];
                isMale = true;
            }
            else if (femalePrefabs.Count > 0)
            {
                prefabToUse = femalePrefabs[Random.Range(0, femalePrefabs.Count)];
                isMale = false;
            }
            else
            {
                // 如果自动加载的预制体都不可用，则使用默认的audiencePrefab
                prefabToUse = audiencePrefab;
            }
            
            // 创建观众
            GameObject audience = Instantiate(prefabToUse, position, rotation);
            
            // 确保名称中包含性别标识，以便RuntimeCharacterManager能正确识别
            if (isMale && !audience.name.ToLower().Contains("male") && !audience.name.ToLower().Contains("man"))
            {
                audience.name = "Male_" + audience.name;
            }
            else if (!isMale && !audience.name.ToLower().Contains("female") && !audience.name.ToLower().Contains("girl") && !audience.name.ToLower().Contains("woman"))
            {
                audience.name = "Female_" + audience.name;
            }
            
            // 设置观众标签
            if (!string.IsNullOrEmpty(audienceTag))
            {
                audience.tag = audienceTag;
            }
            
            // 如果有观众父对象，将观众设为其子对象
            if (audienceParent != null)
            {
                audience.transform.SetParent(audienceParent);
            }
            
            // 添加到观众列表
            audienceMembers.Add(audience);
            
            // 注册到角色管理器
            if (characterManager != null)
            {
                characterManager.RegisterCharacter(audience);
            }
        }
        
        Debug.Log($"成功生成了 {audienceMembers.Count} 个观众");
    }
    
    /// <summary>
    /// 清除所有观众
    /// </summary>
    private void ClearAudience()
    {
        // 获取RuntimeCharacterManager实例
        RuntimeCharacterManager characterManager = RuntimeCharacterManager.Instance;
        
        foreach (GameObject audience in audienceMembers)
        {
            if (audience != null)
            {
                // 从RuntimeCharacterManager中注销角色
                if (characterManager != null)
                {
                    characterManager.UnregisterCharacter(audience);
                }
                
                Destroy(audience);
            }
        }
        
        audienceMembers.Clear();
        currentQuestioner = null;
    }
    
    /// <summary>
    /// 开始问答阶段
    /// </summary>
    public void StartQuestionPhase()
    {
        // 使用RuntimeCharacterManager控制观众动画
        if (characterManager != null)
        {
            // 确保演讲状态为问答阶段
            characterManager.HandleQuestionPhaseStart();
        }
    }
    
    /// <summary>
    /// 结束问答阶段
    /// </summary>
    public void EndQuestionPhase()
    {
        // 如果有当前提问者，重置其状态
        if (currentQuestioner != null && characterManager != null)
        {
            characterManager.PlayAnimation(currentQuestioner, RuntimeCharacterManager.AnimationType.Listen);
            currentQuestioner = null;
        }
    }
    
    /// <summary>
    /// 随机选择观众提问
    /// </summary>
    public GameObject GetRandomQuestioner()
    {
        int index = Random.Range(0, audienceMembers.Count);
        currentQuestioner = audienceMembers[index];
        return currentQuestioner;
    }
    
    /// <summary>
    /// 让所有观众做出反应（如鼓掌）
    /// </summary>
    public void PlayAllAudienceAnimation(RuntimeCharacterManager.AnimationType animationType)
    {
        if (characterManager == null) return;
        
        // 直接使用RuntimeCharacterManager播放所有观众的动画
        switch (animationType)
        {
            case RuntimeCharacterManager.AnimationType.Clap:
                characterManager.PlayAllClap();
                break;
            case RuntimeCharacterManager.AnimationType.Talk:
                characterManager.PlayAllTalk();
                break;
            case RuntimeCharacterManager.AnimationType.Listen:
                characterManager.PlayAllListen();
                break;
            case RuntimeCharacterManager.AnimationType.Idle:
                characterManager.PlayAllIdle();
                break;
        }
    }
    
    /// <summary>
    /// 重置所有观众
    /// </summary>
    public void ResetAllAudience()
    {
        isInQuestionPhase = false;
        
        // 清除当前提问者状态
        currentQuestioner = null;
        
        // 使用RuntimeCharacterManager重置观众状态
        if (characterManager != null)
        {
            characterManager.ResetPresentationState();
        }
    }
    
    /// <summary>
    /// 在场景中的椅子上放置人物
    /// </summary>
    public void PlaceCharactersOnChairs()
    {
        // 清除之前放置的人物
        ClearAudience();
        
        // 确保已加载人物预制体
        if (malePrefabs.Count == 0 && femalePrefabs.Count == 0)
        {
            LoadCharacterPrefabs();
            
            // 如果还是没有预制体，直接返回
            if (malePrefabs.Count == 0 && femalePrefabs.Count == 0)
            {
                Debug.LogError("无法加载任何人物预制体，无法放置人物");
                return;
            }
        }
        
        // 获取场景中的所有椅子对象
        List<GameObject> sceneChairs = FindChairsInScene();
        
        if (sceneChairs.Count == 0)
        {
            Debug.LogWarning("未在场景中找到椅子对象，无法放置人物到椅子上");
            return;
        }

        // 找出第一排椅子的z坐标范围
        float maxZ = float.MinValue;
        float minZ = float.MaxValue;
        foreach (GameObject chair in sceneChairs)
        {
            float chairZ = chair.transform.position.z;
            maxZ = Mathf.Max(maxZ, chairZ);
            minZ = Mathf.Min(minZ, chairZ);
        }
        
        // 计算第一排的范围（假设第一排占20%的z轴范围）
        float firstRowThreshold = maxZ - (maxZ - minZ) * 0.2f;
        
        // 过滤掉第一排的椅子
        List<GameObject> availableChairs = sceneChairs.Where(chair => chair.transform.position.z < firstRowThreshold).ToList();
        
        // 计算要放置人物的椅子数量
        int chairsToFill = Mathf.RoundToInt(availableChairs.Count * chairCoverageRate);
        
        // 随机打乱椅子列表
        System.Random random = new System.Random();
        availableChairs = availableChairs.OrderBy(x => random.Next()).ToList();
        
        // 查找或创建观众父对象
        Transform audienceParent = null;
        if (!string.IsNullOrEmpty(audienceParentName))
        {
            GameObject parent = GameObject.Find(audienceParentName);
            if (parent == null)
            {
                parent = new GameObject(audienceParentName);
            }
            audienceParent = parent.transform;
        }

        // 用于记录已使用的预制体
        HashSet<string> usedPrefabs = new HashSet<string>();
        
        // 放置人物到椅子上
        for (int i = 0; i < chairsToFill && i < availableChairs.Count; i++)
        {
            GameObject chair = availableChairs[i];
            
            // 根据性别比例决定放置男性还是女性
            float randomValue = Random.value;
            bool isMale = randomValue < maleRatio;
            
            // 获取人物预制体
            List<GameObject> prefabsToUse = isMale ? malePrefabs : femalePrefabs;
            
            // 如果没有该性别的预制体，使用另一个性别的
            if (prefabsToUse.Count == 0)
            {
                prefabsToUse = (isMale ? femalePrefabs : malePrefabs);
                isMale = !isMale;
                
                // 如果还是没有，则跳过
                if (prefabsToUse.Count == 0)
                {
                    continue;
                }
            }

            // 过滤掉已使用的预制体
            List<GameObject> availablePrefabs = prefabsToUse.Where(prefab => !usedPrefabs.Contains(prefab.name)).ToList();
            
            // 如果所有预制体都已使用，则清空使用记录重新开始
            if (availablePrefabs.Count == 0)
            {
                usedPrefabs.Clear();
                availablePrefabs = prefabsToUse;
            }
            
            // 随机选择一个未使用的预制体
            GameObject characterPrefab = availablePrefabs[Random.Range(0, availablePrefabs.Count)];
            
            // 记录已使用的预制体
            usedPrefabs.Add(characterPrefab.name);
            
            // 实例化人物到椅子上方
            GameObject character = Instantiate(characterPrefab, 
                chair.transform.position + new Vector3(0.2f, -0.5f,0.0f), 
                chair.transform.rotation);
            
            // 确保名称中包含性别标识，以便RuntimeCharacterManager能正确识别
            if (isMale && !character.name.ToLower().Contains("male") && !character.name.ToLower().Contains("man"))
            {
                character.name = "Male_" + character.name;
            }
            else if (!isMale && !character.name.ToLower().Contains("female") && !character.name.ToLower().Contains("girl") && !character.name.ToLower().Contains("woman"))
            {
                character.name = "Female_" + character.name;
            }
            
            // 设置观众标签
            if (!string.IsNullOrEmpty(audienceTag))
            {
                character.tag = audienceTag;
            }
            
            // 如果有观众父对象，将观众设为其子对象
            if (audienceParent != null)
            {
                character.transform.SetParent(audienceParent);
            }
            
            // 添加到观众列表
            audienceMembers.Add(character);
            
            // 注册到角色管理器
            if (characterManager != null)
            {
                characterManager.RegisterCharacter(character);
            }
        }
        
        Debug.Log($"已在椅子上放置 {audienceMembers.Count} 个人物");
    }
    
    /// <summary>
    /// 查找场景中的椅子
    /// </summary>
    private List<GameObject> FindChairsInScene()
    {
        List<GameObject> result = new List<GameObject>();
        
        // 查找名称包含指定关键词的游戏对象
        GameObject[] objects = GameObject.FindGameObjectsWithTag(chairTag);
        foreach (GameObject obj in objects)
        {
            if (obj.name.Contains(chairNameKeyword))
            {
                result.Add(obj);
            }
        }
        
        // 如果没有找到椅子，查找所有mesh包含椅子关键词的对象
        if (result.Count == 0)
        {
            foreach (MeshFilter meshFilter in FindObjectsOfType<MeshFilter>())
            {
                if (meshFilter.sharedMesh != null && meshFilter.sharedMesh.name.Contains(chairNameKeyword))
                {
                    result.Add(meshFilter.gameObject);
                }
                
                if (meshFilter.sharedMesh != null && meshFilter.sharedMesh.name.Contains(chairNameKeyword1))
                {
                    result.Add(meshFilter.gameObject);
                }
            }
        }
        
        Debug.Log($"在场景中找到 {result.Count} 个椅子对象");
        return result;
    }
    
    /// <summary>
    /// 销毁所有创建的观众
    /// </summary>
    public void DestroyAllAudience()
    {
        ClearAudience();
        
        // 如果创建了观众父对象且为空，也销毁它
        if (!string.IsNullOrEmpty(audienceParentName))
        {
            GameObject parent = GameObject.Find(audienceParentName);
            if (parent != null && parent.transform.childCount == 0)
            {
                Destroy(parent);
            }
        }
    }

    /// <summary>
    /// 控制所有观众播放随机闲置动画
    /// </summary>
    public void PlayAllAudienceIdle()
    {
        if (characterManager != null)
        {
            characterManager.PlayAllIdle();
        }
    }

    /// <summary>
    /// 控制所有观众播放倾听动画
    /// </summary>
    public void PlayAllAudienceListen()
    {
        if (characterManager != null)
        {
            characterManager.PlayAllListen();
        }
    }


} 
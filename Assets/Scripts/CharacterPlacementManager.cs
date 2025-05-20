using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

/// <summary>
/// 运行时在椅子上随机放置人物的管理器
/// </summary>
public class CharacterPlacementManager : MonoBehaviour
{
    [Header("预制体参考")]
    [SerializeField] private List<GameObject> maleSummerPrefabs = new List<GameObject>();
    [SerializeField] private List<GameObject> maleAutumnPrefabs = new List<GameObject>();
    [SerializeField] private List<GameObject> femaleSummerPrefabs = new List<GameObject>();
    [SerializeField] private List<GameObject> femaleAutumnPrefabs = new List<GameObject>();
    
    [Header("设置")]
    [Range(0, 1)]
    [SerializeField] private float coverageRate = 0.5f;
    [SerializeField] private bool isSummerClothing = true;
    [Range(0, 1)]
    [SerializeField] private float maleRatio = 0.5f;
    
    [Header("动画播放器")]
    [SerializeField] private CharacterAnimationPlayer animationPlayer;
    
    [Header("UI引用")]
    [SerializeField] private Slider coverageSlider;
    [SerializeField] private Toggle summerToggle;
    [SerializeField] private Toggle autumnToggle;
    [SerializeField] private Slider maleRatioSlider;
    [SerializeField] private Button placeButton;
    [SerializeField] private Button clearButton;
    [SerializeField] private Button playAnimationsButton;
    
    // 已放置的人物
    private List<GameObject> placedCharacters = new List<GameObject>();
    
    private void Start()
    {
        // 连接UI控件事件
        SetupUI();
    }
    
    private void SetupUI()
    {
        if (coverageSlider != null)
        {
            coverageSlider.value = coverageRate;
            coverageSlider.onValueChanged.AddListener(SetCoverageRate);
        }
        
        if (summerToggle != null)
        {
            summerToggle.isOn = isSummerClothing;
            summerToggle.onValueChanged.AddListener(SetSummerClothing);
        }
        
        if (autumnToggle != null)
        {
            autumnToggle.isOn = !isSummerClothing;
            autumnToggle.onValueChanged.AddListener(SetAutumnClothing);
        }
        
        if (maleRatioSlider != null)
        {
            maleRatioSlider.value = maleRatio;
            maleRatioSlider.onValueChanged.AddListener(SetMaleRatio);
        }
        
        if (placeButton != null)
        {
            placeButton.onClick.AddListener(PlaceCharactersOnChairs);
        }
        
        if (clearButton != null)
        {
            clearButton.onClick.AddListener(ClearPlacedCharacters);
        }
        
        if (playAnimationsButton != null)
        {
            playAnimationsButton.onClick.AddListener(PlayAllAnimations);
        }
    }
    
    public void SetCoverageRate(float value)
    {
        coverageRate = value;
    }
    
    public void SetSummerClothing(bool isOn)
    {
        if (isOn)
        {
            isSummerClothing = true;
            if (autumnToggle != null) autumnToggle.isOn = false;
        }
    }
    
    public void SetAutumnClothing(bool isOn)
    {
        if (isOn)
        {
            isSummerClothing = false;
            if (summerToggle != null) summerToggle.isOn = false;
        }
    }
    
    public void SetMaleRatio(float value)
    {
        maleRatio = value;
    }
    
    public void PlaceCharactersOnChairs()
    {
        // 清除之前放置的人物
        ClearPlacedCharacters();
        
        // 获取场景中的所有椅子对象
        List<GameObject> sceneChairs = FindChairsInScene();
        
        if (sceneChairs.Count == 0)
        {
            Debug.LogWarning("场景中没有找到椅子对象");
            return;
        }
        
        // 计算要放置人物的椅子数量
        int chairsToFill = Mathf.RoundToInt(sceneChairs.Count * coverageRate);
        
        // 随机打乱椅子列表
        System.Random random = new System.Random();
        sceneChairs = sceneChairs.OrderBy(x => random.Next()).ToList();
        
        // 确定要使用的人物预制体列表
        List<GameObject> malePrefabs = isSummerClothing ? maleSummerPrefabs : maleAutumnPrefabs;
        List<GameObject> femalePrefabs = isSummerClothing ? femaleSummerPrefabs : femaleAutumnPrefabs;
        
        if (malePrefabs.Count == 0 || femalePrefabs.Count == 0)
        {
            Debug.LogWarning("未找到人物预制体");
            return;
        }
        
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
            GameObject character = Instantiate(characterPrefab, 
                chair.transform.position + new Vector3(0, 0.1f, 0), 
                chair.transform.rotation);
            
            // 添加可选择组件
            SelectableCharacter selectable = character.AddComponent<SelectableCharacter>();
            if (animationPlayer != null && selectable != null)
            {
                // 使用反射设置animationPlayer字段
                var field = typeof(SelectableCharacter).GetField("animationPlayer", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                if (field != null)
                {
                    field.SetValue(selectable, animationPlayer);
                }
            }
            
            // 添加到已放置列表
            placedCharacters.Add(character);
        }
        
        Debug.Log($"已在椅子上放置 {placedCharacters.Count} 个人物");
        
        // 将放置的角色传递给动画播放器
        if (animationPlayer != null)
        {
            animationPlayer.SetCharacters(placedCharacters);
        }
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
    
    public void ClearPlacedCharacters()
    {
        // 如果有动画播放器，清除其角色引用
        if (animationPlayer != null)
        {
            animationPlayer.ClearCharacters();
        }
        
        // 删除所有已放置的人物
        foreach (GameObject character in placedCharacters)
        {
            if (character != null)
            {
                Destroy(character);
            }
        }
        
        placedCharacters.Clear();
        Debug.Log("已清除所有放置的人物");
    }
    
    /// <summary>
    /// 播放所有角色的动画
    /// </summary>
    public void PlayAllAnimations()
    {
        if (animationPlayer != null)
        {
            if (placedCharacters.Count > 0)
            {
                animationPlayer.PlayAllAnimations();
            }
            else
            {
                Debug.LogWarning("未放置任何人物，无法播放动画");
            }
        }
        else
        {
            Debug.LogError("未找到动画播放器组件");
        }
    }
    
    /// <summary>
    /// 获取已放置的角色列表
    /// </summary>
    public List<GameObject> GetPlacedCharacters()
    {
        return placedCharacters;
    }
    
    // 用于编辑器中预先设置预制体的帮助方法
    public void SetPresets(List<GameObject> maleSummer, List<GameObject> maleAutumn,
                          List<GameObject> femaleSummer, List<GameObject> femaleAutumn)
    {
        if (maleSummer != null && maleSummer.Count > 0)
            maleSummerPrefabs = maleSummer;
        
        if (maleAutumn != null && maleAutumn.Count > 0)
            maleAutumnPrefabs = maleAutumn;
        
        if (femaleSummer != null && femaleSummer.Count > 0)
            femaleSummerPrefabs = femaleSummer;
        
        if (femaleAutumn != null && femaleAutumn.Count > 0)
            femaleAutumnPrefabs = femaleAutumn;
    }
} 
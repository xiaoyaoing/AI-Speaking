using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    [System.Serializable]
    public class CharacterData
    {
        public string characterName;
        public GameObject characterPrefab;
        public Sprite portrait;
        [TextArea(3, 5)]
        public string description;
    }
    
    [Header("角色设置")]
    public List<CharacterData> availableCharacters = new List<CharacterData>();
    public Transform[] spawnPoints;
    
    [Header("角色行为")]
    public float movementSpeed = 2.0f;
    public float rotationSpeed = 120.0f;
    public float interactionRadius = 2.0f;
    
    private List<GameObject> spawnedCharacters = new List<GameObject>();
    private int selectedCharacterIndex = 0;
    
    // 单例模式
    private static CharacterManager _instance;
    public static CharacterManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<CharacterManager>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject("CharacterManager");
                    _instance = obj.AddComponent<CharacterManager>();
                }
            }
            return _instance;
        }
    }
    
    void Awake()
    {
        // 确保单例实现
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    void Start()
    {
        // 初始化检查
        ValidateSetup();
    }
    
    private void ValidateSetup()
    {
        if (availableCharacters.Count == 0)
        {
            Debug.LogWarning("未设置任何可用角色");
        }
        
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("未设置任何生成点");
        }
    }
    
    // 生成所有角色
    public void SpawnAllCharacters()
    {
        ClearSpawnedCharacters();
        
        for (int i = 0; i < Mathf.Min(availableCharacters.Count, spawnPoints.Length); i++)
        {
            SpawnCharacter(i, spawnPoints[i]);
        }
    }
    
    // 生成特定角色
    public GameObject SpawnCharacter(int characterIndex, Transform spawnPoint)
    {
        if (characterIndex < 0 || characterIndex >= availableCharacters.Count)
        {
            Debug.LogError("角色索引超出范围: " + characterIndex);
            return null;
        }
        
        if (spawnPoint == null)
        {
            Debug.LogError("生成点为空");
            return null;
        }
        
        CharacterData data = availableCharacters[characterIndex];
        if (data.characterPrefab == null)
        {
            Debug.LogError("角色预制体为空: " + data.characterName);
            return null;
        }
        
        GameObject characterObj = Instantiate(data.characterPrefab, spawnPoint.position, spawnPoint.rotation);
        characterObj.name = data.characterName;
        
        // 添加到已生成角色列表
        spawnedCharacters.Add(characterObj);
        
        return characterObj;
    }
    
    // 清除所有生成的角色
    public void ClearSpawnedCharacters()
    {
        foreach (GameObject character in spawnedCharacters)
        {
            if (character != null)
            {
                Destroy(character);
            }
        }
        
        spawnedCharacters.Clear();
    }
    
    // 获取角色信息
    public CharacterData GetCharacterData(int index)
    {
        if (index >= 0 && index < availableCharacters.Count)
        {
            return availableCharacters[index];
        }
        return null;
    }
    
    // 选择角色
    public void SelectCharacter(int index)
    {
        if (index >= 0 && index < availableCharacters.Count)
        {
            selectedCharacterIndex = index;
        }
    }
    
    // 获取当前选择的角色
    public CharacterData GetSelectedCharacter()
    {
        return GetCharacterData(selectedCharacterIndex);
    }
    
    // 让角色移动到指定位置
    public void MoveCharacterTo(GameObject character, Vector3 destination)
    {
        if (character == null) return;
        
        StartCoroutine(MoveToPosition(character, destination));
    }
    
    // 移动协程
    private IEnumerator MoveToPosition(GameObject character, Vector3 destination)
    {
        Transform charTransform = character.transform;
        destination.y = charTransform.position.y; // 保持相同的高度
        
        while (Vector3.Distance(charTransform.position, destination) > 0.1f)
        {
            // 旋转朝向目标
            Vector3 direction = (destination - charTransform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            charTransform.rotation = Quaternion.RotateTowards(
                charTransform.rotation, 
                targetRotation, 
                rotationSpeed * Time.deltaTime
            );
            
            // 移动角色
            charTransform.position = Vector3.MoveTowards(
                charTransform.position, 
                destination, 
                movementSpeed * Time.deltaTime
            );
            
            yield return null;
        }
    }
    
    // 让角色互动
    public void InteractCharacters(GameObject character1, GameObject character2)
    {
        if (character1 == null || character2 == null) return;
        
        // 检查距离
        float distance = Vector3.Distance(character1.transform.position, character2.transform.position);
        if (distance > interactionRadius)
        {
            // 如果太远，先移动到对方附近
            Vector3 midPoint = (character1.transform.position + character2.transform.position) / 2;
            StartCoroutine(SetupInteraction(character1, character2, midPoint));
        }
        else
        {
            // 直接进行互动
            PerformInteraction(character1, character2);
        }
    }
    
    // 设置互动的协程
    private IEnumerator SetupInteraction(GameObject character1, GameObject character2, Vector3 meetingPoint)
    {
        // 移动两个角色到中间点附近
        StartCoroutine(MoveToPosition(character1, meetingPoint - new Vector3(1, 0, 0)));
        StartCoroutine(MoveToPosition(character2, meetingPoint + new Vector3(1, 0, 0)));
        
        // 等待两个角色都到达目的地
        while (Vector3.Distance(character1.transform.position, meetingPoint - new Vector3(1, 0, 0)) > 0.2f ||
               Vector3.Distance(character2.transform.position, meetingPoint + new Vector3(1, 0, 0)) > 0.2f)
        {
            yield return null;
        }
        
        // 让他们面对面
        character1.transform.LookAt(character2.transform);
        character2.transform.LookAt(character1.transform);
        
        // 执行互动
        PerformInteraction(character1, character2);
    }
    
    // 执行角色互动
    private void PerformInteraction(GameObject character1, GameObject character2)
    {
        // 可以在这里添加动画触发、对话系统等
        Debug.Log(character1.name + " 正在与 " + character2.name + " 互动");
        
        // 触发角色上的互动事件
        IInteractable interactable1 = character1.GetComponent<IInteractable>();
        IInteractable interactable2 = character2.GetComponent<IInteractable>();
        
        if (interactable1 != null)
            interactable1.Interact(character2);
        
        if (interactable2 != null)
            interactable2.Interact(character1);
    }
}

// 互动接口
public interface IInteractable
{
    void Interact(GameObject other);
} 
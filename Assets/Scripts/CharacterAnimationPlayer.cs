using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 角色动画播放器，用于播放角色所有可用的动画
/// </summary>
public class CharacterAnimationPlayer : MonoBehaviour
{
    // 常见动画名称
    private readonly string[] commonAnimationNames = new string[] 
    { 
        "idle1", "idle2", "walk", "run", "talk1", "talk2", "listen", "claphands", "cheer"
    };
    
    // 当前选中的角色列表
    private List<GameObject> selectedCharacters = new List<GameObject>();
    
    // 动画播放设置
    [Header("动画设置")]
    [Range(1f, 10f)]
    [SerializeField] private float animationDuration = 3f;
    [SerializeField] private bool autoRotateCharacters = true;
    [SerializeField] private float rotationSpeed = 30f;
    
    private int currentAnimationIndex = -1;
    private bool isPlaying = false;
    private Coroutine playAnimationCoroutine;
    
    /// <summary>
    /// 设置要播放动画的角色
    /// </summary>
    public void SetCharacters(List<GameObject> characters)
    {
        selectedCharacters = new List<GameObject>(characters);
    }
    
    /// <summary>
    /// 添加一个角色到播放列表
    /// </summary>
    public void AddCharacter(GameObject character)
    {
        if (character != null && !selectedCharacters.Contains(character))
        {
            selectedCharacters.Add(character);
        }
    }
    
    /// <summary>
    /// 清除所有选中的角色
    /// </summary>
    public void ClearCharacters()
    {
        selectedCharacters.Clear();
        currentAnimationIndex = -1;
        if (isPlaying)
        {
            StopAnimations();
        }
    }
    
    /// <summary>
    /// 从指定场景中查找所有人物角色并添加到播放列表
    /// </summary>
    public void FindAndAddAllCharactersInScene()
    {
        // 清除当前列表
        selectedCharacters.Clear();
        
        // 查找所有带有Animator组件的对象
        Animator[] allAnimators = FindObjectsOfType<Animator>();
        
        // 过滤掉非人物角色的对象（这里根据角色prefab名称包含特定关键词来判断）
        foreach (Animator animator in allAnimators)
        {
            // 检查对象名称或预制体名称包含人物关键词
            string objName = animator.gameObject.name.ToLower();
            if (objName.Contains("male") || objName.Contains("female") || 
                objName.Contains("girl") || objName.Contains("man") ||
                objName.Contains("casual") || objName.Contains("business") || 
                objName.Contains("sportive"))
            {
                selectedCharacters.Add(animator.gameObject);
            }
        }
        
        Debug.Log($"找到并添加了 {selectedCharacters.Count} 个角色");
    }
    
    /// <summary>
    /// 播放所有角色的所有动画
    /// </summary>
    public void PlayAllAnimations()
    {
        if (selectedCharacters.Count == 0)
        {
            Debug.LogWarning("没有选中的角色");
            return;
        }
        
        if (isPlaying)
        {
            StopAnimations();
        }
        
        currentAnimationIndex = 0;
        isPlaying = true;
        playAnimationCoroutine = StartCoroutine(PlayAnimationSequence());
    }
    
    /// <summary>
    /// 播放单个角色的所有动画
    /// </summary>
    public void PlayAllAnimationsForSingleCharacter(GameObject character)
    {
        if (character == null)
        {
            Debug.LogWarning("角色对象为空");
            return;
        }
        
        if (isPlaying)
        {
            StopAnimations();
        }
        
        // 清空现有角色列表并只添加指定角色
        selectedCharacters.Clear();
        selectedCharacters.Add(character);
        
        currentAnimationIndex = 0;
        isPlaying = true;
        playAnimationCoroutine = StartCoroutine(PlayAnimationSequence());
        
        Debug.Log($"开始播放角色 {character.name} 的所有动画");
    }
    
    /// <summary>
    /// 为场景中最近的角色播放所有动画
    /// </summary>
    public void PlayAllAnimationsForNearestCharacter()
    {
        // 查找场景中所有角色
        FindAndAddAllCharactersInScene();
        
        if (selectedCharacters.Count == 0)
        {
            Debug.LogWarning("场景中没有找到角色");
            return;
        }
        
        // 找到最近的角色（与相机或角色控制器的距离）
        GameObject nearestCharacter = FindNearestCharacter();
        if (nearestCharacter != null)
        {
            PlayAllAnimationsForSingleCharacter(nearestCharacter);
        }
    }
    
    /// <summary>
    /// 找到最近的角色
    /// </summary>
    private GameObject FindNearestCharacter()
    {
        if (selectedCharacters.Count == 0)
            return null;
        
        // 使用相机位置或玩家位置作为参考点
        Vector3 referencePosition = Camera.main != null ? Camera.main.transform.position : Vector3.zero;
        
        GameObject nearest = null;
        float minDistance = float.MaxValue;
        
        foreach (GameObject character in selectedCharacters)
        {
            if (character != null)
            {
                float distance = Vector3.Distance(character.transform.position, referencePosition);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = character;
                }
            }
        }
        
        return nearest;
    }
    
    /// <summary>
    /// 停止所有动画播放
    /// </summary>
    public void StopAnimations()
    {
        if (playAnimationCoroutine != null)
        {
            StopCoroutine(playAnimationCoroutine);
            playAnimationCoroutine = null;
        }
        
        isPlaying = false;
        currentAnimationIndex = -1;
        
        // 将所有角色重置为默认状态
        foreach (GameObject character in selectedCharacters)
        {
            if (character != null)
            {
                Animator animator = character.GetComponent<Animator>();
                if (animator != null)
                {
                    animator.Play("idle1", 0, 0);
                }
            }
        }
    }
    
    /// <summary>
    /// 播放特定动画
    /// </summary>
    public void PlaySpecificAnimation(string animationName)
    {
        if (selectedCharacters.Count == 0)
        {
            Debug.LogWarning("没有选中的角色");
            return;
        }
        
        foreach (GameObject character in selectedCharacters)
        {
            if (character != null)
            {
                Animator animator = character.GetComponent<Animator>();
                if (animator != null)
                {
                    animator.Play(animationName, 0, 0);
                }
            }
        }
    }
    
    /// <summary>
    /// 为指定角色播放特定动画
    /// </summary>
    public void PlaySpecificAnimationForCharacter(GameObject character, string animationName)
    {
        if (character == null)
        {
            Debug.LogWarning("角色对象为空");
            return;
        }
        
        Animator animator = character.GetComponent<Animator>();
        if (animator != null)
        {
            animator.Play(animationName, 0, 0);
            Debug.Log($"为角色 {character.name} 播放动画: {animationName}");
        }
        else
        {
            Debug.LogWarning($"角色 {character.name} 没有Animator组件");
        }
    }
    
    private IEnumerator PlayAnimationSequence()
    {
        while (isPlaying && selectedCharacters.Count > 0)
        {
            // 获取当前要播放的动画名称
            string animationName = commonAnimationNames[currentAnimationIndex];
            
            // 在所有选中的角色上播放该动画
            foreach (GameObject character in selectedCharacters)
            {
                if (character != null)
                {
                    Animator animator = character.GetComponent<Animator>();
                    if (animator != null)
                    {
                        animator.Play(animationName, 0, 0);
                    }
                }
            }
            
            // 显示当前播放的动画名称
            Debug.Log($"播放动画: {animationName}");
            
            // 等待一段时间
            float elapsed = 0f;
            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                
                // 如果需要旋转角色
                if (autoRotateCharacters)
                {
                    foreach (GameObject character in selectedCharacters)
                    {
                        if (character != null)
                        {
                            character.transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
                        }
                    }
                }
                
                yield return null;
            }
            
            // 移动到下一个动画
            currentAnimationIndex = (currentAnimationIndex + 1) % commonAnimationNames.Length;
        }
        
        isPlaying = false;
    }
    
    /// <summary>
    /// 获取动画名称列表
    /// </summary>
    public string[] GetAnimationNames()
    {
        return commonAnimationNames;
    }
    
    /// <summary>
    /// 设置动画播放时长
    /// </summary>
    public void SetAnimationDuration(float duration)
    {
        animationDuration = Mathf.Clamp(duration, 1f, 10f);
    }
    
    /// <summary>
    /// 设置是否自动旋转角色
    /// </summary>
    public void SetAutoRotateCharacters(bool autoRotate)
    {
        autoRotateCharacters = autoRotate;
    }
    
    /// <summary>
    /// 获取当前角色数量
    /// </summary>
    public int GetCharacterCount()
    {
        return selectedCharacters.Count;
    }
    
    /// <summary>
    /// 获取当前选中的角色
    /// </summary>
    public GameObject GetCurrentCharacter()
    {
        if (selectedCharacters.Count > 0)
        {
            return selectedCharacters[0];
        }
        return null;
    }
    
    /// <summary>
    /// 切换到下一个角色
    /// </summary>
    public void SwitchToNextCharacter()
    {
        if (selectedCharacters.Count <= 1)
            return;
        
        // 如果有当前选中的角色，将它移到列表最后，让下一个角色成为当前角色
        if (selectedCharacters.Count > 0)
        {
            GameObject current = selectedCharacters[0];
            selectedCharacters.RemoveAt(0);
            selectedCharacters.Add(current);
            
            Debug.Log($"已切换到下一个角色: {selectedCharacters[0].name}");
        }
    }
    
    /// <summary>
    /// 切换到上一个角色
    /// </summary>
    public void SwitchToPreviousCharacter()
    {
        if (selectedCharacters.Count <= 1)
            return;
        
        // 如果有角色，将列表最后一个移到最前面，让它成为当前角色
        if (selectedCharacters.Count > 0)
        {
            GameObject last = selectedCharacters[selectedCharacters.Count - 1];
            selectedCharacters.RemoveAt(selectedCharacters.Count - 1);
            selectedCharacters.Insert(0, last);
            
            Debug.Log($"已切换到上一个角色: {selectedCharacters[0].name}");
        }
    }
} 
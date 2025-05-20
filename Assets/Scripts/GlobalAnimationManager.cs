using UnityEngine;
using System.Collections.Generic;

public class GlobalAnimationManager : MonoBehaviour
{
    public enum AnimationType
    {
        Listen,
        Talk,
        Clap
    }

    private static GlobalAnimationManager instance;
    public static GlobalAnimationManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("GlobalAnimationManager");
                instance = go.AddComponent<GlobalAnimationManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    private Dictionary<GameObject, Animator> characterAnimators = new Dictionary<GameObject, Animator>();
    private Dictionary<GameObject, string> characterTypes = new Dictionary<GameObject, string>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void RegisterCharacter(GameObject character)
    {
        if (character == null) return;

        // 获取Animator组件
        Animator animator = character.GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError($"角色 {character.name} 缺少Animator组件！");
            return;
        }

        // 确定角色类型
        string characterType = DetermineCharacterType(character.name);
        if (string.IsNullOrEmpty(characterType))
        {
            Debug.LogWarning($"无法识别角色类型: {character.name}");
            return;
        }

        // 注册角色
        characterAnimators[character] = animator;
        characterTypes[character] = characterType;

        // 设置初始动画为listen
        PlayAnimation(character, AnimationType.Listen);
    }

    public void UnregisterCharacter(GameObject character)
    {
        if (character == null) return;

        characterAnimators.Remove(character);
        characterTypes.Remove(character);
    }

    public void PlayAnimation(GameObject character, AnimationType type)
    {
        if (character == null || !characterAnimators.ContainsKey(character)) return;

        string animationName = characterTypes[character] + type.ToString().ToLower();
        characterAnimators[character].Play(animationName);
    }

    // 全局动画控制方法
    public void PlayAllListen()
    {
        foreach (var character in characterAnimators.Keys)
        {
            PlayAnimation(character, AnimationType.Listen);
        }
    }

    public void PlayAllTalk()
    {
        foreach (var character in characterAnimators.Keys)
        {
            PlayAnimation(character, AnimationType.Talk);
        }
    }

    public void PlayAllClap()
    {
        foreach (var character in characterAnimators.Keys)
        {
            PlayAnimation(character, AnimationType.Clap);
        }
    }

    private string DetermineCharacterType(string characterName)
    {
        string name = characterName.ToLower();
        if (name.Contains("man")) return "m_";
        if (name.Contains("girlwithheel")) return "f_heel_";
        if (name.Contains("girlnoheel")) return "f_noheel_";
        return "";
    }
} 
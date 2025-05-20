using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.IO;

public class EditorSceneCharacterManager
{
    private static EditorSceneCharacterManager instance;
    public static EditorSceneCharacterManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new EditorSceneCharacterManager();
            }
            return instance;
        }
    }

    private Dictionary<GameObject, Animator> characterAnimators = new Dictionary<GameObject, Animator>();
    private Dictionary<GameObject, string> characterTypes = new Dictionary<GameObject, string>();

    private EditorSceneCharacterManager() { }

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

        // 获取动画控制器
        RuntimeAnimatorController controller = animator.runtimeAnimatorController;
        if (controller == null)
        {
            Debug.LogError($"角色 {character.name} 的Animator组件缺少动画控制器！");
            return;
        }

        // 从控制器名称确定角色类型
        string controllerName = controller.name.ToLower();
        string characterType = "";
        if (controllerName.Contains("male") || controllerName.Contains("man"))
        {
            characterType = "m_";
        }
        else if (controllerName.Contains("female") || controllerName.Contains("girl") || controllerName.Contains("woman"))
        {
            characterType = "f_";
        }

        if (string.IsNullOrEmpty(characterType))
        {
            Debug.LogWarning($"无法从动画控制器名称识别角色类型: {controllerName}");
            return;
        }

        // 注册角色
        characterAnimators[character] = animator;
        characterTypes[character] = characterType;

        // 设置初始动画为talk
        PlayAnimation(character, AnimationType.Talk);
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

    public enum AnimationType
    {
        Listen,
        Talk,
        Clap
    }
}
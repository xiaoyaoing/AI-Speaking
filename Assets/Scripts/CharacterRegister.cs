using UnityEngine;

public class CharacterRegister : MonoBehaviour
{
    private void Start()
    {
        // 注册角色到全局动画管理器
        GlobalAnimationManager.Instance.RegisterCharacter(gameObject);
    }

    private void OnDestroy()
    {
        // 从全局动画管理器中注销角色
        GlobalAnimationManager.Instance.UnregisterCharacter(gameObject);
    }
} 
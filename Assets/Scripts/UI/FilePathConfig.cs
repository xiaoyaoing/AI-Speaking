using UnityEngine;

/// <summary>
/// 文件路径配置类 - 管理默认文件路径
/// </summary>
[CreateAssetMenu(fileName = "FilePathConfig", menuName = "Config/File Path Config")]
public class FilePathConfig : ScriptableObject
{
    [Header("默认文件路径")]
    [Tooltip("默认PPT文件路径（相对于Assets/Resources/）")]
    public string defaultPptPath = "ppts/hci_ppt.pptx";
    
    [Tooltip("默认TXT文件路径（相对于Assets/Resources/）")]
    public string defaultTxtPath = "演讲稿.txt";
    
    [Header("文件显示名称")]
    [Tooltip("PPT文件显示名称")]
    public string pptDisplayName = "hci_ppt.pptx";
    
    [Tooltip("TXT文件显示名称")]
    public string txtDisplayName = "演讲稿.txt";
    
    /// <summary>
    /// 获取完整的PPT文件路径
    /// </summary>
    public string GetFullPptPath()
    {
        return System.IO.Path.Combine(Application.dataPath, "Resources", defaultPptPath);
    }
    
    /// <summary>
    /// 获取完整的TXT文件路径
    /// </summary>
    public string GetFullTxtPath()
    {
        return System.IO.Path.Combine(Application.dataPath, "Resources", defaultTxtPath);
    }
    
    /// <summary>
    /// 检查所有默认文件是否存在
    /// </summary>
    public bool ValidateDefaultFiles()
    {
        bool pptExists = System.IO.File.Exists(GetFullPptPath());
        bool txtExists = System.IO.File.Exists(GetFullTxtPath());
        
        if (!pptExists)
        {
            Debug.LogWarning($"默认PPT文件不存在: {GetFullPptPath()}");
        }
        
        if (!txtExists)
        {
            Debug.LogWarning($"默认TXT文件不存在: {GetFullTxtPath()}");
        }
        
        return pptExists && txtExists;
    }
} 
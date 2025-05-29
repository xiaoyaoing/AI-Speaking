using UnityEngine;
using UnityEngine.UI;

public class MainSceneDataReceiver : MonoBehaviour
{
    [Header("UI显示组件")]
    public Text pptFileNameText;
    public Text txtFileNameText;
    public Text statusText;
    
    [Header("调试信息")]
    public bool showDebugInfo = true;
    
    private FileDataManager.FileData receivedData;
    
    private void Start()
    {
        // 从数据管理器获取文件数据
        receivedData = UploadPage.GetFileDataFromManager();
        
        if (receivedData != null)
        {
            DisplayFileInfo();
            ProcessReceivedData();
        }
        else
        {
            if (showDebugInfo)
            {
                Debug.LogWarning("MainSceneDataReceiver: 没有接收到文件数据");
            }
            
            if (statusText != null)
            {
                statusText.text = "未找到文件数据";
            }
        }
    }
    
    private void DisplayFileInfo()
    {
        // 显示文件信息到UI
        if (pptFileNameText != null)
        {
            pptFileNameText.text = $"PPT文件: {receivedData.pptFileName}";
        }
        
        if (txtFileNameText != null)
        {
            txtFileNameText.text = $"文本文件: {receivedData.txtFileName}";
        }
        
        if (statusText != null)
        {
            statusText.text = "文件数据已加载";
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"接收到PPT文件: {receivedData.pptPath}");
            Debug.Log($"接收到文本文件: {receivedData.txtPath}");
        }
    }
    
    private void ProcessReceivedData()
    {
        // 在这里处理接收到的文件数据
        // 例如：加载PPT图片、读取文本内容等
        
        if (!string.IsNullOrEmpty(receivedData.pptPath))
        {
            // 处理PPT文件路径
            ProcessPptFile(receivedData.pptPath);
        }
        
        if (!string.IsNullOrEmpty(receivedData.txtPath))
        {
            // 处理文本文件路径
            ProcessTxtFile(receivedData.txtPath);
        }
    }
    
    private void ProcessPptFile(string pptPath)
    {
        if (showDebugInfo)
        {
            Debug.Log($"开始处理PPT文件: {pptPath}");
        }
        
        // 这里可以添加PPT处理逻辑
        // 例如：加载转换后的图片、设置幻灯片等
    }
    
    private void ProcessTxtFile(string txtPath)
    {
        if (showDebugInfo)
        {
            Debug.Log($"开始处理文本文件: {txtPath}");
        }
        
        // 这里可以添加文本处理逻辑
        // 例如：读取演讲稿内容、分段处理等
        
        try
        {
            string content = System.IO.File.ReadAllText(txtPath);
            if (showDebugInfo)
            {
                Debug.Log($"文本内容长度: {content.Length} 字符");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"读取文本文件失败: {e.Message}");
        }
    }
    
    // 公共方法，供其他脚本获取接收到的数据
    public FileDataManager.FileData GetReceivedData()
    {
        return receivedData;
    }
    
    // 检查是否有有效的文件数据
    public bool HasValidData()
    {
        return receivedData != null && 
               !string.IsNullOrEmpty(receivedData.pptPath) && 
               !string.IsNullOrEmpty(receivedData.txtPath);
    }
} 
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Diagnostics;
using UnityEngine.SceneManagement;

public class UploadPage : MonoBehaviour
{
    [SerializeField] private Button selectPptButton;
    [SerializeField] private Button selectTxtButton;
    [SerializeField] private Button startButton;
    [SerializeField] private Text pptPathText;
    [SerializeField] private Text txtPathText;
    [SerializeField] private Text statusText;
    [SerializeField] private Image backgroundImage; // 背景图片组件

    private string selectedPptPath;
    private string selectedTxtPath;
    private bool isProcessing = false;

    private void Start()
    {
        selectPptButton.onClick.AddListener(SelectPptFile);
        selectTxtButton.onClick.AddListener(SelectTxtFile);
        startButton.onClick.AddListener(StartProcess);
        
        // 初始化状态
        statusText.text = "请选择PPT文件和文本文件";
        startButton.interactable = false;

        // 尝试加载背景图片
        LoadBackgroundImage();
    }

    private void LoadBackgroundImage()
    {
        if (backgroundImage != null)
        {
            // 尝试加载EXR格式
            Texture2D bgTexture = Resources.Load<Texture2D>("Backgrounds/upload_bg");
            if (bgTexture != null)
            {
                // 创建材质
                Material bgMaterial = new Material(Shader.Find("Unlit/Texture"));
                bgMaterial.mainTexture = bgTexture;
                
                // 设置背景图片的材质
                backgroundImage.material = bgMaterial;
                
                // 设置图片的显示方式
                backgroundImage.preserveAspect = true;
                backgroundImage.type = Image.Type.Simple;
                
                // 调整图片的缩放和位置
                RectTransform rectTransform = backgroundImage.GetComponent<RectTransform>();
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;
                
                // 设置图片颜色为白色，保持原始颜色
                backgroundImage.color = Color.white;
            }
        }
    }

    private void SelectPptFile()
    {
        if (isProcessing) return;

        string path = OpenFileDialog("选择PPT文件", "PowerPoint文件|*.ppt;*.pptx");
        if (!string.IsNullOrEmpty(path))
        {
            selectedPptPath = path;
            pptPathText.text = Path.GetFileName(path);
            CheckReadyState();
        }
    }

    private void SelectTxtFile()
    {
        if (isProcessing) return;

        string path = OpenFileDialog("选择文本文件", "文本文件|*.txt");
        if (!string.IsNullOrEmpty(path))
        {
            selectedTxtPath = path;
            txtPathText.text = Path.GetFileName(path);
            CheckReadyState();
        }
    }

    private void CheckReadyState()
    {
        // 只有当两个文件都选择了，且不在处理中时，才启用开始按钮
        startButton.interactable = !string.IsNullOrEmpty(selectedPptPath) && 
                                 !string.IsNullOrEmpty(selectedTxtPath) &&
                                 !isProcessing;
    }

    private void StartProcess()
    {
        if (isProcessing) return;

        if (string.IsNullOrEmpty(selectedPptPath) || string.IsNullOrEmpty(selectedTxtPath))
        {
            statusText.text = "请先选择所有必需的文件";
            return;
        }

        isProcessing = true;
        statusText.text = "正在处理...";
        startButton.interactable = false;
        selectPptButton.interactable = false;
        selectTxtButton.interactable = false;

        // 调用Python脚本转换PPT
        string outputFolder = Path.Combine(Application.dataPath, "Resources/ppts");
        string pythonScript = Path.Combine(Application.dataPath, "Scripts/ppt_to_images.py");

        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.FileName = "python";
        startInfo.Arguments = $"\"{pythonScript}\" \"{selectedPptPath}\" \"{outputFolder}\"";
        startInfo.UseShellExecute = false;
        startInfo.RedirectStandardOutput = true;
        startInfo.CreateNoWindow = true;

        try
        {
            using (Process process = Process.Start(startInfo))
            {
                process.WaitForExit();
                if (process.ExitCode == 0)
                {
                    statusText.text = "处理完成！";
                    // 保存文本文件到Resources文件夹
                    string txtOutputPath = Path.Combine(Application.dataPath, "Resources/script.txt");
                    File.Copy(selectedTxtPath, txtOutputPath, true);
                    
                    // 延迟加载主场景
                    Invoke("LoadMainScene", 1f);
                }
                else
                {
                    statusText.text = "处理失败，请重试";
                    ResetUI();
                }
            }
        }
        catch (System.Exception e)
        {
            statusText.text = $"错误: {e.Message}";
            ResetUI();
        }
    }

    private void ResetUI()
    {
        isProcessing = false;
        startButton.interactable = true;
        selectPptButton.interactable = true;
        selectTxtButton.interactable = true;
    }

    private void LoadMainScene()
    {
        SceneManager.LoadScene("MainScene");
    }

    private string OpenFileDialog(string title, string filter)
    {
        // 在Unity编辑器中，使用EditorUtility.OpenFilePanel
        #if UNITY_EDITOR
        return UnityEditor.EditorUtility.OpenFilePanel(title, "", filter);
        #else
        // 在运行时，可以使用系统文件对话框
        // 这里需要根据具体平台实现
        return "";
        #endif
    }
} 
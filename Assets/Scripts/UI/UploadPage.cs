using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Diagnostics;
using UnityEngine.SceneManagement;
using System.Collections;

// 数据管理器，用于在场景之间传递数据
public class FileDataManager : MonoBehaviour
{
    public static FileDataManager Instance;
    
    [System.Serializable]
    public class FileData
    {
        public string pptPath;
        public string txtPath;
        public string pptFileName;
        public string txtFileName;
    }
    
    public FileData fileData = new FileData();
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void SetFileData(string pptPath, string txtPath)
    {
        fileData.pptPath = pptPath;
        fileData.txtPath = txtPath;
        fileData.pptFileName = string.IsNullOrEmpty(pptPath) ? "" : Path.GetFileName(pptPath);
        fileData.txtFileName = string.IsNullOrEmpty(txtPath) ? "" : Path.GetFileName(txtPath);
    }
    
    public FileData GetFileData()
    {
        return fileData;
    }
}

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
    private FileDataManager dataManager;
    private Process currentProcess; // 保存当前进程引用

    private void Start()
    {
        // 创建或获取数据管理器
        GameObject dataManagerObj = GameObject.Find("FileDataManager");
        if (dataManagerObj == null)
        {
            dataManagerObj = new GameObject("FileDataManager");
            dataManager = dataManagerObj.AddComponent<FileDataManager>();
        }
        else
        {
            dataManager = dataManagerObj.GetComponent<FileDataManager>();
        }
        
        // 初始化状态
        statusText.text = "请选择PPT文件和文本文件";
        startButton.interactable = false;

        // 尝试加载背景图片
        LoadBackgroundImage();
        
        // 如果有之前保存的数据，恢复显示
        RestoreFileData();
    }

    private void OnDestroy()
    {
        // 确保在对象销毁时清理进程
        if (currentProcess != null && !currentProcess.HasExited)
        {
            try
            {
                currentProcess.Kill();
            }
            catch { }
        }
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

    public void SelectPptFile()
    {
        if (isProcessing) return;

        string path = OpenFileDialog("选择PPT文件", "PowerPoint文件|*.ppt;*.pptx");
        if (!string.IsNullOrEmpty(path))
        {
            selectedPptPath = path;
            pptPathText.text = Path.GetFileName(path);
            
            // 保存到数据管理器
            dataManager.SetFileData(selectedPptPath, selectedTxtPath);
            
            CheckReadyState();
        }
    }

    public void SelectTxtFile()
    {
        if (isProcessing) return;

        string path = OpenFileDialog("选择文本文件", "文本文件|*.txt");
        if (!string.IsNullOrEmpty(path))
        {
            selectedTxtPath = path;
            txtPathText.text = Path.GetFileName(path);
            
            // 保存到数据管理器
            dataManager.SetFileData(selectedPptPath, selectedTxtPath);
            
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

    public void StartProcess()
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

        // 确保数据管理器有最新的文件路径
        dataManager.SetFileData(selectedPptPath, selectedTxtPath);

        // 启动协程处理
        StartCoroutine(ProcessPptAsync());
    }

    private System.Collections.IEnumerator ProcessPptAsync()
    {
        // 调用Python脚本转换PPT
        string outputFolder = Path.Combine(Application.dataPath, "Resources", "ppts");
        string pythonScript = Path.Combine(Application.dataPath, "Scripts", "ppt_to_images.py");

        // 标准化路径
        outputFolder = Path.GetFullPath(outputFolder);
        pythonScript = Path.GetFullPath(pythonScript);
        string normalizedPptPath = Path.GetFullPath(selectedPptPath);

        UnityEngine.Debug.Log($"标准化路径:");
        UnityEngine.Debug.Log($"  PPT文件: {normalizedPptPath}");
        UnityEngine.Debug.Log($"  输出目录: {outputFolder}");
        UnityEngine.Debug.Log($"  Python脚本: {pythonScript}");

        // 确保输出文件夹存在
        if (!Directory.Exists(outputFolder))
        {
            try
            {
                Directory.CreateDirectory(outputFolder);
                UnityEngine.Debug.Log($"创建输出目录: {outputFolder}");
            }
            catch (System.Exception createEx)
            {
                statusText.text = $"创建输出目录失败: {createEx.Message}";
                UnityEngine.Debug.LogError($"Failed to create output directory: {createEx.Message}");
                ResetUI();
                yield break;
            }
        }

        // 检查Python脚本是否存在
        if (!File.Exists(pythonScript))
        {
            statusText.text = "错误: 找不到Python脚本文件";
            UnityEngine.Debug.LogError($"Python script not found: {pythonScript}");
            ResetUI();
            yield break;
        }

        // 检查PPT文件是否存在
        if (!File.Exists(normalizedPptPath))
        {
            statusText.text = "错误: 找不到PPT文件";
            UnityEngine.Debug.LogError($"PPT file not found: {normalizedPptPath}");
            ResetUI();
            yield break;
        }

        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.FileName = "python3";
        startInfo.Arguments = $"-u \"{pythonScript}\" \"{normalizedPptPath}\" \"{outputFolder}\"";
        startInfo.UseShellExecute = false;
        startInfo.RedirectStandardOutput = true;
        startInfo.RedirectStandardError = true;
        startInfo.CreateNoWindow = true;
        startInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;
        startInfo.StandardErrorEncoding = System.Text.Encoding.UTF8;
        
        // 设置环境变量以确保Python输出UTF-8编码
        startInfo.EnvironmentVariables["PYTHONIOENCODING"] = "utf-8";

        // 启动进程
        bool processStarted = false;
        try
        {
            currentProcess = Process.Start(startInfo);
            processStarted = true;
            
            // 输出启动信息
            UnityEngine.Debug.Log($"开始PPT转换: {Path.GetFileName(selectedPptPath)}");
            UnityEngine.Debug.Log($"Python命令: python \"{pythonScript}\" \"{normalizedPptPath}\" \"{outputFolder}\"");
        }
        catch (System.Exception e)
        {
            statusText.text = $"启动处理失败: {e.Message}";
            UnityEngine.Debug.LogError($"Process start error: {e.Message}");
            ResetUI();
            yield break;
        }

        if (!processStarted || currentProcess == null)
        {
            statusText.text = "启动处理失败";
            ResetUI();
            yield break;
        }

        // 等待进程完成（不在try/catch中）
        yield return StartCoroutine(WaitForProcessCompletion());

        // 处理结果
        yield return StartCoroutine(HandleProcessResult());
    }

    private System.Collections.IEnumerator WaitForProcessCompletion()
    {
        // 设置超时时间（60秒）
        float timeout = 60f;
        float elapsed = 0f;

        // 非阻塞等待进程完成
        while (currentProcess != null && !currentProcess.HasExited && elapsed < timeout)
        {
            elapsed += Time.deltaTime;

            // 更新进度显示
            int dots = Mathf.FloorToInt(elapsed * 2) % 4;
            string progressText = "正在处理" + new string('.', dots);
            statusText.text = $"{progressText} ({elapsed:F1}s)";

            // 尝试读取实时输出（非阻塞）
            try
            {
                if (currentProcess != null && !currentProcess.StandardOutput.EndOfStream)
                {
                    string line = currentProcess.StandardOutput.ReadLine();
                    if (!string.IsNullOrEmpty(line))
                    {
                        // 检查是否是错误信息
                        if (line.Contains("出错") || line.Contains("错误") || line.Contains("失败") || line.Contains("Error"))
                        {
                            UnityEngine.Debug.LogWarning($"[PPT转换警告] {line}");
                        }
                        else if (line.Contains("成功") || line.Contains("完成") || line.Contains("转换"))
                        {
                            UnityEngine.Debug.Log($"[PPT转换进度] {line}");
                        }
                        else
                        {
                            UnityEngine.Debug.Log($"[PPT转换] {line}");
                        }
                    }
                }
            }
            catch { }

            yield return null; // 等待一帧
        }

        // 检查是否超时
        if (currentProcess != null && !currentProcess.HasExited)
        {
            // 超时，强制结束进程
            statusText.text = "处理超时，正在终止...";
            UnityEngine.Debug.LogWarning("PPT转换超时，正在终止进程...");
            
            try
            {
                currentProcess.Kill();
                currentProcess.WaitForExit(5000); // 等待5秒让进程完全结束
            }
            catch (System.Exception killEx)
            {
                UnityEngine.Debug.LogError($"终止进程失败: {killEx.Message}");
            }

            statusText.text = "处理超时，请检查文件或重试";
            CleanupProcess();
            ResetUI();
        }
    }

    private System.Collections.IEnumerator HandleProcessResult()
    {
        if (currentProcess == null)
        {
            yield break;
        }

        bool processCompleted = currentProcess.HasExited;
        int exitCode = -1;
        string standardOutput = "";
        string errorOutput = "";
        bool hasError = false;

        if (processCompleted)
        {
            try
            {
                exitCode = currentProcess.ExitCode;

                // 读取所有输出
                standardOutput = currentProcess.StandardOutput.ReadToEnd();
                errorOutput = currentProcess.StandardError.ReadToEnd();

                // 输出标准输出日志
                if (!string.IsNullOrEmpty(standardOutput))
                {
                    UnityEngine.Debug.Log($"[PPT转换输出]\n{standardOutput}");
                }

                // 输出错误日志
                if (!string.IsNullOrEmpty(errorOutput))
                {
                    if (exitCode == 0)
                    {
                        // 即使退出码为0，也可能有警告信息
                        UnityEngine.Debug.LogWarning($"[PPT转换警告]\n{errorOutput}");
                    }
                    else
                    {
                        // 分析错误类型并提供建议
                        string errorAnalysis = AnalyzeError(errorOutput);
                        UnityEngine.Debug.LogError($"[PPT转换错误]\n{errorOutput}");
                        if (!string.IsNullOrEmpty(errorAnalysis))
                        {
                            UnityEngine.Debug.LogError($"[错误分析] {errorAnalysis}");
                        }
                    }
                }

                // 输出进程完成信息
                UnityEngine.Debug.Log($"PPT转换进程完成，退出码: {exitCode}");
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"读取进程结果失败: {e.Message}");
                statusText.text = "读取处理结果失败";
                CleanupProcess();
                ResetUI();
                hasError = true;
            }
        }

        // 如果有错误，退出协程
        if (hasError)
        {
            yield break;
        }

        // 清理进程
        CleanupProcess();

        // 处理结果
        if (processCompleted)
        {
            if (exitCode == 0)
            {
                statusText.text = "PPT转换完成，正在保存文本...";
                UnityEngine.Debug.Log("PPT转换成功完成！");
                yield return new WaitForSeconds(0.1f); // 让UI更新

                // 保存文本文件
                yield return StartCoroutine(SaveTextFile());
            }
            else
            {
                string errorMsg = string.IsNullOrEmpty(errorOutput) ? "未知错误" : errorOutput;
                statusText.text = $"处理失败: {errorMsg}";
                UnityEngine.Debug.LogError($"PPT转换失败，退出码: {exitCode}");
                if (!string.IsNullOrEmpty(errorOutput))
                {
                    UnityEngine.Debug.LogError($"错误详情: {errorOutput}");
                }
                ResetUI();
            }
        }
    }

    private System.Collections.IEnumerator SaveTextFile()
    {
        bool saveSuccess = false;
        string errorMessage = "";

        // 执行文件操作（不包含yield）
        try
        {
            // 保存文本文件到Resources文件夹
            string txtOutputPath = Path.Combine(Application.dataPath, "Resources/script.txt");

            // 确保Resources文件夹存在
            string resourcesDir = Path.GetDirectoryName(txtOutputPath);
            if (!Directory.Exists(resourcesDir))
            {
                Directory.CreateDirectory(resourcesDir);
            }

            File.Copy(selectedTxtPath, txtOutputPath, true);
            saveSuccess = true;
        }
        catch (System.Exception e)
        {
            errorMessage = e.Message;
            UnityEngine.Debug.LogError($"File copy error: {e.Message}");
        }

        // 处理结果（包含yield）
        if (saveSuccess)
        {
            statusText.text = "处理完成！即将跳转...";
            yield return new WaitForSeconds(1f);

            // 加载主场景
            LoadMainScene();
        }
        else
        {
            statusText.text = $"保存文本文件失败: {errorMessage}";
            ResetUI();
        }
    }

    private void CleanupProcess()
    {
        if (currentProcess != null)
        {
            try
            {
                if (!currentProcess.HasExited)
                {
                    currentProcess.Kill();
                }
            }
            catch { }

            try
            {
                currentProcess.Dispose();
            }
            catch { }

            currentProcess = null;
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

    private void RestoreFileData()
    {
        FileDataManager.FileData fileData = dataManager.GetFileData();
        selectedPptPath = fileData.pptPath;
        selectedTxtPath = fileData.txtPath;
        pptPathText.text = fileData.pptFileName;
        txtPathText.text = fileData.txtFileName;
        CheckReadyState();
    }
    
    // 公共方法，供其他脚本获取当前选择的文件数据
    public FileDataManager.FileData GetCurrentFileData()
    {
        if (dataManager != null)
        {
            return dataManager.GetFileData();
        }
        return null;
    }
    
    // 静态方法，供其他场景获取文件数据
    public static FileDataManager.FileData GetFileDataFromManager()
    {
        if (FileDataManager.Instance != null)
        {
            return FileDataManager.Instance.GetFileData();
        }
        return null;
    }

    private string AnalyzeError(string errorOutput)
    {
        if (string.IsNullOrEmpty(errorOutput))
            return "";

        string analysis = "";

        // PowerPoint COM错误
        if (errorOutput.Contains("-2147467259") || errorOutput.Contains("δָĴ"))
        {
            analysis += "PowerPoint COM错误：可能是PowerPoint版本不兼容或权限问题。";
            analysis += "\n建议：1) 确保PowerPoint已正确安装 2) 以管理员身份运行 3) 检查PPT文件是否损坏";
        }

        // PowerPoint Visible属性错误
        if (errorOutput.Contains("-2147188160") || errorOutput.Contains("Hiding the application window is not allowed"))
        {
            analysis += "PowerPoint窗口隐藏错误：此版本的PowerPoint不允许隐藏应用程序窗口。";
            analysis += "\n建议：这是正常现象，PowerPoint将以可见模式运行，不影响转换功能";
        }

        // 编码问题
        if (errorOutput.Contains("") || errorOutput.Contains("encoding"))
        {
            analysis += "编码问题：中文字符显示异常。";
            analysis += "\n建议：检查系统区域设置和Python环境配置";
        }

        // 文件访问错误
        if (errorOutput.Contains("找不到") || errorOutput.Contains("FileNotFound") || errorOutput.Contains("Access"))
        {
            analysis += "文件访问错误：无法访问PPT文件或输出目录。";
            analysis += "\n建议：1) 检查文件路径是否正确 2) 确保有足够的磁盘空间 3) 检查文件权限";
        }

        // PowerPoint进程错误
        if (errorOutput.Contains("PowerPoint") && errorOutput.Contains("无法"))
        {
            analysis += "PowerPoint应用程序错误。";
            analysis += "\n建议：1) 关闭所有PowerPoint进程后重试 2) 重启PowerPoint应用程序 3) 检查PPT文件格式";
        }

        // Python环境错误
        if (errorOutput.Contains("ModuleNotFoundError") || errorOutput.Contains("ImportError"))
        {
            analysis += "Python模块缺失。";
            analysis += "\n建议：安装缺失的Python包：pip install comtypes pillow";
        }

        // 内存或资源错误
        if (errorOutput.Contains("内存") || errorOutput.Contains("Memory") || errorOutput.Contains("资源"))
        {
            analysis += "系统资源不足。";
            analysis += "\n建议：1) 关闭其他应用程序释放内存 2) 重启计算机 3) 分批处理大文件";
        }

        return analysis;
    }
} 
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Runtime.InteropServices;
using TMPro;
using System.Text;
// 以下引用需在导入iTextSharp库后取消注释
// using iTextSharp.text.pdf;
// using iTextSharp.text.pdf.parser;

public class MenuAdminSprite : MonoBehaviour
{
    // Start is called before the first frame update
    private float timer = 0.0f;
    
    // 报告文本的静态变量，在场景之间保持
    public static string ReportText = "";
    
    // 可选：引用输入框，用于显示文件名或内容预览
    [SerializeField] private TMP_InputField reportInputField;
    
    // 文件格式检测
    private enum FileType { TXT, PDF, DOC, UNSUPPORTED }
    
    // Windows文件对话框调用
    #if UNITY_STANDALONE_WIN
    [DllImport("Comdlg32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern bool GetOpenFileName([In, Out] OpenFileName ofn);
    
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private class OpenFileName
    {
        public int lStructSize = 0;
        public IntPtr hwndOwner = IntPtr.Zero;
        public IntPtr hInstance = IntPtr.Zero;
        public string lpstrFilter = null;
        public string lpstrCustomFilter = null;
        public int nMaxCustFilter = 0;
        public int nFilterIndex = 0;
        public string lpstrFile = null;
        public int nMaxFile = 0;
        public string lpstrFileTitle = null;
        public int nMaxFileTitle = 0;
        public string lpstrInitialDir = null;
        public string lpstrTitle = null;
        public int Flags = 0;
        public short nFileOffset = 0;
        public short nFileExtension = 0;
        public string lpstrDefExt = null;
        public IntPtr lCustData = IntPtr.Zero;
        public IntPtr lpfnHook = IntPtr.Zero;
        public string lpTemplateName = null;
        public IntPtr pvReserved = IntPtr.Zero;
        public int dwReserved = 0;
        public int FlagsEx = 0;
    }
    #endif
    
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= 5.0)
        {
            SceneManager.LoadScene("Conference Room");
        }
    }
    
    public void GameStart()
    {
        // 如果有输入框，将其文本保存到静态变量
        if (reportInputField != null && !string.IsNullOrEmpty(reportInputField.text))
        {
            ReportText = reportInputField.text;
        }
        
        SceneManager.LoadScene("Conference Room");
    }

    public void GameQuit()
    {
        Application.Quit();
    }
    
    // 打开文件选择对话框并读取文件
    public void OpenFileDialog()
    {
        string filePath = OpenFileExplorer();
        if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
        {
            try
            {
                // 尝试多种编码读取文本文件
                string fileContent = ReadTextFileWithMultipleEncodings(filePath);
                
                // 保存读取的文本
                ReportText = fileContent;
                
                // 如果有输入框，显示内容在输入框中
                if (reportInputField != null)
                {
                    reportInputField.text = ReportText;
                }
                
                Debug.Log("文件读取成功：" + filePath);
            }
            catch (System.Exception e)
            {
                Debug.LogError("读取文件出错：" + e.Message);
                
                if (reportInputField != null)
                {
                    reportInputField.text = "读取文件出错: " + e.Message + "\n\n请手动输入或选择其他文件。";
                }
            }
        }
    }
    
    // 使用多种编码尝试读取文本文件
    private string ReadTextFileWithMultipleEncodings(string filePath)
    {
        // 首先检查文件扩展名
        string extension = Path.GetExtension(filePath).ToLower();
        if (extension != ".txt" && extension != ".text")
        {
            return "请选择.txt格式的文本文件。";
        }
        
        // 准备多种编码尝试读取
        Encoding[] encodings = new Encoding[]
        {
            Encoding.UTF8,
            Encoding.ASCII,
            Encoding.GetEncoding("ISO-8859-1"),
            Encoding.Unicode,
            Encoding.UTF32
        };
        
        string content = "";
        Exception lastException = null;
        
        // 尝试用不同编码读取
        foreach (Encoding encoding in encodings)
        {
            try
            {
                content = File.ReadAllText(filePath, encoding);
                
                // 检查内容是否包含乱码
                if (!ContainsGarbage(content))
                {
                    // 找到有效编码，结束循环
                    return CleanupTextContent(content);
                }
            }
            catch (Exception ex)
            {
                lastException = ex;
            }
        }
        
        // 如果所有尝试都失败，使用二进制方式读取
        try
        {
            byte[] bytes = File.ReadAllBytes(filePath);
            content = Encoding.ASCII.GetString(bytes);
            content = CleanupTextContent(content);
            return content;
        }
        catch (Exception)
        {
            if (lastException != null)
            {
                throw lastException;
            }
            else
            {
                throw new Exception("无法读取文件内容。");
            }
        }
    }
    
    // 检查内容是否包含大量乱码
    private bool ContainsGarbage(string text)
    {
        if (string.IsNullOrEmpty(text)) return true;
        
        // 检查不可打印字符的比例
        int unprintableCount = 0;
        foreach (char c in text)
        {
            // 如果不是英文字母、数字、标点或常见符号
            if ((c < 32 || c > 126) && c != '\r' && c != '\n' && c != '\t')
            {
                unprintableCount++;
            }
        }
        
        // 如果不可打印字符超过10%，认为是乱码
        return (float)unprintableCount / text.Length > 0.1;
    }
    
    // 清理文本内容
    private string CleanupTextContent(string text)
    {
        if (string.IsNullOrEmpty(text)) return "";
        
        // 去除BOM标记和其他非打印字符
        StringBuilder cleaned = new StringBuilder();
        foreach (char c in text)
        {
            if (c >= 32 && c <= 126 || c == '\r' || c == '\n' || c == '\t')
            {
                cleaned.Append(c);
            }
        }
        
        string result = cleaned.ToString();
        
        // 规范化换行符
        result = result.Replace("\r\n", "\n").Replace("\r", "\n");
        
        return result;
    }
    
    // 打开文件浏览器
    private string OpenFileExplorer()
    {
        string filePath = "";
        
        #if UNITY_EDITOR || UNITY_STANDALONE_WIN
        #if UNITY_EDITOR
        // Unity编辑器中的实现
        filePath = UnityEditor.EditorUtility.OpenFilePanel("选择英文文本文件", "", "txt");
        #elif UNITY_STANDALONE_WIN
        // Windows平台的实现
        OpenFileName ofn = new OpenFileName();
        ofn.lStructSize = Marshal.SizeOf(ofn);
        ofn.lpstrFilter = "文本文件\0*.txt\0所有文件\0*.*\0\0";
        ofn.lpstrFile = new string(new char[256]);
        ofn.nMaxFile = ofn.lpstrFile.Length;
        ofn.lpstrFileTitle = new string(new char[64]);
        ofn.nMaxFileTitle = ofn.lpstrFileTitle.Length;
        ofn.lpstrTitle = "选择英文文本文件";
        ofn.Flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000008;
        
        if (GetOpenFileName(ofn))
        {
            filePath = ofn.lpstrFile;
        }
        #endif
        #elif UNITY_ANDROID
        // Android平台上，需要使用Unity的插件或原生插件
        Debug.Log("Android平台暂不支持文件选择");
        #elif UNITY_IOS
        // iOS平台上，需要使用Unity的插件或原生插件
        Debug.Log("iOS平台暂不支持文件选择");
        #endif
        
        return filePath;
    }
}
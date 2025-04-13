using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ReportTextProcessor : MonoBehaviour
{
    [SerializeField] private TMP_Text reportDisplayText;
    [SerializeField] private float scrollSpeed = 0.05f;
    [SerializeField] private float startDelay = 3f;
    
    private string reportText;
    private bool isDisplaying = false;
    
    void Start()
    {
        // 从StartScreenManager获取报告文本
        reportText = StartScreenManager.ReportText;
        
        if (string.IsNullOrEmpty(reportText))
        {
            Debug.LogWarning("未获取到报告文本，使用默认文本");
            reportText = "这是一个示例学术报告...";
        }
        
        // 初始显示为空
        if (reportDisplayText != null)
        {
            reportDisplayText.text = "";
            // 延迟启动文本显示
            Invoke("BeginTextDisplay", startDelay);
        }
    }
    
    void BeginTextDisplay()
    {
        if (!isDisplaying && !string.IsNullOrEmpty(reportText))
        {
            StartCoroutine(ScrollText());
        }
    }
    
    IEnumerator ScrollText()
    {
        isDisplaying = true;
        char[] chars = reportText.ToCharArray();
        
        for (int i = 0; i < chars.Length; i++)
        {
            reportDisplayText.text += chars[i];
            yield return new WaitForSeconds(scrollSpeed);
        }
        
        isDisplaying = false;
    }
    
    // 公开方法，允许其他脚本重置显示
    public void ResetDisplay()
    {
        if (reportDisplayText != null)
        {
            StopAllCoroutines();
            reportDisplayText.text = "";
            isDisplaying = false;
            Invoke("BeginTextDisplay", 1f);
        }
    }
} 
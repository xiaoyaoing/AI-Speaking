using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class StartScreenManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField reportTextInput;
    [SerializeField] private Button startSimulationButton;
    
    // 存储报告文本的静态变量，以便在场景切换后还能访问
    public static string ReportText = "";
    
    void Start()
    {
        // 添加按钮点击事件监听
        if (startSimulationButton != null)
        {
            startSimulationButton.onClick.AddListener(StartSimulation);
        }
    }
    
    // 开始模拟按钮点击事件
    public void StartSimulation()
    {
        if (reportTextInput != null)
        {
            // 保存用户输入的报告文本
            ReportText = reportTextInput.text;
            Debug.Log("保存报告文本: " + ReportText);
        }
        
        // 加载主场景
        SceneManager.LoadScene("SampleScene");
    }
} 
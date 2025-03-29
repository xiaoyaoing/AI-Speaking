using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TCPListener : MonoBehaviour
{
    private TcpClient client;
    private NetworkStream stream;
    private Thread clientThread;
    private string receivedMessage = "";
    private bool keepTrying = true;
    public GameObject administratorSprite;
    public TMP_Text stateUpdateMessage;
    public GameObject finalFeedbackMessagePanel;
    public TMP_Text finalFeedbackMessage;
    public GameObject runtimeFeedbackMessagePanel;
    public TMP_Text runtimeFeedbackMessage;
    public Text errorText;
    private string errorMessage;

    void Start()
    {
        clientThread = new Thread(new ThreadStart(ListenForMessages));
        clientThread.IsBackground = true;
        clientThread.Start();
        finalFeedbackMessagePanel.SetActive(false);
        runtimeFeedbackMessagePanel.SetActive(false);
        LogError("Start...");
    }
    
    public void LogError(string message)
    {
        Debug.LogError(message);
        errorMessage = message;
        StartCoroutine(ClearTextAfterDelay(3));  // 启动协程，等待3秒后清空文本
    }

    IEnumerator ClearTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);  // 等待指定的时间
        errorMessage = "";  // 清空文本内容
    }

    private void ListenForMessages()
    {
        while (keepTrying)
        {
            try
            {
                client = new TcpClient("localhost", 9999);
                stream = client.GetStream();

                byte[] receivedBuffer = new byte[1024];

                while (true)
                {
                    if (stream.DataAvailable)
                    {
                        int bytesRead = stream.Read(receivedBuffer, 0, receivedBuffer.Length);
                        receivedMessage = Encoding.ASCII.GetString(receivedBuffer, 0, bytesRead);
                        Debug.Log("Received: " + receivedMessage);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Socket error: " + e);
                // LogError("Socket error: " + e.Message);
                Thread.Sleep(1000);
            }
        }
    }

    void OnApplicationQuit()
    {
        keepTrying = false;
        if (stream != null)
            stream.Close();
        if (client != null)
            client.Close();
        if (clientThread != null)
            clientThread.Abort();
    }

    void Update()
    {
        if (!string.IsNullOrEmpty(receivedMessage))
        {
            Debug.Log("Processed in Update: " + receivedMessage);
            Analyse(receivedMessage);
            receivedMessage = "";
        }

        errorText.text = errorMessage;
    }

    public void Analyse(string message)
    {
        // 首先检查消息是否包含冒号，这是我们的基本格式要求
        if (!message.Contains(":"))
        {
            Debug.LogError("Message format error: No colon found.");
            return;
        }

        // 分割消息为两部分：类型和内容
        string[] parts = message.Split(new string[] { ": " }, System.StringSplitOptions.None);
        if (parts.Length != 2)
        {
            Debug.LogError("Message format error: Incorrect parts count.");
            return;
        }

        // 获取消息类型和内容
        string messageType = parts[0].Trim();
        string messageContent = parts[1].Trim();

        // 根据消息类型，执行相应的逻辑
        switch (messageType)
        {
            case "state update message":
                HandleStateUpdate(messageContent);
                break;
            case "runtime feedback message":
                HandleRuntimeFeedback(messageContent);
                break;
            case "final feedback message":
                HandleFinalFeedback(messageContent);
                break;
            default:
                Debug.Log("Unknown message type: " + messageType);
                break;
        }
    }

    // 处理状态更新消息
    private void HandleStateUpdate(string content)
    {
        // 添加状态更新的具体逻辑
        // Debug.Log("Handling state update: " + content);
        // 检查stateUpdateMessage是否已绑定
        if (stateUpdateMessage == null)
        {
            Debug.LogError("State update message text component is not assigned.");
            return;
        }

        // 将TMP_Text组件的text属性设置为传入的内容
        stateUpdateMessage.text = content;
        // Debug.Log("State updated with message: " + content);
        // TODO: 实现状态更新逻辑
    }

    // 处理运行时反馈消息
    // content: {文本}; {路径}
    private void HandleRuntimeFeedback(string content)
    {
        {
            string[] parts = content.Split(new string[] { "; " }, System.StringSplitOptions.None);
            if (parts.Length != 2)
            {
                Debug.LogError("Runtime feedback message format error.");
                return;
            }

            string textContent = parts[0];
            string musicPath = parts[1];

            runtimeFeedbackMessagePanel.SetActive(true);
            StartCoroutine(TypeText(textContent));
            AdminSprite adminScript = administratorSprite.GetComponent<AdminSprite>();
            if (adminScript != null)
            {
                adminScript.PlayMusic(musicPath);
            }
            else
            {
                Debug.LogError("AdminSprite script not found on administratorSprite.");
            }

            StartCoroutine(DisablePanelAfterDelay(30)); // 30秒后关闭面板
        }
    }

    // 处理最终反馈消息
    private void HandleFinalFeedback(string content)
    {
        // 检查文本组件和面板是否已绑定
        if (finalFeedbackMessage == null || finalFeedbackMessagePanel == null)
        {
            Debug.LogError("Final feedback components are not assigned.");
            return;
        }

        // 激活面板
        finalFeedbackMessagePanel.SetActive(true);

        // 更新文本内容
        finalFeedbackMessage.text = content;
        Debug.Log("Final feedback updated with message: " + content);
    }
    
    IEnumerator TypeText(string text)
    {
        runtimeFeedbackMessage.text = ""; // 清空现有文本
        foreach (char c in text)
        {
            runtimeFeedbackMessage.text += c; // 逐字符添加文本
            yield return new WaitForSeconds(0.05f); // 等待时间可以调整以控制打字速度
        }
    }

    IEnumerator DisablePanelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        runtimeFeedbackMessagePanel.SetActive(false);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.Text;

/// <summary>
/// WebSocket管理器 - 处理与Python服务器的通信
/// </summary>
public class WebSocketManager : MonoBehaviour
{
    [Header("WebSocket设置")]
    [Tooltip("WebSocket服务器URL")]
    public string serverUrl = "ws://localhost:8765";
    
    [Tooltip("自动连接")]
    public bool autoConnect = true;
    
    [Tooltip("自动重连")]
    public bool autoReconnect = true;
    
    [Tooltip("重连间隔(秒)")]
    public float reconnectInterval = 5f;
    
    [Header("调试")]
    [Tooltip("启用调试日志")]
    public bool enableDebugLog = true;
    
    // 学术汇报管理器引用
    private AcademicPresentationManager presentationManager;
    
    // WebSocket客户端
    private ClientWebSocket webSocket;
    
    // 取消令牌源
    private CancellationTokenSource cancellationTokenSource;
    
    // 连接状态
    private bool isConnected = false;
    private bool isConnecting = false;
    
    // 接收到的消息队列
    private Queue<string> messageQueue = new Queue<string>();
    
    // 接收到的音频数据
    private byte[] receivedAudioData;
    private string receivedQuestionText;
    private bool hasNewAudio = false;
    
    private void Start()
    {
        // 获取学术汇报管理器引用
        presentationManager = FindObjectOfType<AcademicPresentationManager>();
        
        if (presentationManager == null)
        {
            Debug.LogError("未能找到AcademicPresentationManager组件！");
        }
        
        // 自动连接
        if (autoConnect)
        {
            ConnectToServer();
        }
    }
    
    private void Update()
    {
        // 处理消息队列
        ProcessMessageQueue();
        
        // 处理新的音频数据
        if (hasNewAudio && receivedAudioData != null && presentationManager != null)
        {
            hasNewAudio = false;
            
            try
            {
                // 播放接收到的音频
                presentationManager.PlayQuestionFromBytes(receivedAudioData, receivedQuestionText);
            }
            catch (Exception e)
            {
                Debug.LogError($"播放接收到的音频时出错: {e.Message}");
            }
        }
    }
    
    /// <summary>
    /// 连接到WebSocket服务器
    /// </summary>
    public async void ConnectToServer()
    {
        if (isConnected || isConnecting) return;
        
        isConnecting = true;
        
        try
        {
            // 创建新的WebSocket客户端
            webSocket = new ClientWebSocket();
            cancellationTokenSource = new CancellationTokenSource();
            
            // 连接到服务器
            Uri serverUri = new Uri(serverUrl);
            
            if (enableDebugLog)
            {
                Debug.Log($"正在连接到WebSocket服务器: {serverUrl}");
            }
            
            await webSocket.ConnectAsync(serverUri, cancellationTokenSource.Token);
            
            isConnected = true;
            isConnecting = false;
            
            if (enableDebugLog)
            {
                Debug.Log("已连接到WebSocket服务器");
            }
            
            // 开始接收消息
            _ = ReceiveMessages();
        }
        catch (Exception e)
        {
            Debug.LogError($"连接到WebSocket服务器时出错: {e.Message}");
            isConnected = false;
            isConnecting = false;
            
            // 自动重连
            if (autoReconnect)
            {
                Debug.Log($"将在 {reconnectInterval} 秒后尝试重连...");
                Invoke("ConnectToServer", reconnectInterval);
            }
        }
    }
    
    /// <summary>
    /// 断开与服务器的连接
    /// </summary>
    public async void DisconnectFromServer()
    {
        if (!isConnected) return;
        
        try
        {
            // 取消所有正在进行的操作
            cancellationTokenSource.Cancel();
            
            // 关闭WebSocket连接
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "客户端主动断开", CancellationToken.None);
            
            if (enableDebugLog)
            {
                Debug.Log("已断开与WebSocket服务器的连接");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"断开连接时出错: {e.Message}");
        }
        finally
        {
            isConnected = false;
            webSocket.Dispose();
            webSocket = null;
        }
    }
    
    /// <summary>
    /// 发送消息到服务器
    /// </summary>
    /// <param name="message">要发送的消息</param>
    public async void SendMessage(string message)
    {
        if (!isConnected || webSocket == null) return;
        
        try
        {
            // 将消息转换为字节数组
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            
            // 发送消息
            await webSocket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, cancellationTokenSource.Token);
            
            if (enableDebugLog)
            {
                Debug.Log($"已发送消息: {message}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"发送消息时出错: {e.Message}");
            
            // 如果连接已断开，尝试重连
            if (webSocket.State != WebSocketState.Open && autoReconnect)
            {
                Debug.Log("连接已断开，尝试重新连接...");
                DisconnectFromServer();
                Invoke("ConnectToServer", reconnectInterval);
            }
        }
    }
    
    /// <summary>
    /// 异步接收消息
    /// </summary>
    private async Task ReceiveMessages()
    {
        if (webSocket == null) return;
        
        byte[] buffer = new byte[4096];
        
        try
        {
            while (webSocket.State == WebSocketState.Open && !cancellationTokenSource.Token.IsCancellationRequested)
            {
                // 接收消息
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationTokenSource.Token);
                
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    // 服务器请求关闭连接
                    Debug.Log("服务器请求关闭连接");
                    DisconnectFromServer();
                    return;
                }
                
                // 处理文本消息
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    
                    lock (messageQueue)
                    {
                        messageQueue.Enqueue(message);
                    }
                    
                    if (enableDebugLog)
                    {
                        Debug.Log($"收到文本消息: {message}");
                    }
                }
                // 处理二进制消息（音频数据）
                else if (result.MessageType == WebSocketMessageType.Binary)
                {
                    // 创建临时缓冲区
                    using (var ms = new System.IO.MemoryStream())
                    {
                        // 写入已接收的数据
                        ms.Write(buffer, 0, result.Count);
                        
                        // 如果消息未完成，继续接收
                        while (!result.EndOfMessage)
                        {
                            result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationTokenSource.Token);
                            ms.Write(buffer, 0, result.Count);
                        }
                        
                        // 保存接收到的数据
                        receivedAudioData = ms.ToArray();
                        receivedQuestionText = "从Python接收的音频问题";
                        hasNewAudio = true;
                        
                        if (enableDebugLog)
                        {
                            Debug.Log($"收到二进制消息: {receivedAudioData.Length} 字节");
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            if (e is OperationCanceledException)
            {
                Debug.Log("WebSocket接收操作已取消");
            }
            else
            {
                Debug.LogError($"接收消息时出错: {e.Message}");
                
                // 如果连接已断开，尝试重连
                if (autoReconnect && webSocket != null)
                {
                    Debug.Log("连接已断开，尝试重新连接...");
                    DisconnectFromServer();
                    Invoke("ConnectToServer", reconnectInterval);
                }
            }
        }
    }
    
    /// <summary>
    /// 处理接收到的消息
    /// </summary>
    private void ProcessMessageQueue()
    {
        if (messageQueue.Count == 0) return;
        
        List<string> messagesToProcess = new List<string>();
        
        lock (messageQueue)
        {
            while (messageQueue.Count > 0)
            {
                messagesToProcess.Add(messageQueue.Dequeue());
            }
        }
        
        foreach (string message in messagesToProcess)
        {
            try
            {
                // 使用简单的格式解析消息，格式如: "type:question_info|text:这是问题内容"
                if (message.Contains(":") && message.Contains("|"))
                {
                    string[] parts = message.Split('|');
                    Dictionary<string, string> data = new Dictionary<string, string>();
                    
                    foreach (string part in parts)
                    {
                        string[] keyValue = part.Split(':');
                        if (keyValue.Length == 2)
                        {
                            data[keyValue[0].Trim()] = keyValue[1].Trim();
                        }
                    }
                    
                    // 根据消息类型处理
                    if (data.ContainsKey("type"))
                    {
                        string messageType = data["type"];
                        
                        switch (messageType)
                        {
                            case "question_info":
                                // 更新问题文本
                                if (data.ContainsKey("text"))
                                {
                                    receivedQuestionText = data["text"];
                                    Debug.Log($"收到问题信息: {receivedQuestionText}");
                                }
                                break;
                                
                            case "playback_control":
                                // 控制播放速度
                                if (data.ContainsKey("speed") && float.TryParse(data["speed"], out float speed) && presentationManager != null)
                                {
                                    presentationManager.SetAudioPlaybackSpeed(speed);
                                }
                                break;
                                
                            // 可以添加更多消息类型的处理
                            default:
                                Debug.LogWarning($"未知的消息类型: {messageType}");
                                break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"处理消息时出错: {e.Message}");
            }
        }
    }
    
    private void OnDestroy()
    {
        // 断开连接
        DisconnectFromServer();
    }
} 
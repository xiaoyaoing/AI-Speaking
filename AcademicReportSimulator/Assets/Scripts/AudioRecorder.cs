using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(AudioSource))]
public class AudioRecorder : MonoBehaviour
{
    public AudioSource audioSource;
    private bool isRecording = false;
    private AudioClip recordingClip;

    private float time = 0f;
    public TMP_Text feedbackText;
    
    // 添加对语速和剩余时间文本组件的引用
    public TMP_Text voiceSpeedText; // 语速显示
    public TMP_Text timeLeftText;   // 剩余时间显示
    
    // 演讲总时间为5分钟
    private float totalPresentationTime = 300f; // 5分钟 = 300秒
    private float remainingTime;
    
    // 语速相关变量
    private float currentVoiceSpeed;
    private float minVoiceSpeed = 120f; // 最小语速(词/分钟)
    private float maxVoiceSpeed = 180f; // 最大语速(词/分钟)
    private float voiceSpeedUpdateInterval = 3f; // 语速更新间隔
    private float voiceSpeedTimer = 0f;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        // StartRecording();
        // isRecording = true;
        
        // 初始化剩余时间
        remainingTime = totalPresentationTime;
        
        // 初始化语速
        UpdateVoiceSpeed();
        
        // 查找文本组件(如果没有在Inspector中赋值)
        if (voiceSpeedText == null)
        {
            voiceSpeedText = GameObject.Find("VoiceSpeed")?.GetComponent<TMP_Text>();
        }
        
        if (timeLeftText == null)
        {
            timeLeftText = GameObject.Find("TimeLeft")?.GetComponent<TMP_Text>();
        }
        
        // 更新初始UI显示
        UpdateTimeLeftUI();
        UpdateVoiceSpeedUI();
    }

    void Update()
    {
        // 原有的录音逻辑
        time += Time.deltaTime;
        if (time >= 5.0 && isRecording)
        {
            StopRecordingAndSend();
            isRecording = false;
        }

        if (time >= 5.0 && (!isRecording))
        {
            time = 0.0f;
            StartRecording();
            isRecording = true;
        }

        if (Input.GetKeyDown(KeyCode.Space) && (!isRecording))
        {
            time = 0.0f;
            StartRecording();
            isRecording = true;
        }
        
        // 更新演讲剩余时间
        remainingTime -= Time.deltaTime;
        if (remainingTime < 0)
            remainingTime = 0;
        
        // 定期更新UI显示
        UpdateTimeLeftUI();
        
        // 定期更新语速
        voiceSpeedTimer += Time.deltaTime;
        if (voiceSpeedTimer >= voiceSpeedUpdateInterval)
        {
            voiceSpeedTimer = 0f;
            UpdateVoiceSpeed();
            UpdateVoiceSpeedUI();
        }
    }
    
    // 更新语速 - 随机生成
    void UpdateVoiceSpeed()
    {
        currentVoiceSpeed = Random.Range(minVoiceSpeed, maxVoiceSpeed);
    }
    
    // 更新语速UI
    void UpdateVoiceSpeedUI()
    {
        if (voiceSpeedText != null)
        {
            string speedCategory = GetSpeedCategory(currentVoiceSpeed);
            voiceSpeedText.text = speedCategory + " - " + currentVoiceSpeed.ToString("F0") + " words/min";
            
            // 根据语速设置颜色
            if (currentVoiceSpeed < 140)
                voiceSpeedText.color = Color.green; // 较慢语速显示为绿色
            else if (currentVoiceSpeed > 160)
                voiceSpeedText.color = Color.red;   // 较快语速显示为红色
            else
                voiceSpeedText.color = Color.yellow; // 中等语速显示为黄色
        }
    }
    
    // 获取语速分类
    string GetSpeedCategory(float speed)
    {
        if (speed < 130)
            return "Slow";
        else if (speed < 150)
            return "Moderate";
        else if (speed < 170)
            return "Fast";
        else
            return "Very Fast";
    }
    
    // 更新剩余时间UI
    void UpdateTimeLeftUI()
    {
        if (timeLeftText != null)
        {
            int minutes = Mathf.FloorToInt(remainingTime / 60);
            int seconds = Mathf.FloorToInt(remainingTime % 60);
            
            timeLeftText.text = string.Format("Time Left: {0:00}:{1:00}", minutes, seconds);
            
            // 根据剩余时间设置颜色
            if (remainingTime > 120) // 超过2分钟
                timeLeftText.color = Color.green;
            else if (remainingTime > 60) // 1-2分钟
                timeLeftText.color = Color.yellow;
            else // 不到1分钟
                timeLeftText.color = Color.red;
        }
    }

    void StartRecording()
    {
        isRecording = true;
        recordingClip = Microphone.Start(null, false, 5, 44100);
        Debug.Log("Recording started...");
    }

    void StopRecordingAndSend()
    {
        Microphone.End(null);
        isRecording = false;
        Debug.Log("Recording stopped.");

        SendAudioToServer(recordingClip);
    }

    void SendAudioToServer(AudioClip clip)
    {
        var samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);
        var bytes = new byte[samples.Length * 4];
        System.Buffer.BlockCopy(samples, 0, bytes, 0, bytes.Length);

        StartCoroutine(HttpSender.SendAudioData("http://127.0.0.1:5000/analyze", bytes, HandleAudioResponse));
    }
    
    void HandleAudioResponse(string jsonResponse)
    {
        if (!string.IsNullOrEmpty(jsonResponse))
        {
            // 假设 jsonResponse 是一个包含 `feedback` 字段的 JSON 字符串
            FeedbackData feedbackData = JsonUtility.FromJson<FeedbackData>(jsonResponse);
            feedbackText.text = feedbackData.feedback; // 更新TextMeshPro文本
        }
        else
        {
            feedbackText.text = "Failed to receive or parse the server response.";
        }
    }

    [System.Serializable]
    public class FeedbackData
    {
        public string feedback;
    }
}

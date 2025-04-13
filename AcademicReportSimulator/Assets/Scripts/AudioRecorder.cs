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

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        // StartRecording();
        // isRecording = true;
    }

    void Update()
    {
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
        // if (Input.GetKeyUp(KeyCode.Space))
        // {
        //     if (!isRecording)
        //     {
        //         StartRecording();
        //     }
        //     else
        //     {
        //         StopRecordingAndSend();
        //     }
        // }
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;
using System;

[RequireComponent(typeof(AudioSource))]
public class AudioRecorder : MonoBehaviour
{
    public static AudioClip LastRecordedClip { get; private set; }

    public AudioSource audioSource;
    private bool isRecording = false;
    private AudioClip recordingClip;

    private float time = 0f;
    public TMP_Text feedbackText;
    
    // Storage settings
    private string saveFolder = "Recordings";
    private int fileCounter = 0;
    private const int MAX_RECORDINGS = 100; // Prevent unlimited storage

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        
        // Create save directory if it doesn't exist
        string fullPath = Path.Combine(Application.dataPath, saveFolder);
        if (!Directory.Exists(fullPath))
        {
            Directory.CreateDirectory(fullPath);
        }
        Debug.Log("音频存储目录: " + fullPath);
        // Initialize file counter based on existing files
        InitializeFileCounter();
    }

    void InitializeFileCounter()
    {
        string fullPath = Path.Combine(Application.dataPath, saveFolder);
        if (Directory.Exists(fullPath))
        {
            string[] files = Directory.GetFiles(fullPath, "*.wav");
            fileCounter = files.Length;
        }
    }

    void Update()
    {
        time += Time.deltaTime;
        // if (time >= 5.0 && isRecording)
        // {
        //     StopRecordingAndSend();
        //     isRecording = false;
        // }

    if (!isRecording)
    {
        StartRecording();
    }

        if (Input.GetKeyDown(KeyCode.Z) && (isRecording))
        {
            time = 0.0f;
            StopRecordingAndSend();
            // isRecording = false;  
        }
    }

    void StartRecording()
    {
        isRecording = true;
        // 0 表示无限录制，true 表示循环缓冲
        recordingClip = Microphone.Start(null, true, 10, 44100); // 10 是缓冲长度，可以调整
        Debug.Log("Recording started (Press Z to stop)...");
    }

    void StopRecordingAndSend()
    {
           Microphone.End(null);
        isRecording = false;
        Debug.Log("Recording stopped.");

        // Save the recording
        SaveAudioClip(recordingClip);

    }
AudioClip TrimAudioClip(AudioClip clip, int recordingPos)
{
    // 获取原始音频数据
    float[] samples = new float[clip.samples * clip.channels];
    clip.GetData(samples, 0);

    // 计算有效数据的长度
    int effectiveSamples = recordingPos * clip.channels;
    if (effectiveSamples <= 0) effectiveSamples = samples.Length;

    // 提取有效部分
    float[] trimmedSamples = new float[effectiveSamples];
    Array.Copy(samples, trimmedSamples, effectiveSamples);

    // 创建新的 AudioClip
    AudioClip trimmedClip = AudioClip.Create(
        "TrimmedClip",
        effectiveSamples / clip.channels,
        clip.channels,
        clip.frequency,
        false
    );
    trimmedClip.SetData(trimmedSamples, 0);

    return trimmedClip;
}
    void SaveAudioClip(AudioClip clip)
    {
        Debug.Log("fileCounter:"+fileCounter);
        if (fileCounter >= MAX_RECORDINGS)
        {
            Debug.LogWarning("Maximum number of recordings reached. Not saving this one.");
            // return;
        }

        string fileName = $"recording_{DateTime.Now:yyyyMMdd_HHmmss}.wav";
        string fullPath = Path.Combine(Application.dataPath, saveFolder, fileName);
        
        try
        {
            SavWav.Save(fullPath, clip);
            // fileCounter++;
            Debug.Log($"Audio saved to: {fullPath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save audio: {e.Message}");
        }
    }

    public byte[] GetAudioBytes(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogError("AudioClip is null!");
            return null;
        }

        var samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);

        var bytes = new byte[samples.Length * 4];
        System.Buffer.BlockCopy(samples, 0, bytes, 0, bytes.Length);

        return bytes;
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
            FeedbackData feedbackData = JsonUtility.FromJson<FeedbackData>(jsonResponse);
            feedbackText.text = feedbackData.feedback;
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

// Helper class to save WAV files
public static class SavWav
{
    const int HEADER_SIZE = 44;

    public static bool Save(string filepath, AudioClip clip)
    {
        if (!filepath.ToLower().EndsWith(".wav"))
        {
            filepath += ".wav";
        }

        Directory.CreateDirectory(Path.GetDirectoryName(filepath));

        using (var fileStream = CreateEmpty(filepath))
        {
            ConvertAndWrite(fileStream, clip);
            WriteHeader(fileStream, clip);
        }

        return true;
    }

    static FileStream CreateEmpty(string filepath)
    {
        var fileStream = new FileStream(filepath, FileMode.Create);
        byte emptyByte = new byte();

        for (int i = 0; i < HEADER_SIZE; i++)
        {
            fileStream.WriteByte(emptyByte);
        }

        return fileStream;
    }

    static void ConvertAndWrite(FileStream fileStream, AudioClip clip)
    {
        var samples = new float[clip.samples];
        clip.GetData(samples, 0);

        Int16[] intData = new Int16[samples.Length];
        Byte[] bytesData = new Byte[samples.Length * 2];

        int rescaleFactor = 32767; //to convert float to Int16

        for (int i = 0; i < samples.Length; i++)
        {
            intData[i] = (short)(samples[i] * rescaleFactor);
            Byte[] byteArr = new Byte[2];
            byteArr = BitConverter.GetBytes(intData[i]);
            byteArr.CopyTo(bytesData, i * 2);
        }

        fileStream.Write(bytesData, 0, bytesData.Length);
    }

    static void WriteHeader(FileStream fileStream, AudioClip clip)
    {
        var hz = clip.frequency;
        var channels = clip.channels;
        var samples = clip.samples;

        fileStream.Seek(0, SeekOrigin.Begin);

        Byte[] riff = System.Text.Encoding.UTF8.GetBytes("RIFF");
        fileStream.Write(riff, 0, 4);

        Byte[] chunkSize = BitConverter.GetBytes(fileStream.Length - 8);
        fileStream.Write(chunkSize, 0, 4);

        Byte[] wave = System.Text.Encoding.UTF8.GetBytes("WAVE");
        fileStream.Write(wave, 0, 4);

        Byte[] fmt = System.Text.Encoding.UTF8.GetBytes("fmt ");
        fileStream.Write(fmt, 0, 4);

        Byte[] subChunk1 = BitConverter.GetBytes(16);
        fileStream.Write(subChunk1, 0, 4);

        UInt16 one = 1;

        Byte[] audioFormat = BitConverter.GetBytes(one);
        fileStream.Write(audioFormat, 0, 2);

        Byte[] numChannels = BitConverter.GetBytes(channels);
        fileStream.Write(numChannels, 0, 2);

        Byte[] sampleRate = BitConverter.GetBytes(hz);
        fileStream.Write(sampleRate, 0, 4);

        Byte[] byteRate = BitConverter.GetBytes(hz * channels * 2); // sampleRate * bytesPerSample*number of channels
        fileStream.Write(byteRate, 0, 4);

        UInt16 blockAlign = (ushort)(channels * 2);
        fileStream.Write(BitConverter.GetBytes(blockAlign), 0, 2);

        UInt16 bps = 16;
        Byte[] bitsPerSample = BitConverter.GetBytes(bps);
        fileStream.Write(bitsPerSample, 0, 2);

        Byte[] datastring = System.Text.Encoding.UTF8.GetBytes("data");
        fileStream.Write(datastring, 0, 4);

        Byte[] subChunk2 = BitConverter.GetBytes(samples * channels * 2);
        fileStream.Write(subChunk2, 0, 4);
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 音频工具类 - 提供音频处理工具函数
/// </summary>
public static class AudioUtility
{
    /// <summary>
    /// 从WAV格式的字节数组创建AudioClip
    /// </summary>
    /// <param name="wavData">WAV格式的字节数组</param>
    /// <param name="clipName">AudioClip名称</param>
    /// <returns>创建的AudioClip</returns>
    public static AudioClip CreateAudioClipFromWAV(byte[] wavData, string clipName = "clip_from_bytes")
    {
        // 注意：此方法仅支持标准16位PCM WAV格式
        try
        {
            // 简单验证WAV头部
            if (wavData.Length < 44) // 最小的WAV头
            {
                Debug.LogError("WAV数据过短，无法解析头部");
                return null;
            }

            // 验证RIFF标记
            if (wavData[0] != 'R' || wavData[1] != 'I' || wavData[2] != 'F' || wavData[3] != 'F')
            {
                Debug.LogError("无效的WAV格式：未找到RIFF标记");
                return null;
            }

            // 验证WAVE标记
            if (wavData[8] != 'W' || wavData[9] != 'A' || wavData[10] != 'V' || wavData[11] != 'E')
            {
                Debug.LogError("无效的WAV格式：未找到WAVE标记");
                return null;
            }

            // 解析基本音频参数
            int channelCount = wavData[22] | (wavData[23] << 8);
            int sampleRate = wavData[24] | (wavData[25] << 8) | (wavData[26] << 16) | (wavData[27] << 24);
            int bitsPerSample = wavData[34] | (wavData[35] << 8);

            // 寻找数据块
            int dataStartIndex = -1;
            for (int i = 12; i < wavData.Length - 4; i++)
            {
                if (wavData[i] == 'd' && wavData[i + 1] == 'a' && wavData[i + 2] == 't' && wavData[i + 3] == 'a')
                {
                    dataStartIndex = i + 8; // 跳过"data"标记和数据大小
                    break;
                }
            }

            if (dataStartIndex == -1)
            {
                Debug.LogError("无效的WAV格式：未找到数据块");
                return null;
            }

            // 计算样本数
            int dataLength = wavData.Length - dataStartIndex;
            int sampleCount = dataLength / (bitsPerSample / 8) / channelCount;

            // 创建音频剪辑
            AudioClip clip = AudioClip.Create(clipName, sampleCount, channelCount, sampleRate, false);

            // 解析音频数据
            float[] audioData = new float[sampleCount * channelCount];
            int audioIndex = 0;

            // 仅支持16位PCM
            if (bitsPerSample == 16)
            {
                for (int i = dataStartIndex; i < wavData.Length; i += 2)
                {
                    if (audioIndex >= audioData.Length) break;

                    short sample = (short)(wavData[i] | (wavData[i + 1] << 8));
                    audioData[audioIndex] = sample / 32768f; // 将16位有符号整数转换为[-1, 1]范围的浮点数
                    audioIndex++;
                }
            }
            else
            {
                Debug.LogWarning($"不支持的WAV格式：{bitsPerSample}位，仅支持16位PCM");
                return null;
            }

            // 设置音频数据
            clip.SetData(audioData, 0);
            return clip;
        }
        catch (Exception e)
        {
            Debug.LogError($"创建音频剪辑时出错：{e.Message}\n{e.StackTrace}");
            return null;
        }
    }

    /// <summary>
    /// 获取WAV文件的音频长度(秒)
    /// </summary>
    /// <param name="wavData">WAV格式的字节数组</param>
    /// <returns>音频长度(秒)</returns>
    public static float GetWAVDuration(byte[] wavData)
    {
        try
        {
            // 简单验证WAV头部
            if (wavData.Length < 44)
            {
                Debug.LogError("WAV数据过短，无法解析头部");
                return 0f;
            }

            // 解析基本音频参数
            int channelCount = wavData[22] | (wavData[23] << 8);
            int sampleRate = wavData[24] | (wavData[25] << 8) | (wavData[26] << 16) | (wavData[27] << 24);
            int bitsPerSample = wavData[34] | (wavData[35] << 8);

            // 寻找数据块
            int dataStartIndex = -1;
            int dataSize = 0;
            for (int i = 12; i < wavData.Length - 8; i++)
            {
                if (wavData[i] == 'd' && wavData[i + 1] == 'a' && wavData[i + 2] == 't' && wavData[i + 3] == 'a')
                {
                    dataSize = wavData[i + 4] | (wavData[i + 5] << 8) | (wavData[i + 6] << 16) | (wavData[i + 7] << 24);
                    dataStartIndex = i + 8;
                    break;
                }
            }

            if (dataStartIndex == -1)
            {
                Debug.LogError("无效的WAV格式：未找到数据块");
                return 0f;
            }

            // 计算播放时长
            int bytesPerSample = bitsPerSample / 8;
            int bytesPerSecond = sampleRate * channelCount * bytesPerSample;
            float durationInSeconds = (float)dataSize / bytesPerSecond;

            return durationInSeconds;
        }
        catch (Exception e)
        {
            Debug.LogError($"计算WAV时长时出错：{e.Message}");
            return 0f;
        }
    }
} 
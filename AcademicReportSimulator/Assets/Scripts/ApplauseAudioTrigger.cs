using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplauseAudioTrigger : MonoBehaviour
{
    private AudioSource audioSource;

    void Start()
    {
        // 获取当前GameObject的AudioSource组件
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        // 检查是否按下了'A'键
        if (Input.GetKeyDown(KeyCode.C))
        {
            // 播放音频
            PlayAudio();
        }
    }

    void PlayAudio()
    {
        if (audioSource != null && !audioSource.isPlaying)
        {
            // 播放绑定到AudioSource组件的音频剪辑
            audioSource.Play();
        }
    }
}

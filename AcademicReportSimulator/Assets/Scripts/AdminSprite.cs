using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AdminSprite : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject audioSourceCarrier;
    public GameObject settingMenuPanel;
    private AudioSource _audioSource;
    public Slider volumeSlider;
    public GameObject player;
    void Start()
    {
        _audioSource = audioSourceCarrier.GetComponent<AudioSource>();
        Debug.Log(_audioSource.name);
        // StartCoroutine(LoadAndPlay("file://D:/CsharpProjects/LightExercise/Assets/TestAudio.mp3"));
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        settingMenuPanel.SetActive(false);
        if (volumeSlider != null)
        {
            volumeSlider.value = _audioSource.volume; // 初始化滑动条位置
            volumeSlider.onValueChanged.AddListener(SetVolume); // 为滑动条添加监听器
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            ToggleSettingsMenu();
        }
    }
    
    IEnumerator LoadAndPlay(string path)
    {
        Debug.Log("LoadAndPlay Called.");
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.MPEG))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError)
            {
                Debug.Log(www.error);
            }
            else
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                if (clip != null)
                {
                    _audioSource.clip = clip;
                    _audioSource.Play();
                }
                else
                {
                    Debug.Log("AudioSourceNull.");
                }
            }
        }
    }

    public void PlayMusic(string musicPath)
    {
        StartCoroutine(LoadAndPlay(musicPath));
    }

    public void BackToMenu()
    {
        Debug.Log("AdminSprite.BackToMenu() called.");
    }

    public void BackToGame()
    {
        Debug.Log("AdminSprite.BackToGame() called.");
    }
    
    void ToggleSettingsMenu()
    {
        // 检查设置菜单的当前活动状态，并切换它
        if (settingMenuPanel.activeSelf)
        {
            // 如果菜单当前是显示的，隐藏它并锁定光标，重新激活镜头移动脚本
            settingMenuPanel.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            player.GetComponent<CameraMovable>().enabled = true;
        }
        else
        {
            // 如果菜单当前是隐藏的，显示它并解锁光标，取消激活镜头移动脚本
            settingMenuPanel.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            player.GetComponent<CameraMovable>().enabled = false;
        }
    }
    
    public void SetVolume(float volume)
    {
        if (_audioSource != null)
        {
            _audioSource.volume = volume; // 设置音量
        }
    }
    
    public void BackButton()
    {
        ToggleSettingsMenu();
    }

    public void MenuButton()
    {
        SceneManager.LoadScene("MainMenu");
    }
}

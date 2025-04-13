using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class CameraCapture : MonoBehaviour
{
    private WebCamTexture webCamTexture;
    private float timer = 15f;  // 时间间隔为15秒
    private bool isCapturing = false;  // 控制是否开始捕捉

    public TMP_Text feedbackText;

    void Update()
    {
        // 检查是否按下空格键且摄像头尚未启动
        if (Input.GetKeyDown(KeyCode.Space) && !isCapturing)
        {
            StartCapturing();
        }

        // 如果摄像头正在工作，则处理计时和发送图像
        if (isCapturing)
        {
            if (timer <= 0)
            {
                StartCoroutine(SendCurrentImage());
                timer = 15f;  // 重置计时器为15秒
                feedbackText.text = "Calculating Emotion";  // 显示“Calculating Emotion”
            }
            timer -= Time.deltaTime;  // 更新计时器
        }
    }

    void StartCapturing()
    {
        if (webCamTexture == null)
        {
            webCamTexture = new WebCamTexture();
        }
        webCamTexture.Play();  // 开始捕捉摄像头视频
        isCapturing = true;  // 标记摄像头已开始工作
        feedbackText.text = "Capturing Started";  // 显示“Capturing Started”
    }

    IEnumerator SendCurrentImage()
    {
        Texture2D image = new Texture2D(webCamTexture.width, webCamTexture.height);
        image.SetPixels(webCamTexture.GetPixels());
        image.Apply();

        byte[] bytes = image.EncodeToPNG();  // 将图片编码为PNG
        Destroy(image);  // 释放Texture2D资源

        WWWForm form = new WWWForm();
        form.AddBinaryData("file", bytes, "cameraImage.png", "image/png");

        using (UnityWebRequest www = UnityWebRequest.Post("http://127.0.0.1:5000/camera", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + www.error);
                feedbackText.text = "Error: " + www.error;  // 显示错误信息
            }
            else
            {
                Debug.Log("Image sent successfully");
                // 解析服务器返回的JSON数据
                string json = www.downloadHandler.text;
                FeedbackData feedback = JsonUtility.FromJson<FeedbackData>(json);
                feedbackText.text = "Expression: " + feedback.expression + "\n Score: " + feedback.score;  // 更新UI文本
                
                yield return new WaitForSeconds(7f);  // 等待5秒
                
                feedbackText.text = "Capturing Started";  // 显示“Capturing Started”
                isCapturing = false;  // 停止捕捉，等待下一次按键启动
            }
        }
    }

    [System.Serializable]
    public class FeedbackData
    {
        public string message;      // 添加message字段
        public string expression;   // 调整expression字段以匹配后端返回的格式
        public float score;
    }
}

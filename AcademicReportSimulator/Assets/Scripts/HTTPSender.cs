using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;

public class HttpSender : MonoBehaviour
{
    public static IEnumerator SendAudioData(string url, byte[] audioData, Action<string> onResponseReceived)
    {
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(audioData);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/octet-stream");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error: " + request.error);
            onResponseReceived?.Invoke(null); // Invoke callback with null if error
        }
        else
        {
            Debug.Log("Response received: " + request.downloadHandler.text);
            onResponseReceived?.Invoke(request.downloadHandler.text); // Invoke callback with response
        }
    }
}

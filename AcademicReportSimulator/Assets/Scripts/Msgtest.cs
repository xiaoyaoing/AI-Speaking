using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TimedMessage
{
    public float triggerTime;  // 触发时间，以秒为单位
    public string message;     // 要发送的消息
    public bool hasBeenSent;   // 标记消息是否已发送
}

public class Msgtest : MonoBehaviour
{
    // Start is called before the first frame update
    public MessageQueueManager messageQueueManager;
    public GameObject sePlayer;
    private float time = 0.0f;
    private string count = "I";
    private bool isStart = false;
    public List<TimedMessage> timedMessages;
    void Start()
    {
        // messageQueueManager.SendMessage("Hello, this is a new message!");
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            isStart = true;
            time = 0f;
        }

        if (isStart)
        {
            foreach (var timedMessage in timedMessages)
            {
                if (!timedMessage.hasBeenSent && time >= timedMessage.triggerTime)
                {
                    // messageQueueManager.SendMessage(timedMessage.message);
                    timedMessage.hasBeenSent = true; // 标记为已发送，防止重复发送
                }
            }
        }
    }
}
